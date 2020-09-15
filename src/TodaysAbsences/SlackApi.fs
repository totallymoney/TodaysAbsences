module SlackApi


open Chiron
open Http
open Domain


type AttachmentField =
    {
        Title : string
        Value : string
    }

    static member ToJson (f:AttachmentField) = json {
        do! Json.write "title" f.Title
        do! Json.write "value" f.Value
    }


type Attachment =
    {
        Fallback : string
        Color : string
        Pretext : string
        Text : string
        Fields : AttachmentField list
    }

    static member ToJson (a:Attachment) = json {
        do! Json.write "fallback" a.Fallback
        do! Json.write "color" a.Color
        do! Json.write "pretext" a.Pretext
        do! Json.write "text" a.Text
        do! Json.write "fields" a.Fields
    }

type Message =
    {
        Attachments : Attachment list
    }

    static member ToJson (m:Message) = json {
        do! Json.write "attachments" m.Attachments
    }

let private titleAttachment = {
    Fallback = "Today's absences and holidays, from Bob"
    Color = "#34495e"
    Pretext = "Today's Absences and Holidays, from <https://app.hibob.com/home|Bob>"
    Text = "Sorted by Department, then by first name within departments"
    Fields = []
}

let private absenceStrings logger =
    List.sortBy (fun a -> (a.Employee.DisplayName))
    >> List.map (fun abs -> abs |> Absence.toString logger) 
    >> String.concat "\n"

let private departmentField logger (department, absences) = {
    Title = department
    Value = absenceStrings logger absences
}

let private fields logger =
    List.groupBy (fun a -> a.Employee.Department |> Department.unwrap) 
    >> List.sortBy fst 
    >> List.map (departmentField logger)

let messageJson logger absences = 
    { Attachments = [{ titleAttachment with Fields = fields logger absences }]}

let messageJsonString (message:Message) =
    Json.serialize message |> Json.format

module Http =
    let sendMessage logger url =
        messageJson logger >> messageJsonString >> postJson url
