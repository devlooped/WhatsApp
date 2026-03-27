using System.Text.Json;
using System.Text.Json.Nodes;

namespace Devlooped.WhatsApp;

/// <summary>
/// Generates valid and invalid WhatsApp Flow JSON test data for version 7.3.
/// Provides <see cref="ValidFlows"/> and <see cref="InvalidFlows"/> as
/// <c>IEnumerable&lt;object[]&gt;</c> for use with xUnit <c>[MemberData]</c>.
/// </summary>
static class FlowJsonGenerator
{
    static readonly JsonSerializerOptions s_options = new() { WriteIndented = true };

    #region Helpers

    static string ToJson(JsonNode node) => node.ToJsonString(s_options);

    static JsonObject MakeFlow(params JsonObject[] screens)
    {
        var arr = new JsonArray();
        foreach (var s in screens)
            arr.Add(s);
        return new JsonObject
        {
            ["version"] = "7.3",
            ["screens"] = arr
        };
    }

    static JsonObject MakeEndpointFlow(JsonObject routingModel, params JsonObject[] screens)
    {
        var flow = MakeFlow(screens);
        flow["routing_model"] = routingModel;
        flow["data_api_version"] = "3.0";
        return flow;
    }

    static JsonObject MakeScreen(string id, bool terminal = false, JsonArray? children = null,
        JsonObject? data = null, string? title = null)
    {
        var screen = new JsonObject { ["id"] = id };
        if (terminal)
            screen["terminal"] = true;
        if (title != null)
            screen["title"] = title;
        if (data != null)
            screen["data"] = data;
        screen["layout"] = new JsonObject
        {
            ["type"] = "SingleColumnLayout",
            ["children"] = children ?? new JsonArray()
        };
        return screen;
    }

    static JsonArray Children(params JsonNode[] nodes)
    {
        var arr = new JsonArray();
        foreach (var n in nodes)
            arr.Add(n);
        return arr;
    }

    // Footer helpers
    static JsonObject MakeFooter(string label, string actionName, string? nextScreen = null, JsonObject? payload = null)
    {
        var action = new JsonObject { ["name"] = actionName };
        if (nextScreen != null)
            action["next"] = new JsonObject { ["type"] = "screen", ["name"] = nextScreen };
        action["payload"] = payload ?? new JsonObject();
        return new JsonObject
        {
            ["type"] = "Footer",
            ["label"] = label,
            ["on-click-action"] = action
        };
    }

    static JsonObject CompleteFooter(string label = "Done", JsonObject? payload = null)
        => MakeFooter(label, "complete", payload: payload);

    static JsonObject NavigateFooter(string label, string nextScreen, JsonObject? payload = null)
        => MakeFooter(label, "navigate", nextScreen, payload);

    static JsonObject DataExchangeFooter(string label = "Submit", JsonObject? payload = null)
        => MakeFooter(label, "data_exchange", payload: payload);

    static JsonObject FooterWithCaptions(string label, string actionName,
        string? leftCaption = null, string? rightCaption = null, string? centerCaption = null,
        string? nextScreen = null)
    {
        var footer = MakeFooter(label, actionName, nextScreen);
        if (leftCaption != null) footer["left-caption"] = leftCaption;
        if (rightCaption != null) footer["right-caption"] = rightCaption;
        if (centerCaption != null) footer["center-caption"] = centerCaption;
        return footer;
    }

    // Text components
    static JsonObject TextBody(string text, bool? markdown = null)
    {
        var obj = new JsonObject { ["type"] = "TextBody", ["text"] = text };
        if (markdown.HasValue)
            obj["markdown"] = markdown.Value;
        return obj;
    }

    static JsonObject TextHeading(string text) =>
        new() { ["type"] = "TextHeading", ["text"] = text };

    static JsonObject TextSubheading(string text) =>
        new() { ["type"] = "TextSubheading", ["text"] = text };

    static JsonObject TextCaption(string text) =>
        new() { ["type"] = "TextCaption", ["text"] = text };

    // Input components
    static JsonObject TextInput(string name, string label, string? inputType = null, bool? required = null)
    {
        var obj = new JsonObject { ["type"] = "TextInput", ["name"] = name, ["label"] = label };
        if (inputType != null)
            obj["input-type"] = inputType;
        if (required.HasValue)
            obj["required"] = required.Value;
        return obj;
    }

    static JsonObject TextArea(string name, string label, bool? required = null)
    {
        var obj = new JsonObject { ["type"] = "TextArea", ["name"] = name, ["label"] = label };
        if (required.HasValue)
            obj["required"] = required.Value;
        return obj;
    }

    // Data-source helpers
    static JsonArray DataSource(params (string id, string title)[] items)
    {
        var arr = new JsonArray();
        foreach (var (id, title) in items)
            arr.Add(new JsonObject { ["id"] = id, ["title"] = title });
        return arr;
    }

    static JsonObject Dropdown(string name, string label, JsonArray dataSource, JsonObject? onSelectAction = null)
    {
        var obj = new JsonObject
        {
            ["type"] = "Dropdown",
            ["name"] = name,
            ["label"] = label,
            ["data-source"] = dataSource
        };
        if (onSelectAction != null)
            obj["on-select-action"] = onSelectAction;
        return obj;
    }

    static JsonObject CheckboxGroup(string name, string label, JsonArray dataSource) =>
        new()
        {
            ["type"] = "CheckboxGroup",
            ["name"] = name,
            ["label"] = label,
            ["data-source"] = dataSource
        };

    static JsonObject RadioButtonsGroup(string name, string label, JsonArray dataSource) =>
        new()
        {
            ["type"] = "RadioButtonsGroup",
            ["name"] = name,
            ["label"] = label,
            ["data-source"] = dataSource
        };

    static JsonObject ChipsSelector(string name, string label, JsonArray dataSource) =>
        new()
        {
            ["type"] = "ChipsSelector",
            ["name"] = name,
            ["label"] = label,
            ["data-source"] = dataSource
        };

    // Date/Calendar
    static JsonObject DatePicker(string name, string label, string? minDate = null, string? maxDate = null)
    {
        var obj = new JsonObject { ["type"] = "DatePicker", ["name"] = name, ["label"] = label };
        if (minDate != null) obj["min-date"] = minDate;
        if (maxDate != null) obj["max-date"] = maxDate;
        return obj;
    }

    static JsonObject CalendarPicker(string name, string label, string mode = "single") =>
        new()
        {
            ["type"] = "CalendarPicker",
            ["name"] = name,
            ["label"] = label,
            ["mode"] = mode
        };

    // Media
    static JsonObject Image(string? src = null, int? width = null, int? height = null, string? altText = null)
    {
        var obj = new JsonObject
        {
            ["type"] = "Image",
            ["src"] = src ?? "data:image/png;base64,iVBOR"
        };
        if (width.HasValue) obj["width"] = width.Value;
        if (height.HasValue) obj["height"] = height.Value;
        if (altText != null) obj["alt-text"] = altText;
        return obj;
    }

