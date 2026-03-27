# WhatsApp .NET SDK — Agent Reference

> **Canonical source**: `.github/copilot-instructions.md` covers build/test/format commands.
> This document covers architecture, design decisions, and codebase conventions.

---

## Repository Overview

**Package**: `Devlooped.WhatsApp` (`src/WhatsApp/`)  
**NuGet ID**: `Devlooped.WhatsApp`  
**Targets**: `net8.0`, `net10.0`  

A .NET SDK for building WhatsApp for Business bots and agents on Azure Functions. It provides a handler pipeline pattern, message/response abstractions, Azure Storage-backed idempotency and conversation storage, WhatsApp Flows support, and optional OpenTelemetry/logging middleware.

---

## Project Structure

```
src/
  WhatsApp/          # Main library (Devlooped.WhatsApp)
  Tests/             # xUnit test project
  CodeAnalysis/      # Roslyn analyzer (SendStringAnalyzer)
  SampleApp/         # Sample Azure Functions apps
    Dashboard/       # Dashboard sample
    Sample/          # Bot sample
  Console/           # dotnet-whatsapp CLI tool
  Directory.Build.props   # Shared MSBuild props (versioning, signing, NuGet)
  Directory.Build.targets # Shared targets
```

---

## Core Abstractions

### `IMessage` (interface)
The universal message interface. All messages and responses implement it.
- `Id`, `UserNumber`, `ServiceId`, `Timestamp`, `Context`, `Type`
- `AdditionalProperties` — extensible bag for extra data
- JSON polymorphic via `[JsonDerivedType]` — discriminator maps type strings like `"content"`, `"status"`, `"response/text"`, `"flow/int"`, etc.

### `Message` (abstract record)
Base for all **incoming** messages from WhatsApp Cloud API.
- Holds `Service` (id + phone number), `User` (number + name), `Timestamp`
- Deserialized from raw webhook JSON via a JQ transform (`Message.jq` embedded resource) using `JQ.ExecuteAsync` before standard JSON deserialization
- Subtypes: `ContentMessage`, `StatusMessage`, `ErrorMessage`, `ReactionMessage`, `InteractiveMessage`, `InteractiveFlowMessage`, `UnsupportedMessage`
- `UserMessage` — abstract base for messages users can interact with (`ContentMessage`, `InteractiveMessage`, etc.)

### `Response` (abstract record)
Base for all **outgoing** responses.
- `SendCoreAsync(IWhatsAppClient, CancellationToken)` — implemented by each subtype to send via the API
- `SendAsync()` is internal — called only by `SendResponsesHandler`
- Subtypes: `TextResponse`, `TemplateResponse`, `ReactionResponse`, `TypingResponse`, `CallToActionResponse`, `CallToFlowResponse`, `AnonymousResponse`, `FlowDataResponse`

### `IWhatsAppHandler` (interface)
```csharp
IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default);
```
The core processing contract. Implementations yield `Response` objects asynchronously.

### `DelegatingWhatsAppHandler` (base class)
Middleware base — holds `InnerHandler`, delegates by default. Extend this for pipeline middleware. Implements `IDisposable`, disposes inner handler if disposable.

### `WhatsAppHandler` (static class)
- `WhatsAppHandler.Stop` — terminal no-op handler (yields no responses)
- `WhatsAppHandler.Continue` — skip marker; builder skips this handler when building the pipeline (useful for conditional registration)

---

## Handler Pipeline

### `WhatsAppHandlerBuilder`
Builder that assembles the middleware pipeline (similar to `M.E.AI` chat client builder and ASP.NET middleware).

- `Use(Func<IWhatsAppHandler, IWhatsAppHandler>)` — add middleware
- `Use(Func<IWhatsAppHandler, IServiceProvider, IWhatsAppHandler>)` — add DI-aware middleware
- `Use(Func<IEnumerable<IMessage>, IWhatsAppHandler, CancellationToken, IAsyncEnumerable<Response>>)` — add anonymous delegate middleware
- `Build(IServiceProvider?)` — constructs the pipeline (outermost = first `Use` call)
- Automatically wraps the pipeline in `SendResponsesHandler` (both innermost and outermost) when `IWhatsAppClient` is available

**Order**: factories applied in reverse so first-registered is outermost (ASP.NET middleware convention).

### `SendResponsesHandler`
Internal `DelegatingWhatsAppHandler` that intercepts responses and calls `response.SendAsync(client)` for any unsent response (Id empty and Timestamp == 0). Placed at both ends of pipeline by `WhatsAppHandlerBuilder`.

