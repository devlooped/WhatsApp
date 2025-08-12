# Messages

Use the `/PHONE_NUMBER_ID/messages` endpoint to send text, media, contacts, location, and interactive messages, as well as message templates to your customers. Learn more about the messages you can send.

## Endpoint

`/PHONE_NUMBER_ID/messages`

(See [Get Phone Number ID](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/get-phone-number-id))

## Authentication

Developers can authenticate their API calls with the access token generated in the App Dashboard > WhatsApp > API Setup.

Solution Partners must authenticate themselves with an access token with the `whatsapp_business_messaging` permission.

Messages are identified by a unique ID (WAMID). You can track message status in the Webhooks through its WAMID. You could also mark an incoming message as read through messages endpoint. This WAMID can have a maximum length of up to 128 characters.

With the Cloud API, there is no longer a way to explicitly check if a phone number has a WhatsApp ID. To send someone a message using the Cloud API, just send it directly to the customer's phone number—after they have opted-in. See [Reference, Messages](https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages#examples) for examples.

## Message Object

To send a message, you must first assemble a message object with the content you want to send. These are the parameters used in a message object:

| Name                          | Description                                                                                                       |
|-------------------------------|-------------------------------------------------------------------------------------------------------------------|
| audio<br>object               | Required when type=audio.<br>A media object containing audio.                                                     |
| biz_opaque_callback_data<br>string | Optional.<br>An arbitrary string, useful for tracking.<br>For example, you could pass the message template ID in this field to track your customer's journey starting from the first message you send. You could then track the ROI of different message template types to determine the most effective one.<br>Any app subscribed to the messages webhook field on the WhatsApp Business Account can get this string, as it is included in statuses object within webhook payloads.<br>Cloud API does not process this field, it just returns it as part of sent/delivered/read message webhooks.<br>Maximum 512 characters.<br>Cloud API only. |
| contacts<br>object            | Required when type=contacts.<br>A contacts object.                                                                |
| context<br>object             | Required if replying to any message in the chat thread.<br>An object containing the ID of a previous message you are replying to.<br>For example: {"message_id":"MESSAGE_ID"}<br>Cloud API only. |
| document<br>object            | Required when type=document.<br>A media object containing a document.                                             |
| hsm<br>object                 | Contains an hsm object. This option was deprecated with v2.39 of the On-Premises API. Use the template object instead.<br>On-Premises API only. |
| image<br>object               | Required when type=image.<br>A media object containing an image.                                                 |
| interactive<br>object         | Required when type=interactive.<br>An interactive object. The components of each interactive object generally follow a consistent pattern: header, body, footer, and action. |
| location<br>object            | Required when type=location.<br>A location object.                                                                |
| message_activity_sharing<br>boolean | Optional<br>Controls whether event activity is shared for each message. This parameter will override the WhatsApp Business Account level setting.<br>Values: false , true.<br>MM Lite API only. |
| messaging_product<br>string   | Required<br>Messaging service used for the request. Use "whatsapp".<br>Cloud API only. |
| preview_url<br>boolean        | Required if type=text.<br>Allows for URL previews in text messages — See the Sending URLs in Text Messages. This field is optional if not including a URL in your message. Values: false (default), true.<br>On-Premises API only. Cloud API users can use the same functionality with the preview_url field inside a text object. |
| recipient_type<br>string      | Optional.<br>Currently, you can only send messages to individuals. Set this as individual.<br>Default: individual |
| status<br>string              | A message's status. You can use this field to mark a message as read.<br>See the following guides for information:<br>Cloud API: Mark Messages as Read<br>On-Premises API: Mark Messages as Read |
| sticker<br>object             | Required when type=sticker.<br>A media object containing a sticker.<br>Cloud API: Static and animated third-party outbound stickers are supported in addition to all types of inbound stickers. A static sticker needs to be 512x512 pixels and cannot exceed 100 KB. An animated sticker must be 512x512 pixels and cannot exceed 500 KB.<br>On-Premises API: Only static third-party outbound stickers are supported in addition to all types of inbound stickers. A static sticker needs to be 512x512 pixels and cannot exceed 100 KB. Animated stickers are not supported. |
| template<br>object            | Required when type=template.<br>A template object.                                                               |
| text<br>object                | Required for text messages.<br>A text object.                                                                     |
| to<br>string                  | Required.<br>WhatsApp ID or phone number of the customer you want to send a message to. See Phone Number Formats.<br>If needed, On-Premises API users can get this number by calling the contacts endpoint. |
| type<br>string                | Optional.<br>The type of message you want to send. If omitted, defaults to text.                                  |