    static JsonObject PhotoPicker(string name, string label) =>
        new() { ["type"] = "PhotoPicker", ["name"] = name, ["label"] = label };

    static JsonObject DocumentPicker(string name, string label) =>
        new() { ["type"] = "DocumentPicker", ["name"] = name, ["label"] = label };

    static JsonObject ImageCarousel(params string[] sources)
    {
        var images = new JsonArray();
        foreach (var src in sources)
            images.Add(new JsonObject { ["src"] = src, ["alt-text"] = "image" });
        return new JsonObject
        {
            ["type"] = "ImageCarousel",
            ["images"] = images
        };
    }

    // Opt-in
    static JsonObject OptIn(string name, string label, JsonObject? onClickAction = null)
    {
        var obj = new JsonObject { ["type"] = "OptIn", ["name"] = name, ["label"] = label };
        if (onClickAction != null)
            obj["on-click-action"] = onClickAction;
        return obj;
    }

    // Structural components
    static JsonObject If(string condition, JsonArray thenChildren, JsonArray? elseChildren = null)
    {
        var obj = new JsonObject
        {
            ["type"] = "If",
            ["condition"] = condition,
            ["then"] = thenChildren
        };
        if (elseChildren != null)
            obj["else"] = elseChildren;
        return obj;
    }

    static JsonObject Switch(string value, params (string key, JsonArray children)[] cases)
    {
        var casesObj = new JsonObject();
        foreach (var (key, children) in cases)
            casesObj[key] = children;
        return new JsonObject
        {
            ["type"] = "Switch",
            ["value"] = value,
            ["cases"] = casesObj
        };
    }

    static JsonObject Form(string name, JsonArray children) =>
        new() { ["type"] = "Form", ["name"] = name, ["children"] = children };

    // Navigation list
    static JsonObject NavigationList(string label, string name, string nextScreen, JsonArray items, JsonObject? payload = null)
    {
        var action = new JsonObject
        {
            ["name"] = "navigate",
            ["next"] = new JsonObject { ["type"] = "screen", ["name"] = nextScreen },
            ["payload"] = payload ?? new JsonObject { [$"{name}_selected"] = $"${{form.{name}}}" }
        };
        return new JsonObject
        {
            ["type"] = "NavigationList",
            ["label"] = label,
            ["name"] = name,
            ["on-click-action"] = action,
            ["list-items"] = items
        };
    }

    static JsonObject NavListItem(string title, string? description = null)
    {
        var mainContent = new JsonObject { ["title"] = title };
        if (description != null)
            mainContent["description"] = description;
        return new JsonObject { ["main-content"] = mainContent };
    }

    // Embedded link
    static JsonObject EmbeddedLink(string text, string actionName, string? nextScreen = null, string? url = null, JsonObject? payload = null)
    {
        var action = new JsonObject { ["name"] = actionName };
        if (nextScreen != null)
        {
            action["next"] = new JsonObject { ["type"] = "screen", ["name"] = nextScreen };
            action["payload"] = payload ?? new JsonObject();
        }
        if (url != null)
            action["url"] = url;
        return new JsonObject
        {
            ["type"] = "EmbeddedLink",
            ["text"] = text,
            ["on-click-action"] = action
        };
    }

    // Rich text
    static JsonObject RichText(JsonArray text) =>
        new() { ["type"] = "RichText", ["text"] = text };

    // Data model for screen data definitions
    static JsonObject DataModel(params (string name, string type, string example)[] fields)
    {
        var data = new JsonObject();
        foreach (var (name, type, example) in fields)
            data[name] = new JsonObject { ["type"] = type, ["__example__"] = example };
        return data;
    }

    // Open-URL action (for EmbeddedLink / OptIn)
    static JsonObject OpenUrlAction(string url) =>
        new() { ["name"] = "open_url", ["url"] = url };

    // Update-data action (for Dropdown on-select-action)
    static JsonObject UpdateDataAction(JsonObject payload) =>
        new() { ["name"] = "update_data", ["payload"] = payload };

    #endregion