### `PipelineRunner`
Orchestrates processing of a single raw JSON webhook payload:
1. `Message.DeserializeAsync(json)` — JQ transform + JSON deserialization
2. `idempotency.IsProcessedAsync()` — skip if already processed
3. `user.SendProgress()` — mark read / send typing indicator (if `UserMessage`)
4. `idempotency.TrySetProcessedAsync()` — atomic claim; skip if already claimed
5. `handler().HandleAsync([message]).ToArrayAsync()` — run pipeline
6. On exception: `idempotency.ResetProcessedAsync()` — release claim for retry

---

## Message Processing Entries

### Webhook (`AzureFunctionsWebhook`)
Azure Function at `POST /whatsapp` and `GET /whatsapp`.
- **GET**: Webhook verification (Meta hub challenge)
- **POST**: Deserializes payload; detects encrypted Flows data exchange or normal messages
  - Normal messages: sends progress indicators, enqueues via `IMessageProcessor`
  - Encrypted flow: decrypts with `FlowCryptography`, handles `ping` health check, dispatches `FlowDataRequest` directly through handler
- **`ProcessSecret`** in `WhatsAppOptions`: bypass queue, process inline if `X-WHATSAPP-SECRET` header matches

### `IMessageProcessor` (interface)
Decouples webhook from processing. Three implementations:

| Processor | Registration | Description |
|-----------|-------------|-------------|
| `QueueMessageProcessor` | `UseQueueProcessor()` (default) | Azure Storage Queue (`whatsapp` queue) |
| `EventGridProcessor` | `UseEventGridProcessor()` | Azure Event Grid |
| `TaskSchedulerMessageProcessor` | `UseTaskSchedulerProcessor()` | In-process via `TaskScheduler` (testing/simple scenarios) |

### Queue/EventGrid Processors (`AzureFunctionsProcessors`)
Azure Functions triggered by queue or Event Grid to call `PipelineRunner.ProcessAsync(json)`.

### Console Endpoint (`AzureFunctionsConsole`)
Dev-only endpoint at `POST /whatsapp/cli`. Accepts `IMessage`-shaped JSON from the `dotnet-whatsapp` CLI tool. Marks messages with `FromConsole = true`. Not available in Production.

---

## Configuration

### `MetaOptions` (bound from `"Meta"` config section)
```json
{
  "Meta": {
    "ApiVersion": "v22.0",
    "VerifyToken": "required",
    "PrivateKey": "optional, for Flows decryption",
    "Accounts": { "accountId": "token" },
    "Numbers": { "numberId": "token" }
  }
}
```

### `WhatsAppOptions` (bound from `"WhatsApp"` config section)
| Property | Default | Description |
|----------|---------|-------------|
| `ConversationWindowSeconds` | 300 | Window for grouping messages into a conversation |
| `ReadOnMessage` | `true` | Mark read on webhook receipt |
| `TypingOnMessage` | `true` | Send typing indicator on webhook receipt |
| `ReadOnProcess` | `null` | Mark read when processing starts |
| `TypingOnProcess` | `true` | Typing indicator when processing starts |
| `ReactOnMessage` | `null` | Emoji reaction on webhook receipt |
| `ReactOnProcess` | `null` | Emoji reaction on processing start |
| `ReactOnConversation` | `null` | Emoji reaction when restoring conversation |
| `ProcessSecret` | `null` | Secret for inline processing bypass |

---

## DI Registration

### Startup pattern
```csharp
// 1. In Program.cs (IFunctionsWorkerApplicationBuilder):
app.UseWhatsApp(); // registers IFunctionContextAccessor middleware

// 2. In services (IServiceCollection):
services.AddWhatsApp<MyHandler>()   // or lambda/instance overloads
    .UseConversation()              // optional conversation storage
    .UseLogging()                   // optional logging middleware
    .UseOpenTelemetry()             // optional OTel tracing
    .UseConsole();                  // optional dev console support
```

### `AddWhatsApp` overloads
- `AddWhatsApp(IWhatsAppHandler)` — instance
- `AddWhatsApp<THandler>()` — generic type
- `AddWhatsApp(Func<IServiceProvider, IWhatsAppHandler>)` — factory
- `AddWhatsApp(Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>>)` — anonymous lambda
- `AddWhatsApp<TService1..TService6>(Func<...>)` — lambda with up to 6 injected services

