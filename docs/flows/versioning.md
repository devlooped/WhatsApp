# Versioning

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/versioning

Versioning allows you to control the details of the services you interact with so you can maintain stability for your Flows even as functionality is added and modified.

---

## Overview

WhatsApp Flows uses three versioning systems:

| Version Type | Controls | Format | Example |
|---|---|---|---|
| **Flow JSON version** | Implementation and parameters used with Flow components and layouts | `{major}.{minor}` | `5.1` |
| **Data API version** | Encryption and payload format for your endpoint (endpoint Flows only) | `{major}.{minor}` | `3.0` |
| **Message version** | The message payload version | integer | `1` |

---

## Major vs. Minor Versions

Following [Semantic Versioning](https://semver.org) conventions:

1. **Major versions** — incremented for **breaking changes** (e.g., a field removed or behavior altered)
2. **Minor versions** — incremented for **non-breaking additions** (e.g., new parameters or features)
3. Later versions include all features from previous versions unless explicitly deprecated
4. Version numbers increment separately — the version after `1.9` is `1.10`, **not** `2.0`
5. After a major version increase, new non-breaking changes are added to the latest major version only

### Example Version Timeline

| Change | Version |
|--------|---------|
| Initial release | `1.0` |
| Add a new parameter | `1.1` |
| Add new non-breaking functionality | `1.2` |
| Deprecate and remove functionality | `2.0` |
| Add a new layout type | `2.1` |
| Add a new component | `2.2` |
| Breaking behavior change | `3.0` |

---

## Early Release Versions

- Intended to allow early integration before full client device support
- Work the same as standard versions, but may **not be deliverable to all client devices**
- If a Flow message cannot be delivered due to an unsupported version, the [131026 Message Undeliverable error](https://developers.facebook.com/docs/whatsapp/cloud-api/support/error-codes#other-errors) is returned

---

## Version Support and Lifecycle

### States

| State | Description |
|-------|-------------|
| **Frozen** | Publishing a Flow targeting this version is **prohibited**. Existing Flows using this version can still be sent and opened. |
| **Expired** | Flows targeting this version can **no longer be sent or opened** by customers. |

### Target Support Schedule

- The [changelog](https://developers.facebook.com/docs/whatsapp/flows/changelogs) is updated with dates before a version freezes or expires
- Generally, **90 days notice** is given before a version is frozen
- Circumstances may shorten this period — security vulnerabilities may skip the freeze stage entirely

---

## Example Support Timelines

### General Support Timeline

A new major version (`2.0`) releases 2 months after `1.0`:

| Date | Event |
|------|-------|
| 1-Jan-2024 | Version `1.0` launches |
| 1-Mar-2024 | Version `2.0` launches; notice period for `1.0` freeze begins |
| 31-May-2024 | Version `1.0` is **frozen** — no new Flows can be published targeting `1.0` |

### Reduced Timeline (Security Vulnerability)

A security vulnerability is found in `1.1` — the version skips freeze and goes directly to expired:

| Date | Event |
|------|-------|
| 1-Dec-2023 | Version `1.1` launches |
| 31-Dec-2023 | Security vulnerability discovered; version `1.2` launches with fix |
| 1-Jan-2024 | Notice period for `1.1` expiry begins |
| 31-Mar-2024 | Version `1.1` is **expired** — no Flows using `1.1` can be sent or opened |

---

## See Also

- [Changelog](https://developers.facebook.com/docs/whatsapp/flows/changelogs) — currently supported versions and dates
- [Flow JSON Reference](./flowjson.md)
- [Flows API](./flows-api.md)
