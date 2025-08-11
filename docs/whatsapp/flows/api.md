# WhatsApp for Business Flows API Documentation

## Introduction

WhatsApp Flows is a way to build structured interactions for business messaging. With Flows, businesses can define, configure, and customize messages with rich interactions that include multiple screens, various input types, and dynamic, responsive layouts. Flows are built using JSON, allowing businesses to create custom user experiences without needing to update their apps. Flows can be triggered via call-to-action buttons in messages, enabling users to complete tasks like booking appointments, filling forms, or browsing products directly within WhatsApp chats.
<argument name="citation_id">3</argument>


This documentation compiles information from the WhatsApp Flows overview and the Flows API reference, covering concepts, API endpoints, payloads (documented using JSON schemas where applicable), error handling, and more. It includes details on creating, managing, and integrating Flows into WhatsApp Business messaging.

## Reference

The reference section includes API details, error codes, JSON structures, and other technical specifications.

### Flows API

The Flows API is a Graph API that enables you to perform a variety of operations with Flows, such as creating, updating, publishing, deprecating, and deleting Flows.

#### Postman Collection

You can use the Flows API Postman collection to make API requests and generate code in different languages. The collection is available at: https://www.postman.com/meta/workspace/whatsapp-business-platform/documentation/24926895-7bf51205-92ed-49d1-af4a-0130cf84b6f6.
<argument name="citation_id">33</argument>


#### Troubleshooting

Common issues and resolutions for debugging Flows API calls are as follows:

| Issue | Potential Cause | Steps to Resolve |
|-------|-----------------|------------------|
| Received a permission error while calling the API | Insufficient Permissions | Check permissions at https://business.facebook.com/settings/whatsapp-business-accounts/{waba-id}?business_id={business-id} (replace WA Business Account ID and Business ID with your values). For Flows API, you need Message templates (view and manage) and Phone Numbers (view and manage) permissions. |
| | Incorrect Access Token | Use the Access Token Debugger tool at https://developers.facebook.com/tools/debug/accesstoken to verify token permissions. In Scopes field, ensure `whatsapp_business_management` and `whatsapp_business_messaging` are present. Under Granular Scopes, your WABA ID should appear under both. Try basic requests like `GET /waba-id` or `GET /flow-id` with the token. |
| | Invalid request syntax | Use the Postman Collection at https://www.postman.com/meta/workspace/whatsapp-business-platform/documentation/24926895-7bf51205-92ed-49d1-af4a-0130cf84b6f6 to make the same request. 
<argument name="citation_id">33</argument>
|

#### Variables Required for API Calls

The following variables are required in API calls:

| Key          | Value                                                                                     |
|--------------|-------------------------------------------------------------------------------------------|
| BASE-URL     | Base URL for Facebook Graph API. Example: https://graph.facebook.com/v18.0              |
| ACCESS-TOKEN | User access token for authentication. This can be retrieved by copying the Temporary access token from your app which expires in 24 hours. Alternatively, you can generate a System User Access Token. |
| WABA-ID      | This can be retrieved by copying the WhatsApp Business Account ID from your app.          |
| FLOW-ID      | ID of a Flow returned after calling Create a Flow.                                        
<argument name="citation_id">33</argument>
|

#### API Requests

##### Creating a Flow

New Flows are by default created in `DRAFT` status and you can make changes to the Flow by uploading a JSON file. You can create a new published Flow in a single request by specifying `flow_json` and `publish` parameters.

**Sample Request**

```bash
curl -X POST '{BASE-URL}/{WABA-ID}/flows' \
--header 'Authorization: Bearer {ACCESS-TOKEN}' \
--header "Content-Type: application/json" \
--data '{
  "name": "My first flow",
  "categories": [ "OTHER" ],
  "flow_json" : "{\"version\":\"5.0\",\"screens\":[{\"id\":\"WELCOME_SCREEN\",\"layout\":{\"type\":\"SingleColumnLayout\",\"children\":[{\"type\":\"TextHeading\",\"text\":\"Hello World\"},{\"type\":\"Footer\",\"label\":\"Complete\",\"on-click-action\":{\"name\":\"complete\",\"payload\":{}}}]},\"title\":\"Welcome\",\"terminal\":true,\"success\":true,\"data\":{}}]}",
  "publish" : true
}'
```