### Services auto-registered by `AddWhatsApp`
- `IWhatsAppClient` → `WhatsAppClient`
- `IWhatsAppHandler` → built pipeline
- `PipelineRunner`
- `Idempotency`
- `TableServiceClient` (from `AzureWebJobsStorage`)
- `CloudStorageAccount` (from `AzureWebJobsStorage`)
- `HybridCache`
- Named `HttpClient` `"whatsapp"` with standard resilience
- Options: `MetaOptions`, `WhatsAppOptions`

---

## Conversation Support

### `UseConversation()` extension
Adds `ConversationHandler` to the pipeline and registers `ConversationStorage` (Azure Blob-backed).

### `ConversationHandler`
- Assigns conversation IDs (ULID) based on time window or explicit reply context
- Saves each message and response to `IConversationStorage`
- Passes entire conversation history (not just the current message) to the inner handler
- Skips `TypingResponse` and `ReactionResponse` from persistence

### `IConversationStorage`
- `GetMessageAsync(number, id)` — retrieve single message
- `GetMessagesAsync(number)` — all messages for a number
- `GetMessagesAsync(number, conversationId)` — messages in a conversation
- `GetActiveConversationAsync(number)` — most recent conversation within window
- `SaveAsync(IMessage)` — persist message or response

### `ConversationStorage`
Azure Blob-backed implementation. Messages sorted in-memory by timestamp (ascending) since Table Storage lacks ordering.

### `IMessage.ConversationId` (via `IMessage` extension)
Stored in `AdditionalProperties["ConversationId"]`. Helpers: `message.ConversationId`, `message.With(...)`.

---

## Idempotency

### `Idempotency` class
- Uses Azure Table Storage (`WhatsAppWebhook` table) for durable deduplication
- Uses `HybridCache` (local + distributed) for fast in-memory lookups (3-day local, 30-day distributed TTL)
- Row key: `wamid.*` message IDs used directly; others hashed with MD5 + Base62-encoded
- `TrySetProcessedAsync` → atomic `AddEntity` (409 = already processed)
- `ResetProcessedAsync` → delete entity + invalidate cache on processing failure

---

## `IWhatsAppClient`

### `WhatsAppClient`
- `CreateHttp(numberId)` — authenticated `HttpClient` with base address `https://graph.facebook.com/{ApiVersion}/`
- `SendAsync(numberId, payload)` — POST to `{numberId}/messages`, returns message ID

### `WhatsAppClientExtensions` (extension methods)
Rich set of send helpers (all on `IWhatsAppClient`):
- `MarkReadAsync(message)` — mark as read
- `ReactAsync(message, emoji)` — send reaction
- `SendTypingAsync(message)` — typing indicator
- `SendAsync(serviceId, userNumber, text, [buttons])` — text message
- `ReplyAsync(serviceId, userNumber, context, text, [buttons])` — reply to message
- `SendTemplateAsync(...)` — template message
- `CallToActionAsync(...)` — CTA URL button
- Media sending: `SendImageAsync`, `SendAudioAsync`, `SendVideoAsync`, `SendDocumentAsync`, `SendStickerAsync`

---

## WhatsApp Flows

Namespace: `Devlooped.WhatsApp.Flows`

### Flow Lifecycle
1. Bot sends `CallToFlowResponse` → user sees interactive message with CTA button
2. User opens flow → encrypted `FlowDataRequest` arrives at webhook
3. Webhook decrypts with `FlowCryptography` (RSA/AES), dispatches `FlowDataRequest` to handler
4. Handler returns `FlowDataResponse` with screen + data → webhook encrypts and returns to WhatsApp

### Key Types
| Type | Description |
|------|-------------|
| `CallToFlowResponse` | Initiates a flow from bot response |
| `FlowDataRequest` | Incoming data exchange request from flow |
| `FlowDataResponse` | Response to data exchange (returned synchronously by handler) |
| `FlowToken` | Encodes `ServiceId` + `UserNumber` + `Flow` name in the flow token |
| `FlowCryptography` | RSA/AES encryption/decryption for flow data exchange |
| `FlowParameters` | Parameters for flow initiation (id or name, mode, action, payload) |
| `IWhatsAppFlowsClient` | CRUD API for Flow management (create, update, publish, deprecate, delete) |
| `WhatsAppFlowsClient` | Implementation using `MetaOptions.Accounts` for authentication |

