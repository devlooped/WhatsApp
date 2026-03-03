# WhatsApp Flows Reference

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference

Refer to this section for details on the Flows API, Flow JSON, and other technical topics for WhatsApp Flows.

## Contents

| Page | Description |
|------|-------------|
| [Flow JSON](./flowjson.md) | Learn how to define Flows using Flow JSON |
| [Flow JSON Components](./components.md) | All available UI components for Flow screens |
| [Media Upload Components](./media-upload.md) | PhotoPicker and DocumentPicker components |
| [Flows API](./flows-api.md) | Programmatic management of Flows via Graph API |
| [Metrics API](./metrics-api.md) | Query endpoint performance metrics |
| [Webhooks](./webhooks.md) | Webhook notifications for Flow events |
| [Error Codes](./error-codes.md) | Error codes and resolutions |
| [Versioning](./versioning.md) | Flow JSON, Data API, and Message versioning |
| [Lifecycle of a Flow](./lifecycle.md) | Flow states and transitions |

## Terminology

| Term | Description |
|------|-------------|
| **Flows** | A use case or workflow (e.g. "Book an appointment") consisting of screens, components, assets, and optionally an endpoint |
| **Flow JSON** | Custom JSON object used to programmatically define Flows |
| **Components** | Individual building blocks making up a screen (text fields, buttons, etc.) |
| **Screens** | A collection of Components on a single screen, defined in Flow JSON |
| **Endpoint** | Communication channel between WhatsApp screens and the business server for data-driven interactions |

## Supported Platforms

- Android running OS 6.0 and newer
- iPhone running iOS 12 and newer

> WhatsApp Flows are **not** supported on companion devices (e.g. WhatsApp Web).