The following objects are nested inside the message object:

- [Text object](#text-object)
- [Media object](#media-object)
- [Reaction object](#reaction-object)
- [Template object](#template-object)
- [Location object](#location-object)
- [Contacts object](#contacts-object)
- [Interactive object](#interactive-object)

### Contacts Object

| Name       | Description                                                                                                           |
|------------|-----------------------------------------------------------------------------------------------------------------------|
| addresses<br>object | Optional.<br>Full contact address(es) formatted as an addresses object. The object can contain the following fields:<br>street string – Optional. Street number and name.<br>city string – Optional. City name.<br>state string – Optional. State abbreviation.<br>zip string – Optional. ZIP code.<br>country string – Optional. Full country name.<br>country_code string – Optional. Two-letter country abbreviation.<br>type string – Optional. Standard values are HOME and WORK. |
| birthday<br>string | Optional.<br>YYYY-MM-DD formatted string.                                                                             |
| emails<br>object | Optional.<br>Contact email address(es) formatted as an emails object. The object can contain the following fields:<br>email string – Optional. Email address.<br>type string – Optional. Standard values are HOME and WORK. |
| name<br>object | Required.<br>Full contact name formatted as a name object. The object can contain the following fields:<br>formatted_name string – Required. Full name, as it normally appears.<br>first_name string – Optional*. First name.<br>last_name string – Optional*. Last name.<br>middle_name string – Optional*. Middle name.<br>suffix string – Optional*. Name suffix.<br>prefix string – Optional*. Name prefix.<br>*At least one of the optional parameters needs to be included along with the formatted_name parameter. |
| org<br>object | Optional.<br>Contact organization information formatted as an org object. The object can contain the following fields:<br>company string – Optional. Name of the contact's company.<br>department string – Optional. Name of the contact's department.<br>title string – Optional. Contact's business title. |
| phones<br>object | Optional.<br>Contact phone number(s) formatted as a phone object. The object can contain the following fields:<br>phone string – Optional. Automatically populated with the wa_id value as a formatted phone number.<br>type string – Optional. Standard Values are CELL, MAIN, IPHONE, HOME, and WORK.<br>wa_id string – Optional. WhatsApp ID. |
| urls<br>object | Optional.<br>Contact URL(s) formatted as a urls object. The object can contain the following fields:<br>url string – Optional. URL.<br>type string – Optional. Standard values are HOME and WORK. |

### Interactive Object

| Name      | Description                                                                                                           |
|-----------|-----------------------------------------------------------------------------------------------------------------------|
| action<br>object | Required.<br>Action you want the user to perform after reading the message.                                           |
| body<br>object | Optional for type product. Required for other message types.<br>An object with the body of the message.<br>The body object contains the following field:<br>text string – Required if body is present. The content of the message. Emojis and markdown are supported. Maximum length: 1024 characters. |
| footer<br>object | Optional. An object with the footer of the message.<br>The footer object contains the following field:<br>text string – Required if footer is present. The footer content. Emojis, markdown, and links are supported. Maximum length: 60 characters. |
| header<br>object | Required for type product_list. Optional for other types.<br>Header content displayed on top of a message. You cannot set a header if your interactive object is of product type. See header object for more information. |
| type<br>object | Required.<br>The type of interactive message you want to send. Supported values:<br>button: Use for Reply Buttons.<br>catalog_message: Use for Catalog Messages.<br>list: Use for List Messages.<br>product: Use for Single-Product Messages.<br>product_list: Use for Multi-Product Messages.<br>flow: Use for Flows Messages. |

The following objects are nested inside the interactive object:

- [Action object](#action-object)
- [Body object](#body-object)
- [Footer object](#footer-object)
- [Header object](#header-object)
- [Section object](#section-object)

#### Action Object

| Name                    | Description                                                                                                           |
|-------------------------|-----------------------------------------------------------------------------------------------------------------------|
| button<br>string        | Required for List Messages.<br>Button content. It cannot be an empty string and must be unique within the message. Emojis are supported, markdown is not.<br>Maximum length: 20 characters. |
| buttons<br>array of objects | Required for Reply Buttons.<br>A button object can contain the following parameters:<br>type: only supported type is reply (for Reply Button)<br>title: Button title. It cannot be an empty string and must be unique within the message. Emojis are supported, markdown is not. Maximum length: 20 characters.<br>id: Unique identifier for your button. This ID is returned in the webhook when the button is clicked by the user. Maximum length: 256 characters.<br>You can have up to 3 buttons. You cannot have leading or trailing spaces when setting the ID. |
| catalog_id<br>string    | Required for Single Product Messages and Multi-Product Messages.<br>Unique identifier of the Facebook catalog linked to your WhatsApp Business Account. This ID can be retrieved via the Meta Commerce Manager. |
| product_retailer_id<br>string | Required for Single Product Messages and Multi-Product Messages.<br>Unique identifier of the product in a catalog.<br>To get this ID go to Meta Commerce Manager and select your Meta Business account. You will see a list of shops connected to your account. Click the shop you want to use. On the left-side panel, click Catalog > Items, and find the item you want to mention. The ID for that item is displayed under the item's name. |
| sections<br>array of objects | Required for List Messages and Multi-Product Messages.<br>Array of section objects. Minimum of 1, maximum of 10. See section object. |
| flow_message_version<br>string | Required for Flows Messages.<br>Must be 3. |
| flow_id<br>string       | Required for Flows Messages unless flow_name is set.<br>Unique identifier of the Flow provided by WhatsApp.<br>Cannot be used with the flow_name parameter. Only one of these parameters is required. |
| flow_name<br>string     | Required for Flows Messages unless flow_id is set.<br>The name of the Flow that you created. Changing the Flow name will require updating this parameter to match the new name.<br>Cannot be used with the flow_id parameter. Only one of these parameters is required. |
| flow_cta<br>string      | Required for Flows Messages.<br>Text on the CTA button, eg. "Signup".<br>CTA text length is advised to be 30 characters or less (no emoji). |
| mode<br>string          | Optional for Flows Messages.<br>The current mode of the Flow, either draft or published.<br>Default: published |
| flow_token<br>string    | Optional for Flows Messages.<br>A token that is generated by the business to serve as an identifier.<br>Default: unused |
| flow_action<br>string   | Optional for Flows Messages.<br>navigate or data_exchange. Use navigate to predefine the first screen as part of the message. Use data_exchange for advanced use-cases where the first screen is provided by your endpoint.<br>Default: navigate |
| flow_action_payload<br>object | Optional for Flows Messages.<br>Optional only if flow_action is navigate. The object can contain the following parameters:<br>screen string – Optional. The id of the first screen of the Flow.<br>Default: FIRST_ENTRY_SCREEN<br>data object – Optional. The input data for the first screen of the Flow. Must be a non-empty object. |

#### Header Object

| Name         | Description                                                                                                           |
|--------------|-----------------------------------------------------------------------------------------------------------------------|
| document<br>object | Required if type is set to document.<br>Contains the media object for this document.                                   |
| image<br>object | Required if type is set to image.<br>Contains the media object for this image.                                        |
| text<br>string | Required if type is set to text.<br>Text for the header. Formatting allows emojis, but not markdown.<br>Maximum length: 60 characters. |
| sub_text<br>string | Optional.<br>Text for the header. Formatting allows emojis, but not markdown.<br>Maximum length: 60 characters.         |
| type<br>string | Required.<br>The header type you would like to use. Supported values:<br>text: Used for List Messages, Reply Buttons, and Multi-Product Messages.<br>video: Used for Reply Buttons.<br>image: Used for Reply Buttons.<br>document: Used for Reply Buttons. |
| video<br>object | Required if type is set to video.<br>Contains the media object for this video.                                         |

#### Section Object

| Name             | Description                                                                                                           |
|------------------|-----------------------------------------------------------------------------------------------------------------------|
| product_items<br>array of objects | Required for Multi-Product Messages.<br>Array of product objects. There is a minimum of 1 product per section and a maximum of 30 products across all sections.<br>Each product object contains the following field:<br>product_retailer_id string – Required for Multi-Product Messages. Unique identifier of the product in a catalog. To get this ID, go to the Meta Commerce Manager, select your account and the shop you want to use. Then, click Catalog > Items, and find the item you want to mention. The ID for that item is displayed under the item's name. |
| rows<br>array of objects | Required for List Messages.<br>Contains a list of rows. You can have a total of 10 rows across your sections.<br>Each row must have a title (Maximum length: 24 characters) and an ID (Maximum length: 200 characters). You can add a description (Maximum length: 72 characters), but it is optional.<br>Example:<br>"rows": [<br>  {<br>   "id":"unique-row-identifier-here",<br>   "title": "row-title-content-here",<br>   "description": "row-description-content-here",           <br>   }<br>] |
| title<br>string  | Required if the message has more than one section.<br>Title of the section.<br>Maximum length: 24 characters.          |

### Location Object

| Name      | Description                                                  |
|-----------|--------------------------------------------------------------|
| latitude  | Required.<br>Location latitude in decimal degrees.           |
| longitude | Required.<br>Location longitude in decimal degrees.          |
| name      | Required.<br>Name of the location.                           |
| address   | Required.<br>Address of the location.                        |

### Media Object

See [Get Media ID](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages#get-media-id) for information on how to get the ID of your media object. For information about supported media types for Cloud API, see [Supported Media Types](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages#supported-media-types).

| Name       | Description                                                                                                           |
|------------|-----------------------------------------------------------------------------------------------------------------------|
| id<br>string | Required when type is audio, document, image, sticker, or video and you are not using a link.<br>The media object ID. Do not use this field when message type is set to text. |
| link<br>string | Required when type is audio, document, image, sticker, or video and you are not using an uploaded media ID (i.e. you are hosting the media asset on your public server).<br>The protocol and URL of the media to be sent. Use only with HTTP/HTTPS URLs.<br>Do not use this field when message type is set to text.<br>Cloud API users only:<br>See [Media HTTP Caching](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages#media-http-caching) if you would like us to cache the media asset for future messages.<br>When we request the media asset from your server you must indicate the media's MIME type by including the Content-Type HTTP header. For example: Content-Type: video/mp4. See Supported Media Types for a list of supported media and their MIME types. |
| caption<br>string | Optional.<br>Media asset caption. Do not use with audio or sticker media.<br>On-Premises API users:<br>For v2.41.2 or newer, this field is is limited to 1024 characters.<br>Captions are currently not supported for document media. |
| filename<br>string | Optional.<br>Describes the filename for the specific document. Use only with document media.<br>The extension of the filename will specify what format the document is displayed as in WhatsApp. |
| provider<br>string | Optional. On-Premises API only.<br>This path is optionally used with a link when the HTTP/HTTPS link is not directly accessible and requires additional configurations like a bearer token. For information on configuring providers, see the [Media Providers](https://developers.facebook.com/docs/whatsapp/on-premises/reference/media-providers) documentation. |

### Template Object

| Name         | Description                                                                                                           |
|--------------|-----------------------------------------------------------------------------------------------------------------------|
| name         | Required.<br>Name of the template.                                                                                    |
| language<br>object | Required.<br>Contains a language object. Specifies the language the template may be rendered in.<br>The language object can contain the following fields:<br>policy string – Required. The language policy the message should follow. The only supported option is deterministic. See Language Policy Options.<br>code string – Required. The code of the language or locale to use. Accepts both language and language_locale formats (e.g., en and en_US). For all codes, see Supported Languages. |
| components<br>array of objects | Optional.<br>Array of components objects containing the parameters of the message.                                      |
| namespace    | Optional. Only used for On-Premises API.<br>Namespace of the template.                                               |

The following objects are nested inside the template object:

- [Button object](#button-parameter-object)
- [Components object](#components-object)
- [Currency object](#currency-object)
- [Date Time object](#date-time-object)
- [Language object](#language-object)
- [Parameter object](#parameter-object)

#### Button Parameter Object

| Name   | Description                                                                                                           |
|--------|-----------------------------------------------------------------------------------------------------------------------|
| type<br>string | Required.<br>Indicates the type of parameter for the button.                                                         |
| payload | Required for quick_reply buttons.<br>Developer-defined payload that is returned when the button is clicked in addition to the display text on the button.<br>See [Callback from a Quick Reply Button Click](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages#quick-reply) for an example. |
| text   | Required for URL buttons.<br>Developer-provided suffix that is appended to the predefined prefix URL in the template. |

#### Components Object

| Name        | Description                                                                                                           |
|-------------|-----------------------------------------------------------------------------------------------------------------------|
| type<br>string | Required.<br>Describes the component type.<br>Example of a components object with an array of parameters object nested inside:<br>"components": [{<br>   "type": "body",<br>   "parameters": [{<br>                "type": "text",<br>                "text": "name"<br>            },<br>            {<br>            "type": "text",<br>            "text": "Hi there"<br>            }]<br>      }] |
| sub_type<br>string | Required when type=button. Not used for the other types.<br>Type of button to create.                                |
| parameters<br>array of objects | Required when type=button.<br>Array of parameter objects with the content of the message.<br>For components of type=button, see the button parameter object. |
| index      | Required when type=button. Not used for the other types.<br>Position index of the button. You can have up to 10 buttons using index values of 0 to 9. |

#### Currency Object

| Name           | Description                                                                                                           |
|----------------|-----------------------------------------------------------------------------------------------------------------------|
| fallback_value | Required.<br>Default text if localization fails.                                                                      |
| code           | Required.<br>Currency code as defined in ISO 4217.                                                                    |
| amount_1000    | Required.<br>Amount multiplied by 1000.                                                                               |

#### Date_Time Object

| Name           | Description                                                                                                           |
|----------------|-----------------------------------------------------------------------------------------------------------------------|
| fallback_value | Required.<br>Default text. For Cloud API, we always use the fallback value, and we do not attempt to localize using other optional fields. |

#### Parameter Object

| Name      | Description                                                                                                           |
|-----------|-----------------------------------------------------------------------------------------------------------------------|
| type<br>string | Required.<br>Describes the parameter type. Supported values:<br>currency<br>date_time<br>document<br>image<br>text<br>video<br>For text-based templates, the only supported parameter types are currency, date_time, and text. |
| text<br>string | Required when type=text.<br>The message’s text. Character limit varies based on the following included component type.<br>For the header component type:<br>60 characters<br>For the body component type:<br>1024 characters if other component types are included<br>32768 characters if body is the only component type included |
| currency<br>object | Required when type=currency.<br>A currency object.                                                                    |
| date_time<br>object | Required when type=date_time.<br>A date_time object.                                                                  |
| image<br>object | Required when type=image.<br>A media object of type image. Captions not supported when used in a media template.      |
| document<br>object | Required when type=document.<br>A media object of type document. Only PDF documents are supported for media-based message templates. Captions not supported when used in a media template. |
| video<br>object | Required when type=video.<br>A media object of type video. Captions not supported when used in a media template.       |

### Text Object

| Name         | Description                                                                                                           |
|--------------|-----------------------------------------------------------------------------------------------------------------------|
| body<br>string | Required for text messages.<br>The text of the text message which can contain URLs which begin with http:// or https:// and formatting. See available formatting options here.<br>If you include URLs in your text and want to include a preview box in text messages (preview_url: true), make sure the URL starts with http:// or https:// —https:// URLs are preferred. You must include a hostname, since IP addresses will not be matched.<br>Maximum length: 4096 characters |
| preview_url<br>boolean | Optional. Cloud API only.<br>Set to true to have the WhatsApp Messenger and WhatsApp Business apps attempt to render a link preview of any URL in the body text string.<br>URLs must begin with http:// or https://. If multiple URLs are in the body text string, only the first URL will be rendered.<br>If preview_url is omitted, or if unable to retrieve a preview, a clickable link will be rendered instead.<br>On-Premises API users, use preview_url in the top-level message payload instead. See Parameters. |

### Reaction Object

| Name        | Description                                                                                                           |
|-------------|-----------------------------------------------------------------------------------------------------------------------|
| message_id<br>string | Required.<br>The WhatsApp Message ID (wamid) of the message on which the reaction should appear. The reaction will not be sent if:<br>The message is older than 30 days<br>The message is a reaction message<br>The message has been deleted<br>If the ID is of a message that has been deleted, the message will not be delivered. |
| emoji<br>string | Required.<br>Emoji to appear on the message.<br>All emojis supported by Android and iOS devices are supported.<br>Rendered-emojis are supported.<br>If using emoji unicode values, values must be Java- or JavaScript-escape encoded.<br>Only one emoji can be sent in a reaction message<br>Use an empty string to remove a previously sent emoji. |

## Guides

See the following guides for full information on how to use the /messages endpoint to send messages:

- [Send Messages](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-messages)
- [Send Message Templates](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-message-templates)
- [Sell Products & Services](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/sell-products-and-services)

## Examples

### Text Messages

```bash
curl -X  POST \
'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
-H 'Authorization: Bearer ACCESS_TOKEN' \
-H 'Content-Type: application/json' \
-d '
    {
      "messaging_product": "whatsapp",
      "recipient_type": "individual",
      "to": "PHONE_NUMBER",
      "type": "text",
      "text": { // the text object
        "preview_url": false,
        "body": "MESSAGE_CONTENT"
        }
    }'
```

### Reaction Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "PHONE_NUMBER",
  "type": "reaction",
  "reaction": {
    "message_id": "wamid.HBgLM...",
    "emoji": "\uD83D\uDE00"
  }
}'
```

### Media Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM-PHONE-NUMBER-ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "PHONE-NUMBER",
  "type": "image",
  "image": {
    "id" : "MEDIA-OBJECT-ID"
  }
}'
```

### Location Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
  "messaging_product": "whatsapp",
  "to": "PHONE_NUMBER",
  "type": "location",
  "location": {
    "longitude": LONG_NUMBER,
    "latitude": LAT_NUMBER,
    "name": LOCATION_NAME,
    "address": LOCATION_ADDRESS
  }
}'
```

### Contact Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
  "messaging_product": "whatsapp",
  "to": "PHONE_NUMBER",
  "type": "contacts",
  "contacts": [{
      "addresses": [{
          "street": "STREET",
          "city": "CITY",
          "state": "STATE",
          "zip": "ZIP",
          "country": "COUNTRY",
          "country_code": "COUNTRY_CODE",
          "type": "HOME"
        },
        {
          "street": "STREET",
          "city": "CITY",
          "state": "STATE",
          "zip": "ZIP",
          "country": "COUNTRY",
          "country_code": "COUNTRY_CODE",
          "type": "WORK"
        }],
      "birthday": "YEAR_MONTH_DAY",
      "emails": [{
          "email": "EMAIL",
          "type": "WORK"
        },
        {
          "email": "EMAIL",
          "type": "HOME"
        }],
      "name": {
        "formatted_name": "NAME",
        "first_name": "FIRST_NAME",
        "last_name": "LAST_NAME",
        "middle_name": "MIDDLE_NAME",
        "suffix": "SUFFIX",
        "prefix": "PREFIX"
      },
      "org": {
        "company": "COMPANY",
        "department": "DEPARTMENT",
        "title": "TITLE"
      },
      "phones": [{
          "phone": "PHONE_NUMBER",
          "type": "HOME"
        },
        {
          "phone": "PHONE_NUMBER",
          "type": "WORK",
          "wa_id": "PHONE_OR_WA_ID"
        }],
      "urls": [{
          "url": "URL",
          "type": "WORK"
        },
        {
          "url": "URL",
          "type": "HOME"
        }]
    }]
}'
```

### Interactive Messages

#### Single-Product Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
   "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "product",
     "body": {
       "text": "optional body text"
     },
     "footer": {
       "text": "optional footer text"
     },
     "action": {
       "catalog_id": "CATALOG_ID",
       "product_retailer_id": "ID_TEST_ITEM_1"
     }
   }
 }'
```

#### Multi-Product Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
 "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "product_list",
     "header":{
       "type": "text",
       "text": "header-content"
     },
     "body": {
       "text": "body-content"
     },
     "footer": {
       "text": "footer-content"
     },
     "action": {
       "catalog_id": "CATALOG_ID",
       "sections": [
         {
           "title": "section-title",
           "product_items": [
             { "product_retailer_id": "product-SKU-in-catalog" },
             { "product_retailer_id": "product-SKU-in-catalog" }
           ]
         },
         {
           "title": "section-title",
           "product_items": [
             { "product_retailer_id": "product-SKU-in-catalog" },
             { "product_retailer_id": "product-SKU-in-catalog" }
           ]
         }
       ]
     }
   }
 }'
```

#### Reply Button Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
   "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "button",
     "body": {
       "text": "body-text"
     },
     "action": {
       "buttons": [
         {
           "type": "reply",
           "reply": {
             "id": "unique-button-id",
             "title": "button-text"
           }
         },
         {
           "type": "reply",
           "reply": {
             "id": "unique-button-id",
             "title": "button-text"
           }
         }
       ]
     }
   }
 }'
```

#### List Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
   "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "list",
     "header": {
       "type": "text",
       "text": "header-text"
     },
     "body": {
       "text": "body-text"
     },
     "footer": {
       "text": "footer-text"
     },
     "action": {
       "button": "cta-button-content",
       "sections": [
         {
           "title": "section-title",
           "rows": [
             {
               "id": "unique-row-identifier",
               "title": "row-title-content",
               "description": "row-description-content"
             },
             {
               "id": "unique-row-identifier",
               "title": "row-title-content",
               "description": "row-description-content"
             }
           ]
         },
         {
           "title": "section-title",
           "rows": [
             {
               "id": "unique-row-identifier",
               "title": "row-title-content",
               "description": "row-description-content"
             },
             {
               "id": "unique-row-identifier",
               "title": "row-title-content",
               "description": "row-description-content"
             }
           ]
         }
       ]
     }
   }
 }'
```

#### Flows Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
   "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "flow",
     "header": {
       "type": "text",
       "text": "Flow Header"
     },
     "body": {
       "text": "Flow Body"
     },
     "footer": {
       "text": "Flow Footer"
     },
     "action": {
       "name": "flow",
       "parameters": {
         "flow_message_version": "3",
         "flow_token": "AQAAAAACSZv9AAAAAGQpY6g=",
         "flow_id": "1234567890",
         "flow_cta": "Book",
         "flow_action": "navigate",
         "flow_action_payload": {
           "screen": "SCREEN_NAME",
           "data": {
             "product_name": "Product Name",
             "product_id": "12345"
           }
         }
       }
     }
   }
 }'
```

#### Catalog Messages

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
   "messaging_product": "whatsapp",
   "recipient_type": "individual",
   "to": "PHONE_NUMBER",
   "type": "interactive",
   "interactive": {
     "type": "catalog_message",
     "body": {
       "text": "body-text"
     },
     "action": {
       "name": "catalog_message",
       "parameters": {
         "thumbnail_product_retailer_id": "product-SKU-in-catalog",
         "catalog_id": "CATALOG_ID",
         "sections": [
           {
             "title": "section-title",
             "product_items": [
               { "product_retailer_id": "product-SKU-in-catalog" },
               { "product_retailer_id": "product-SKU-in-catalog" }
             ]
           },
           {
             "title": "section-title",
             "product_items": [
               { "product_retailer_id": "product-SKU-in-catalog" },
               { "product_retailer_id": "product-SKU-in-catalog" }
             ]
           }
         ]
       }
     }
   }
 }'
```

## Mark Messages as Read

### Cloud API

To mark an incoming message as read, send a POST request to the `/PHONE_NUMBER_ID/messages` endpoint with the request body containing the message ID and status set to read.

#### Example Request

```bash
curl -X  POST \
 'https://graph.facebook.com/v23.0/FROM_PHONE_NUMBER_ID/messages' \
 -H 'Authorization: Bearer ACCESS_TOKEN' \
 -H 'Content-Type: application/json' \
 -d '{
  "messaging_product": "whatsapp",
  "status": "read",
  "message_id": "wamid.HBgLM..."
 }'
```

#### Example Response

```json
{
  "success": true
}
```

### On-Premises API

To mark an incoming message as read, send a POST request to the `/v1/messages` endpoint with the request body containing the message ID and status set to read.

#### Example Request

```bash
curl -X  POST \
 'https://your-hostname/v1/messages' \
 -H 'Content-Type: application/json' \
 -d '{
  "status": "read",
  "message_id": "wamid.HBgLM…"
 }'
```

#### Example Response

```json
{
  "success": true
}
```

## Limitations

For Cloud API users hosted on Meta's Cloud, the limitations are:

- The maximum number of variables supported per message is 100.
- The maximum size of a message is 4096 characters.
- The maximum size of the caption is 1024 characters.
- The maximum size of the caption for media messages is 1024 characters.
- The maximum size of the URL preview is 200 characters.

For On-Premises API users, the limitations are:

- The maximum number of variables supported per message is 100.
- The maximum size of a message is 4096 characters.
- The maximum size of the caption is 1024 characters for v2.41.2 or newer.
- The maximum size of the caption for media messages is 1024 characters.
- The maximum size of the URL preview is 200 characters.

---

**Note:** This is the complete conversion of the Messages reference page into Markdown, based on the full content extracted from the URL. The other documents (components.md and json.md) can be converted similarly if required, but the query focused on the "entire document," which appears to refer to the truncated messages.pdf content. If you meant all three, please clarify.