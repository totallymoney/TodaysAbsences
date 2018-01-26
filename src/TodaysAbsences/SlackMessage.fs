module SlackMessage


type MessageJsonAttachmentField = {
    title : string
    value : string
}


type MessageJsonAttachment = {
    fallback : string
    color : string
    pretext : string
    text : string
    fields : MessageJsonAttachmentField list
}


type MessageJson = {
    attachments : MessageJsonAttachment list
}


let messageJson absences = {
    attachments = []
}