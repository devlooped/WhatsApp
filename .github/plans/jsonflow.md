# Flow JSON Validation Feature — Implementation Plan

## Problem Statement

The WhatsApp .NET SDK has a mature Flows API client (`IWhatsAppFlowsClient`) that submits Flow JSON to Meta's API and captures server-side `ValidationError` responses. However, there is **no client-side validation** — users must submit invalid JSON to discover errors. We want to validate Flow JSON locally before API submission, surfacing structural and semantic errors early.

## Feasibility Analysis: Client-Side JSON Schema vs Programmatic Validation

### What JSON Schema CAN validate (~60% of rules)
- ✅ Top-level structure (`version`, `screens`, `routing_model`, `data_api_version`)
- ✅ Screen properties (`id`, `layout`, `terminal`, `success`, `title`, `data`, etc.)
- ✅ Layout structure (`type: "SingleColumnLayout"`, `children` array)
- ✅ Component types and their required/optional properties
- ✅ Property types (string, boolean, number, array, object)
- ✅ Enum values (component types, input-types, scale-types, action names, etc.)
- ✅ Character limits (`maxLength` on text, labels, descriptions, metadata)
- ✅ Array bounds (`minItems`/`maxItems` on data-source, list-items, etc.)
- ✅ Pattern matching (screen ID alphanumeric+underscore, dynamic reference `${data.xxx}`)
- ✅ Property dependencies (Footer: left/right-caption vs center-caption mutual exclusion)
- ✅ Version-gated features via `if`/`then`/`else` (e.g., components outside Form from v4.0+)
- ✅ `additionalProperties: false` to catch unknown properties (`INVALID_PROPERTY_KEY`)
- ✅ Reserved keyword enforcement (screen id ≠ "SUCCESS")

