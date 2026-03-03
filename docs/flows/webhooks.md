# Webhooks

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/flowswebhooks

Subscribe to and monitor the following webhooks related to WhatsApp Flows:

| Webhook | Description |
|---------|-------------|
| [Flow Response Message](#flow-response-message-webhook) | Triggered when a user completes a Flow |
| [Flow Status Changes](#status-change-webhook) | Triggered when a Flow's status changes |
| [Client Error Rates](#client-error-rate-webhook) | Triggered when client-side error rate crosses thresholds |
| [Endpoint Error Rates](#endpoint-error-rate-webhook) | Triggered when endpoint error rate crosses thresholds |
| [Endpoint Latency](#endpoint-latency-webhook) | Triggered when endpoint p90 latency crosses thresholds |
| [Endpoint Availability](#endpoint-availability-webhook) | Triggered when endpoint availability drops below 90% |
| [Flow Version Warnings](#flow-version-freezeexpiry-warning-webhook) | Triggered when a Flow version is about to freeze or expire |

---

## Webhook Setup

### 1. Create an Endpoint

Your endpoint must handle two HTTPS request types:
- **Verification Requests** — for initial webhook setup
- **Event Notifications** — for ongoing webhook events

Requirements:
- Valid TLS/SSL certificate (self-signed certificates not supported)
- See [Verifying Requests and Event Notifications](https://developers.facebook.com/docs/graph-api/webhooks/getting-started#create-endpoint)

### 2. Subscribe to Webhooks

1. Go to your **App Dashboard** → WhatsApp → **Configuration**
2. Find **Webhooks** → click **Configure a webhook**
3. Provide:
   - **Callback URL** — your endpoint URL
   - **Verify Token** — set when creating your endpoint
4. Click **Verify and Save**
5. In **Webhooks > Manage**, subscribe to both **`flows`** and **`messages`**

> A Meta App can only have **one** endpoint configured. Use multiple apps to send to multiple endpoints.

---

## Webhook Notification Object

All webhook payloads share this top-level structure:

| Field | Type | Description |
|-------|------|-------------|
| `object` | string | The webhook type the business subscribed to |
| `entry` | array | Array of entry objects |
| `entry[].id` | string | WhatsApp Business Account ID |
| `entry[].changes` | array | Array of change objects |
| `entry[].changes[].value` | object | Details of the change — see [Value Object](#value-object) |
| `entry[].changes[].field` | string | Always `"flows"` |

### Value Object

| Field | Type | Description |
|-------|------|-------------|
| `flow_id` | string | ID of the Flow |
| `threshold` | number | Alert threshold reached or recovered from |
| `event` | string | One of: `FLOW_STATUS_CHANGE`, `CLIENT_ERROR_RATE`, `ENDPOINT_ERROR_RATE`, `ENDPOINT_LATENCY`, `ENDPOINT_AVAILABILITY` |
| `message` | string | Detailed description of the webhook |
| `old_status` | string | Previous status: `DRAFT`, `PUBLISHED`, `DEPRECATED`, `BLOCKED`, `THROTTLED` |
| `new_status` | string | New status: `DRAFT`, `PUBLISHED`, `DEPRECATED`, `BLOCKED`, `THROTTLED` |
| `alert_state` | string | `ACTIVATED` or `DEACTIVATED` |
| `requests_count` | integer | Number of requests used to calculate the metric |
| `errors` | array | Array of error objects: `error_count`, `error_rate`, `error_type` |
| `p50_latency` | integer | P50 latency of endpoint requests (ms) |
| `p90_latency` | integer | P90 latency of endpoint requests (ms) |
| `error_rate` | integer | Overall error rate for the alert |

---

## Flow Response Message Webhook

Sent when a user completes a Flow. Delivered as a standard WhatsApp message webhook.

```json
{
  "messages": [{
    "context": {
      "from": "16315558151",
      "id": "gBGGEiRVVgBPAgm7FUgc73noXjo"
    },
    "from": "<USER_ACCOUNT_NUMBER>",
    "id": "<MESSAGE_ID>",
    "type": "interactive",
    "interactive": {
      "type": "nfm_reply",
      "nfm_reply": {
        "name": "flow",
        "body": "Sent",
        "response_json": "{\"flow_token\": \"<FLOW_TOKEN>\", \"optional_param1\": \"<value1>\"}"
      }
    },
    "timestamp": "<MESSAGE_SEND_TIMESTAMP>"
  }]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `context.from` | string | User's WhatsApp account number |
| `context.id` | string | Message ID |
| `interactive.type` | string | Always `nfm_reply` |
| `interactive.nfm_reply.name` | string | Always `flow` |
| `interactive.nfm_reply.body` | string | Always `Sent` |
| `interactive.nfm_reply.response_json` | string | Flow-specific data (defined by Flow JSON `complete` action or endpoint final response) |
| `timestamp` | string | Time the response was sent |

---

## Status Change Webhook

Sent when a Flow transitions to `Published`, `Throttled`, `Blocked`, or `Deprecated`. Also sent on Flow creation.

```json
{
  "entry": [{
    "id": "644600416743275",
    "time": 1684969340,
    "changes": [{
      "value": {
        "event": "FLOW_STATUS_CHANGE",
        "message": "Flow Webhook 3 changed status from DRAFT to PUBLISHED",
        "flow_id": "6627390910605886",
        "old_status": "DRAFT",
        "new_status": "PUBLISHED"
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

On creation, `old_status` is absent and `new_status` is `DRAFT`.

---

## Client Error Rate Webhook

Sent when client-side screen navigation error rate crosses (or recovers from) thresholds.

> Client error rate is **approximate** — not available for all devices and regions.

**Thresholds:** 5%, 10%, 50%  
**Detection window:** 60 minutes

**Possible resolutions:** Check the [Error Codes reference](./error-codes.md#webhook-error-types) for details on listed errors.

```json
{
  "entry": [{
    "id": "106181168862417",
    "time": 1674160476,
    "changes": [{
      "value": {
        "event": "CLIENT_ERROR_RATE",
        "message": "The flow client request error rate has reached the 5% threshold in the last 60 minutes.",
        "flow_id": "691244242662581",
        "error_rate": 14.28,
        "threshold": 10,
        "alert_state": "ACTIVATED",
        "errors": [
          { "error_type": "INVALID_SCREEN_TRANSITION", "error_rate": 66.66, "error_count": 2 },
          { "error_type": "PUBLIC_KEY_MISSING", "error_rate": 33.33, "error_count": 1 }
        ]
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

---

## Endpoint Error Rate Webhook

Sent when endpoint request error rate crosses (or recovers from) thresholds.

**Thresholds:** 5%, 10%, 50%  
**Detection window:** 30 minutes

**Possible resolutions:** Check the [Error Codes reference](./error-codes.md#webhook-error-types).

```json
{
  "entry": [{
    "id": "106181168862417",
    "time": 1674160476,
    "changes": [{
      "value": {
        "event": "ENDPOINT_ERROR_RATE",
        "message": "The flow endpoint request error rate has reached the 10% threshold in the last 30 minutes.",
        "flow_id": "691244242662581",
        "error_rate": 14.28,
        "threshold": 10,
        "alert_state": "ACTIVATED",
        "errors": [
          { "error_type": "CAPABILITY_ERROR", "error_rate": 66.66, "error_count": 2 },
          { "error_type": "TIMEOUT", "error_rate": 33.33, "error_count": 1 }
        ]
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

---

## Endpoint Latency Webhook

Sent when p90 endpoint latency crosses (or recovers from) thresholds.

**Thresholds:** 1s, 5s, 7s  
**Detection window:** 30 minutes

**Possible resolutions:** Improve endpoint responsiveness — aim for responses under 1 second.

```json
{
  "entry": [{
    "id": "106181168862417",
    "time": 1674160476,
    "changes": [{
      "value": {
        "event": "ENDPOINT_LATENCY",
        "message": "Flow endpoint latency has reached the p90 threshold in the last 30 minutes.",
        "flow_id": "691244242662581",
        "p90_latency": 8000,
        "p50_latency": 500,
        "requests_count": 34,
        "threshold": 7000,
        "alert_state": "ACTIVATED"
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

---

## Endpoint Availability Webhook

Sent when endpoint availability drops below 90% (or recovers above it).

**Threshold:** 90%  
**Detection window:** 10 minutes

**Possible resolutions:**
- Ensure your endpoint is publicly reachable
- Implement and correctly respond to [health check requests](https://developers.facebook.com/docs/whatsapp/flows/reference/encryptedsecuredatachannel#h)

```json
{
  "entry": [{
    "id": "106181168862417",
    "time": 1674160476,
    "changes": [{
      "value": {
        "event": "ENDPOINT_AVAILABILITY",
        "message": "The flow endpoint availability has breached the 90% threshold in the last 10 minutes.",
        "flow_id": "12345678",
        "alert_state": "ACTIVATED",
        "availability": 75,
        "threshold": 90
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

---

## Flow Version Freeze/Expiry Warning Webhook

Sent when a Flow is created or sent using a version that is **about to be frozen or expired**.

**Possible resolutions:** Migrate to the [recommended version](https://developers.facebook.com/docs/whatsapp/flows/changelogs#currently-supported-versions) as soon as possible.

**On creation (version about to freeze):**

```json
{
  "entry": [{
    "id": "644600416743275",
    "time": 1684969340,
    "changes": [{
      "value": {
        "event": "FLOW_STATUS_CHANGE",
        "message": "Flow Webhook 3 has been created with DRAFT status",
        "flow_id": "6627390910605886",
        "new_status": "DRAFT",
        "warning": "Your current Flow version will freeze in 21 days. You won't be able to send the Flow after it expires. Please migrate to the recommended version as soon as possible."
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```

**On sending (version about to expire):**

```json
{
  "entry": [{
    "id": "644600416743275",
    "time": 1684969340,
    "changes": [{
      "value": {
        "event": "FLOW_VERSION_EXPIRY_WARNING",
        "warning": "Your current Flow version will freeze in 21 days. Please migrate to the recommended version as soon as possible.",
        "flow_id": "6627390910605886"
      },
      "field": "flows"
    }]
  }],
  "object": "whatsapp_business_account"
}
```
