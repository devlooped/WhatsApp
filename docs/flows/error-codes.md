# Error Codes

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/error-codes

---

## Flow Management API Error Codes

Errors returned when creating, updating, deleting, or publishing Flows.

> For general Cloud API errors, see [Cloud API error codes](https://developers.facebook.com/docs/whatsapp/cloud-api/support/error-codes/).

| Error Code | Description | Possible Solutions |
|---|---|---|
| `100` | Flow name is not unique | Use a unique name per WABA. See [Creating a Flow](./flows-api.md#create). |
| `100` | Invalid Flow JSON version | Check for typos or expired versions. See [Changelog](https://developers.facebook.com/docs/whatsapp/flows/changelogs). |
| `100` | Invalid `data_api_version` | Check for typos or upgrade to a supported version. |
| `100` | Flow with specified ID does not exist | Verify the ID and credentials. |
| `100` | Only one clone source can be set | Provide either `clone_flow_id` or `clone_template`, not both. |
| `100` | Specify Endpoint URI in Flow JSON | For Flow JSON < v3.0, use `data_channel_uri` inside Flow JSON instead of `endpoint_uri`. |
| `100` | Invalid Endpoint URI | Provide a valid URL. |
| `139000` | Blocked by Integrity | Contact [Support](https://developers.facebook.com/docs/whatsapp/support). |
| `139001` | Flow can't be updated | Clone the published Flow and republish the clone. |
| `139001` | Error while processing Flow JSON | Retry; if persistent, contact [Support](https://developers.facebook.com/docs/whatsapp/support). |
| `139002` | Publishing Flow in invalid state | Only Draft Flows can be published. Clone and republish if needed. |
| `139002` | Publishing Flow with validation errors | Fix all errors in the Flow Builder or via API before publishing. |
| `139002` | Publishing without `endpoint_uri` | Set `endpoint_uri` via the Flows API before publishing (v3.0+). |
| `139002` | Unsupported Flow JSON version | Upgrade to a [supported version](https://developers.facebook.com/docs/whatsapp/flows/changelogs). |
| `139002` | No Phone Number connected to WABA | [Add a phone number](https://developers.facebook.com/docs/whatsapp/cloud-api/phone-numbers). |
| `139002` | Missing Flows Signed Public Key | [Upload and sign a public key](https://developers.facebook.com/docs/whatsapp/flows/reference/implementingyourflowendpoint#upload-public-key). |
| `139002` | No Application connected to Flow | Connect a [Meta app](https://developers.facebook.com/docs/development/create-an-app). |
| `139002` | Endpoint not available | Ensure endpoint is reachable and implements [health checks](https://developers.facebook.com/docs/whatsapp/flows/reference/implementingyourflowendpoint#health_check_request). |
| `139002` | WABA not subscribed to Flows Webhooks | [Subscribe to Flows webhooks](./webhooks.md#subscribe-to-webhooks). |
| `139003` | Can't deprecate unpublished Flow | Only published Flows can be deprecated. Delete drafts instead. |
| `139003` | Flow is already deprecated | No action needed — target state already achieved. |
| `139004` | Can't delete published Flow | Deprecate instead of deleting. |
| `139006` | Metrics threshold not reached | Not enough data (minimum 250 requests required). |

---

## Business Endpoint HTTP Error Codes

Error codes your endpoint can return to trigger specific client-side behaviors.

| HTTP Status | Server Situation | Client Behavior |
|---|---|---|
| `421` | Payload cannot be decrypted | Client re-fetches public key and retries. On second failure, shows generic error. |
| `432` | Request signature authentication fails | Generic error shown to user. |
| `427` | Flow token is no longer valid | Generic error shown; CTA button disabled. You may include a custom `error_msg`. |

**Custom error message example (HTTP 427):**
```http
HTTP/2 427
Content-Type: application/json

{"error_msg": "The order has already been placed"}
```

---

## On-Premise Client Error Codes

| Code | Description |
|------|-------------|
| `2064` | Invalid Flow ID — Flow doesn't exist, doesn't belong to your WABA, or is in an invalid state |
| `2065` | Invalid Flow Message Version |
| `2066` | Invalid Flow Mode — Draft Flow sent without draft mode, or Published Flow sent with draft mode |
| `2067` | Flow DRAFT Mode Not Allowed — cannot send Draft Flow |
| `2068` | Flow is blocked — may indicate incomplete endpoint setup (missing public key) |
| `2069` | Flow is throttled — 10 messages already sent in the last hour |
| `2070` | Invalid or expired Flow version |

---

## Cloud API Error Codes

| Code | Description | HTTP Status |
|------|-------------|-------------|
| `132068` | Flow is blocked | 400 Bad Request |
| `132069` | Flow is throttled (10 messages/hour limit reached) | 400 Bad Request |

---

## Webhook Alert / Endpoint Error Types

These error codes appear in [webhook alert payloads](./webhooks.md) or are propagated back from client devices.

| Code | Description | Resolution |
|------|-------------|------------|
| `timeout_error` | Endpoint request exceeded 10 seconds | Improve endpoint performance |
| `missing_capability` | App lacks required endpoint capability | — |
| `cannot_be_served` | Flow not in DRAFT or PUBLISHED state, or WABA blocked | Check WABA quality and Flow JSON properties |
| `no_http_response_error` | Connection closed without a valid HTTP response | Ensure endpoint always returns a valid response |
| `unexpected_http_status_code` | Endpoint returned unexpected status (e.g. 500) | Ensure endpoint returns expected status codes |
| `public-key-missing` | Client couldn't retrieve business public key | Upload correct public key for the sending phone number |
| `public-key-signiture-verification` | Couldn't verify public key signature | Re-upload public key with updated signature |
| `response-decryption-error` | Client couldn't decrypt endpoint payload | Verify the uploaded key matches the encryption key |
| `invalid-screen-transition` | Next screen doesn't match the routing model | Update routing model (requires cloning and republishing) |
| `payload-schema-error` | Screen data doesn't match the Flow JSON schema | Ensure endpoint data matches the Flow JSON schema |
| `business-decryption-error` | Client received 421 even after key refresh | Re-upload the Flow's public key |

---

## Static Validation Errors

Errors returned during Flow JSON development/compilation.

### Schema Validation Errors

| Error Code | Message Pattern | Description |
|---|---|---|
| `INVALID_PROPERTY_KEY` | `Property (name) cannot be specified at (path)` | Additional/unknown property present |
| `INVALID_PROPERTY_VALUE` | `Invalid value found for property (name) at (path)` | Property has an invalid value |
| `INVALID_PROPERTY_TYPE` | `Expected (path) to be (expected) but found (actual)` | Wrong data type |
| `INVALID_PROPERTY_VALUE_FORMAT` | `Property (name) should be in (format) format` | Value not in required format (e.g., URI) |
| `MIN_ITEMS_REQUIRED` / `MIN_CHARS_REQUIRED` | `Property (name) should have at least (n) (unit)` | Below minimum items/characters |
| `MISSING_REQUIRED_TYPE_PROPERTY` | `Required property (name) is missing` | Mandatory field absent |
| `PATTERN_MISMATCH` | Various pattern-related messages | Value doesn't match required pattern |
| `INVALID_ENUM_VALUE` | `Value should be one of: [values]` | Value not in allowed enum list |
| `INVALID_DEPENDENCIES` | `Footer should have (property) when (other property) is present` | Dependent property missing |
| `NOT_KEYWORD_SCHEMA_VALIDATION_FAILED` | Properties must be present exclusively | Mutually exclusive properties used together |

### Flow JSON Version Errors

| Error Code | Message | Description |
|---|---|---|
| `INVALID_FLOW_JSON_VERSION` | Invalid Flow JSON version | Version string is malformed |
| `MISSING_FLOW_JSON_VERSION` | Flow JSON version is not specified | `version` property is absent |
| `UNSUPPORTED_FLOW_JSON_VERSION` | Unsupported Flow JSON version | Version is no longer supported |
| `UNAVAILABLE_FLOW_JSON_VERSION` | Version not available for your WABA ID | Version is in beta and not yet public |
| `NO_SUPPORTED_DATA_API_VERSION` | No supported Data API version for given version | Data API version not compatible |
| `INVALID_PROPERTY_KEY` | `data_api_version` not supported in Flow JSON v3.0+ | Use Flows API to configure endpoint URI |

### Routing Model Errors

| Error Code | Message Pattern | Description |
|---|---|---|
| `INVALID_ROUTING_MODEL` | Screens missing in routing model | Screen defined but not in `routing_model` |
| `INVALID_ROUTING_MODEL` | Invalid screens in routing model | `routing_model` references non-existent screen IDs |
| `INVALID_ROUTING_MODEL` | Screens not connected | Some screens are unreachable from other screens |
| `INVALID_ROUTING_MODEL` | No entry screen found | No screen with zero inbound edges |
| `INVALID_ROUTING_MODEL` | Loop detected | Circular reference between screens |
| `INVALID_ROUTING_MODEL` | Branches exceed max of 10 | A screen has more than 10 outgoing routes |
| `INVALID_ROUTING_MODEL` | Backward route not allowed | Forward route A→B means B→A is not permitted |
| `INVALID_ROUTING_MODEL` | Missing direct route in routing model | Navigate action references a route not in `routing_model` |

### Action Errors

| Error Code | Message | Description |
|---|---|---|
| `INVALID_ON_CLICK_ACTION_PAYLOAD` | Missing Form component `${expression}` | Referenced form binding doesn't exist |
| `INVALID_ON_CLICK_ACTION_PAYLOAD` | Missing dynamic data `${expression}` | Referenced data binding not in data model |
| `INVALID_ON_CLICK_ACTION_PAYLOAD` | PhotoPicker `max-uploaded-photos` > 1 in complete payload | Limit to 1 when using PhotoPicker in `complete` action |
| `INVALID_ON_CLICK_ACTION_PAYLOAD` | DocumentPicker `max-uploaded-documents` > 1 in complete payload | Limit to 1 when using DocumentPicker in `complete` action |
| `INVALID_ON_CLICK_ACTION_PAYLOAD` | PhotoPicker/DocumentPicker not allowed in `navigate` payload | Use global dynamic referencing instead |
| `INVALID_COMPLETE_ACTION` | `complete` only on terminal screens | Move the Footer with `complete` to a terminal screen |
| `INVALID_NAVIGATE_ACTION_PAYLOAD` | No data model in next screen | Target screen needs a `data` model |
| `INVALID_NAVIGATE_ACTION_PAYLOAD` | Fields missing in next screen's data model | Add missing fields to target screen's `data` |
| `INVALID_NAVIGATE_ACTION_PAYLOAD` | Fields expected but missing in payload | Add missing fields to the navigate action payload |
| `INVALID_NAVIGATE_ACTION_PAYLOAD` | Schema mismatch between payload and data model | Fix type mismatches between payload and screen data |
| `INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME` | Same screen navigation (loop) | Cannot navigate to the current screen |
| `INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME` | Unknown screen IDs | Target screen doesn't exist in the Flow |
| `INVALID_FLOW_JSON` | Flow JSON is not valid | Syntax error in JSON (e.g., trailing comma) |
| `KEYWORD_ONE_OF` | Component must be inside Form | Before v4.0, interactive components require a Form parent |
