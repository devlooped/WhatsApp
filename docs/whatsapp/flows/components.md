[WhatsApp Flows](https://developers.facebook.com/docs/whatsapp/flows)

# Components

Components are like building blocks. They allow you to build complex UIs and display business data using attribute models. **The maximum number of components per screen is 50.** Please refer to [best practices for components](https://developers.facebook.com/docs/whatsapp/extensions/bestpractices#number-of-components).

The following components are supported:

* [Basic Text (Heading, Subheading, Caption, Body)](#text)
* [RichText](#richtext)
* [TextEntry](#textentry)
* [CheckboxGroup](#checkbox)
* [RadioButtonsGroup](#radio)
* [Footer](#foot)
* [OptIn](#opt)
* [Dropdown](#drop)
* [EmbeddedLink](#embed)
* [DatePicker](#dp)

* [CalendarPicker](#calendarpicker)

* [Image](#img)
* [If](#if)
* [Switch](#switch)
* [Media upload](#media_upload)

* [NavigationList](#navlist)

* [Chips Selector](#chips_selector)

* [Image Carousel](#image_carousel)

## Text Components

### Heading

This is the top level title of a page.

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "TextHeading" |
| `text` (required) string | Dynamic "${data.text}" |
| `visible` Boolean | Dynamic "${data.is_visible}"  Default: True |

### Subheading

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "TextSubheading" |
| `text` (required) string | Dynamic "${data.text}" |
| `visible` Boolean | Dynamic "${data.is_visible}"  Default: True |

### Body

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | `TextBody` |
| `text` (required) string | Dynamic "${data.text}" |
| `font-weight` enum | {'bold','italic','bold_italic','normal'} Dynamic "${data.font_weight}" |
| `strikethrough` Boolean | Dynamic "${data.strikethrough}" |
| `visible` Boolean | Dynamic "${data.is_visible}"  Default: True |
| `markdown` Boolean | Default: False |

### Caption

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "TextCaption" |
| `text` (required) string | Dynamic "${data.text}" |
| `font-weight` enum | {'bold','italic','bold_italic','normal'} Dynamic "${data.font_weight}" |
| `strikethrough` Boolean | Dynamic "${data.strikethrough}" |
| `visible` Boolean | Dynamic "${data.is_visible}"  Default: True |
| `markdown` Boolean | Default: False |

### Limits and Restrictions

| Component  | Type            | Limit / Restriction |
|------------|-----------------|---------------------|
| Heading    | Character Limit | 80 |
| Subheading | Character Limit | 80 |
| Body       | Character Limit | 4096 |
| Caption    | Character Limit | 409 |
| Heading / Subheading / Body / Caption | Text | Empty or blank value is not accepted |

### Additional capabilities for Text components

`TextBody` and `TextCaption` support limited markdown when `"markdown": true`.

```
{
   "type": "TextBody",
   "markdown": true,
   "text": [
     "This text is ~~***really important***~~",
   ]
}
```

```
{
   "type": "TextCaption",
   "markdown": true,
   "text": [
     "This text is ~~***really important***~~",
   ]
}
```

For comparison purposes, we show how the text components look like next to one another:

## Rich Text

`RichText` provides rich formatting capabilities and renders large texts (Terms & Conditions, Policy Documents, User Agreements, etc.) beyond the limits of basic text components.

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "RichText" |
| `text` (required) string / string[] | Dynamic "${data.text}" |
| `visible` Boolean | Dynamic "${data.is_visible}"  Default: True |

`RichText` component utilizes a select subset of the `Markdown` specification. It adheres strictly to standard `Markdown` syntax without introducing any custom modifications. Content created for the `RichText` component is fully compatible with standard `Markdown` documents.

**Note:** `RichText` can appear with a `Footer` on the same screen. For shorter mixed text needs, consider basic text components.

### Supported Syntax

#### Headings

The current syntax supports only `Heading (h1)` and `Subheading (h2)`. Other heading levels will be parsed but rendered as normal text - `TextBody`.

| Flow JSON Example | Flow Component |
|-------------------|----------------|
| `{ "type": "RichText", "text": [ "# Heading level 1" ] }` | `TextHeading` |
| `{ "type": "RichText", "text": [ "## Heading level 2" ] }` | `TextSubheading` |
| `{ "type": "RichText", "text": [ "### Heading level 3", "#### Heading level 4", "##### Heading level 5", "###### Heading level 6" ] }` | `TextBody` |

#### Paragraphs

To create paragraphs, split your text into different array items:

```
{
       "type": "RichText",
       "text": [
         "Paragraph 1",
        "Paragraph 2",
       ]
    }
```

or add a blank line in your markdown document that you bind using dynamic binding syntax `${data.your_dynamic_field}`

```

# Heading 1
Paragraph 1

Paragraph 2

```

```
{
       "type": "RichText",
       "text": "${data.text}"
    }
```

#### Text Formatting

| Flow JSON Example | Rendered As |
|-------------------|-------------|
| `{ "type": "RichText", "text": [ "Let’s make a **bold** statement" ] }` | `TextBody (bold)` |
| `{ "type": "RichText", "text": [ "Let's make this text *italic*" ] }` | `TextBody (italic)` |
| `{ "type": "RichText", "text": [ "Let's make this text ~~Strikethrough~~" ] }` | `TextBody (strikethrough)` |
| `{ "type": "RichText", "text": [ "This text is ~~***really important***~~" ] }` | `TextBody (bold-italic-strikethrough)` |

#### Lists

You can organize items into ordered and unordered lists. At the moment, only single level lists are supported.

| Flow JSON Example | Rendered As |
|-------------------|-------------|
| `{ "type": "RichText", "text": [ "1. Item 1", "2. Item 2", "3. Item 3" ] }` | `OrderedList` (inline within RichText) |
| `{ "type": "RichText", "text": [ "- Item 1", "- Item 2", "- Item 3" ] }` | `UnorderedList` (inline within RichText) |
| `{ "type": "RichText", "text": [ "+ Item 1", "+ Item 2", "+ Item 3" ] }` | `UnorderedList` (inline within RichText) |

#### Images

You can also include images in the content. Please note, external URIs are not supported and you can only include base64 inline images

```
{
   "type": "RichText",
   "text": ["![Image alt text](data:image/png;base64,<base64 content>)"]
}
```

**Recommended image formats:**

1. png
2. jpg / jpeg
3. webp (please note, webp is only supported starting from IOS 14.6+, that corresponds to ~98% of IOS devices)

#### Links

To create a link, enclose the link text in brackets and then follow it immediately with the URL in parentheses

```
{
   "type": "RichText",
   "text": [
     "[Whatsapp Flows are awesome](https://business.whatsapp.com/products/whatsapp-flows)",
   ]
}
```

#### Tables

To add a table, use three or more hyphens (---) to create each column’s header, and use pipes (|) to separate each column. For compatibility, you should also add a pipe on either end of the row.

Cell content can be combined with the following syntax:

1. Italic, bold, strikethrough
2. Images
3. Links

```
{
   "type": "RichText",
   "text": [
     "| Column Header 1     | Column Header 2                                             |",
     "| -------------       |  -------------                                              |",
     "| **Bold** text 1     | [Link](<URI>)                                               |",
     "| **Bold** text 1     | ![Image alt text](data:image/png;base64,<base64 content>)   |",
   ]
}
```

**Width of the columns:**

Width of the column is based on the Header content size. Markdown specification doesn’t provide a specific syntax for controlling a column width. If you want to make a certain column wider, simply add additional content to the header:

```
{
   "type": "RichText",
   "text": [
     "| Column Header 1 - Extended width  | Column Header 2       |",
     "| -------------                     |  -------------        |",
     "| **Bold** text 1                   | Cell text 2           |",
   ]
}
```

#### Working with large texts

If your text content for markdown has a limited size, you can incorporate it as a static text as shown in all examples above, however if your text is large and you expect to update it often on your server, we recommend sending it as a part of dynamic data, this will improve overall readability of the JSON and allow to load always up to date text from your server.

**Please note:** We use array text property for static cases since it’s easier to read. However the components support both types: `Array of strings` and `string`. Your markdown can be sent as a normal string, you don’t need to convert it to an array of strings.

#### Syntax cheatsheet

Overview of supported syntax across `RichText`, `TextBody` and `TextCaption` components.

| Syntax | RichText | TextBody | TextCaption |
|--------|----------|---------|-------------|
| `# Text Heading` | ✅ | ❌ | ❌ |
| `## Text Subheading` | ✅ | ❌ | ❌ |
| `**bold**` | ✅ | ✅ | ✅ |
| `*italic*` | ✅ | ✅ | ✅ |
| `~~strikethrough~~` | ✅ | ✅ | ✅ |
| Normal paragraph | ✅ | ✅ | ✅ |
| Ordered list (`1. Item`) | ✅ | ✅ | ✅ |
| Unordered list (`- Item` / `+ Item`) | ✅ | ✅ | ✅ |
| `[Link text](https://your-url.here)` | ✅ | ✅ | ✅ |
| `![Image Alt](data:image/png;base64,...)` | ✅ | ❌ | ❌ |
| Tables | ✅ | ❌ | ❌ |

#### Usage example

## Text Entry Components

### TextInput

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "TextInput" |
| `label` (required) string | Dynamic "${data.label}" |
| `label-variant` string | "large" (prominent multi-line style) |
| `input-type` enum | {'text','number','email','password','passcode','phone'} |
| `pattern` String | Regex the value must match. Requires helper-text. Raw regex (no / /). Applies alongside base validator for number/passcode. |
| `required` Boolean | Dynamic "${data.is_required}" |
| `min-chars` String | Dynamic "${data.min_chars}" |
| `max-chars` String | Dynamic "${data.max_chars}" (Default 80) |
| `helper-text` String | Dynamic "${data.helper_text}" |
| `name` (required) String |  |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `init-value` String | Dynamic "${data.init-value}" (Outside Form only) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form only) |

### TextArea

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "TextArea" |
| `label` (required) string | Dynamic "${data.label}" |
| `label-variant` string | "large" |
| `required` Boolean | Dynamic "${data.is_required}" |
| `max-length` String | Dynamic "${data.max_length}" (Default 600) |
| `name` (required) String |  |
| `helper-text` String | Dynamic "${data.helper_text}" |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `init-value` String | Dynamic "${data.init-value}" (Outside Form only) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form only) |

### Limits and Restrictions

| Component  | Item        | Limit |
|------------|-------------|-------|
| TextInput  | Helper Text | 80 characters |
| TextInput  | Error Text  | 30 characters |
| TextInput  | Label       | 20 characters |
| TextArea   | Helper Text | 80 characters |
| TextArea   | Label       | 20 characters |

Together, the text entry components look like as shown:

## CheckboxGroup

CheckboxGroup component allows users to pick multiple selections from a list of options.

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "CheckboxGroup" |
| `data-source` (required) Array | Dynamic "${data.data_source}" (Array< id, title, description, metadata, enabled, image, alt-text, color, on-select-action, on-unselect-action >) |
| `name` (required) String |  |
| `min-selected-items` Integer | Dynamic "${data.min_selected_items}" |
| `max-selected-items` Integer | Dynamic "${data.max_selected_items}" |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `label` string | Dynamic "${data.label}" |
| `required` Boolean | Dynamic "${data.is_required}" |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `on-select-action` Action | `data_exchange`, `update_data` |
| `on-unselect-action` Action | `update_data` (if absent, on-select handles both) |
| `description` String | Dynamic "${data.description}" |
| `init-value` Array<String> | Dynamic "${data.init-value}" (Outside Form) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form) |
| `media-size` enum | {'regular','large'} Dynamic "${data.media-size}" |

Images in WEBP format are not supported on iOS versions prior to iOS 14.

### Example

For the `data-source` field, you can declare it dynamically or statically.

### Static Example

This static example hardcodes the respective `id`'s and `title`'s for the `data-source` field.

#### Dynamic Example

In this dynamic example, you can see that `data-source` references the `days_per_week_options` of type `array` defined before it using `days_per_week_options`. When defining such a structure, you need to specify `items` in the `array`, which will be of type `object`. Then inside the `items` object, you have a `properties` dictionary with `id` and `title` just like in the static declaration. Both `id` and `title` will always be of type `String`. Within the `days_per_week_options` array, you must define concrete examples in the `__example__` field.

### Limits and Restrictions

| Field          | Limit |
|----------------|-------|
| Label Content  | 30 Characters |
| Title          | 30 Characters |
| Description    | 300 Characters |
| Metadata       | 20 Characters |
| Min # options  | 1 |
| Max # options  | 20 |
| Image size     | 100KB |

## RadioButtonsGroup

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "RadioButtonsGroup" |
| `data-source` (required) Array | Dynamic "${data.data_source}" (Array< id, title, description, metadata, enabled, image, alt-text, color, on-select-action, on-unselect-action >) |
| `name` (required) String |  |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `label` string | Dynamic "${data.label}" |
| `required` Boolean | Dynamic "${data.is_required}" |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `on-select-action` Action | `data_exchange`, `update_data` |
| `on-unselect-action` Action | `update_data` |
| `description` String | Dynamic "${data.description}" |
| `init-value` Array<String> | Dynamic "${data.init-value}" (Outside Form) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form) |
| `media-size` enum | {'regular','large'} Dynamic "${data.media-size}" |

Images in WEBP format are not supported on iOS versions prior to iOS 14.

### Example

For the `data-source` field, you can declare it dynamically or statically.

### Static Example

This static example hardcodes the respective `id`'s and `title`'s for the `data-source` field.

### Dynamic Example

In this dynamic example, you can see that `data-source` references the `experience_level_options` of type `array` defined before it using `data.experience_level_options`. When defining such a structure, you need to specify `items` in the `array`, which will be of type `object`. Then inside the `items` object, you have a `properties` dictionary with `id` and `title` just like in the static declaration. Both `id` and `title` will always be of type `String`. Within in the `experience_level_options` array you must define concrete examples in the `__example__` field.

### Limits and Restrictions

| Field          | Limit |
|----------------|-------|
| Label Content  | 30 Characters |
| Title          | 30 Characters |
| Description    | 300 Characters |
| Metadata       | 20 Characters |
| Min # options  | 1 |
| Max # options  | 20 |
| Image size     | 100KB |

## Footer

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "Footer" |
| `label` (required) string | Dynamic "${data.label}" |
| `left-caption` String | Dynamic "${data.left_caption}" (Mutually exclusive with center-caption) |
| `center-caption` String | Dynamic "${data.center_caption}" (Mutually exclusive with left+right) |
| `right-caption` String | Dynamic "${data.right_caption}" (Mutually exclusive with center-caption) |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `on-click-action` (required) Action | Action |

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Label Max Characters | 35 |
| Caption Max Characters | 15 |

## OptIn

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "OptIn" |
| `label` (required) string | Dynamic "${data.label}" |
| `required` Boolean | Dynamic "${data.is_required}" |
| `name` (required) String |  |
| `on-click-action` Action | Executes on "Read more". Allowed: `data_exchange`,`navigate`,`open_url` |
| `on-select-action` Action | `update_data` |
| `on-unselect-action` Action | `update_data` |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `init-value` Boolean | Dynamic "${data.init-value}" (Outside Form) |

### Example

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Content Max Characters | 120 |
| Max Opt-Ins Per Screen | 5 |

## Dropdown

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "Dropdown" |
| `label` (required) string |  |
| `data-source` (required) Array | Dynamic "${data.data_source}" (Array< id, title, description, metadata, enabled, image, alt-text, color, on-select-action, on-unselect-action >) |
| `required` Boolean | Dynamic "${data.is_required}" |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `on-select-action` Action | `data_exchange`, `update_data` |
| `on-unselect-action` Action | `update_data` |
| `init-value` String | Dynamic "${data.init-value}" (Outside Form) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form) |

Images in WEBP format are not supported on iOS versions prior to iOS 14.

### Example

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Label | 20 characters |
| Title | 30 characters |
| Min dropdown options | 1 |
| Max dropdown options | 200 (no images) / 100 (with images) |
| Description | 300 characters |
| Metadata | 20 characters |
| Image size | 100KB |

For the `data-source` field, you can declare it dynamically or statically.

#### Static Example

This static example hardcodes the respective `id`'s and `title`'s for the `data-source` field.

### Dynamic Example

In this dynamic example, you can see that `data-source` references the `experience_level_options` of type `array` defined before it using `experience_level_options`. When defining such a structure, you need to specify `items` in the `array`, which will be of type `object`. Then inside the `items` object, you have a `properties` dictionary with `id` and `title` just like in the static declaration. Both `id` and `title` will always be of type `String`. Within the `experience_level_options` array you must define concrete examples in the `__example__` field.

## Embedded Link

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "EmbeddedLink" |
| `text` (required) string | Dynamic "${data.text}" |
| `on-click-action` (required) Action | Allowed: `data_exchange`,`navigate`,`open_url` |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |

### Limits and Restrictions

| Field | Limit / Restriction |
|-------|---------------------|
| Character limit | 25 |
| Case | No restriction on formatting |
| Max embedded links per screen | 2 |
| Text | Empty or blank value not accepted |

## DatePicker

The DatePicker component allows users to input dates through an intuitive date selection interface.

Before Flow JSON version 5.0, the DatePicker doesn't support scenarios where the business and the end user are in different
time zones. We recommend only using the component if you plan to send your Flows to users in a specific
timezone. For details, please refer to section
[Guidelines for Usage](#datepicker-guidelines)

DatePicker uses a date-only string value "YYYY-MM-DD" (e.g. "2024-10-21") for min, max, unavailable, init and selected values. Treat these as pure calendar dates (no time zone arithmetic required).

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "DatePicker" |
| `label` (required) string | Dynamic "${data.label}" |
| `min-date` String | Dynamic "${data.min_date}" (YYYY-MM-DD) |
| `max-date` String | Dynamic "${data.max_date}" (YYYY-MM-DD) |
| `name` (required) string |  |
| `unavailable-dates` Array<String> | Dynamic "${data.unavailable_dates}" (YYYY-MM-DD) |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `helper-text` String | Dynamic "${data.helper_text}" |
| `enabled` Boolean | Dynamic "${data.is_enabled}" Default: True |
| `on-select-action` Action | `data_exchange` only |
| `init-value` String | Dynamic "${data.init-value}" (Outside Form) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form) |

Payload sent via on-select action is the selected date string (YYYY-MM-DD).

### Guidelines for Usage

Specify any of: min-date, max-date, and unavailable-dates using YYYY-MM-DD. If no min/max provided the selectable range defaults to 1900-01-01 .. 2100-12-31. Values are always date strings; do not supply or convert timestamps.

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Label Max Length | 40 characters |
| Helper Text Max Length | 80 characters |
| Error Message Max Length | 80 characters |

## CalendarPicker

The CalendarPicker component allows users to select a single date or a range of dates from a full calendar interface.

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) String | "CalendarPicker" |
| `name` (required) String |  |
| `title` String | Dynamic "${data.title}" (range mode only) |
| `description` String | Dynamic "${data.description}" (range mode only) |
| `label` (required) String | Dynamic "${data.label}" (range mode: JSON {"start-date":String,"end-date":String}) |
| `helper-text` String | Dynamic "${data.helper_text}" (range mode: same JSON shape) |
| `required` Boolean | Dynamic "${data.is_required}" Default: False (range mode JSON shape variant) |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `enabled` Boolean | Dynamic "${data.is_enabled}" Default: True |
| `mode` enum | {"single","range"} Dynamic "${data.mode}" Default: "single" |
| `min-date` String | Dynamic "${data.min_date}" (YYYY-MM-DD) |
| `max-date` String | Dynamic "${data.max_date}" (YYYY-MM-DD) |
| `unavailable-dates` Array<String> | Dynamic "${data.unavailable_dates}" (YYYY-MM-DD) |
| `include-days` Array<enum> | {Mon..Sun} Dynamic "${data.include_days}" Default all |
| `min-days` Integer | Dynamic "${data.min_days}" (range mode) |
| `max-days` Integer | Dynamic "${data.max_days}" (range mode) |
| `on-select-action` Action | `data_exchange` only. Payload: string (single) or {start-date,end-date} (range) |
| `init-value` String | Dynamic "${data.init-value}" (range: JSON shape) Outside Form |
| `error-message` String | Dynamic "${data.error-message}" (range: JSON shape) Outside Form |

### Examples

#### CalendarPicker single mode example

#### CalendarPicker range mode example

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Title Max Length | 80 characters |
| Description Max Length | 300 characters |
| Label Max Length | 40 characters |
| Helper Text Max Length | 80 characters |
| Error Message Max Length | 80 characters |

## Image

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "Image" |
| `src` (required) string | Base64 image. Dynamic "${data.src}" |
| `width` Integer | Dynamic "${data.width}" |
| `height` Integer | Dynamic "${data.height}" |
| `scale-type` string | `cover` or `contain` (Default `contain`) |
| `aspect-ratio` Number | Dynamic "${data.aspect_ratio}" (Default 1) |
| `alt-text` string | Accessibility alt text. Dynamic "${data.alt_text}" |

### Image Scale Types

| Scale Type | Description |
|------------|-------------|
| `cover` | Image clipped to fit container. Full width if no height. Cropped within fixed height maintaining aspect until clipped. |
| `contain` | Image contained within container preserving aspect ratio. Consider specifying dimensions; Android may default height to 400 causing spacing. |

### Example

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Max images per screen | 3 |
| Recommended image size | Up to 300KB |
| Total data channel payload size | 1 MB |
| Supported formats | JPEG, PNG |

## If

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "If" |
| `condition` (required) string | Boolean expression; supports dynamic/static data. |
| `then` (required) Array<Component> | Rendered when condition is true. Allowed components listed below (nest up to 3 If). |
| `else` Array<Component> | Rendered when condition is false. Same allowed components. |

### Supported Operators

| Operator | Symbol | Types | Description / Examples |
|----------|--------|-------|------------------------|
| Parentheses | `()` | boolean, number, string | Define precedence. Examples: `${form.opt_in} || (${data.num_value} > 5)` ; `${form.opt_in} && (${form.address} != '')` ; `!${form.value1}` |
| Equal to | `==` | boolean, number, string | Compare same-type values; one side dynamic. `${data.num_value} == 5` |
| Not equal | `!=` | boolean, number, string | `${form.city} != 'London'` |
| AND | `&&` | boolean | High priority. `${form.opt_in} && ${data.boolean_value}` |
| OR | `||` | boolean | `${form.opt_in} || ${data.boolean_value}` |
| NOT | `!` | boolean | Negates. `!(${data.num_value} > 5)` |
| Greater than | `>` | number | `${data.num_value} > 5` |
| Greater or equal | `>=` | number | `${data.num_value} >= 5` |
| Less than | `<` | number | `${data.num_value} < 5` |
| Less or equal | `<=` | number | `${data.num_value} <= 5` |

### Example

### Rules

#### Condition

* Should have at least one dynamic value (e.g. `${data...}` or `${form...}`).
* Should always be resolved into a boolean (i.e. no strings or number values).
* Can be used with literals but should not only contain literals.

#### Footer

* `Footer` can be added within `If` only in the first level, not inside a nested `If`.
* If there is a `Footer` within `If`, it should exist in both branches (i.e. `then` and `else`). This means that `else` becomes mandatory.
* If there is a `Footer` within `If` it cannot exist a footer outside, because the max count of `Footer` is 1 per screen.

### Limitations and restrictions

The table below show examples of limitations and validation errors that will be shown for certain cases.

| Scenario | Validation error shown |
|----------|------------------------|
| Footer in `then` only (no else) | Missing Footer inside one of the if branches. Branch "else" should exist and contain one Footer. |
| Footer only in `then` (else exists but no footer) | Missing Footer inside one of the if branches. |
| Footer only in `else` | Missing Footer inside one of the if branches. |
| Footers in both branches plus footer outside | You can only have 1 Footer component per screen. |
| Empty `then` array | Invalid value at path .../then due to empty array (must contain at least one component). |

## Switch

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "Switch" |
| `value` (required) string | Variable evaluated at runtime, e.g. `${data.animal}` |
| `cases` (required) Map<string,Component[]> | Map of key to component array. Allowed components listed (may include ChipsSelector). |

### Example

### Rules

#### Cases

* Should have at least one value. It cannot be empty (e.g. `"cases": {}`)

### Limitations and restrictions

The table below show examples of limitations and validation errors that will be shown for certain cases.

Scenario | Validation error shown || * `Given` there is a `Switch` component * `And` its `cases` property is empty * `When` validating the flow * `Then` it should show a validation error | Invalid empty property found at: "$root/screens/path\_to\_your\_component/cases". |

## Media upload

Please refer to the specific page for [media upload components](https://developers.facebook.com/docs/whatsapp/flows/reference/media_upload).

## Navigation List

The NavigationList component allows users to navigate effectively between different screens in a Flow, by exploring and interacting with a list of options. Each list item can display rich content such as text, images and tags.

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "NavigationList" |
| `name` (required) string |  |
| `list-items` (required) array | Dynamic "${data.list_items}" |
| `label` string | Dynamic "${data.label}" |
| `description` string | Dynamic "${data.description}" |
| `media-size` enum | {'regular','large'} Default 'regular' Dynamic "${data.media-size}" |
| `on-click-action` action | `data_exchange` and `navigate` supported |

Each item in the list of items supports the following properties:

| Parámetro | Descripción |
|-----------|-------------|
| `main-content` (required) object | title (req), description, metadata |
| `end` object | title, description, metadata |
| `start` object | image (req, base64), alt-text |
| `badge` string |  |
| `tags` Array<string> |  |
| `on-click-action` action | `data_exchange`, `navigate` |

Images in WEBP format are not supported on iOS versions prior to iOS 14.

The `on-click-action` is required for the component, and it can be defined either:

* Once at component-level and it will apply the same action for all items in the list.
* Individually, on each item in the list to allow for different actions to be triggered.

### Example

### Dynamic Example

In this dynamic example, you can see that `list-items` references the `insurances` of type `array` defined before it using `insurances`. When defining such a structure, you need to specify `items` in the `array`, which will be of type `object`. Then inside the `items` object, you have a `properties` dictionary with `id` and `main-content` just like in the static declaration. Both `id` will always be of type `string` and `main-content` will always be of type `object`, and accompanied by a definition of its structure. Within the `insurances` array, you must define concrete examples in the `__example__` field.

### Limits and Restrictions

* The `Navigation List` component cannot be used on a terminal screen.
* There can be at most 2 `Navigation List` components per screen.
* The `Navigation List` components cannot be used in combination with any other components in the same screen.
* There can be only one item with a `badge` per list.
* The `end` add-on cannot be used in combination with `media-size` set to `large`.
* The `on-click-action` cannot be defined simultaneously on component-level and on item-level.

#### Component restrictions

| Property | Limit / Restriction |
|----------|---------------------|
| list-items | Min 1 / Max 20 items (excess not rendered) |
| label | 80 characters (truncated) |
| description | 300 characters (truncated) |

#### List items restrictions

Content over the specified limits is not rendered.

| Add-on / Section | Property | Limit / Restriction |
|------------------|----------|---------------------|
| start | image | 100KB (over limit replaced by placeholder) |
| main-content | title | 30 characters |
| main-content | description | 20 characters |
| main-content | metadata | 80 characters |
| end | title | 10 characters |
| end | description | 10 characters |
| end | metadata | 10 characters |
| badge | (value) | 15 characters |
| tags | (each) | 15 characters (max 3 items) |

## Chips Selector

Chips Selector component allows users to pick multiple selections from a list of options.

Supported starting with Flow JSON version 6.3

| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "ChipsSelector" |
| `data-source` (required) Array | Dynamic "${data.data_source}" (Array< id, title, enabled, on-select-action, on-unselect-action >) |
| `name` (required) String |  |
| `min-selected-items` Integer | Dynamic "${data.min_selected_items}" |
| `max-selected-items` Integer | Dynamic "${data.max_selected_items}" |
| `enabled` Boolean | Dynamic "${data.is_enabled}" |
| `label` (required) string | Dynamic "${data.label}" |
| `required` Boolean | Dynamic "${data.is_required}" |
| `visible` Boolean | Dynamic "${data.is_visible}" Default: True |
| `description` String | Dynamic "${data.description}" |
| `init-value` Array<String> | Dynamic "${data.init-value}" (Outside Form) |
| `error-message` String | Dynamic "${data.error-message}" (Outside Form) |
| `on-select-action` Action | `data_exchange`,`update_data` |
| `on-unselect-action` Action | `update_data` |

If `on-unselect-action` is not added, `on-select-action` will continue to handle both selection and unselection events. However, if `on-unselect-action` is defined, it will exclusively handle unselection, while `on-select-action` will be used solely for selection.

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Label | 80 Characters |
| Description | 300 Characters |
| Min # options | 2 |
| Max # options | 20 |

### Example

## Image Carousel

The Image Carousel component allows users to slide through multiple images.


| Parámetro | Descripción |
|-----------|-------------|
| `type` (required) string | "ImageCarousel" |
| `images` (required) array | Dynamic "${data.images}" |
| `aspect-ratio` string | "4:3" or "16:9" (Default 4:3) |
| `scale-type` string | "contain" or "cover" (Default contain) |

Each item in the list of images supports the following properties:

| Parámetro | Descripción |
|-----------|-------------|
| `src` (required) string | Base64 image |
| `alt-text` (required) string | Accessibility alt text |

### Limits and Restrictions

| Field | Limit |
|-------|-------|
| Min # images | 1 |
| Max # images | 3 |
| Max carousels per screen | 2 |
| Max carousels per flow | 3 |

### Example

## Dynamic components

Here's a corrected version:

If you check the attribute model of certain components (`Dropdown`, `DatePicker`, `RadioGroup` and `CheckboxGroup`), you will find that some of them accept the `on-xxxx-action` attribute. This attribute allows the component to trigger a data-exchange action. It can be used in the following scenarios:

1. When a user selects a date in the DatePicker component.
2. When the business needs to fetch available data (such as table slots, tickets, etc.) for this selected date by calling a data\_exchange action.
3. Once the data is received, the user will see an updated screen with new data.

## Prerequisites

The following steps require communication between the client and the business server. Please ensure that you have configured the data channel before attempting to use this feature.

## Step 1 - Defining the layout

Let's begin with a minimal example, consisting of an empty form and a CTA button, and gradually add more components.

So, we want to build a simple form that takes a date and displays the list of available time slots. First, we'll add a `DatePicker` component:

Next step is to add a `Dropdown` where we will display all available timeslots:

## Step 2 - Defining 3P Data

Until now, we've been incorporating static mock data, but now we aim to connect a screen with dynamic data. Dynamic data can originate from various sources:

1. Initial message payload
2. `navigate` - transitioning from the previous screen using a `navigate` action
3. `data_exchange` - a request to the business server

In this example, we'll assume that the data will come from a `data_exchange` request. So, let's instruct Flow JSON to use the data channel request by providing the `"data_api_version": "3.0"` property.

## Step 3 - Allowing DatePicker to Make a Request to the Server

Let's provide `"on-select-action"` to the `DatePicker` component so we can execute the call to the business server. In the `payload`, we can pass any data we want to the business server to understand the type of request.

```
{
   "on-select-action":{
      "name":"data_exchange",
      "payload":{
         "date":"${form.date}",
         "component_action":"update_date"
      }
   }
}
```

In this example, we'll send the value of the field `date` to the action payload, and we'll also add some static data `"component_action": "update_date"` to help the server recognize the type of request. There is no strict format here; you can choose whatever works for your case.

Now when you try to select a date, a `data_exchange` request will be executed. The server may return the data that can change the UI. For now, our Flow doesn't expect or use any data from the server. Let's fix it by first defining the data model that we expect for a screen.

## Step 4 - Define a Server Data Model

Let's declare a `data` property for the screen outlining the data that we expect to receive from the server. So, we want to receive an `available_slots` array with timeslot options.

It should have the following model. The `__example__` field is mock data used to display the data within the web preview.

```
{
    "available_slots": {
        "type": "array",
        "items": {
            "type": "object",
            "properties": { "id": {"type": "string"}, "title": {"type": "string"} }
        },
        "__example__": [ {"id": "1", "title": "08:00"}, {"id": "2", "title": "09:00"} ]
    }
}
```

It means that the expected payload to be returned from server can look like the following:

```
{
    "version": "3.0",
    "screen": "BOOKING",
    "data": {
       "available_slots": [ {"id": "1", "title": "08:00"}, {"id": "2", "title": "09:00"} ]
    }
}
```

So you Flow JSON now should look like the following:

## Step 5 - Control Visibility of the Component

Now, when we select a date in `DatePicker`, the application will send a request to the business server to get available timeslots. However, we don't want a `Dropdown` to be visible until there is data to display. How can we hide it?

For this purpose, we can use the `visible` attribute on `Dropdown` and connect it with server data. The business server can control the visibility of the component based on a set condition.

So, we need to make the following changes:

1. Define `is_dropdown_visible` in the `data` model of the screen.
2. Connect a property via dynamic binding `"visible": "${data.is_dropdown_visible}"`.
3. Ensure that the server returns the correct data.

**Let's update our code:**

*NOTE: The current version of the playground doesn't support endpoint requests*

## Summary

That's it! Now you have a dynamic component set up. If you're facing any challenges, feel free to ask a question on the developer forum. We'll be happy to help!

[←

Anterior

Flow JSON](/docs/whatsapp/flows/reference/flowjson)[→

Siguiente

Flows API](/docs/whatsapp/flows/reference/flowsapi)

![](https://www.facebook.com/tr?id=675141479195042&ev=PageView&noscript=1)![](https://www.facebook.com/tr?id=574561515946252&ev=PageView&noscript=1)![](https://www.facebook.com/tr?id=1754628768090156&ev=PageView&noscript=1)
