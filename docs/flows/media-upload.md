# Media Upload Components

> **Source:** https://developers.facebook.com/docs/whatsapp/flows/reference/media_upload

> ⚠️ **Security notice:** WhatsApp does not guarantee that data shared by customers is non-malicious. Use well-tested, up-to-date media processing libraries when handling uploads.

Two components allow users to upload media in a Flow:

- **PhotoPicker** — upload images from camera or gallery
- **DocumentPicker** — upload files from the file system or gallery

Both require **Flow JSON version 4.0+**.

---

## PhotoPicker

Allows users to upload photos from their camera or gallery.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"PhotoPicker"` |
| `name` | string | ✅ | Component name — must be unique on the screen |
| `label` | string | ✅ | Header text. Max 80 chars. Supports dynamic: `"${data.label}"` |
| `description` | string | — | Body text. Max 300 chars. Supports dynamic: `"${data.description}"` |
| `photo-source` | enum | — | `camera_gallery` (default), `camera`, or `gallery` |
| `max-file-size-kb` | integer | — | Max file size in kibibytes. Default: `25600` (25 MiB). Range: [1, 25600] |
| `min-uploaded-photos` | integer | — | Minimum required uploads. Set to `0` for optional. Default: `0`. Range: [0, 30] |
| `max-uploaded-photos` | integer | — | Maximum uploads allowed. Default: `30`. Range: [1, 30] |
| `enabled` | boolean \| string | — | Enable/disable user interaction. Default: `true` |
| `visible` | boolean \| string | — | Show/hide the component. Default: `true` |
| `error-message` | string \| object | — | Error display. String = generic error; Object = per-image errors: `{ "media_id_1": "error 1" }` |

> **Note:** For media sent as part of a response message (not via `data_exchange`): max **10 files**, max **100 MiB** aggregated.

### `photo-source` Values

| Value | Description |
|-------|-------------|
| `camera_gallery` | User selects from gallery or takes a photo |
| `gallery` | User selects from gallery only |
| `camera` | User takes a photo only |

### Limitations

| Constraint | Validation Error |
|------------|-----------------|
| `min-uploaded-photos` > `max-uploaded-photos` | `"min-uploaded-photos" cannot be greater than "max-uploaded-photos"` |
| `init-values` used to pre-fill PhotoPicker | `"init-values" property should not contain a value for PhotoPicker"` |
| More than 1 PhotoPicker per screen | `"You can only have a maximum of 1 PhotoPicker per screen"` |
| PhotoPicker and DocumentPicker on same screen | Not allowed |
| PhotoPicker in `navigate` action payload | Not allowed — use [Global Dynamic Referencing](./flowjson.md#global-dynamic-and-form-properties) instead |
| Nested in action payload (non-top-level) | Only valid as a top-level string property in `data_exchange` or `complete` payloads |

**Valid payload usage:**
```json
"on-click-action": {
  "name": "data_exchange",
  "payload": {
    "media": "${form.photo_picker}"
  }
}
```

**Invalid payload usage:**
```json
"on-click-action": {
  "name": "data_exchange",
  "payload": {
    "media": { "photo": "${form.photo_picker}" }
  }
}
```

---

## DocumentPicker

Allows users to upload documents from their file system or gallery.

### Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `type` | string | ✅ | `"DocumentPicker"` |
| `name` | string | ✅ | Component name — must be unique on the screen |
| `label` | string | ✅ | Header text. Max 80 chars. Supports dynamic: `"${data.label}"` |
| `description` | string | — | Body text. Max 300 chars. Supports dynamic: `"${data.description}"` |
| `max-file-size-kb` | integer | — | Max file size in kibibytes. Default: `25600` (25 MiB). Range: [1, 25600] |
| `min-uploaded-documents` | integer | — | Minimum required uploads. Default: `0`. Range: [0, 30] |
| `max-uploaded-documents` | integer | — | Maximum uploads allowed. Default: `30`. Range: [1, 30] |
| `allowed-mime-types` | array\<string\> | — | Allowlist of MIME types. Default: all supported types |
| `enabled` | boolean \| string | — | Enable/disable user interaction. Default: `true` |
| `visible` | boolean \| string | — | Show/hide the component. Default: `true` |
| `error-message` | string \| object | — | Error display. String = generic error; Object = per-document errors: `{ "media_id_1": "error 1" }` |

### Supported MIME Types

| # | MIME Type |
|---|-----------|
| 1 | `application/gzip` |
| 2 | `application/msword` |
| 3 | `application/pdf` |
| 4 | `application/vnd.ms-excel` |
| 5 | `application/vnd.ms-powerpoint` |
| 6 | `application/vnd.oasis.opendocument.presentation` |
| 7 | `application/vnd.oasis.opendocument.spreadsheet` |
| 8 | `application/vnd.oasis.opendocument.text` |
| 9 | `application/vnd.openxmlformats-officedocument.presentationml.presentation` |
| 10 | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` |
| 11 | `application/vnd.openxmlformats-officedocument.wordprocessingml.document` |
| 12 | `application/x-7z-compressed` |
| 13 | `application/zip` |
| 14 | `image/avif` |
| 15 | `image/gif` |
| 16 | `image/heic` |
| 17 | `image/heif` |
| 18 | `image/jpeg` |
| 19 | `image/png` |
| 20 | `image/tiff` |
| 21 | `image/webp` |
| 22 | `text/plain` |
| 23 | `video/mp4` |
| 24 | `video/mpeg` |

> Including `image/jpeg` also enables photo gallery selection.  
> Some older Android/iOS versions may not honor all MIME type restrictions.

### Limitations

Same constraints as PhotoPicker apply (swapping `Photo` → `Document`). DocumentPicker values cannot be used in `navigate` action payloads.

**Valid payload usage:**
```json
"on-click-action": {
  "name": "data_exchange",
  "payload": {
    "media": "${form.document_picker}"
  }
}
```

---

## Endpoint Media Handling

Media uploaded by users is temporarily stored in **WhatsApp CDN** (up to 20 days), encrypted with **AES256-CBC + HMAC-SHA256 + PKCS7**.

### Endpoint Payload Example

```json
{
  "photo_picker": [{
    "media_id": "790aba14-5f4a-4dbd-aa9e-0d75401da14b",
    "cdn_url": "https://mmg.whatsapp.net/v/redacted",
    "file_name": "IMG_5237.jpg",
    "encryption_metadata": {
      "encrypted_hash": "/QvkBvpBED2q2AHPIFuhXfLpkn22zj2kO6ggzjvhHv0=",
      "iv": "5SHjLrrsfPXTSJTcbrVSkg==",
      "encryption_key": "lPa4SXcWbk3sy2so3OxjyXmpV4aE6CcIKd+4byr5hBw=",
      "hmac_key": "15l+E9Z5gcL15WH9OQ8GgK7VVCKkfbVigoSiM9djvGU=",
      "plaintext_hash": "AOF2dHXVEpm9efk9udNy3R1cUJWnpjFwQKGBEdALqXI="
    }
  }]
}
```

### Decryption Steps

The CDN file format is: `cdn_file = ciphertext || hmac10` (ciphertext concatenated with first 10 bytes of HMAC-SHA256)

1. Download `cdn_file` from `cdn_url`
2. Verify: `SHA256(cdn_file) == encrypted_hash`
3. Validate HMAC-SHA256:
   - Calculate HMAC using `hmac_key` and `iv` over the ciphertext
   - Verify first 10 bytes match `hmac10`
4. Decrypt: Run AES-CBC with `encryption_key` and `iv` on ciphertext, then remove PKCS7 padding → `decrypted_media`
5. Verify: `SHA256(decrypted_media) == plaintext_hash`

---

## Response Message Webhook (Cloud API)

Media can also be received in the [Flow response message webhook](./webhooks.md#flow-response-message-webhook):

```json
{
  "nfm_reply": {
    "response_json": {
      "photo_picker": [
        {
          "file_name": "IMG_5237.jpg",
          "mime_type": "image/jpeg",
          "sha256": "PqHgadp8cJ/N6mvAYGNMxhs9Ra5hbZFcctCtCClXsMU=",
          "id": "3631120727156756"
        }
      ],
      "flow_token": "xyz",
      "name": "John"
    }
  }
}
```

Media can be downloaded using the standard [WhatsApp Cloud API media download steps](https://developers.facebook.com/docs/whatsapp/cloud-api/reference/media/#download-media).