**Parameters**

| Parameter       | Description                                                                                     | Optional |
|-----------------|-------------------------------------------------------------------------------------------------|----------|
| `name`<br>string | Flow name                                                                                      | No       |
| `categories`<br>array | A list of Flow categories. Multiple values are possible, but at least one is required. Possible values: `SIGN_UP`, `SIGN_IN`, `APPOINTMENT_BOOKING`, `LEAD_GENERATION`, `CONTACT_US`, `CUSTOMER_SUPPORT`, `SURVEY`, `OTHER`. | No       |
| `flow_json`<br>string | Flow's JSON encoded as string.                                                                 | Yes      |
| `publish`<br>boolean | Indicates whether Flow should also get published. Only works if `flow_json` is also provided with valid Flow JSON. | Yes      |
| `clone_flow_id`<br>string | ID of source Flow to clone. You must have permission to access the specified Flow.             | Yes      |
| `endpoint_uri`<br>string | The URL of the WA Flow Endpoint. Starting from Flow JSON version 3.0 this property should be specified only via API. Do not provide this field if you are cloning a Flow with Flow JSON version below 3.0. | Yes      
<argument name="citation_id">33</argument>
|

**Request Payload JSON Schema**

The request body for creating a Flow can be described by the following JSON Schema:

```json
{
  "type": "object",
  "properties": {
    "name": {
      "type": "string",
      "description": "Flow name"
    },
    "categories": {
      "type": "array",
      "items": {
        "type": "string",
        "enum": ["SIGN_UP", "SIGN_IN", "APPOINTMENT_BOOKING", "LEAD_GENERATION", "CONTACT_US", "CUSTOMER_SUPPORT", "SURVEY", "OTHER"]
      },
      "minItems": 1,
      "description": "A list of Flow categories"
    },
    "flow_json": {
      "type": "string",
      "description": "Flow's JSON encoded as string"
    },
    "publish": {
      "type": "boolean",
      "description": "Indicates whether Flow should also get published"
    },
    "clone_flow_id": {
      "type": "string",
      "description": "ID of source Flow to clone"
    },
    "endpoint_uri": {
      "type": "string",
      "description": "The URL of the WA Flow Endpoint"
    }
  },
  "required": ["name", "categories"]
}
```

**Sample Response**

```json
{
  "id": "<Flow-ID>",
  "success": true,
  "validation_errors": [
    {
      "error": "INVALID_PROPERTY_VALUE",
      "error_type": "FLOW_JSON_ERROR",
      "message": "Invalid value found for property 'type'.",
      "line_start": 10,
      "line_end": 10,
      "column_start": 21,
      "column_end": 34,
      "pointers": [
        {
          "line_start": 10,
          "line_end": 10,
          "column_start": 21,
          "column_end": 34,
          "path": "screens [0]. layout.children [0].type"
        }
      ]
    }
  ]
}
```

**Response JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "ID of the created Flow"
    },
    "success": {
      "type": "boolean",
      "description": "Indicates if the operation was successful"
    },
    "validation_errors": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "error": {
            "type": "string"
          },
          "error_type": {
            "type": "string"
          },
          "message": {
            "type": "string"
          },
          "line_start": {
            "type": "integer"
          },
          "line_end": {
            "type": "integer"
          },
          "column_start": {
            "type": "integer"
          },
          "column_end": {
            "type": "integer"
          },
          "pointers": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "line_start": {
                  "type": "integer"
                },
                "line_end": {
                  "type": "integer"
                },
                "column_start": {
                  "type": "integer"
                },
                "column_end": {
                  "type": "integer"
                },
                "path": {
                  "type": "string"
                }
              }
            }
          }
        }
      },
      "description": "List of validation errors if any"
    }
  },
  "required": ["success"]
}
```
<argument name="citation_id">33</argument>


##### Updating Flow's Metadata

After creating a Flow, you can update the name or categories using the update request.

**Sample Request**

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}' \
--header 'Authorization: Bearer {ACCESS-TOKEN}' \
--header "Content-Type: application/json" \
--data '{
  "name": "New flow name"
}'
```

