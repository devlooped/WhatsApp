# Flow JSON

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/flowjson

Flow JSON enables businesses to create workflows in WhatsApp using a custom JSON structure. Workflows are initiated, run, and managed entirely inside WhatsApp, supporting multiple screens, data flows, and response messages.

> **Tip:** Use the [Flow Builder](https://business.facebook.com/wa/manage/) to visualize and preview Flows in real time.

---

## Structure Overview

| Section | Description |
|---------|-------------|
| **Screen Data Model** | Commands to define static types that power the screen |
| **Screens** | Layouts composed from standard UI library components |
| **Components** | Individual building blocks (text fields, buttons, etc.) |
| **Routing Model** | Rules for state transitions between screens |
| **Actions** | Syntax to invoke pre-defined client logic (`navigate`, `data_exchange`, `complete`, `open_url`, `update_data`) |

---

## Top-Level Properties

### Required

| Property | Description |
|----------|-------------|
| `version` | Flow JSON version for compilation. See [supported versions](https://developers.facebook.com/docs/whatsapp/flows/changelogs#currently-supported-versions) |
| `screens` | Array of screen definitions |

### Optional

| Property | Description |
|----------|-------------|
| `routing_model` | Directed graph of screen transitions. Auto-generated if no Data Endpoint is used. **Required** when using a Data Endpoint. |
| `data_api_version` | Version for Data Endpoint communication. Currently `3.0`. Required when using data-channel capability. |

**Example (with endpoint, URI set via API):**
```json
{
  "version": "3.1",
  "data_api_version": "3.0",
  "routing_model": { "MY_FIRST_SCREEN": ["MY_SECOND_SCREEN"] },
  "screens": [...]
}
```

---

## Screens

Each screen is a single node in the Flow's state machine.

```json
{
  "id": "string",
  "terminal": "?boolean",
  "success": "?boolean",
  "title": "?string",
  "refresh_on_back": "?boolean",
  "sensitive": "?array",
  "data": "?object",
  "layout": "object"
}
```

### Required Properties

| Property | Description |
|----------|-------------|
| `id` | Unique screen identifier. `SUCCESS` is reserved and cannot be used. |
| `layout` | The screen's UI layout (see [Layout](#layout)) |

### Optional Properties

| Property | Description |
|----------|-------------|
| `terminal` | Marks this as the end state. Multiple screens can be terminal. Terminal screens require a Footer component. |
| `data` | JSON Schema declaration of dynamic data for the screen. Must include `__example__` for all fields. |
| `title` | Displayed in the top navigation bar. |
| `success` | (Terminal screens only) Whether completing on this screen is a successful outcome. Default: `true`. |
| `refresh_on_back` | Trigger a data exchange request when navigating back to this screen. Default: `false`. |
| `sensitive` | (v5.1+) Array of field names to hide/mask in the Flow completion summary. |

### `refresh_on_back` Behavior

| Value | Behavior |
|-------|----------|
| `false` (default) | Returns to previous screen with cached data and prior user input — faster, avoids a round-trip |
| `true` | Sends a new request to the Data Endpoint with `action: "BACK"` — use when data must be revalidated |

- ![refresh_on_back=false](https://lookaside.fbsbx.com/elementpath/media/?media_id=1332561274017186&version=1760103259)
- ![refresh_on_back=true](https://lookaside.fbsbx.com/elementpath/media/?media_id=2611752639002090&version=1760103259)

### `sensitive` Field Masking (v5.1+)

| Component | Masking | User Experience |
|-----------|---------|-----------------|
| TextInput | ✅ | Masked (`••••••••••••`) |
| Password / OTP | ❌ | Hidden completely |
| TextArea | ✅ | Masked |
| DatePicker | ✅ | Masked |
| Dropdown | ✅ | Masked |
| CheckboxGroup | ✅ | Masked |
| RadioButtonsGroup | ✅ | Masked |
| OptIn | ❌ | Displayed as-is |
| DocumentPicker | ✅ | Documents hidden |
| PhotoPicker | ✅ | Media hidden |

---

## Layout

The layout defines the screen's UI content.

| Property | Description |
|----------|-------------|
| `type` | Layout identifier. Currently only `"SingleColumnLayout"` (vertical flexbox) is supported. |
| `children` | Array of components from the WhatsApp Flows Library |

---

## Routing Model

Required only when using a Data Endpoint. Defines a directed graph of screen transitions (max **10 branches** per screen).

![Routing model example](https://lookaside.fbsbx.com/elementpath/media/?media_id=443014077950763&version=1760103259)

**Example routing model:**
```
Item Catalog => [Item Details Page]
Item Details Page => [Item Catalog, Checkout]
Checkout => []
```

### Routing Rules

1. A route cannot point to the current screen (self-loops are not allowed)
2. If an edge exists between two screens, users can navigate back and forth using the BACK button
3. Specify only **forward** routes — don't add a reverse route if the forward is already defined
4. Routes may be empty for a screen with no forward transitions
5. There must be exactly one **entry** screen — a screen with no inbound edges
6. All routes must eventually reach a terminal screen

---

## Properties: Static vs. Dynamic

### Static Properties

Set once and never change. Simplest way to build a Flow.

### Dynamic Properties

Bound to data or form objects via `"${data.field}"` or `"${form.field}"` syntax.

Supported data types: `string`, `number`, `boolean`, `object`, `array`

| Binding Type | Syntax | Description |
|---|---|---|
| Form property | `"${form.field_name}"` | User-entered input from the current screen |
| Screen property | `"${data.field_name}"` | Data from the server or a previous screen's `navigate` payload |

### Nested Expressions (v6.0+)

Wrap properties in backticks (`` ` ``) to enable expressions. Available operations:

| Operation | Operators | Types | Returns |
|-----------|-----------|-------|---------|
| Equality comparisons | `==`, `!=` | string, number, boolean | boolean |
| Math comparisons | `<`, `<=`, `>`, `>=` | number | boolean |
| Logical comparisons | `&&`, `\|\|` | boolean | boolean |
| String concatenation | (space-separated) | string, number, boolean | string |
| Math operations | `+`, `-`, `/`, `%` | number | number |

> Division/modulo by zero returns `0` (not NaN).  
> To include a literal backtick in a string, escape it with `\\`.

**Examples:**
```json
{ "visible": "`${form.age} > 18`" }
{ "text": "`'Hello ' ${form.first_name}`" }
{ "text": "`${data.total} / ${form.group_size}`" }
```

---

## Screen Data Declaration

### Without Endpoint

```json
{
  "data": {
    "hello_world_text": {
      "type": "string",
      "__example__": "Hello, World!"
    }
  }
}
```

Reference in a component: `"${data.hello_world_text}"`

- `__example__` is **mandatory** — used as mock data during development

### With Endpoint (v3.0+)

Add `data_api_version`, `routing_model` to the Flow JSON. The endpoint supplies the actual data matching the declared schema.

---

## Forms and Form Properties

The `Form` component is **optional**. Without a Form, use `init-value` and `error-message` directly on individual components.

**HTML equivalent:**
```html
<form>
  <input type="text" name="first_name">
  <input type="text" name="last_name">
</form>
```

### Form Configuration

| Attribute | Description |
|-----------|-------------|
| `init-values` | `{ field_name: initial_value }` — pre-fills inputs. Types: `string`, `Array<string>`, or dynamic `"${data.init_values}"` |
| `error-messages` | `{ field_name: error_message }` — sets per-field error messages from the server |

**`init-values` data types by component:**

| Component | Data Type |
|-----------|-----------|
| CheckboxGroup | `Array<string>` |
| RadioButtonsGroup | `string` |
| TextInput / TextArea | `string` |
| Dropdown | `string` |

### Using Form Properties

Reference user input via `"${form.field_name}"` using the component's `name` property.

**Pass data to next screen:**
```json
{
  "type": "Footer",
  "label": "Submit data",
  "on-click-action": {
    "name": "navigate",
    "next": { "type": "screen", "name": "NEXT_SCREEN" },
    "payload": {
      "name": "${form.first_name}",
      "lang": "${form.favourite_language}"
    }
  }
}
```

**Submit data to server:**
```json
{
  "type": "Footer",
  "label": "Submit data",
  "on-click-action": {
    "name": "data_exchange",
    "payload": {
      "name": "${form.first_name}",
      "lang": "${form.favourite_language}"
    }
  }
}
```

---

## Global Dynamic and Form Properties

Access data from any screen using:

```
${screen.<screen_name>.(form|data).<field-name>}
```

| Segment | Description |
|---------|-------------|
| `screen` | Global variable for screen storage |
| `screen_name` | The screen ID to reference |
| `form` \| `data` | Whether to access form input or screen data |
| `field-name` | The field to access |

**Use cases:**
1. **Carrying data forward** — reference prior screen data without passing it in `navigate` payload
2. **No data declaration needed** — global fields don't need to be declared in the target screen's `data` model
3. **Forward references** — reference data from future screens (use conditional rendering to handle empty values)

**Example: Access data from a previous screen**
```json
{
  "type": "TextBody",
  "text": "${screen.SCREEN_ONE.data.field1}"
}
```

---

## Actions

Actions are triggered by interactive UI elements.

| Action | Description | Payload |
|--------|-------------|---------|
| `navigate` | Transition to the next screen | Static JSON payload |
| `complete` | Terminate the Flow and send response | Static JSON payload |
| `data_exchange` | Send data to the Flow Data Endpoint | Customizable JSON `{ [key: string]: any }` |
| `update_data` (v6.0+) | Immediately update the current screen's state | Static JSON payload |
| `open_url` (v6.0+) | Open a URL in the device's default browser | No payload — only a `url` property |

### `navigate` Action

Transitions to another screen. The payload becomes available as `${data.field_name}` on the next screen.

> Do not use on the Footer of a terminal screen — this prevents the Flow from terminating.

```json
{
  "type": "Footer",
  "label": "Continue",
  "on-click-action": {
    "name": "navigate",
    "next": { "type": "screen", "name": "NEXT_SCREEN" },
    "payload": {
      "name": "${form.first_name}"
    }
  }
}
```

### `complete` Action

Terminates the Flow and sends data via webhook. The business receives `flow_token` and all payload parameters.

> Only include user-inputted data. Keep payload size minimal — avoid base64 images.

```json
{
  "type": "Footer",
  "label": "Submit",
  "on-click-action": {
    "name": "complete",
    "payload": {
      "discount_code": "${data.discount_code}",
      "items": "${form.selected_items}"
    }
  }
}
```

### `data_exchange` Action

Sends data to the Flow Data Endpoint and waits for a response before continuing (endpoint Flows only).

```json
{
  "type": "Footer",
  "label": "Submit data",
  "on-click-action": {
    "name": "data_exchange",
    "payload": {
      "discount_code": "${data.discount_code}",
      "items": "${form.selected_items}"
    }
  }
}
```

### `update_data` Action (v6.0+)

Updates the current screen's state based on user interaction without navigating away.

**Use cases:**
- Immediate updates in response to user input (e.g., selecting a country updates the state dropdown)
- Dynamic data handling where relationships are defined in component `data-source`
- Reusable templates that repopulate with different data

### `open_url` Action (v6.0+)

Opens a URL in the device's default web browser. Only supported on `EmbeddedLink` and `OptIn` components.

```json
{
  "type": "EmbeddedLink",
  "text": "View Terms and Conditions",
  "on-click-action": {
    "name": "open_url",
    "url": "https://www.example.com/terms"
  }
}
```

---

## Limitations

- Flow JSON content string cannot exceed **10 MB**
