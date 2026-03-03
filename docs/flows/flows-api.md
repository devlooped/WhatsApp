# Flows API

> **Source:** https://developers.facebook.com/docs/whatsapp/extensions/reference/extensionsapi

The Flows API is a [Graph API](https://developers.facebook.com/docs/graph-api/) that enables you to programmatically create, update, publish, deprecate, and manage WhatsApp Flows.

**Postman Collection:** [WhatsApp Business Platform Workspace](https://www.postman.com/meta/workspace/whatsapp-business-platform/documentation/24926895-7bf51205-92ed-49d1-af4a-0130cf84b6f6)

---

## Variables

| Key | Description |
|-----|-------------|
| `BASE-URL` | Base URL for Facebook Graph API. Example: `https://graph.facebook.com/v18.0` |
| `ACCESS-TOKEN` | User access token (temporary, 24h) or a [System User Access Token](https://developers.facebook.com/docs/whatsapp/business-management-api/get-started#system-user-access-tokens) |
| `WABA-ID` | WhatsApp Business Account ID |
| `FLOW-ID` | Flow ID returned after creating a Flow |

---

## API Reference

### Create a Flow

Creates a new Flow in `DRAFT` status. Optionally create and publish in a single request by providing `flow_json` and `publish: true`.

```bash
curl -X POST '{BASE-URL}/{WABA-ID}/flows' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}' \
  --header 'Content-Type: application/json' \
  --data '{
    "name": "My first flow",
    "categories": ["OTHER"],
    "flow_json": "{\"version\":\"5.0\",\"screens\":[...]}",
    "publish": true
  }'
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | ✅ | Flow name (must be unique per WABA) |
| `categories` | array | ✅ | One or more: `SIGN_UP`, `SIGN_IN`, `APPOINTMENT_BOOKING`, `LEAD_GENERATION`, `CONTACT_US`, `CUSTOMER_SUPPORT`, `SURVEY`, `OTHER` |
| `flow_json` | string | — | Flow JSON encoded as string |
| `publish` | boolean | — | Publish immediately (requires valid `flow_json`) |
| `clone_flow_id` | string | — | ID of source Flow to clone |
| `endpoint_uri` | string | — | WA Flow Endpoint URL (Flow JSON v3.0+) |

**Response:**

```json
{
  "id": "<Flow-ID>",
  "success": true,
  "validation_errors": [
    {
      "error": "INVALID_PROPERTY_VALUE",
      "error_type": "FLOW_JSON_ERROR",
      "message": "Invalid value found for property 'type'.",
      "line_start": 10,
      "line_end": 10,
      "column_start": 21,
      "column_end": 34,
      "pointers": [
        {
          "line_start": 10,
          "line_end": 10,
          "column_start": 21,
          "column_end": 34,
          "path": "screens[0].layout.children[0].type"
        }
      ]
    }
  ]
}
```

---

### Update Flow Metadata

Update a Flow's name, categories, endpoint URI, or connected application.

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}' \
  --header 'Content-Type: application/json' \
  --data '{ "name": "New flow name" }'
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | — | New Flow name |
| `categories` | array | — | Updated categories (at least one required if provided) |
| `endpoint_uri` | string | — | WA Flow Endpoint URL (Flow JSON v3.0+) |
| `application_id` | string | — | Meta application ID to connect to the Flow |

**Response:** `{ "success": true }`

---

### Update Flow JSON

Upload a new Flow JSON asset file. File must be attached as `multipart/form-data`.

```bash
curl -X POST '{BASE-URL}/{FLOW_ID}/assets' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}' \
  --form 'file=@"/path/to/flow.json";type=application/json' \
  --form 'name="flow.json"' \
  --form 'asset_type="FLOW_JSON"'
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | ✅ | Must be `flow.json` |
| `asset_type` | string | ✅ | Must be `FLOW_JSON` |
| `file` | json | ✅ | JSON file content (max 10 MB) |

Returns `validation_errors` if any exist in the Flow JSON.

---

### Generate Web Preview

Generate a shareable preview URL to visualize and interact with the Flow. Preview URLs are public and expire in **30 days**.

```bash
curl '{BASE-URL}/{FLOW-ID}?fields=preview.invalidate(false)' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:**

```json
{
  "preview": {
    "preview_url": "https://business.facebook.com/wa/manage/flows/550.../preview/?token=b9d6....",
    "expires_at": "2023-05-21T11:18:09+0000"
  },
  "id": "flow-1"
}
```

**Embed as iframe:**

```html
<iframe src="https://business.facebook.com/wa/manage/flows/550.../preview/?token=b9d6...." width="430" height="800"></iframe>
```

**URL Parameters for Interactive Preview:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `interactive` | boolean | Enable interactive mode. Default: `false` |
| `flow_token` | string | Token sent with each request (required for endpoint Flows) |
| `flow_action` | `navigate` \| `data_exchange` | First action when Flow starts |
| `flow_action_payload` | string | Initial screen data as JSON, URL-encoded. Required if `flow_action` is `navigate` |
| `phone_number` | string | Phone number for encrypting the request payload (required for endpoint Flows) |
| `debug` | string | Show action panel during preview (requires `interactive=true`) |

**Sample interactive URL:**

```
https://business.facebook.com/wa/manage/flows/550.../preview/?token=b9d6...&interactive=true&flow_action=navigate&flow_action_payload=%7B%22screen%22%3A%22FIRST_SCREEN%22%7D&debug=true
```

---

### Delete a Flow

Deletes a Flow that is in `DRAFT` status.

```bash
curl -X DELETE '{BASE-URL}/{FLOW-ID}' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:** `{ "success": true }`

---

### List Flows

Retrieve all Flows under a WhatsApp Business Account.

```bash
curl '{BASE-URL}/{WABA-ID}/flows' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:**

```json
{
  "data": [
    { "id": "flow-1", "name": "flow 1", "status": "DRAFT",      "categories": ["CONTACT_US"],      "validation_errors": [] },
    { "id": "flow-2", "name": "flow 2", "status": "PUBLISHED",   "categories": ["SURVEY"],          "validation_errors": [] },
    { "id": "flow-3", "name": "flow 3", "status": "DRAFT",       "categories": ["LEAD_GENERATION"], "validation_errors": [] }
  ],
  "paging": {
    "cursors": { "before": "QVFI...", "after": "QVFI..." }
  }
}
```

---

### Retrieve Flow Details

Get full details for a single Flow.

```bash
curl '{BASE-URL}/{FLOW-ID}?fields=id,name,categories,preview,status,validation_errors,json_version,data_api_version,endpoint_uri,whatsapp_business_account,application,health_status' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

To check health for a specific phone number:
```bash
?fields=id,name,health_status.phone_number(PHONE_NUMBER_ID)
```

**Response Fields:**

| Field | Returned by Default | Description |
|-------|---------------------|-------------|
| `id` | ✅ | Unique Flow ID |
| `name` | ✅ | User-defined name (not visible to users) |
| `status` | ✅ | `DRAFT`, `PUBLISHED`, `DEPRECATED`, `BLOCKED`, or `THROTTLED` |
| `categories` | ✅ | List of Flow categories |
| `validation_errors` | ✅ | All errors must be fixed before publishing |
| `json_version` | — | Flow JSON version used |
| `data_api_version` | — | Data API version (endpoint Flows only) |
| `endpoint_uri` | — | WA Flow Endpoint URL |
| `preview` | — | Preview URL and expiry |
| `whatsapp_business_account` | — | WABA that owns the Flow |
| `application` | — | Meta app used to create the Flow |
| `health_status` | — | Summary of Flow health status |

**`health_status.can_send_message` values:**

| Value | Meaning |
|-------|---------|
| `AVAILABLE` | Node meets all requirements |
| `LIMITED` | Meets requirements with limitations (includes `additional_info`) |
| `BLOCKED` | Does not meet requirements (includes `errors` with descriptions and solutions) |

---

### List Flow Assets

Returns all assets attached to a Flow.

```bash
curl '{BASE-URL}/{FLOW-ID}/assets' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:**

```json
{
  "data": [
    {
      "name": "flow.json",
      "asset_type": "FLOW_JSON",
      "download_url": "https://scontent.xx.fbcdn.net/m1/v/..."
    }
  ],
  "paging": { "cursors": { "before": "QVFIU...", "after": "QVFIU..." } }
}
```

---

### Publish a Flow

Transitions a Flow from `DRAFT` to `PUBLISHED`.

**Prerequisites before publishing:**
- Business is [verified](https://developers.facebook.com/docs/development/release/business-verification/) with high message quality
- All validation errors and [publishing checks](https://developers.facebook.com/docs/whatsapp/flows/guides/healthmonitoring#publishing-checks) are resolved
- Flow meets [design principles](https://developers.facebook.com/docs/whatsapp/flows/guides/bestpractices)
- Flow complies with WhatsApp Terms of Service and Business Messaging Policy

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}/publish' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:** `{ "success": true }`

---

### Deprecate a Flow

Marks a published Flow as deprecated. Deprecated Flows cannot be sent, but may still be present on users' devices.

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}/deprecate' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Response:** `{ "success": true }`

---

### Migrate Flows

Copy Flows from one WABA to another (within the same Meta business).

```bash
curl -X POST '{BASE-URL}/<DESTINATION_WABA_ID>/migrate_flows?source_waba_id=<SOURCE_WABA_ID>&source_flow_names=<SOURCE_FLOW_NAMES>' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Parameters:**

| Placeholder | Required | Description |
|-------------|----------|-------------|
| `<DESTINATION_WABA_ID>` | ✅ | Destination WABA ID |
| `<SOURCE_WABA_ID>` | ✅ | Source WABA ID |
| `<SOURCE_FLOW_NAMES>` | — | Specific Flow names to migrate (max 100). If omitted, migrates all Flows |

**Notes:**
- Flows with the same name in the destination WABA are skipped (error returned per flow)
- Migrated Flows get new Flow IDs
- Published Flows are published in the destination; drafts remain drafts

**Response:**

```json
{
  "migrated_flows": [
    { "source_name": "appointment-booking", "source_id": "1234", "migrated_id": "5678" }
  ],
  "failed_flows": [
    { "source_name": "lead-gen", "error_code": "4233041", "error_message": "Flow with the same name exists in destination WABA." }
  ]
}
```