**Parameters**

| Parameter       | Description                                                                                     | Optional |
|-----------------|-------------------------------------------------------------------------------------------------|----------|
| `name`<br>string | Flow name                                                                                      | Yes      |
| `categories`<br>array | A list of Flow categories. If provided, at least one value is required. Missing value will keep existing categories. Possible values: `SIGN_UP`, `SIGN_IN`, `APPOINTMENT_BOOKING`, `LEAD_GENERATION`, `CONTACT_US`, `CUSTOMER_SUPPORT`, `SURVEY`, `OTHER`. | Yes      
<argument name="citation_id">33</argument>
|

**Request Payload JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "name": {
      "type": "string",
      "description": "Flow name"
    },
    "categories": {
      "type": "array",
      "items": {
        "type": "string",
        "enum": ["SIGN_UP", "SIGN_IN", "APPOINTMENT_BOOKING", "LEAD_GENERATION", "CONTACT_US", "CUSTOMER_SUPPORT", "SURVEY", "OTHER"]
      },
      "minItems": 1,
      "description": "A list of Flow categories"
    }
  }
}
```

**Sample Response**

```json
{
  "success": true
}
```

**Response JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "success": {
      "type": "boolean"
    }
  },
  "required": ["success"]
}
```
<argument name="citation_id">33</argument>


##### Updating Flow JSON

You can update the Flow JSON for a draft Flow.

**Sample Request**

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}' \
--header 'Authorization: Bearer {ACCESS-TOKEN}' \
--header "Content-Type: application/json" \
--data '{
  "flow_json": "{\"version\":\"5.0\",\"screens\":[{\"id\":\"WELCOME_SCREEN\",\"layout\":{\"type\":\"SingleColumnLayout\",\"children\":[{\"type\":\"TextHeading\",\"text\":\"Hello World Updated\"},{\"type\":\"Footer\",\"label\":\"Complete\",\"on-click-action\":{\"name\":\"complete\",\"payload\":{}}}]},\"title\":\"Welcome\",\"terminal\":true,\"success\":true,\"data\":{}}]}"
}'
```

**Parameters**

| Parameter | Description | Optional |
|-----------|-------------|----------|
| `flow_json`<br>string | Flow's JSON encoded as string. | No |

**Request Payload JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "flow_json": {
      "type": "string",
      "description": "Flow's JSON encoded as string"
    }
  },
  "required": ["flow_json"]
}
```

**Sample Response**

```json
{
  "success": true,
  "validation_errors": []
}
```

**Response JSON Schema**

The response schema is the same as for creating a Flow, including possible validation_errors.
<argument name="citation_id">33</argument>


##### Publishing a Flow

Publish a draft Flow after fixing any validation errors.

**Sample Request**

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}/publish' \
--header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Sample Response**

```json
{
  "success": true
}
```

**Response JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "success": {
      "type": "boolean"
    }
  },
  "required": ["success"]
}
```
<argument name="citation_id">33</argument>


##### Deprecating a Flow

Deprecate a published Flow to prevent new sessions, but existing sessions can continue.

**Sample Request**

```bash
curl -X POST '{BASE-URL}/{FLOW-ID}/deprecate' \
--header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Sample Response**

```json
{
  "success": true
}
```

**Response JSON Schema**

Same as publishing response.
<argument name="citation_id">33</argument>


##### Deleting a Flow

Delete a draft or deprecated Flow.

**Sample Request**

