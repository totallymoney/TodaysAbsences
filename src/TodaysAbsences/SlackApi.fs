module SlackApi


open CoreModels
open Chiron
open Http


type AttachmentField =
    {
        title : string
        value : string
    }

    static member ToJson (f:AttachmentField) = json {
        do! Json.write "title" f.title
        do! Json.write "value" f.value
    }


type Attachment =
    {
        fallback : string
        color : string
        pretext : string
        text : string
        fields : AttachmentField list
    }

    static member ToJson (a:Attachment) = json {
        do! Json.write "fallback" a.fallback
        do! Json.write "color" a.color
        do! Json.write "pretext" a.pretext
        do! Json.write "text" a.text
        do! Json.write "fields" a.fields
    }


type Message =
    {
        attachments : Attachment list
    }

    static member ToJson (m:Message) = json {
        do! Json.write "attachments" m.attachments
    }


let private baseAttachment = {
    fallback = "Today's absences and holidays, from PeopleHR"
    color = "#34495e"
    pretext = "Today's Absences and Holidays, from <https://totallymoney.peoplehr.net|PeopleHR>"
    text = "Sorted by Department, then by first name within departments"
    fields = []
}


let private kindString = function
    | Appointment -> "Appointment"
    | Compassionate -> "Compassionate Leave"
    | Holiday -> "Holiday"
    | Sick -> "Sick Leave"
    | StudyLeave -> "Study Leave"
    | Training -> "Training"
    | Wfh -> "Working from Home"


let private durationString = function
    | Days count -> sprintf "%i days" count
    | LessThanADay Am -> "Part-day (AM)"
    | LessThanADay Pm -> "Part-day (PM)"
    | UnknownDuration -> "Unknown duration"


let private absenceString absence =
    sprintf "%s %s - %s - %s"
        absence.employee.firstName 
        absence.employee.lastName 
        (kindString absence.kind) 
        (durationString absence.duration)


let private absenceStrings =
    List.sortBy (fun a -> a.employee.firstName) >> List.map absenceString >> String.concat "\n"


let private departmentField (department, absences) = {
    title = department
    value = absenceStrings absences
}


let private fields =
    List.groupBy (fun a -> a.employee.department) >> List.sortBy fst >> List.map departmentField


let messageJson absences = { attachments = [ { baseAttachment with fields = fields absences } ] }


let messageJsonString (message:Message) =
    Json.serialize message |> Json.format


module Http =


    let sendMessage url =
        messageJson >> messageJsonString >> postJson url