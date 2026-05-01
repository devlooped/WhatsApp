# BSUID Support — User.Id Migration Plan

## Problem

WhatsApp for Business is introducing Business-Scoped User IDs (BSUIDs) for privacy.
Users who opt for usernames will have their phone numbers hidden from businesses.
Webhooks will include a `contacts[].user_id` field (BSUID) alongside or instead of phone numbers.
The SDK must support both identifiers uniformly for sending messages back to users.

## Key API Constraint

The WhatsApp Cloud API requires **different `recipient_type` values**:
- Phone number: `recipient_type: "individual"`
- BSUID: `recipient_type: "business_scoped_user_id"`

The SDK will detect the identifier type and set `recipient_type` automatically.

## Approach

- **Breaking rename**: `UserNumber` → `UserId` throughout the public API.
- **User record**: `User(Name, Id, Number?)` — `Id` always set, `Number` nullable.
- **Normalization**: Only on `User.Number` setter. Sending code uses `User.Id` as-is.

## Webhook Scenarios

| Scenario | `contacts[].user_id` | `messages[].from` | `User.Id` | `User.Number` |
|----------|----------------------|--------------------|-----------|---------------|
| Pre-migration | absent | phone | phone | phone |
| Post-migration, non-privacy | BSUID | phone | BSUID | phone |
| Post-migration, privacy user | BSUID | BSUID | BSUID | null |

---

## Work Items

### 1. Update `User` record (`User.cs`)
- Add `Id` property (string, always set — phone or BSUID)
- Make `Number` nullable (string?, set only when a phone number is available)
- `Id` is NOT normalized; `Number` is normalized in constructor
- Add `IsBSUID` computed property: `Number == null || Id != Number`

### 2. Rename `IMessage.UserNumber` → `UserId` (`IMessage.cs`)
- Rename property in IMessage interface
- Update XML docs to reflect it can be phone or BSUID

### 3. Update `Message` base record (`Message.cs`)
- `IMessage.UserId` implementation returns `User.Id` instead of `User.Number`
- Constructor still takes `User` record

### 4. Update `Response` base record (`Response.cs`)
- Rename parameter `UserNumber` → `UserId`
- Update `Response.Create` static methods
- Update XML docs

### 5. Update all Response subtypes
Files: `TextResponse.cs`, `TemplateResponse.cs`, `ReactionResponse.cs`,
`TypingResponse.cs`, `CallToActionResponse.cs`, `CallToFlowResponse.cs`,
`AnonymousResponse.cs`
- Rename `UserNumber` → `UserId` in all constructors, properties, and usages

### 6. Update Flow types (`Flows/FlowDataExchange.cs`, `Flows/FlowToken.cs`)
- `FlowDataRequest.UserNumber` → `UserId`
- `FlowDataResponse.UserNumber` → `UserId`
- `FlowToken.UserNumber` → `UserId`
- Token key stays `user:` (backward compatible encoding)
- Update `FlowDataRequestExtensions` accordingly

### 7. Update `Message.jq` transform
Current:
```jq
"user": { "name": ..., "number": $msg.from }
```
New:
```jq
"user": {
  "name": ($user.profile.name // ""),
  "id": ($user.user_id // $msg.from),
  "number": (if $user.user_id then
    (if $msg.from != $user.user_id then $msg.from else null end)
  else $msg.from end)
}
```
- For status/error messages: `"id": $status.recipient_id, "number": $status.recipient_id`
- All message type blocks (content, interactive, flow, reaction, unsupported) need the same update

### 8. Update `WhatsAppClientExtensions` (`WhatsAppClientExtensions.cs`)
- Rename all `userNumber` parameters → `userId`
- Replace `to = userNumber.NormalizeNumber()` → `to = userId` (no normalization)
- Replace hardcoded `recipient_type = "individual"` with dynamic detection
- Add helper: `RecipientType(string userId)` — returns `"individual"` or `"business_scoped_user_id"`
- Detection heuristic: phone = all digits, 7–15 chars; otherwise BSUID
- Update all ~20 send method overloads
- Rename XML doc references