### What JSON Schema CANNOT validate (~40% of rules)
- ❌ Cross-screen references (navigate `next.name` must reference an existing screen ID)
- ❌ Routing model graph validation (entry screen, no loops, all paths to terminal, max 10 branches)
- ❌ Data model ↔ payload field matching (navigate payload keys must match next screen's `data` schema)
- ❌ Dynamic expression type checking (`${form.field}` references existing input `name`)
- ❌ Global dynamic data refs (`${screen.SCREEN_NAME.form.field}` — screen must exist)
- ❌ Component counting per screen (max 50 components, max 2 EmbeddedLink, max 5 OptIn, etc.)
- ❌ Footer-on-terminal enforcement (terminal screens must have Footer)
- ❌ `complete` action only on terminal screens
- ❌ If-component Footer rules (must be in both `then` and `else` branches)
- ❌ Nested If max depth (3 levels)
- ❌ Navigate action self-reference (cannot navigate to same screen)
- ❌ NavigationList cannot be on terminal screens
- ❌ PhotoPicker/DocumentPicker mutual exclusion per screen and max 1 each
- ❌ Media components cannot be in navigate payloads (only data_exchange/complete)

### Conclusion

**Both tiers are needed.** JSON Schema catches structural errors early and cheaply. Programmatic C# validation handles the semantic/cross-reference rules. Server-side validation remains available via existing API as final verification.

## Approach

### Two-Tier Validation Architecture

```
FlowJsonValidator.ValidateAsync(json)
  ├─ Tier 1: JSON Schema validation (JsonSchema.Net)
  │   └─ Catches: structure, types, enums, limits, patterns, property rules
  └─ Tier 2: Programmatic C# validation
      └─ Catches: cross-references, routing graphs, component counts, version rules
```

Both tiers produce a unified `FlowValidationResult` with errors matching the Meta API error format (`ValidationError` with line/column info where possible).

### Target Version

Target **version 7.3 only** (the latest recommended version) to simplify scope. The schema and rules assume all v7.3 features are available. Older version support can be added later as incremental work.

## Todos

### 1. `add-jsonschema-dependency` — Add JsonSchema.Net NuGet Package
Add `JsonSchema.Net` package to `src/WhatsApp/WhatsApp.csproj`. This is the standard .NET JSON Schema validation library that supports Draft 2020-12.

### 2. `create-flow-json-schema` — Create the Flow JSON Schema File
Create `src/WhatsApp/Flows/FlowJson.schema.json` as an embedded resource. This single schema covers all supported versions (5.1 through 7.3) using conditional subschemas.

The schema must encode:
- **Top-level**: `version` (required, enum of supported versions), `screens` (required, min 1), `routing_model` (optional map), `data_api_version` (optional, "3.0"/"4.0"), `data_channel_uri` (optional, uri format)
- **Screen**: `id` (required, pattern `^[A-Za-z_][A-Za-z0-9_]*$`, not "SUCCESS"), `layout` (required), `terminal`, `success`, `title`, `refresh_on_back`, `data`, `sensitive`
- **Layout**: `type` = "SingleColumnLayout", `children` array (min 1)
- **Components**: Each component type as a discriminated union via `type` property — TextHeading, TextSubheading, TextBody, TextCaption, RichText, TextInput, TextArea, CheckboxGroup, RadioButtonsGroup, Footer, OptIn, Dropdown, EmbeddedLink, DatePicker, CalendarPicker, Image, If, Switch, PhotoPicker, DocumentPicker, NavigationList, ChipsSelector, ImageCarousel, Form
- **Per-component properties**: types, required fields, enums, maxLength, patterns
- **Actions**: `on-click-action` structure with `name` enum, `next` for navigate, `payload`, `url` for open_url
- **Footer caption exclusivity**: `center-caption` XOR (`left-caption` + `right-caption`)
- **Data model**: `data` property uses JSON Schema for type definitions with `__example__` required

Note: Since we target v7.3 only, no version gating is needed — all features are unconditionally available.

### 3. `create-flow-validator` — Create FlowJsonValidator Class
Create `src/WhatsApp/Flows/FlowJsonValidator.cs` — the public validation API.

```csharp
namespace Devlooped.WhatsApp.Flows;

public class FlowJsonValidator
{
    /// Validates Flow JSON string, returning all structural and semantic errors.
    public FlowValidationResult Validate(string json);
}

public record FlowValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors);
```

Implementation:
- Load embedded JSON Schema resource
- Parse JSON with `System.Text.Json`, preserving line/column info
- Run JsonSchema.Net validation → map results to `ValidationError` with line/column
- Run programmatic validation rules → append to errors
- Return unified `FlowValidationResult`

### 4. `create-programmatic-rules` — Implement Semantic Validation Rules
Create `src/WhatsApp/Flows/FlowJsonRules.cs` — internal class with all programmatic rules.

Rules to implement:
1. **Screen ID uniqueness** — no duplicate screen IDs
2. **Terminal screen requirements** — at least one terminal screen, must have Footer, at least one with `success=true`
3. **Complete action placement** — only on terminal screens
4. **Navigate target validation** — target screen must exist, no self-navigation
5. **Routing model validation** — entry screen exists, no loops, no backward routes, max 10 branches, all screens connected, all routes end at terminal
6. **Data model matching** — navigate payload keys match next screen's `data` fields, types match
7. **Dynamic reference validation** — `${data.xxx}` and `${form.xxx}` reference existing fields/inputs
8. **Global dynamic reference validation** — `${screen.XXX.form.yyy}` references valid screen/field
9. **Component count limits** — max 50 per screen, max 2 EmbeddedLink, max 5 OptIn, max 3 Image, max 1 PhotoPicker, max 1 DocumentPicker, max 2 NavigationList, max 2 ImageCarousel, max 3 ImageCarousel per flow
10. **PhotoPicker/DocumentPicker exclusion** — cannot coexist on same screen
11. **NavigationList restrictions** — not on terminal screens, not mixed with other components
12. **If component rules** — Footer must be in both branches, max 3 nesting levels
13. **Switch component rules** — cases not empty
14. **Footer uniqueness** — max 1 Footer per screen (considering If branches)
15. **Form binding validation** — `${form.field}` references existing input `name`
16. **Version-specific feature gating** — features not available in declared version produce errors
17. **RichText restrictions** — standalone only (before v6.3), with Footer only from v6.3

### 5. `integrate-with-flows-client` — Integrate Validator with Flows Client
Add a `ValidateAsync` overload or extension to `WhatsAppFlowsClientExtensions` that:
- Runs local validation first
- Optionally submits to API for server-side validation
- Returns combined results

Also integrate into `UpdateFlowJsonAsync` as an opt-in pre-validation step.

### 6. `create-test-data-generator` — Build Flow JSON Test Data Generator
Create `src/Tests/FlowJsonGenerator.cs` — generates realistic Flow JSON combinations.

**Valid flows to generate** (auto-combinatorial):
- Single-screen flows (minimal: 1 screen, terminal, with Footer + complete)
- Multi-screen flows (2-5 screens with navigate actions)
- All component types individually and in combinations
- With/without Form component wrapper
- With/without endpoint (routing_model + data_api_version)
- With/without data models and dynamic references
- All action types: navigate, complete, data_exchange, update_data, open_url
- If/Switch conditional components with various nesting depths
- Version 7.3 with all features available (NavigationList, ChipsSelector, ImageCarousel, CalendarPicker, etc.)
- CalendarPicker single and range modes
- PhotoPicker and DocumentPicker (never together)
- Global dynamic references
- Nested expressions

**Invalid flows to generate** (one error per file, well-categorized):
- Missing required properties (version, screens, screen.id, screen.layout, component.type, etc.)
- Invalid property types (string where boolean expected, etc.)
- Exceeded character limits (heading > 80 chars, etc.)
- Invalid enum values (unknown component type, invalid action name)
- Duplicate screen IDs
- No terminal screen
- Complete action on non-terminal screen
- Navigate to non-existent screen
- Navigate to self
- Routing model errors (loops, disconnected screens, exceeded branches)
- Component count exceeded (> 50, > 2 EmbeddedLink, etc.)
- PhotoPicker + DocumentPicker on same screen
- If component: Footer in one branch only
- Nested If depth > 3
- Form component rules (interactive components must be in Form pre-v4.0 — N/A for v7.3, but good regression test)
- Invalid dynamic references
- Reserved screen ID "SUCCESS"

Target: **200+ valid** and **100+ invalid** generated test cases (all targeting v7.3).

### 7. `create-validation-tests` — Data-Driven xUnit Tests
Create `src/Tests/FlowJsonValidationTests.cs` with:

- `[Theory]` + `[MemberData]` using generated valid JSON → all must pass validation
- `[Theory]` + `[MemberData]` using generated invalid JSON → each must fail with expected error code
- Schema-only tests to verify JSON Schema catches structural errors
- Programmatic-only tests to verify semantic rules
- Integration tests combining both tiers
- Regression tests against known Meta API validation errors (from error-codes docs)

### 8. `update-design-docs` — Update Documentation
- Update `.github/design.md` with validation feature documentation
- Add XML doc comments to all public APIs

## Dependencies

```
7. create-validation-tests
   ├── depends on: 6. create-test-data-generator
   ├── depends on: 3. create-flow-validator
   └── depends on: 4. create-programmatic-rules

3. create-flow-validator
   ├── depends on: 1. add-jsonschema-dependency
   └── depends on: 2. create-flow-json-schema

4. create-programmatic-rules
   └── depends on: 3. create-flow-validator (for shared types)

5. integrate-with-flows-client
   └── depends on: 3. create-flow-validator
   └── depends on: 4. create-programmatic-rules

6. create-test-data-generator
   └── (independent, but benefits from schema knowledge)

8. update-design-docs
   └── depends on: all above
```

## Notes & Considerations

- **Schema size**: The Flow JSON schema will be large (~1500+ lines) due to 25+ component types. This is expected and maintainable as an embedded JSON resource.
- **JsonSchema.Net performance**: Schema compilation is cached — first validation is slower, subsequent ones are fast. Suitable for both CLI and server scenarios.
- **Line/column info**: `System.Text.Json` with `JsonDocument` provides byte offsets; we'll need to convert to line/column for error reporting. JsonSchema.Net provides JSON Pointer paths which we can map.
- **Forward compatibility**: New Flow JSON versions will require schema updates. The schema structure makes this maintainable (component definitions as `$defs`).
- **Scope**: v7.3 only initially. Multi-version support can be added later with `if/then/else` version gating in the schema.
- **Test data files**: Generated JSON files should be stored in `src/Tests/Content/Flows/Valid/` and `src/Tests/Content/Flows/Invalid/` as embedded resources for data-driven tests.
