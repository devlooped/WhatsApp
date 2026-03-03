# Metrics API

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/metrics_api

> ⚠️ **Deprecation Notice:** The Metrics API will be deprecated on **April 30, 2026**.

The Metrics API enables you to query system metrics related to the performance of your Flow's endpoint. Metrics include request counts, error counts, latency distribution, error rates, and endpoint availability.

> **Minimum threshold:** Flows must have generated at least **250 requests** before metric data is available. Below this threshold, an exception is returned indicating insufficient data.

> Flow metrics are still being developed and may change. Use them for directional guidance, not historical comparisons or strategic planning.

---

## Available Metrics

All metrics are **not real-time** — it may take a couple of hours for events to be ingested.

### `ENDPOINT_REQUEST_COUNT`

Total endpoint request count for a given period.

```json
[
  {
    "key": "value",
    "value": 315
  }
]
```

---

### `ENDPOINT_REQUEST_ERROR`

Endpoint request errors aggregated by error type:

| Error Type | Description |
|---|---|
| `timeout_error` | Request exceeded the time limit |
| `unexpected_http_status_code` | Received an error HTTP response code |
| `cannot_be_served` | Request not executed because the Flow cannot be served |
| `no_http_response_error` | Connection closed without a valid HTTP response (internal error) |

```json
[
  { "key": "timeout_error", "value": 5 },
  { "key": "unexpected_http_status_code", "value": 10 }
]
```

---

### `ENDPOINT_REQUEST_ERROR_RATE`

Ratio of errors to total requests for a given period (single value).

```json
[
  {
    "key": "value",
    "value": 0.24
  }
]
```
_In this example, 24% of requests failed._

---

### `ENDPOINT_REQUEST_LATENCY_SECONDS_CEIL`

Request latencies grouped into 10 categories (in seconds, rounded up). The last category represents 10+ seconds.

```json
[
  { "key": "1",   "value": 410 },
  { "key": "3",   "value": 61  },
  { "key": "10",  "value": 2   },
  { "key": "10+", "value": 33  }
]
```

_In this example: 410 requests < 1s, 61 requests between 2–3s, 2 requests between 9–10s, 33 requests > 10s._

---

### `ENDPOINT_AVAILABILITY`

Results of periodic availability health checks (available from `data_api_version` 3.0+).

```json
[
  { "key": "succeeded", "value": 10 },
  { "key": "failed",    "value": 5  }
]
```

---

## Variables Required for API Calls

| Key | Value |
|---|---|
| `Base-URL` | `https://graph.facebook.com/v16.0` |
| `User-Access-Token` | Temporary access token from your app (24h expiry), or a [System User Access Token](https://developers.facebook.com/docs/whatsapp/business-management-api/get-started#system-user-access-tokens) |
| `Flow-ID` | ID of the Flow to query metrics for |

---

## API Endpoints

### Query Metric Data Points

Retrieve metric data points for a specified time period and granularity.

**Request:**

```bash
curl '{Base-URL}/{Flow-ID}?fields=metric.name(ENDPOINT_REQUEST_ERROR).granularity(day).since(2024-01-28).until(2024-01-30)' \
  --header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | ✅ | Metric name (see [Available Metrics](#available-metrics)) |
| `granularity` | string | ✅ | `DAY`, `HOUR`, or `LIFETIME` |
| `since` | string (`YYYY-MM-DD`) | — | Start of period. Defaults to oldest allowed date.<br>• `DAY`: max 90 days back<br>• `HOUR`: max 30 days back |
| `until` | string (`YYYY-MM-DD`) | — | End of period. Defaults to current date. |

**Response:**

```json
{
  "id": "<Flow-ID>",
  "metric": {
    "granularity": "DAY",
    "name": "ENDPOINT_REQUEST_ERROR",
    "data_points": [
      {
        "timestamp": "2024-01-28T08:00:00+0000",
        "data": [
          { "key": "timeout_error", "value": 5 }
        ]
      },
      {
        "timestamp": "2024-01-29T08:00:00+0000",
        "data": [
          { "key": "unexpected_http_status_code", "value": 12 }
        ]
      }
    ]
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | The unique ID of the Flow |
| `metric` | object | Metric response object |
| `metric.granularity` | string | Requested time granularity |
| `metric.name` | string | Requested metric name |
| `metric.data_points` | array | List of metric data points |
| `metric.data_points[].timestamp` | string (ISO 8601) | Start of the data point interval |
| `metric.data_points[].data` | array | Metric-specific key-value data |