    /// <summary>
    /// Yields valid WhatsApp Flow JSON documents for v7.3.
    /// Each item is <c>object[] { string name, string json }</c>.
    /// </summary>
    public static IEnumerable<object[]> ValidFlows()
    {
        // ── Minimal Flows ────────────────────────────────────────

        // 1. Single terminal screen with TextBody + Footer (complete)
        yield return
        [
            "minimal-textbody-complete",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Welcome to the flow."),
                    CompleteFooter()))))
        ];

        // 2. Single terminal screen with TextHeading + Footer
        yield return
        [
            "minimal-textheading-complete",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextHeading("Hello World"),
                    CompleteFooter()))))
        ];

        // 3. Single terminal screen with TextSubheading + TextBody + Footer
        yield return
        [
            "minimal-subheading-body-complete",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextSubheading("Getting Started"),
                    TextBody("Follow the instructions below."),
                    CompleteFooter()))))
        ];

        // 4. Single terminal screen with TextCaption + Footer
        yield return
        [
            "minimal-caption-complete",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextCaption("Step 1 of 1"),
                    CompleteFooter()))))
        ];

        // ── Multi-Screen Flows ───────────────────────────────────

        // 5. Two screens: first navigates to second (terminal)
        yield return
        [
            "two-screens-navigate",
            ToJson(MakeFlow(
                MakeScreen("FIRST", children: Children(
                    TextBody("Screen one"),
                    NavigateFooter("Next", "SECOND"))),
                MakeScreen("SECOND", terminal: true, children: Children(
                    TextBody("Screen two"),
                    CompleteFooter()))))
        ];

        // 6. Three screens: linear navigation A → B → C (C terminal)
        yield return
        [
            "three-screens-linear",
            ToJson(MakeFlow(
                MakeScreen("STEP_A", children: Children(
                    TextBody("Step A"),
                    NavigateFooter("Next", "STEP_B"))),
                MakeScreen("STEP_B", children: Children(
                    TextBody("Step B"),
                    NavigateFooter("Next", "STEP_C"))),
                MakeScreen("STEP_C", terminal: true, children: Children(
                    TextBody("Step C — Done"),
                    CompleteFooter()))))
        ];

        // 7. Three screens with branching: A → B, A → C (B and C terminal)
        yield return
        [
            "three-screens-branching",
            ToJson(MakeFlow(
                MakeScreen("CHOICE", data: DataModel(("option", "string", "yes")), children: Children(
                    TextBody("Choose a path"),
                    If("${data.option}",
                        Children(NavigateFooter("Path B", "PATH_B")),
                        Children(NavigateFooter("Path C", "PATH_C"))))),
                MakeScreen("PATH_B", terminal: true, children: Children(
                    TextBody("You chose path B"),
                    CompleteFooter())),
                MakeScreen("PATH_C", terminal: true, children: Children(
                    TextBody("You chose path C"),
                    CompleteFooter()))))
        ];

        // ── Component Variety ────────────────────────────────────

        // 8. TextInput + Footer
        yield return
        [
            "component-text-input",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        TextInput("full_name", "Full Name"),
                        CompleteFooter("Submit", new JsonObject { ["name"] = "${form.full_name}" })))))))
        ];

        // 9. TextArea + Footer
        yield return
        [
            "component-text-area",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        TextArea("comments", "Your Comments"),
                        CompleteFooter("Submit", new JsonObject { ["comments"] = "${form.comments}" })))))))
        ];

        // 10. CheckboxGroup + Footer
        yield return
        [
            "component-checkbox-group",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        CheckboxGroup("interests", "Select Interests",
                            DataSource(("1", "Sports"), ("2", "Music"), ("3", "Travel"))),
                        CompleteFooter()))))))
        ];

        // 11. RadioButtonsGroup + Footer
        yield return
        [
            "component-radio-buttons",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        RadioButtonsGroup("gender", "Gender",
                            DataSource(("m", "Male"), ("f", "Female"), ("o", "Other"))),
                        CompleteFooter()))))))
        ];

        // 12. Dropdown + Footer
        yield return
        [
            "component-dropdown",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        Dropdown("country", "Country",
                            DataSource(("us", "United States"), ("uk", "United Kingdom"), ("ca", "Canada"))),
                        CompleteFooter()))))))
        ];

        // 13. DatePicker + Footer
        yield return
        [
            "component-date-picker",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        DatePicker("dob", "Date of Birth"),
                        CompleteFooter()))))))
        ];

        // 14. CalendarPicker + Footer
        yield return
        [
            "component-calendar-picker",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        CalendarPicker("appointment", "Appointment Date"),
                        CompleteFooter()))))))
        ];

        // 15. OptIn + Footer
        yield return
        [
            "component-opt-in",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        OptIn("terms", "I agree to the terms and conditions"),
                        CompleteFooter()))))))
        ];

        // 16. ChipsSelector + Footer
        yield return
        [
            "component-chips-selector",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        ChipsSelector("tags", "Select Tags",
                            DataSource(("urgent", "Urgent"), ("normal", "Normal"), ("low", "Low Priority"))),
                        CompleteFooter()))))))
        ];

        // 17. All text component types on one screen + Footer
        yield return
        [
            "all-text-components",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextHeading("Main Title"),
                    TextSubheading("Subtitle Here"),
                    TextBody("This is the body text with details."),
                    TextCaption("Caption: additional note"),
                    CompleteFooter()))))
        ];

        // 18. Multiple inputs (TextInput + TextArea + Dropdown) + Footer
        yield return
        [
            "multiple-inputs",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        TextInput("name", "Name"),
                        TextArea("bio", "Short Bio"),
                        Dropdown("role", "Role",
                            DataSource(("dev", "Developer"), ("pm", "Product Manager"), ("des", "Designer"))),
                        CompleteFooter("Submit")))))))
        ];

        // ── Data Source Components ───────────────────────────────

        // 19. CheckboxGroup with static data-source (3 items)
        yield return
        [
            "datasource-checkbox-3-items",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        CheckboxGroup("features", "Desired Features",
                            DataSource(("wifi", "WiFi"), ("pool", "Swimming Pool"), ("gym", "Fitness Center"))),
                        CompleteFooter()))))))
        ];

        // 20. RadioButtonsGroup with static data-source
        yield return
        [
            "datasource-radio-buttons",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        RadioButtonsGroup("priority", "Priority Level",
                            DataSource(("high", "High"), ("medium", "Medium"), ("low", "Low"))),
                        CompleteFooter()))))))
        ];

        // 21. Dropdown with static data-source (5 items)
        yield return
        [
            "datasource-dropdown-5-items",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        Dropdown("department", "Department", DataSource(
                            ("eng", "Engineering"),
                            ("mkt", "Marketing"),
                            ("fin", "Finance"),
                            ("hr", "Human Resources"),
                            ("ops", "Operations"))),
                        CompleteFooter()))))))
        ];

        // 22. Dropdown with 20 items
        {
            var items = Enumerable.Range(1, 20)
                .Select(i => ($"item_{i}", $"Option {i}"))
                .ToArray();
            yield return
            [
                "datasource-dropdown-20-items",
                ToJson(MakeFlow(
                    MakeScreen("MAIN", terminal: true, children: Children(
                        Form("form", Children(
                            Dropdown("large_list", "Select Item", DataSource(items)),
                            CompleteFooter()))))))
            ];
        }

        // 23. ChipsSelector with static data-source (3 items)
        yield return
        [
            "datasource-chips-3-items",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        ChipsSelector("mood", "How are you feeling?",
                            DataSource(("happy", "Happy 😊"), ("ok", "Okay 😐"), ("sad", "Sad 😢"))),
                        CompleteFooter()))))))
        ];

        // ── Media Components ─────────────────────────────────────

        // 24. Image component
        yield return
        [
            "media-image",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Image(width: 300, height: 200, altText: "Sample image"),
                    TextBody("Image displayed above."),
                    CompleteFooter()))))
        ];

        // 25. PhotoPicker
        yield return
        [
            "media-photo-picker",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        PhotoPicker("photo", "Upload a Photo"),
                        CompleteFooter()))))))
        ];

        // 26. DocumentPicker
        yield return
        [
            "media-document-picker",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        DocumentPicker("doc", "Upload Document"),
                        CompleteFooter()))))))
        ];

        // 27. ImageCarousel (2 images)
        yield return
        [
            "media-image-carousel",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    ImageCarousel("data:image/png;base64,iVBOR1", "data:image/png;base64,iVBOR2"),
                    TextBody("Browse the images above."),
                    CompleteFooter()))))
        ];

        // ── Structural Components ────────────────────────────────

        // 28. If component (condition referencing data, both branches with text)
        yield return
        [
            "structural-if-both-branches",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(("is_member", "boolean", "true")),
                    children: Children(
                        If("${data.is_member}",
                            Children(TextBody("Welcome back, member!")),
                            Children(TextBody("Join us today!"))),
                        CompleteFooter()))))
        ];

        // 29. If component containing Footer in both branches
        yield return
        [
            "structural-if-footer-both-branches",
            ToJson(MakeFlow(
                MakeScreen("START",
                    data: DataModel(("has_account", "boolean", "false")),
                    children: Children(
                        TextHeading("Welcome"),
                        If("${data.has_account}",
                            Children(NavigateFooter("Sign In", "SIGN_IN")),
                            Children(NavigateFooter("Register", "REGISTER"))))),
                MakeScreen("SIGN_IN", terminal: true, children: Children(
                    TextBody("Sign in screen"),
                    CompleteFooter())),
                MakeScreen("REGISTER", terminal: true, children: Children(
                    TextBody("Registration screen"),
                    CompleteFooter()))))
        ];

        // 30. Switch component (2 cases with different text)
        yield return
        [
            "structural-switch-2-cases",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(("plan", "string", "basic")),
                    children: Children(
                        Switch("${data.plan}",
                            ("basic", Children(TextBody("Basic plan: limited features."))),
                            ("premium", Children(TextBody("Premium plan: all features included.")))),
                        CompleteFooter()))))
        ];

        // 31. Nested If (2 levels deep)
        yield return
        [
            "structural-nested-if",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(("level1", "boolean", "true"), ("level2", "boolean", "false")),
                    children: Children(
                        If("${data.level1}",
                            Children(
                                TextBody("Level 1 is true"),
                                If("${data.level2}",
                                    Children(TextBody("Level 2 is also true")),
                                    Children(TextBody("Level 2 is false")))),
                            Children(TextBody("Level 1 is false"))),
                        CompleteFooter()))))
        ];

        // 32. Form wrapping inputs
        yield return
        [
            "structural-form-wrapper",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextHeading("Contact Form"),
                    Form("contact_form", Children(
                        TextInput("first_name", "First Name", required: true),
                        TextInput("last_name", "Last Name", required: true),
                        TextInput("email", "Email", inputType: "email"),
                        TextArea("message", "Message"),
                        CompleteFooter("Send", new JsonObject
                        {
                            ["first_name"] = "${form.first_name}",
                            ["last_name"] = "${form.last_name}",
                            ["email"] = "${form.email}",
                            ["message"] = "${form.message}"
                        })))))))
        ];

        // ── Navigation & Data ────────────────────────────────────

        // 33. Navigate action passing payload to next screen with data model
        yield return
        [
            "navigate-with-payload",
            ToJson(MakeFlow(
                MakeScreen("INPUT", children: Children(
                    Form("form", Children(
                        TextInput("user_name", "Your Name"),
                        NavigateFooter("Continue", "CONFIRM", new JsonObject
                        {
                            ["name"] = "${form.user_name}"
                        }))))),
                MakeScreen("CONFIRM", terminal: true,
                    data: DataModel(("name", "string", "John")),
                    children: Children(
                        TextBody("Hello, ${data.name}!"),
                        CompleteFooter()))))
        ];

        // 34. Data model and dynamic references
        yield return
        [
            "data-model-dynamic-refs",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(
                        ("greeting", "string", "Hello"),
                        ("user_name", "string", "World")),
                    children: Children(
                        TextHeading("${data.greeting}"),
                        TextBody("Welcome, ${data.user_name}!"),
                        CompleteFooter()))))
        ];

        // 35. Global dynamic references (cross-screen form reference)
        yield return
        [
            "global-dynamic-refs",
            ToJson(MakeFlow(
                MakeScreen("FIRST", children: Children(
                    Form("input_form", Children(
                        TextInput("city", "City"),
                        NavigateFooter("Next", "SECOND"))))),
                MakeScreen("SECOND", terminal: true, children: Children(
                    TextBody("You entered: ${screen.FIRST.form.city}"),
                    CompleteFooter()))))
        ];

        // 36. data_exchange action on Footer
        yield return
        [
            "data-exchange-footer",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        TextInput("query", "Search"),
                        DataExchangeFooter("Search", new JsonObject
                        {
                            ["search_query"] = "${form.query}"
                        })))))))
        ];

        // ── Special Features ─────────────────────────────────────

        // 37. EmbeddedLink with navigate action
        yield return
        [
            "embedded-link-navigate",
            ToJson(MakeFlow(
                MakeScreen("MAIN", children: Children(
                    TextBody("Read our "),
                    EmbeddedLink("terms of service", "navigate", nextScreen: "TERMS"),
                    NavigateFooter("Continue", "TERMS"))),
                MakeScreen("TERMS", terminal: true, children: Children(
                    TextBody("Terms of service content here."),
                    CompleteFooter()))))
        ];

        // 38. EmbeddedLink with open_url action
        yield return
        [
            "embedded-link-open-url",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Visit our website: "),
                    EmbeddedLink("example.com", "open_url", url: "https://example.com"),
                    CompleteFooter()))))
        ];

        // 39. OptIn with on-click-action (open_url)
        yield return
        [
            "opt-in-with-url",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        OptIn("privacy_policy", "I accept the privacy policy",
                            OpenUrlAction("https://example.com/privacy")),
                        CompleteFooter()))))))
        ];

        // 40. NavigationList with 3 items
        {
            var navItems = new JsonArray();
            navItems.Add(NavListItem("Product A", "Best seller"));
            navItems.Add(NavListItem("Product B", "New arrival"));
            navItems.Add(NavListItem("Product C", "On sale"));
            yield return
            [
                "navigation-list-3-items",
                ToJson(MakeFlow(
                    MakeScreen("CATALOG", children: Children(
                        TextHeading("Our Products"),
                        NavigationList("Select a product", "product_nav", "DETAILS", navItems))),
                    MakeScreen("DETAILS", terminal: true,
                        data: DataModel(("product_nav_selected", "string", "prod_1")),
                        children: Children(
                            TextBody("Product details for: ${data.product_nav_selected}"),
                            CompleteFooter()))))
            ];
        }

        // 41. Dropdown with update_data on-select-action
        yield return
        [
            "dropdown-update-data",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        Dropdown("region", "Select Region",
                            DataSource(("na", "North America"), ("eu", "Europe"), ("ap", "Asia Pacific")),
                            UpdateDataAction(new JsonObject { ["selected_region"] = "${form.region}" })),
                        CompleteFooter()))))))
        ];

        // 42. RichText component
        {
            yield return
            [
                "rich-text",
                ToJson(MakeFlow(
                    MakeScreen("MAIN", terminal: true, children: Children(
                        RichText(new JsonArray(
                            JsonValue.Create("Welcome to our amazing service. We offer premium quality."))),
                        CompleteFooter()))))
            ];
        }

        // 43. CalendarPicker in range mode
        yield return
        [
            "calendar-picker-range",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        CalendarPicker("date_range", "Select Date Range", "range"),
                        CompleteFooter()))))))
        ];

        // 44. DatePicker with min/max dates
        yield return
        [
            "date-picker-min-max",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        DatePicker("event_date", "Event Date",
                            minDate: "1704067200000",
                            maxDate: "1735689600000"),
                        CompleteFooter()))))))
        ];

        // 45. Endpoint-powered flow with routing_model and data_api_version
        yield return
        [
            "endpoint-powered-routing",
            ToJson(MakeEndpointFlow(
                new JsonObject
                {
                    ["WELCOME"] = new JsonArray(JsonValue.Create("DETAILS")),
                    ["DETAILS"] = new JsonArray()
                },
                MakeScreen("WELCOME", children: Children(
                    TextBody("Welcome!"),
                    NavigateFooter("Start", "DETAILS"))),
                MakeScreen("DETAILS", terminal: true, children: Children(
                    TextBody("Details screen"),
                    CompleteFooter()))))
        ];

        // 46. TextBody with markdown=true
        yield return
        [
            "textbody-markdown",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("**Bold text** and _italic text_ with a [link](https://example.com)", markdown: true),
                    CompleteFooter()))))
        ];

        // 47. Footer with left-caption and right-caption
        yield return
        [
            "footer-left-right-caption",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Review your submission."),
                    FooterWithCaptions("Submit", "complete", leftCaption: "Step 3", rightCaption: "Final")))))
        ];

        // 48. Footer with center-caption
        yield return
        [
            "footer-center-caption",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Almost done!"),
                    FooterWithCaptions("Finish", "complete", centerCaption: "Last Step")))))
        ];

        // 49. Screen with title
        yield return
        [
            "screen-with-title",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, title: "My Flow Title", children: Children(
                    TextBody("Screen with an explicit title."),
                    CompleteFooter()))))
        ];

        // 50. Screen with sensitive input fields
        {
            var screen = MakeScreen("MAIN", terminal: true, children: Children(
                Form("form", Children(
                    TextInput("username", "Username"),
                    TextInput("password", "Password", inputType: "password"),
                    TextInput("pin", "PIN", inputType: "passcode"),
                    CompleteFooter("Login")))));
            // Mark sensitive fields at screen level
            var sensitiveArr = new JsonArray();
            sensitiveArr.Add(JsonValue.Create("password"));
            sensitiveArr.Add(JsonValue.Create("pin"));
            screen["sensitive"] = sensitiveArr;
            yield return ["sensitive-fields", ToJson(MakeFlow(screen))];
        }

        // ── Complex Realistic Flows ──────────────────────────────

        // 51. Lead generation: name → contact details → confirmation
        yield return
        [
            "realistic-lead-generation",
            ToJson(MakeFlow(
                MakeScreen("NAME", title: "Your Name", children: Children(
                    TextHeading("Lead Generation"),
                    Form("name_form", Children(
                        TextInput("first_name", "First Name", required: true),
                        TextInput("last_name", "Last Name", required: true),
                        NavigateFooter("Next", "CONTACT", new JsonObject
                        {
                            ["first_name"] = "${form.first_name}",
                            ["last_name"] = "${form.last_name}"
                        }))))),
                MakeScreen("CONTACT", title: "Contact Details",
                    data: DataModel(("first_name", "string", "Jane"), ("last_name", "string", "Doe")),
                    children: Children(
                        TextSubheading("Hi ${data.first_name}!"),
                        Form("contact_form", Children(
                            TextInput("email", "Email Address", inputType: "email", required: true),
                            TextInput("phone", "Phone Number", inputType: "phone"),
                            NavigateFooter("Review", "CONFIRM", new JsonObject
                            {
                                ["email"] = "${form.email}",
                                ["phone"] = "${form.phone}"
                            }))))),
                MakeScreen("CONFIRM", terminal: true, title: "Confirmation",
                    data: DataModel(("email", "string", "jane@example.com"), ("phone", "string", "+1234567890")),
                    children: Children(
                        TextHeading("Confirm Your Details"),
                        TextBody("Email: ${data.email}"),
                        TextBody("Phone: ${data.phone}"),
                        CompleteFooter("Submit", new JsonObject
                        {
                            ["email"] = "${data.email}",
                            ["phone"] = "${data.phone}"
                        })))))
        ];

        // 52. Appointment booking: date → time dropdown → confirmation
        yield return
        [
            "realistic-appointment-booking",
            ToJson(MakeFlow(
                MakeScreen("DATE_SELECT", title: "Select Date", children: Children(
                    TextHeading("Book Appointment"),
                    Form("date_form", Children(
                        CalendarPicker("date", "Preferred Date"),
                        NavigateFooter("Choose Time", "TIME_SELECT", new JsonObject
                        {
                            ["selected_date"] = "${form.date}"
                        }))))),
                MakeScreen("TIME_SELECT", title: "Select Time",
                    data: DataModel(("selected_date", "string", "2025-03-15")),
                    children: Children(
                        TextSubheading("Date: ${data.selected_date}"),
                        Form("time_form", Children(
                            Dropdown("time_slot", "Available Times", DataSource(
                                ("09", "9:00 AM"), ("10", "10:00 AM"), ("11", "11:00 AM"),
                                ("14", "2:00 PM"), ("15", "3:00 PM"), ("16", "4:00 PM"))),
                            NavigateFooter("Confirm", "BOOKING_CONFIRM", new JsonObject
                            {
                                ["time_slot"] = "${form.time_slot}"
                            }))))),
                MakeScreen("BOOKING_CONFIRM", terminal: true, title: "Confirm Booking",
                    data: DataModel(("selected_date", "string", "2025-03-15"), ("time_slot", "string", "10")),
                    children: Children(
                        TextHeading("Booking Summary"),
                        TextBody("Date: ${data.selected_date}"),
                        TextBody("Time: ${data.time_slot}"),
                        CompleteFooter("Book Now")))))
        ];

        // 53. Survey: radio → checkboxes → text area → submit
        yield return
        [
            "realistic-survey",
            ToJson(MakeFlow(
                MakeScreen("Q1", title: "Question 1", children: Children(
                    TextHeading("Customer Survey"),
                    Form("q1_form", Children(
                        RadioButtonsGroup("satisfaction", "How satisfied are you?",
                            DataSource(("5", "Very Satisfied"), ("4", "Satisfied"), ("3", "Neutral"),
                                ("2", "Dissatisfied"), ("1", "Very Dissatisfied"))),
                        NavigateFooter("Next", "Q2", new JsonObject
                        {
                            ["satisfaction"] = "${form.satisfaction}"
                        }))))),
                MakeScreen("Q2", title: "Question 2",
                    data: DataModel(("satisfaction", "string", "5")),
                    children: Children(
                        Form("q2_form", Children(
                            CheckboxGroup("improvements", "What could we improve?",
                                DataSource(("speed", "Speed"), ("quality", "Quality"),
                                    ("support", "Customer Support"), ("price", "Pricing"))),
                            NavigateFooter("Next", "Q3", new JsonObject
                            {
                                ["improvements"] = "${form.improvements}"
                            }))))),
                MakeScreen("Q3", title: "Question 3",
                    data: DataModel(("improvements", "string", "speed,quality")),
                    children: Children(
                        Form("q3_form", Children(
                            TextArea("additional_feedback", "Any additional feedback?"),
                            NavigateFooter("Next", "SUBMIT", new JsonObject
                            {
                                ["feedback"] = "${form.additional_feedback}"
                            }))))),
                MakeScreen("SUBMIT", terminal: true, title: "Thank You",
                    data: DataModel(("feedback", "string", "Great service!")),
                    children: Children(
                        TextHeading("Thank You!"),
                        TextBody("Your feedback has been recorded."),
                        CompleteFooter("Done")))))
        ];

        // 54. Product selection: navigation list → details → checkout
        {
            var productItems = new JsonArray();
            productItems.Add(NavListItem("Laptop Pro", "High performance laptop"));
            productItems.Add(NavListItem("Tablet Air", "Lightweight tablet"));
            productItems.Add(NavListItem("SmartPhone X", "Latest smartphone"));
            yield return
            [
                "realistic-product-selection",
                ToJson(MakeFlow(
                    MakeScreen("PRODUCTS", title: "Products", children: Children(
                        TextHeading("Our Products"),
                        NavigationList("Browse Products", "product_list", "DETAILS", productItems))),
                    MakeScreen("DETAILS",
                        data: DataModel(("product_list_selected", "string", "laptop")),
                        children: Children(
                            TextHeading("Product Details"),
                            TextBody("Selected: ${data.product_list_selected}"),
                            TextBody("Add this item to your cart?"),
                            NavigateFooter("Add to Cart", "CHECKOUT", new JsonObject
                            {
                                ["product"] = "${data.product_list_selected}"
                            }))),
                    MakeScreen("CHECKOUT", terminal: true, title: "Checkout",
                        data: DataModel(("product", "string", "laptop")),
                        children: Children(
                            TextHeading("Checkout"),
                            TextBody("Item: ${data.product}"),
                            Form("checkout_form", Children(
                                TextInput("address", "Shipping Address", required: true),
                                CompleteFooter("Place Order", new JsonObject
                                {
                                    ["product"] = "${data.product}",
                                    ["address"] = "${form.address}"
                                })))))))
            ];
        }
    }

    /// <summary>
    /// Yields invalid WhatsApp Flow JSON documents for v7.3 with expected error codes.
    /// Each item is <c>object[] { string name, string json, string expectedErrorCode }</c>.
    /// </summary>
    public static IEnumerable<object[]> InvalidFlows()
    {
        // ── Missing Required Properties ──────────────────────────

        // 1. Missing version
        {
            var flow = new JsonObject
            {
                ["screens"] = new JsonArray(
                    MakeScreen("MAIN", terminal: true, children: Children(
                        TextBody("Hello"), CompleteFooter())))
            };
            yield return ["missing-version", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 2. Missing screens
        {
            var flow = new JsonObject { ["version"] = "7.3" };
            yield return ["missing-screens", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 3. Empty screens array
        {
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray()
            };
            yield return ["empty-screens", ToJson(flow), "MIN_ITEMS_REQUIRED"];
        }

        // 4. Screen missing id
        {
            var screen = new JsonObject
            {
                ["terminal"] = true,
                ["layout"] = new JsonObject
                {
                    ["type"] = "SingleColumnLayout",
                    ["children"] = Children(TextBody("Text"), CompleteFooter())
                }
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["screen-missing-id", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 5. Screen missing layout
        {
            var screen = new JsonObject
            {
                ["id"] = "MAIN",
                ["terminal"] = true
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["screen-missing-layout", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 6. Layout missing type
        {
            var screen = new JsonObject
            {
                ["id"] = "MAIN",
                ["terminal"] = true,
                ["layout"] = new JsonObject
                {
                    ["children"] = Children(TextBody("Text"), CompleteFooter())
                }
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["layout-missing-type", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 7. Layout missing children
        {
            var screen = new JsonObject
            {
                ["id"] = "MAIN",
                ["terminal"] = true,
                ["layout"] = new JsonObject { ["type"] = "SingleColumnLayout" }
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["layout-missing-children", ToJson(flow), "MISSING_REQUIRED_TYPE_PROPERTY"];
        }

        // 8. Empty children array
        yield return
        [
            "empty-children",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: new JsonArray()))),
            "MIN_ITEMS_REQUIRED"
        ];

        // 9. Component missing type
        {
            var child = new JsonObject { ["text"] = "No type" };
            var arr = new JsonArray();
            arr.Add(child);
            arr.Add(CompleteFooter());
            yield return
            [
                "component-missing-type",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: arr))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 10. Footer missing on-click-action
        {
            var footer = new JsonObject
            {
                ["type"] = "Footer",
                ["label"] = "Submit"
            };
            yield return
            [
                "footer-missing-action",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(TextBody("Text"), footer)))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 11. Footer missing label
        {
            var footer = new JsonObject
            {
                ["type"] = "Footer",
                ["on-click-action"] = new JsonObject
                {
                    ["name"] = "complete",
                    ["payload"] = new JsonObject()
                }
            };
            yield return
            [
                "footer-missing-label",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(TextBody("Text"), footer)))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 12. TextInput missing name
        {
            var input = new JsonObject
            {
                ["type"] = "TextInput",
                ["label"] = "Enter value"
            };
            yield return
            [
                "textinput-missing-name",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(input, CompleteFooter())))))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 13. TextInput missing label
        {
            var input = new JsonObject
            {
                ["type"] = "TextInput",
                ["name"] = "field1"
            };
            yield return
            [
                "textinput-missing-label",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(input, CompleteFooter())))))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 14. TextHeading missing text
        {
            var heading = new JsonObject { ["type"] = "TextHeading" };
            yield return
            [
                "textheading-missing-text",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(heading, CompleteFooter())))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // 15. CheckboxGroup missing data-source
        {
            var cbGroup = new JsonObject
            {
                ["type"] = "CheckboxGroup",
                ["name"] = "choices",
                ["label"] = "Pick"
            };
            yield return
            [
                "checkboxgroup-missing-datasource",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(cbGroup, CompleteFooter())))))),
                "MISSING_REQUIRED_TYPE_PROPERTY"
            ];
        }

        // ── Invalid Property Values ──────────────────────────────

        // 16. Invalid component type
        {
            var component = new JsonObject
            {
                ["type"] = "SuperWidget",
                ["text"] = "Invalid"
            };
            yield return
            [
                "invalid-component-type",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(component, CompleteFooter())))),
                "INVALID_ENUM_VALUE"
            ];
        }

        // 17. Invalid layout type (not SingleColumnLayout)
        {
            var screen = new JsonObject
            {
                ["id"] = "MAIN",
                ["terminal"] = true,
                ["layout"] = new JsonObject
                {
                    ["type"] = "TwoColumnLayout",
                    ["children"] = Children(TextBody("Text"), CompleteFooter())
                }
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["invalid-layout-type", ToJson(flow), "INVALID_PROPERTY_VALUE"];
        }

        // 18. Invalid action name
        {
            var footer = new JsonObject
            {
                ["type"] = "Footer",
                ["label"] = "Go",
                ["on-click-action"] = new JsonObject
                {
                    ["name"] = "teleport",
                    ["payload"] = new JsonObject()
                }
            };
            yield return
            [
                "invalid-action-name",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(TextBody("Text"), footer)))),
                "INVALID_ENUM_VALUE"
            ];
        }

        // 19. Screen id "SUCCESS" (reserved)
        yield return
        [
            "screen-id-reserved-success",
            ToJson(MakeFlow(MakeScreen("SUCCESS", terminal: true, children: Children(
                TextBody("Reserved ID"), CompleteFooter())))),
            "PATTERN_MISMATCH"
        ];

        // 20. Screen id with spaces
        yield return
        [
            "screen-id-with-spaces",
            ToJson(MakeFlow(MakeScreen("MY SCREEN", terminal: true, children: Children(
                TextBody("Spaces in ID"), CompleteFooter())))),
            "PATTERN_MISMATCH"
        ];

        // 21. Screen id starting with number
        yield return
        [
            "screen-id-starts-with-number",
            ToJson(MakeFlow(MakeScreen("1SCREEN", terminal: true, children: Children(
                TextBody("Starts with digit"), CompleteFooter())))),
            "PATTERN_MISMATCH"
        ];

        // 22. TextInput invalid input-type
        {
            var input = new JsonObject
            {
                ["type"] = "TextInput",
                ["name"] = "field",
                ["label"] = "Enter",
                ["input-type"] = "color_picker"
            };
            yield return
            [
                "textinput-invalid-input-type",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(input, CompleteFooter())))))),
                "INVALID_ENUM_VALUE"
            ];
        }

        // ── Invalid Property Types ───────────────────────────────

        // ── Character Limit Violations ───────────────────────────

        // 25. TextHeading text > 80 chars
        yield return
        [
            "textheading-exceeds-80-chars",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextHeading(new string('A', 81)),
                CompleteFooter())))),
            "MAX_CHARS_EXCEEDED"
        ];

        // 26. Footer label > 35 chars
        yield return
        [
            "footer-label-exceeds-35-chars",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"),
                CompleteFooter(new string('B', 36)))))),
            "MAX_CHARS_EXCEEDED"
        ];

        // 27. Footer caption > 15 chars
        yield return
        [
            "footer-caption-exceeds-15-chars",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"),
                FooterWithCaptions("Done", "complete", centerCaption: new string('C', 16)))))),
            "MAX_CHARS_EXCEEDED"
        ];

        // 28. EmbeddedLink text > 25 chars
        yield return
        [
            "embeddedlink-exceeds-25-chars",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Click: "),
                EmbeddedLink(new string('D', 26), "open_url", url: "https://example.com"),
                CompleteFooter())))),
            "MAX_CHARS_EXCEEDED"
        ];

        // ── Unknown Properties ───────────────────────────────────

        // 29. Unknown top-level property
        {
            var flow = MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"), CompleteFooter())));
            flow["unknown_top_level"] = "bad";
            yield return ["unknown-top-level-prop", ToJson(flow), "INVALID_PROPERTY_KEY"];
        }

        // 30. Unknown screen property
        {
            var screen = MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"), CompleteFooter()));
            screen["unknown_screen_prop"] = 42;
            yield return
            [
                "unknown-screen-prop",
                ToJson(MakeFlow(screen)),
                "INVALID_PROPERTY_KEY"
            ];
        }

        // 31. Unknown component property
        {
            var body = TextBody("Text");
            body["unknown_component_prop"] = true;
            yield return
            [
                "unknown-component-prop",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(body, CompleteFooter())))),
                "INVALID_PROPERTY_KEY"
            ];
        }

        // ── Footer Constraint Violations ─────────────────────────

        // 32. Footer with center-caption AND left-caption (mutually exclusive)
        yield return
        [
            "footer-center-and-left-caption",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"),
                FooterWithCaptions("Done", "complete",
                    leftCaption: "Left", rightCaption: "Right", centerCaption: "Center"))))),
            "INCOMPATIBLE_FOOTER_CAPTIONS"
        ];

        // 33. Footer with left-caption but no right-caption (dependency violation)
        yield return
        [
            "footer-left-without-right-caption",
            ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: Children(
                TextBody("Text"),
                FooterWithCaptions("Done", "complete", leftCaption: "Left"))))),
            "INVALID_DEPENDENCIES"
        ];

        // ── Semantic Errors ──────────────────────────────────────

        // 34. Duplicate screen IDs
        yield return
        [
            "duplicate-screen-ids",
            ToJson(MakeFlow(
                MakeScreen("MAIN", children: Children(
                    TextBody("First"), NavigateFooter("Next", "END"))),
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Duplicate"), CompleteFooter())),
                MakeScreen("END", terminal: true, children: Children(
                    TextBody("End"), CompleteFooter())))),
            "DUPLICATE_SCREEN_ID"
        ];

        // 35. No terminal screen
        yield return
        [
            "no-terminal-screen",
            ToJson(MakeFlow(
                MakeScreen("FIRST", children: Children(
                    TextBody("Screen 1"), NavigateFooter("Next", "SECOND"))),
                MakeScreen("SECOND", children: Children(
                    TextBody("Screen 2"), NavigateFooter("Back", "FIRST"))))),
            "MISSING_TERMINAL_SCREEN"
        ];

        // 36. Complete action on non-terminal screen
        yield return
        [
            "complete-on-non-terminal",
            ToJson(MakeFlow(
                MakeScreen("FIRST", children: Children(
                    TextBody("Non-terminal with complete"),
                    CompleteFooter())),
                MakeScreen("SECOND", terminal: true, children: Children(
                    TextBody("Terminal"), CompleteFooter())))),
            "INVALID_COMPLETE_ACTION"
        ];

        // 37. Navigate to non-existent screen
        yield return
        [
            "navigate-to-nonexistent",
            ToJson(MakeFlow(
                MakeScreen("MAIN", children: Children(
                    TextBody("Going nowhere"),
                    NavigateFooter("Next", "DOES_NOT_EXIST"))),
                MakeScreen("END", terminal: true, children: Children(
                    TextBody("End"), CompleteFooter())))),
            "INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME"
        ];

        // 38. Navigate to self
        yield return
        [
            "navigate-to-self",
            ToJson(MakeFlow(
                MakeScreen("LOOP", children: Children(
                    TextBody("Self-referencing"),
                    NavigateFooter("Again", "LOOP"))),
                MakeScreen("END", terminal: true, children: Children(
                    TextBody("End"), CompleteFooter())))),
            "INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME"
        ];

        // 39. Terminal screen without Footer
        yield return
        [
            "terminal-without-footer",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("No footer here"))))),
            "MISSING_FOOTER_ON_TERMINAL"
        ];

        // 40. Routing model with > 10 branches from one screen
        {
            var branches = new JsonArray();
            var screens = new List<JsonObject>();
            for (var i = 1; i <= 11; i++)
            {
                branches.Add(JsonValue.Create($"BRANCH_{i}"));
                screens.Add(MakeScreen($"BRANCH_{i}", terminal: true, children: Children(
                    TextBody($"Branch {i}"), CompleteFooter())));
            }
            var routing = new JsonObject { ["START"] = branches };
            foreach (var s in screens)
                routing[$"{s["id"]}"] = new JsonArray();
            var allScreens = new List<JsonObject>
            {
                MakeScreen("START", children: Children(
                    TextBody("Too many branches"),
                    NavigateFooter("Go", "BRANCH_1")))
            };
            allScreens.AddRange(screens);
            var flow = MakeEndpointFlow(routing, [.. allScreens]);
            yield return ["routing-model-too-many-branches", ToJson(flow), "INVALID_ROUTING_MODEL"];
        }

        // 42. If component: Footer in then only (missing from else)
        yield return
        [
            "if-footer-in-then-only",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(("show", "boolean", "true")),
                    children: Children(
                        If("${data.show}",
                            Children(TextBody("Has footer"), CompleteFooter()),
                            Children(TextBody("No footer here"))))))),
            "MISSING_FOOTER_IN_BRANCH"
        ];

        // 43. Nested If depth > 3 levels
        yield return
        [
            "nested-if-exceeds-max-depth",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(
                        ("a", "boolean", "true"), ("b", "boolean", "true"),
                        ("c", "boolean", "true"), ("d", "boolean", "true")),
                    children: Children(
                        If("${data.a}",
                            Children(If("${data.b}",
                                Children(If("${data.c}",
                                    Children(If("${data.d}",
                                        Children(TextBody("Too deep"))))))))),
                        CompleteFooter())))),
            "MAX_NESTING_EXCEEDED"
        ];

        // 44. PhotoPicker and DocumentPicker on same screen
        yield return
        [
            "photo-and-document-picker-same-screen",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    Form("form", Children(
                        PhotoPicker("photo", "Upload Photo"),
                        DocumentPicker("doc", "Upload Doc"),
                        CompleteFooter())))))),
            "INCOMPATIBLE_COMPONENTS"
        ];

        // 45. More than 50 components on a screen
        {
            var manyChildren = new JsonArray();
            for (var i = 0; i < 51; i++)
                manyChildren.Add(TextBody($"Line {i + 1}"));
            manyChildren.Add(CompleteFooter());
            yield return
            [
                "exceeds-50-components",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true, children: manyChildren))),
                "MAX_COMPONENTS_EXCEEDED"
            ];
        }

        // 46. More than 2 EmbeddedLinks per screen
        yield return
        [
            "exceeds-2-embedded-links",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true, children: Children(
                    TextBody("Links: "),
                    EmbeddedLink("Link 1", "open_url", url: "https://example.com/1"),
                    EmbeddedLink("Link 2", "open_url", url: "https://example.com/2"),
                    EmbeddedLink("Link 3", "open_url", url: "https://example.com/3"),
                    CompleteFooter())))),
            "MAX_COMPONENT_COUNT_EXCEEDED"
        ];

        // 47. More than 5 OptIns per screen
        {
            var kids = new JsonArray();
            for (var i = 1; i <= 6; i++)
                kids.Add(OptIn($"opt_{i}", $"Option {i}"));
            kids.Add(CompleteFooter());
            yield return
            [
                "exceeds-5-optins",
                ToJson(MakeFlow(MakeScreen("MAIN", terminal: true,
                    children: Children(Form("form", kids))))),
                "MAX_COMPONENT_COUNT_EXCEEDED"
            ];
        }

        // 48. NavigationList on terminal screen (navigation requires non-terminal)
        {
            var items = new JsonArray();
            items.Add(NavListItem("Item A"));
            items.Add(NavListItem("Item B"));
            yield return
            [
                "navigation-list-on-terminal",
                ToJson(MakeFlow(
                    MakeScreen("MAIN", terminal: true, children: Children(
                        NavigationList("Pick one", "nav", "OTHER", items),
                        CompleteFooter())),
                    MakeScreen("OTHER", terminal: true, children: Children(
                        TextBody("Other"), CompleteFooter())))),
                "INVALID_COMPONENT_PLACEMENT"
            ];
        }

        // 49. Switch with empty cases
        yield return
        [
            "switch-empty-cases",
            ToJson(MakeFlow(
                MakeScreen("MAIN", terminal: true,
                    data: DataModel(("val", "string", "x")),
                    children: Children(
                        new JsonObject
                        {
                            ["type"] = "Switch",
                            ["value"] = "${data.val}",
                            ["cases"] = new JsonObject()
                        },
                        CompleteFooter())))),
            "MIN_ITEMS_REQUIRED"
        ];

        // 50. Screens as object instead of array
        {
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonObject { ["invalid"] = true }
            };
            yield return ["screens-as-object", ToJson(flow), "INVALID_PROPERTY_TYPE"];
        }

        // 51. Terminal property as string instead of boolean
        {
            var screen = new JsonObject
            {
                ["id"] = "MAIN",
                ["terminal"] = JsonValue.Create("yes"),
                ["layout"] = new JsonObject
                {
                    ["type"] = "SingleColumnLayout",
                    ["children"] = Children(
                        TextBody("Hello"),
                        CompleteFooter())
                }
            };
            var flow = new JsonObject
            {
                ["version"] = "7.3",
                ["screens"] = new JsonArray(screen)
            };
            yield return ["terminal-as-string", ToJson(flow), "INVALID_PROPERTY_TYPE"];
        }

        // 52. Routing model cycle (A→B→A)
        {
            var routing = new JsonObject
            {
                ["SCREEN_A"] = new JsonArray(JsonValue.Create("SCREEN_B")),
                ["SCREEN_B"] = new JsonArray(JsonValue.Create("SCREEN_A"))
            };
            yield return
            [
                "routing-model-loop",
                ToJson(MakeEndpointFlow(routing,
                    MakeScreen("SCREEN_A", children: Children(
                        TextBody("Screen A"),
                        NavigateFooter("Next", "SCREEN_B"))),
                    MakeScreen("SCREEN_B", terminal: true, children: Children(
                        TextBody("Screen B"),
                        CompleteFooter())))),
                "INVALID_ROUTING_MODEL"
            ];
        }
    }
}