### 9. Clean up `NormalizeNumber` usage (`NormalizeNumberExtension.cs`)
- Remove all `NormalizeNumber()` calls from sending code in `WhatsAppClientExtensions`
- Keep the extension method itself — only used in `User` constructor for `Number` property

### 10. Update `Idempotency` (`Idempotency.cs`)
- `message.User.Number` → `message.User.Id` for partition key
- Existing processed-message entries won't collide (safe — new key space)

### 11. Update `ConversationStorage` / `ConversationHandler`
- All `message.UserNumber` → `message.UserId`
- `ConversationStorage` partition keys: `x.UserNumber` → `x.UserId`
- `ConversationHandler`: `message.UserNumber` → `message.UserId`

### 12. Update `Conversation` record (`Conversation.cs`)
- Rename `Number` → `UserId` (partition key parameter)
- Update XML docs

### 13. Update `IConversationStorage` interface
- Rename `number` parameters → `userId` in all methods
- Update XML docs

### 14. Update `MessageExtensions` (`MessageExtensions.cs`)
- All `message.UserNumber` references → `message.UserId`
- `Typing()` method: `message.User.Number` → `message.User.Id`

### 15. Update `OpenTelemetryHandler` (`OpenTelemetryHandler.cs`)
- `message.UserNumber` → `message.UserId` in telemetry tags

### 16. Update webhook / processors
- `AzureFunctionsWebhook.cs`: any `UserNumber` references → `UserId`
- `WhatsAppEndpointRouteBuilderExtensions.cs`: same

### 17. Update `CallToFlowResponse` (`CallToFlowResponse.cs`)
- `to = UserNumber` → `to = UserId`
- Add dynamic `recipient_type`

### 18. Update all tests
- `PipelineTests.cs`, `ConversationStorageTests.cs`, `IdempotencyTests.cs`,
  `FlowTests.cs`, `IntegrationTests.cs`, `WhatsAppModelTests.cs`
- Update test data, assertions, and any `UserNumber` references
- Add new tests for BSUID-specific scenarios (privacy user, mixed)

### 19. Update `JsonContext` / serialization
- Ensure `User` serialization handles nullable `Number`
- Update any `[JsonPropertyName]` if needed

### 20. Update sample apps and console
- `SampleApp/` handlers
- `Console/` CLI tool

### 21. Documentation
- Update `AGENTS.md` with BSUID design decisions
- Update `readme.md` with BSUID support overview
- Update `changelog.md`

---

## Dependencies

```
user-record ──┬──> imessage-rename ──┬──> message-base
              │                      ├──> response-base ──┬──> response-subtypes
              │                      │                    └──> flow-types
              │                      ├──> client-extensions (+ normalize-cleanup)
              │                      ├──> conversation-record ──> conversation-storage
              │                      ├──> otel-handler
              │                      ├──> webhook-processors
              │                      └──> message-extensions
              ├──> jq-transform
              ├──> normalize-cleanup
              └──> idempotency

tests ──> (response-subtypes, flow-types, jq-transform, client-extensions, conversation-storage)
samples-console ──> (response-subtypes, client-extensions)
docs ──> tests
```

## Notes

- **Storage migration**: Switching partition keys from phone→BSUID means existing conversation/idempotency data won't be found for users that transition. This is acceptable — conversations are short-lived and idempotency is best-effort.
- **`recipient_type` detection**: Phone pattern = all digits, 7–15 chars. BSUIDs may also be numeric but are typically longer or have a different format. We can refine detection as Meta publishes final BSUID format specs.
- **Backward compat for FlowToken**: The `user:` key in encoded tokens will now store BSUID instead of phone number. Old tokens with phone numbers will still decode but reference phone-based users.
