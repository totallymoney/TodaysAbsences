module SlackApi

open CoreModels
open Chiron
open Http

type AttachmentField =
    { Title : string
      Value : string }

    static member ToJson (af : AttachmentField) = json {
        do! Json.write "title" af.Title
        do! Json.write "value" af.Value
    }

type Attachment = 
    { Fallback : string
      Color : string
      Pretext : string
      Text : string
      Fields : AttachmentField list }

    static member ToJson (a : Attachment) = json {
        do! Json.write "fallback" a.Fallback
        do! Json.write "color" a.Color
        do! Json.write "pretext" a.Pretext
        do! Json.write "text" a.Text
        do! Json.write "fields" a.Fields
    }

type Message =
    { Attachments : Attachment list }

    static member ToJson (m : Message) = json {
        do! Json.write "attachments" m.Attachments
    }

let private baseAttachment = {
    Fallback = "Today's absences and holidays, from PeopleHR"
    Color = "#34495e"
    Pretext = "Today's Absences and Holidays, from <https://totallymoney.peoplehr.net|PeopleHR>"
    Text = "Sorted by Department, then by first name within departments"
    Fields = []
}

let private absenceStrings =
    List.sortBy (fun abs -> abs.employee.firstName) 
    >> List.map (fun abs -> abs.ToString()) 
    >> String.concat "\n"

let private departmentField (department, absences) = 
    { Title = department
      Value = absenceStrings absences }

let private fields =
    List.groupBy (fun a -> a.employee.department) 
    >> List.sortBy fst 
    >> List.map departmentField

let messageJson absences = 
    { Attachments = [{ baseAttachment with Fields = fields absences }]}

let messageJsonString (message:Message) =
    Json.serialize message |> Json.format

module Http =

    let sendMessage url =
        messageJson >> messageJsonString >> postJson url
