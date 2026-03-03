# Lifecycle of a Flow

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/lifecycle

Flows can exist in a variety of states during their lifetime, with each state conveying different requirements, abilities, and limitations.

![Flow lifecycle diagram](https://lookaside.fbsbx.com/elementpath/media/?media_id=731582578794664&version=1734106641)

---

## Business-set Flow States

These states result from API calls such as creating or publishing a Flow.

### Draft

The initial state when a Flow is created — indicates the Flow is actively being modified.

- Can be sent **for testing only**
- Can be **fully deleted** if no longer needed
- A banner is shown at the top of the Flow when viewed by a user

**Next states:** Deleted, Published

---

### Deleted

Not technically a state — represents a Flow that no longer exists. Flows may only be deleted while in the **Draft** state.

**Next states:** None (terminal)

---

### Published

The Flow is ready to be sent to real users.

After publishing, you can still make changes (metadata or Flow JSON), which will return the Flow to **Draft** state. Consider this for small fixes only.

**Options after publishing:**
- Edit metadata or Flow JSON → returns to Draft (new messages reflect changes, old messages keep prior content)
- Clone the Flow using `clone_flow_id` for significant changes
- Deprecate the Flow using `/deprecate` (cannot delete published Flows)

> Supports up to **5 last versions**. Older versions are deprecated automatically.

**Next states:** Draft, Deprecated, Throttled

---

### Deprecated

The Flow can no longer be sent to real users. May still be present on users' devices — you may continue to receive responses from deprecated Flows.

**Next states:** None (terminal)

---

## System-set Flow States

These states are entered automatically based on WhatsApp monitoring.

### Throttled

Entered when monitoring detects unhealthy endpoint or screen navigation. The Flow can still be opened and sent, but **sending is limited to 10 messages per hour**.

If health improves, the Flow transitions back to **Published**.

**Next states:** Published, Deprecated, Blocked

---

### Blocked

Entered when a Throttled Flow's endpoint health deteriorates further. The Flow **cannot be sent or opened** by users.

WhatsApp monitoring continues checking health. Upon improvement, the Flow moves back to Throttled → Published.

**Next states:** Deprecated, Throttled

---

## Editing a Published Flow

After publishing, you may still update the Flow's metadata or Flow JSON — this returns it to **Draft** state.

**Important notes:**
- Already-sent messages retain the old Flow content
- Only **new** messages reflect updated content after republishing
- Flow endpoint contract must stay in sync with Flow JSON to avoid breaking the experience
- Quality metrics (webhook notifications, error rate, latency) are only measured for the **last published version**

**Restrictions:**
- Some older Flows do not support editing — clone them to get editing support
- Flows editing requires no OnPrem phone numbers linked to the Business Account

---

## Example Flow Lifecycles

### A Successful Flow

| State | Event | Action | New State |
|-------|-------|--------|-----------|
| — | Create a new Flow | [Create](./flows-api.md#create) | Draft |
| Draft | Update the Flow JSON content | [Update JSON](./flows-api.md#update-json) | Draft |
| Draft | Set the endpoint URI | [Update](./flows-api.md#update) | Draft |
| Draft | Ready for production | [Publish](./flows-api.md#publish) | Published |

---

### A Flow with Health Issues

| State | Event | Action | New State |
|-------|-------|--------|-----------|
| — | Create a new Flow | Create | Draft |
| Draft | Update Flow JSON | Update JSON | Draft |
| Draft | Ready for production | Publish | Published |
| Published | Monitoring detects endpoint issues | Throttle | Throttled |
| Throttled | Endpoint healthy again | Unthrottle | Published |
| Published | Monitoring detects issues again | Throttle | Throttled |
| Throttled | Health deteriorates further | Block | Blocked |
| Blocked | Endpoint healthy again | Unblock | Throttled |
| Throttled | Still healthy | Unthrottle | Published |

---

### A Flow That Never Reaches Production

| State | Event | Action | New State |
|-------|-------|--------|-----------|
| — | Create a new Flow | Create | Draft |
| Draft | Update Flow JSON | Update JSON | Draft |
| Draft | Set the endpoint URI | Update | Draft |
| Draft | No longer needed | Delete | Deleted |
