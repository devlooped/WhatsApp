# Flow JSON Components

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/components

Components are the building blocks for Flow screens. The maximum number of components per screen is **50**.

---

## Available Components

- [Text Components](#text-components) (Heading, Subheading, Body, Caption)
- [RichText](#rich-text) *(v5.1+)*
- [TextInput / TextArea](#text-entry-components)
- [CheckboxGroup](#checkboxgroup)
- [RadioButtonsGroup](#radiobuttonsgroup)
- [Footer](#footer)
- [OptIn](#optin)
- [Dropdown](#dropdown)
- [EmbeddedLink](#embedded-link)
- [DatePicker](#datepicker)
- [CalendarPicker](#calendarpicker) *(v6.1+)*
- [Image](#image)
- [If](#if) *(v4.0+)*
- [Switch](#switch)
- [PhotoPicker / DocumentPicker](./media-upload.md)
- [NavigationList](#navigationlist)
- [ChipsSelector](#chips-selector)
- [ImageCarousel](#image-carousel)

---

## Text Components

### Heading

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextHeading"` |
| `text` | string | ✅ | Supports dynamic: `"${data.text}"` |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |

### Subheading

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextSubheading"` |
| `text` | string | ✅ | Supports dynamic: `"${data.text}"` |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |

### Body

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextBody"` |
| `text` | string | ✅ | Supports dynamic: `"${data.text}"` |
| `font-weight` | enum | — | `bold`, `italic`, `bold_italic`, `normal`. Supports dynamic. |
| `strikethrough` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `markdown` | boolean | — | Default: `false`. Enable markdown support. *(v5.1+)* |

### Caption

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextCaption"` |
| `text` | string | ✅ | Supports dynamic: `"${data.text}"` |
| `font-weight` | enum | — | `bold`, `italic`, `bold_italic`, `normal`. Supports dynamic. |
| `strikethrough` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `markdown` | boolean | — | Default: `false`. Enable markdown support. *(v5.1+)* |

### Character Limits

| Component | Character Limit |
|-----------|----------------|
| Heading | 80 |
| Subheading | 80 |
| Body | 4096 |
| Caption | 409 |

> Empty or blank values are not accepted for any text component.

### Markdown Support (v5.1+)

When `markdown: true` is set on `TextBody` or `TextCaption`:

```json
{
  "type": "TextBody",
  "markdown": true,
  "text": "This text is ~~***really important***~~"
}
```

---

## Rich Text

> Supported from **Flow JSON v5.1+**

`RichText` provides full markdown rendering for large text content (Terms of Service, policy documents, etc.).

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"RichText"` |
| `text` | string \| string[] | ✅ | Markdown content. Supports dynamic: `"${data.text}"` |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |

> **Until v6.2:** RichText must be used as a standalone component (no other components on the screen).  
> **From v6.3:** RichText can be combined with a Footer on the same screen.

### Supported Markdown Syntax

| Syntax | RichText | TextBody | TextCaption |
|--------|----------|----------|-------------|
| `# Heading 1` | ✅ | ❌ | ❌ |
| `## Heading 2` | ✅ | ❌ | ❌ |
| `**bold**` | ✅ | ✅ | ✅ |
| `*italic*` | ✅ | ✅ | ✅ |
| `~~strikethrough~~` | ✅ | ✅ | ✅ |
| Normal paragraph | ✅ | ✅ | ✅ |
| Unordered list (`- Item`, `+ Item`) | ✅ | ✅ | ✅ |
| Ordered list (`1. Item`) | ✅ | ✅ | ✅ |
| `[Link text](https://url)` | ✅ | ✅ | ✅ |
| `![Image](data:image/png;base64,...)` | ✅ | ❌ | ❌ |
| Markdown tables | ✅ | ❌ | ❌ |

> External image URIs are not supported — only base64 inline images.

**Table example:**
```json
{
  "type": "RichText",
  "text": [
    "| Header 1      | Header 2     |",
    "| ---           | ---          |",
    "| **Bold** text | [Link](URI)  |"
  ]
}
```

---

## Text Entry Components

### TextInput

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextInput"` |
| `name` | string | ✅ | Field name for form binding |
| `label` | string | ✅ | Supports dynamic. |
| `label-variant` | string | — | `"large"` for a more prominent label style. *(v7.0+)* |
| `input-type` | enum | — | `text`, `number`, `email`, `password`, `passcode`, `phone` |
| `pattern` | string | — | Regex validation pattern. `helper-text` is mandatory when used. *(v6.2+)* |
| `required` | boolean | — | Supports dynamic. |
| `min-chars` | string | — | Minimum character count. Supports dynamic. |
| `max-chars` | string | — | Maximum character count. Default: `80`. Supports dynamic. |
| `helper-text` | string | — | Hint text below input. Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `init-value` | string | — | Initial value (outside Form only). *(v4.0+)* |
| `error-message` | string | — | Custom error message (outside Form only). *(v4.0+)* |

### TextArea

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"TextArea"` |
| `name` | string | ✅ | Field name for form binding |
| `label` | string | ✅ | Supports dynamic. |
| `label-variant` | string | — | `"large"` for prominent label style. *(v7.0+)* |
| `required` | boolean | — | Supports dynamic. |
| `max-length` | string | — | Default: `600`. Supports dynamic. |
| `helper-text` | string | — | Hint text. Supports dynamic. |
| `enabled` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `init-value` | string | — | Initial value (outside Form only). *(v4.0+)* |
| `error-message` | string | — | Custom error message (outside Form only). *(v4.0+)* |

### Limits

| Component | Helper Text | Error Text | Label |
|-----------|-------------|------------|-------|
| TextInput | 80 chars | 30 chars | 20 chars |
| TextArea | 80 chars | — | 20 chars |

---

## CheckboxGroup

Allows users to select multiple options from a list.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"CheckboxGroup"` |
| `name` | string | ✅ | Field name for form binding |
| `label` | string | ✅ (v4.0+) | Group label. Supports dynamic. |
| `data-source` | array | ✅ | Array of options. See format below. |
| `min-selected-items` | integer | — | Supports dynamic. |
| `max-selected-items` | integer | — | Supports dynamic. |
| `enabled` | boolean | — | Supports dynamic. |
| `required` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `description` | string | — | *(v4.0+)* Supports dynamic. |
| `media-size` | enum | — | `regular` or `large`. *(v5.0+)* |
| `on-select-action` | action | — | `data_exchange` or `update_data`. *(v6.0+)* |
| `on-unselect-action` | action | — | `update_data` only. *(v6.0+)* |
| `init-value` | Array\<string\> | — | Outside Form only. *(v4.0+)* |
| `error-message` | string | — | Outside Form only. *(v4.0+)* |

**`data-source` item format:**

| Version | Fields |
|---------|--------|
| Before v5.0 | `id`, `title`, `description`, `metadata`, `enabled` |
| v5.0+ | + `image` (base64), `alt-text`, `color` (6-digit hex) |
| v6.0+ | + `on-select-action`, `on-unselect-action` |

### Limits

| Type | Limit |
|------|-------|
| Label | 30 chars |
| Title | 30 chars |
| Description | 300 chars |
| Metadata | 20 chars |
| Min options | 1 |
| Max options | 20 |
| Image (before v6.0) | 300 KB |
| Image (v6.0+) | 100 KB |

> WEBP images are not supported on iOS versions prior to iOS 14.

---

## RadioButtonsGroup

Allows users to select a single option.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"RadioButtonsGroup"` |
| `name` | string | ✅ | Field name for form binding |
| `label` | string | ✅ (v4.0+) | Group label. Supports dynamic. |
| `data-source` | array | ✅ | Same format as CheckboxGroup |
| `enabled` | boolean | — | Supports dynamic. |
| `required` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. Supports dynamic. |
| `description` | string | — | *(v4.0+)* Supports dynamic. |
| `media-size` | enum | — | `regular` or `large`. *(v5.0+)* |
| `on-select-action` | action | — | `data_exchange` or `update_data`. *(v6.0+)* |
| `on-unselect-action` | action | — | `update_data` only. *(v6.0+)* |
| `init-value` | string | — | Outside Form only. *(v4.0+)* |
| `error-message` | string | — | Outside Form only. *(v4.0+)* |

Same limits apply as CheckboxGroup.

---

## Footer

The primary action button at the bottom of the screen. Required on terminal screens.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"Footer"` |
| `label` | string | ✅ | Button text. Supports dynamic. |
| `on-click-action` | action | ✅ | Action to trigger on click |
| `left-caption` | string | — | Left caption text. Cannot combine with `center-caption`. |
| `center-caption` | string | — | Center caption text. Cannot combine with `left-caption`/`right-caption`. |
| `right-caption` | string | — | Right caption text. Cannot combine with `center-caption`. |
| `enabled` | boolean | — | Supports dynamic. |

### Limits

| Type | Limit |
|------|-------|
| Label | 35 chars |
| Captions | 15 chars each |

---

## OptIn

A checkbox for user consent or opt-in.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"OptIn"` |
| `name` | string | ✅ | Field name |
| `label` | string | ✅ | Opt-in text. Supports dynamic. |
| `required` | boolean | — | Supports dynamic. |
| `on-click-action` | action | — | Shows "Read more" when set. Allowed: `data_exchange`, `navigate`, `open_url` (v6.0+) |
| `on-select-action` | action | — | `update_data` only. *(v6.0+)* |
| `on-unselect-action` | action | — | `update_data` only. *(v6.0+)* |
| `visible` | boolean | — | Default: `true`. |
| `init-value` | boolean | — | Outside Form only. *(v4.0+)* |

### Limits

| Type | Limit |
|------|-------|
| Content | 120 chars |
| Max OptIns per screen | 5 |

---

## Dropdown

A select component for choosing a single option from a list.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"Dropdown"` |
| `name` | string | ✅ | Field name |
| `label` | string | ✅ | Dropdown label. Supports dynamic. |
| `data-source` | array | ✅ | Same format as CheckboxGroup |
| `required` | boolean | — | Supports dynamic. |
| `enabled` | boolean | — | Supports dynamic. |
| `visible` | boolean | — | Default: `true`. |
| `on-select-action` | action | — | `data_exchange` or `update_data`. *(v6.0+)* |
| `on-unselect-action` | action | — | `update_data` only. *(v6.0+)* |
| `init-value` | string | — | Outside Form only. |
| `error-message` | string | — | Outside Form only. |

### Limits

| Type | Limit |
|------|-------|
| Label | 20 chars |
| Title | 30 chars |
| Description | 300 chars |
| Metadata | 20 chars |
| Min options | 1 |
| Max options (no images) | 200 |
| Max options (with images) | 100 |
| Image (before v6.0) | 300 KB |
| Image (v6.0+) | 100 KB |

---

## Embedded Link

An inline text link.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"EmbeddedLink"` |
| `text` | string | ✅ | Link text. Supports dynamic. |
| `on-click-action` | action | ✅ | `data_exchange`, `navigate`, or `open_url` (v6.0+) |
| `visible` | boolean | — | Default: `true`. |

### Limits

| Type | Limit |
|------|-------|
| Character limit | 25 chars |
| Max per screen | 2 |

---

## DatePicker

An interactive date selection component.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"DatePicker"` |
| `name` | string | ✅ | Field name |
| `label` | string | ✅ | Supports dynamic. |
| `min-date` | string | — | Timestamp (ms) in v<5.0; `YYYY-MM-DD` string in v5.0+. |
| `max-date` | string | — | Timestamp (ms) in v<5.0; `YYYY-MM-DD` string in v5.0+. |
| `unavailable-dates` | array | — | Dates to disable. |
| `helper-text` | string | — | Hint text. Supports dynamic. |
| `enabled` | boolean | — | Default: `true`. |
| `visible` | boolean | — | Default: `true`. |
| `on-select-action` | action | — | `data_exchange` only. |
| `init-value` | string | — | Outside Form only. *(v4.0+)* |
| `error-message` | string | — | Outside Form only. *(v4.0+)* |

> **Before v5.0:** DatePicker uses UTC timestamps in milliseconds and only works reliably when business and user are in the **same time zone**.  
> **v5.0+:** DatePicker uses `"YYYY-MM-DD"` strings — timezone-independent.

### Limits

| Type | Limit |
|------|-------|
| Label | 40 chars |
| Helper Text | 80 chars |
| Error Message | 80 chars |

---

## CalendarPicker

> Supported from **Flow JSON v6.1+**

Full-calendar date picker supporting single date or date range selection.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"CalendarPicker"` |
| `name` | string | ✅ | Field name |
| `label` | string | ✅ | In `range` mode: `{"start-date": string, "end-date": string}`. |
| `mode` | enum | — | `single` (default) or `range` |
| `title` | string | — | `range` mode only. |
| `description` | string | — | `range` mode only. |
| `helper-text` | string | — | In `range` mode: `{"start-date": string, "end-date": string}`. |
| `required` | boolean | — | In `range` mode: `{"start-date": boolean, "end-date": boolean}`. Default: `false`. |
| `min-date` | string | — | `YYYY-MM-DD` format. |
| `max-date` | string | — | `YYYY-MM-DD` format. |
| `unavailable-dates` | Array\<string\> | — | `YYYY-MM-DD` strings. Must be within min/max range. |
| `include-days` | Array\<enum\> | — | `Mon`, `Tue`, `Wed`, `Thu`, `Fri`, `Sat`, `Sun`. Default: all days. |
| `min-days` | integer | — | `range` mode only — minimum days between start and end. |
| `max-days` | integer | — | `range` mode only — maximum days between start and end. |
| `visible` | boolean | — | Default: `true`. |
| `enabled` | boolean | — | Default: `true`. |
| `on-select-action` | action | — | `data_exchange` only. Payload: `"YYYY-MM-DD"` (single) or `{"start-date":"...","end-date":"..."}` (range). |
| `init-value` | string | — | Outside Form only. |
| `error-message` | string | — | Outside Form only. |

### Limits

| Type | Limit |
|------|-------|
| Title | 80 chars |
| Description | 300 chars |
| Label | 40 chars |
| Helper Text | 80 chars |
| Error Message | 80 chars |

---

## Image

Displays an image on the screen.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"Image"` |
| `src` | string | ✅ | Base64 encoded image. Supports dynamic: `"${data.src}"`. |
| `width` | integer | — | Supports dynamic. |
| `height` | integer | — | Supports dynamic. |
| `scale-type` | string | — | `cover` or `contain`. Default: `contain`. |
| `aspect-ratio` | number | — | Default: `1`. Supports dynamic. |
| `alt-text` | string | — | Accessibility text. Supports dynamic. |

### Scale Types

| Scale Type | Description |
|------------|-------------|
| `contain` | Image fits within the container preserving aspect ratio. |
| `cover` | Image fills the container, cropping as needed. |

> On Android, WhatsApp defaults to a height of 400 when none is set — consider specifying explicit dimensions.

### Limits

| Type | Limit |
|------|-------|
| Max images per screen | 3 |
| Recommended image size | Up to 300 KB |
| Total data channel payload | 1 MB |
| Supported formats | JPEG, PNG |

---

## If

> Supported from **Flow JSON v4.0+**

Conditional rendering — shows different components based on a boolean expression.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"If"` |
| `condition` | string | ✅ | Boolean expression using dynamic/static data |
| `then` | Array\<Component\> | ✅ | Components rendered when condition is `true` |
| `else` | Array\<Component\> | — | Components rendered when condition is `false` |

> Nesting is allowed up to **3 levels** deep.

### Supported Operators

| Operator | Symbol | Types | Returns |
|----------|--------|-------|---------|
| Parentheses | `()` | any | (grouping) |
| Equal to | `==` | boolean, number, string | boolean |
| Not equal to | `!=` | boolean, number, string | boolean |
| Less than | `<` | number | boolean |
| Less than or equal | `<=` | number | boolean |
| Greater than | `>` | number | boolean |
| Greater than or equal | `>=` | number | boolean |
| Logical AND | `&&` | boolean | boolean |
| Logical OR | `\|\|` | boolean | boolean |
| Logical NOT | `!` | boolean | boolean |

**Examples:**
```json
{ "condition": "${form.opt_in} == true" }
{ "condition": "${data.age} >= 18" }
{ "condition": "${form.opt_in} && (${form.address} != '')" }
```

---

## Switch

Renders one of multiple sets of components based on a matched value (similar to a `switch/case` statement). Refer to the official docs for detailed syntax.

---

## NavigationList

Displays a list of items that users can tap to navigate. Refer to the official docs for detailed syntax.

---

## Chips Selector

A compact multi-select or single-select component using chip-style buttons. *(v7.1+)* Refer to the official docs for detailed syntax.

---

## Image Carousel

Displays a horizontally scrollable set of images. Refer to the official docs for detailed syntax.