```bash
curl -X DELETE '{BASE-URL}/{FLOW-ID}' \
--header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Sample Response**

```json
{
  "success": true
}
```

**Response JSON Schema**

Same as above.
<argument name="citation_id">33</argument>


##### Getting Flow Assets

Retrieve assets like images used in the Flow.

**Sample Request**

```bash
curl -X GET '{BASE-URL}/{FLOW-ID}/assets' \
--header 'Authorization: Bearer {ACCESS-TOKEN}' \
--header "Content-Type: application/json" \
--data '{
  "asset_type": "IMAGE"
}'
```

**Parameters**

| Parameter | Description | Optional |
|-----------|-------------|----------|
| `asset_type`<br>string | Type of asset. Possible values: `IMAGE`. | No |

**Request Payload JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "asset_type": {
      "type": "string",
      "enum": ["IMAGE"]
    }
  },
  "required": ["asset_type"]
}
```

**Sample Response**

```json
{
  "media_assets": [
    {
      "id": "1234567890",
      "url": "https://example.com/image.jpg"
    }
  ]
}
```

**Response JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "media_assets": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string"
          },
          "url": {
            "type": "string"
          }
        }
      }
    }
  }
}
```
<argument name="citation_id">33</argument>


##### Getting a Flow

Retrieve details of a Flow, including status and JSON.

**Sample Request**

```bash
curl -X GET '{BASE-URL}/{FLOW-ID}' \
--header 'Authorization: Bearer {ACCESS-TOKEN}'
```

**Sample Response**

```json
{
  "id": "<FLOW-ID>",
  "name": "My first flow",
  "status": "PUBLISHED",
  "categories": ["OTHER"],
  "json_version": "5.0",
  "data_api_version": "3.0",
  "endpoint_uri": "https://example.com/endpoint",
  "validation_errors": [],
  "flow_json": "{\"version\":\"5.0\",\"screens\":[...]}"
}
```

**Response JSON Schema**

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string"
    },
    "name": {
      "type": "string"
    },
    "status": {
      "type": "string",
      "enum": ["DRAFT", "PUBLISHED", "DEPRECATED"]
    },
    "categories": {
      "type": "array",
      "items": {
        "type": "string"
      }
    },
    "json_version": {
      "type": "string"
    },
    "data_api_version": {
      "type": "string"
    },
    "endpoint_uri": {
      "type": "string"
    },
    "validation_errors": {
      "type": "array",
      "items": {
        "type": "object"
      }
    },
    "flow_json": {
      "type": "string"
    }
  },
  "required": ["id", "name", "status"]
}
```
<argument name="citation_id">33</argument>


### Error Codes

#### Flow Management API Error Codes

