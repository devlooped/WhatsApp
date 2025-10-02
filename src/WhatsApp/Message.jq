(
  .entry[] | 
  .id as $notification |
  .changes[] |
  select(.value.messages != null) |
  (.value.metadata as $phone |
   .value.contacts[0] as $user |
   .value.messages[0] as $msg |
   select($msg != null) |
   ($msg.type as $msgType |
    # Compute context once for all message types
    (if $msgType == "reaction" then $msg.reaction.message_id else ($msg.context.id // null) end) as $context |
    if $msgType == "interactive" and ($msg.interactive.type == "nfm_reply") then
      # Parse response_json once, but keep original string if parsing fails
      ($msg.interactive.nfm_reply.response_json | (fromjson? // .)) as $data |
      {
        "$type": "flow",
        "notification": $notification,
        "id": $msg.id,
        "context": $context,
        "timestamp": $msg.timestamp | tonumber,
        "service": {
          "id": $phone.phone_number_id,
          "number": $phone.display_phone_number
        },
        "user": {
          "name": ($user.profile.name // ""),
          "number": $msg.from
        },
        "data": $data,
        "source": ($data.flow_token // null)
      }
    elif $msgType == "interactive" or $msgType == "button" then
      {
        "$type": "interactive",
        "notification": $notification,
        "id": $msg.id,
        "context": $context,
        "timestamp": $msg.timestamp | tonumber,
        "service": {
          "id": $phone.phone_number_id,
          "number": $phone.display_phone_number
        },
        "user": {
          "name": ($user.profile.name // ""),
          "number": $msg.from
        },
        "selection": {
          "id": ($msg.interactive.button_reply?.id // $msg.interactive.list_reply?.id // $msg.button.payload),
          "title": ($msg.interactive.button_reply?.title // $msg.interactive.list_reply?.title // $msg.button.text)
        }
      }
    elif $msgType == "reaction" then
      {
        "$type": "reaction",
        "notification": $notification,
        "id": "",                    
        "context": $context,              
        "timestamp": $msg.timestamp | tonumber,
        "service": {
          "id": $phone.phone_number_id,
          "number": $phone.display_phone_number
        },
        "user": {
          "name": ($user.profile.name // ""),
          "number": $msg.from
        },
        "emoji": $msg.reaction.emoji
      }
    elif $msgType == "document" or $msgType == "contacts" or $msgType == "text" or $msgType == "location" or $msgType == "image" or $msgType == "video" or $msgType == "audio" then
      {
        "$type": "content",
        "notification": $notification,
        "id": $msg.id,
        "context": $context,
        "timestamp": $msg.timestamp | tonumber,
        "service": {
          "id": $phone.phone_number_id,
          "number": $phone.display_phone_number
        },
        "user": {
          "name": ($user.profile.name // ""),
          "number": $msg.from
        },
        "content": (
          if $msgType == "document" then
            {
              "$type": "document",
              "id": $msg.document.id,
              "name": $msg.document.filename,
              "mime": $msg.document.mime_type,
              "sha256": $msg.document.sha256
            }
          elif $msgType == "contacts" then
            {
              "$type": "contacts",
              "contacts": $msg.contacts | map({
                  "name": .name.first_name,
                  "surname": .name.last_name,
                  "fullname": .name.formatted_name,
                  "numbers": [.phones[] | select(.wa_id? != null) | .wa_id]
              })
            }
          elif $msgType == "text" then
            {
              "$type": "text",
              "text": $msg.text.body
            }
          elif $msgType == "location" then
            {
              "$type": "location",
              "location": {
                "latitude": $msg.location.latitude,
                "longitude": $msg.location.longitude
              },
              "address": $msg.location.address,
              "name": $msg.location.name,
              "url": $msg.location.url
            }
          elif $msgType == "image" or $msgType == "video" or $msgType == "audio" then
            {
              "$type": $msgType,
              "id": $msg[$msgType].id,
              "caption": $msg[$msgType].caption,
              "mime": $msg[$msgType].mime_type,
              "sha256": $msg[$msgType].sha256
            }
          end
        )
      }
    else
      {
        "$type": "unsupported",
        "notification": $notification,
        "id": "",
        "context": $context,
        "timestamp": $msg.timestamp | tonumber,
        "service": {
          "id": $phone.phone_number_id,
          "number": $phone.display_phone_number
        },
        "user": {
          "name": ($user.profile.name // ""),
          "number": $msg.from
        },
        "raw": $msg
      }
    end
   )
  )
),
(
  .entry[] | 
  .id as $notification |
  .changes[] |
  select(.value.statuses != null) |
  (.value.metadata as $phone |
   .value.statuses[0] as $status |
   select($status != null) |
   if $status.errors? then
     $status.errors[] |
     {
       "$type": "error",
       "notification": $notification,
       "id": "",
       "timestamp": $status.timestamp | tonumber,
       "service": {
         "id": $phone.phone_number_id,
         "number": $phone.display_phone_number
       },
       "user": {
         "name": "",
         "number": $status.recipient_id
       },
       "error": {
         "code": .code,
         "message": (.error_data.details // .message)
       }
     }
   else
     {
       "$type": "status",
       "notification": $notification,
       "id": "",
       "context": $status.id,
       "timestamp": $status.timestamp | tonumber,
       "service": {
         "id": $phone.phone_number_id,
         "number": $phone.display_phone_number
       },
       "user": {
         "name": "",
         "number": $status.recipient_id
       },
       "status": $status.status
     }
   end
  )
)