### Important Constraint
Only **one** `FlowDataResponse` can be returned per `FlowDataRequest`. Multiple responses result in HTTP 409 Conflict.

### Flow JSON Validation (`FlowJsonValidator`)

Client-side validation of WhatsApp Flow JSON before submission to Meta's API. Uses a two-tier architecture:

**Tier 1: JSON Schema** (`FlowJsonSchema.json`, embedded resource)
- Draft 2020-12 schema with 42 `$defs` covering all 24 v7.3 component types
- Component type discrimination via `allOf` + `if/then` blocks on `type` property
- Validates: structure, types, enums, character limits, patterns, property constraints
- Footer caption mutual exclusion via `dependentRequired`
- Screen `id` blocked from "SUCCESS" via negative lookahead pattern
- `additionalProperties: false` on all concrete objects

**Tier 2: Programmatic Rules** (`FlowJsonRules.cs`, internal static class)
- Screen ID uniqueness and terminal screen requirements
- Navigate target validation (must exist, no self-navigation)
- Complete action only on terminal screens
- Routing model validation (branch limits, cycle detection via DFS)
- Component count limits per screen (max 50 total, per-type limits)
- PhotoPicker/DocumentPicker mutual exclusion per screen
- NavigationList cannot be on terminal screens
- If component: Footer in both branches, max 3 nesting levels
- ImageCarousel flow-wide limit (max 3)
- Footer caption constraint enforcement (center vs left/right exclusion)

**Public API:**
```csharp
var validator = new FlowJsonValidator();
FlowValidationResult result = validator.Validate(jsonString);
// result.IsValid, result.Errors (IReadOnlyList<ValidationError>)
```

**Integration with Flows client:**
```csharp
// Extension method on IWhatsAppFlowsClient
client.ValidateFlowJson(json); // throws FlowValidationException if invalid

// UpdateFlowJsonAsync with optional pre-validation
await client.UpdateFlowJsonAsync(flowId, json, validate: true);
```

**Schema evaluation noise filtering** (`FlowJsonValidator.ValidateSchema`):
- Skips `/if/` condition failures (normal `allOf` + `if/then` evaluation)
- Skips `/not/` inner failures (successful `not` blocks)
- Skips `oneOf` branch failures (individual branch mismatches)
- Maps empty-string keyword (`""`) from `additionalProperties: false` to `INVALID_PROPERTY_KEY`

**Testing** (`FlowJsonValidationTests.cs` + `FlowJsonGenerator.cs`):
- 107 data-driven tests per TFM (214 total across net8.0 + net10.0)
- 54 valid flow combinations (all component types, multi-screen, actions, routing models, conditionals)
- 53 invalid flows (each targeting a specific error code)

---

## Cross-Cutting Middleware

### `LoggingHandler` (`UseLogging()`)
Logs incoming messages and outgoing responses at Debug/Information level. Skips if `NullLoggerFactory`.

### `OpenTelemetryHandler` (`UseOpenTelemetry()`)
Wraps pipeline in an `Activity` span with configurable source name.

### `ConsoleHandler` (`UseConsole()`)
Dev-only. Detects `FromConsole = true` messages, wraps `Service` in `CompositeService` so responses are mirrored to both WhatsApp and the CLI console. Returns `WhatsAppHandler.Continue` in Production (zero overhead).

---

## Message/Response Type Reference

### Incoming Message Types (`MessageType` enum)
| Type | Class | Description |
|------|-------|-------------|
| `Content` | `ContentMessage` | Text, image, audio, video, document, location, contact, sticker |
| `Status` | `StatusMessage` | Delivery/read status updates |
| `Error` | `ErrorMessage` | Error notifications |
| `Reaction` | `ReactionMessage` | Emoji reaction to a message |
| `Interactive` | `InteractiveMessage` | Button/list interactive replies |
| `InteractiveFlow` | `InteractiveFlowMessage` | Completed flow submission |
| `FlowData` | `FlowDataRequest` | Flow data exchange request |

### Outgoing Response Types
| Type | Description |
|------|-------------|
| `TextResponse` | Text message with optional ≤3 quick-reply buttons |
| `TemplateResponse` | WhatsApp template message |
| `ReactionResponse` | Emoji reaction |
| `TypingResponse` | Typing indicator / mark-read |
| `CallToActionResponse` | Interactive CTA URL button message |
| `CallToFlowResponse` | Interactive message to initiate a Flow |
| `FlowDataResponse` | Synchronous flow data exchange response |
| `AnonymousResponse` | Delegate-based custom response (`Response.Create(...)`) |