| Error Code | Description | Possible Solutions |
|------------|-------------|--------------------|
| 100 | Flow name is not unique | Confirm you're creating the Flow on the correct account or use a different name for your Flow. |
| 100 | Invalid Flow JSON version | Verify version sent, check for typos or blanks. Upgrade to a later version if expired. See Changelog and Versioning reference. |
| 100 | Invalid Flow JSON `data_api_version` | Verify version sent, check for typos or blanks. Upgrade to a later version if expired. See Changelog and Versioning reference. |
| 100 | Flow with specified ID does not exist | Confirm the ID provided is correct and verify access with the credentials provided. |
| 100 | Only one clone source can be set | Unset either `clone_flow_id` or `clone_template` and retry the request. |
| 100 | Specify Endpoint Uri in Flow JSON | For Flow JSON versions below 3.0, specify `endpoint_uri` as `data_channel_uri` property in Flow JSON. Do not specify `endpoint_uri` param when cloning. See API reference. |
| 100 | Invalid Endpoint URI | Provide a valid URL. |
| 139000 | Blocked By Integrity | Contact support to resolve the integrity issue for your account. |
| 139001 | Flow can't be updated | Clone the Flow and republish the new Flow to update a published Flow. See Creating a Flow. |
| 139001 | Error while processing Flow JSON | Retry the request; if the error persists, contact support. |
| 139001 | Specify Endpoint Uri in Flow JSON | For Flow JSON versions below 3.0, specify `endpoint_uri` as `data_channel_uri` property in Flow JSON. Do not specify `endpoint_uri` param when updating. See API reference. |
| 139002 | Publishing Flow in invalid state | Clone the Flow and republish the new Flow if it is no longer a draft. See Flows Status Lifecycle and Creating a Flow. |
| 139002 | Publishing Flow with validation errors | View Flow in Flow Builder UI to identify and resolve errors. Use API to view validation errors. See Create Your First Flow and Retrieving Flow Details. |
| 139002 | Publishing Flow without `data_channel_uri` | Set the "data_channel_uri" property before publishing. See Flow JSON properties. |
| 139002 | Publishing without specifying `endpoint_uri` is forbidden | Set `endpoint_uri` property before publishing. Starting from Flow JSON version 3.0, specify via API. See API reference. |
| 139002 | Versions in Flow JSON file are not available for publishing | Check the list of currently available versions in the Changelog. |
| 139002 | No Phone Number connected to WhatsApp Business Account | Add a phone number to your WhatsApp Business Account before publishing. See phone numbers documentation. |
| 139002 | Missing Flows Signed Public Key | Upload and sign a public key to a phone number before publishing. See implementing your flow endpoint. |
| 139002 | No Application Connected to the Flow | Connect a Meta app to the flow before publishing. See development documentation. |
| 139002 | Endpoint Not Available | Verify endpoint availability and implement a health check before publishing. See implementing your flow endpoint. |
| 139002 | WhatsApp Business Account is not subscribed to Flows Webhooks | Verify subscription to Flows webhooks. See health monitoring webhooks. |
| 139003 | Can't deprecate unpublished flow | Delete any Flows you no longer need if they are still drafts. See Deprecating a Flow and Deleting a Flow. |
| 139003 | Flow is already deprecated | Ignore the error as the Flow is already deprecated. |
| 139004 | Can't delete published Flow | Deprecate the Flow instead of deleting it. |
| 139006 | Metrics threshold is not reached | Not enough data to provide flow metrics. No specific resolution provided. 
<argument name="citation_id">35</argument>
|

#### Business Endpoint Error Codes

| HTTP Response Code | Description | Client-side behavior and details |
|--------------------|-------------|----------------------------------|
| 421 | The payload cannot be decrypted | WhatsApp client will re-fetch a public key and re-send the request. If fails, show generic error. See Implementing Endpoints for Flows. |
| 432 | The request signature authentication fails | Show a generic error to the user. See Implementing Endpoints for Flows. |
| 433 | The response signature generation fails | Show a generic error to the user. See Implementing Endpoints for Flows. |
| 434 | The Flow token is expired or invalid | Show a generic error to the user. See Implementing Endpoints for Flows. 
<argument name="citation_id">35</argument>
|

## Additional Resources

- **Playground**: Use the Flows Playground to quickly configure and preview a basic Flow. For production Flows, use the Flow Builder UI in WhatsApp Manager.
<argument name="citation_id">34</argument>

- **Guides**: Refer to sub-guides for sending Flows, receiving responses, testing, debugging, and examples for use cases like lead generation.
<argument name="citation_id">6</argument>

- **Versioning**: Control service details for stability as functionality evolves.
<argument name="citation_id">9</argument>

- **Metrics API**: Track endpoint performance with metrics like request counts and latencies.
<argument name="citation_id">10</argument>

- **Lifecycle of a Flow**: Details on Flow states (DRAFT, PUBLISHED, DEPRECATED) and transitions.
<argument name="citation_id">12</argument>

- **Components**: Building blocks for UIs, including attribute models for displaying business data.
<argument name="citation_id">28</argument>


This documentation provides a comprehensive overview based on available sources. For the latest updates, visit the official Meta for Developers pages.