### `Content` Subtypes (on `ContentMessage.Content`)
Detected via `ContentType` enum: `Text`, `Image`, `Audio`, `Video`, `Document`, `Location`, `Contact`, `Sticker`, `Unknown`.

---

## Notable Design Patterns

### JQ-Based Deserialization
Raw WhatsApp Cloud API webhook payloads are transformed by the embedded `Message.jq` query before JSON deserialization. This normalizes the nested webhook structure into the flat `Message` model.

### `AdditionalProperties` / Extension Data
Both `IMessage` and `Response` carry an `AdditionalPropertiesDictionary` for extensible metadata (e.g., `FromConsole`, `ConversationId`, `__json`). Use `message.With(x => x["key"] = value)` extension method.

### `CompositeService`
When `UseConsole()` is active, a `UserMessage.Service` can be a `CompositeService` combining the WhatsApp service ID and the console service ID. `TextResponse` and `CallToActionResponse` detect this and send to both channels.

### `NormalizeNumberExtension`
Phone numbers are normalized (e.g., strip leading `+`) before sending via API.

### `Ulid` for IDs
New conversation IDs and synthetic message IDs use `Ulid.NewUlid()` (lexicographically sortable ULIDs).

### `ThisAssembly` Source Generators
`ThisAssembly.Resources` exposes `ThisAssembly.Resources.Message.Text` (the embedded JQ script) and `ThisAssembly.Resources.Flows.FlowJsonSchema.Text` (the embedded Flow JSON Schema).
`ThisAssembly.AssemblyInfo` exposes version/build metadata.

### `AnonymousDelegatingWhatsAppHandler` / `AnonymousWhatsAppHandler`
Allow registering pipeline steps and handlers as plain lambda functions without creating named classes.

### Static `throw` helpers (`Throw.cs`)
Global `using static` for `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException` — use `Throw.IfNull(...)`, `Throw.InvalidOperationException(...)`.  
These are injected into all projects via `Directory.Build.props` `<Using>` items — no explicit `using` needed.

### `MessageExtensions` — Shorthand on message instances
Extension methods on `IMessage` / `UserMessage` for common response creation:
- `message.React(emoji)` → `ReactionResponse`
- `message.Reply(text, [buttons])` → `TextResponse` with `Context` set
- `message.Typing()` → `TypingResponse`
- `message.WithConsoleText(text)` / `message.ConsoleOnly()` — CLI-targeted response variants

### `UseIgnore()` pipeline extension
Skips certain message types (e.g., `StatusMessage`) so they never reach the inner handler. Registered before other middleware to short-circuit processing for non-actionable messages.

### `FlowDataAction` enum
Values used in `FlowDataRequest.Action`: `Init` (flow opened), `Back` (back navigation), `DataExchange` (mid-flow data request).

---

## CodeAnalysis Project

`SendStringAnalyzer` — Roslyn analyzer that prevents accidentally passing raw `string` where a typed message/response is expected. Referenced by the main project as an `Analyzer` (not output assembly).

---

## Testing Patterns (`src/Tests/`)

- Framework: **xUnit** + **Moq**
- `MockHttpClientFactory` — creates mock `HttpClient` instances for testing `WhatsAppClient`
- `MockLogger<T>` — captures log messages for assertions
- Key test files:
  - `PipelineTests` — handler pipeline composition
  - `WhatsAppModelTests` — message deserialization from real webhook payloads
  - `ConversationStorageTests` — storage logic
  - `IdempotencyTests` — dedup logic
  - `FlowTests` — Flows encryption/decryption and data exchange
  - `FlowJsonValidationTests` — Flow JSON validation (data-driven, 107 cases)
  - `FlowJsonGenerator` — generates valid/invalid Flow JSON test data
  - `IntegrationTests` — end-to-end with mocked HTTP
  - `AnalyzerTests` — Roslyn analyzer verification
- Test payloads stored in `Tests/Content/` directory

---

## Build & Versioning

- Local builds: `VersionPrefix = 42.42.42` (always > public packages for dogfooding)
- CI builds: `VersionLabel` env var drives version (branch name → semver label)
- Tag `refs/tags/v*` → release version
- `WarningsAsErrors = true` in CI and Release configuration
- `NuGetizer` used for pack configuration
- `PolySharp` provides C# polyfills for older targets
