module CoreModels

open Helpers

type KindOfAbsense =
    | Appointment
    | Compassionate
    | Holiday
    | Sick
    | StudyLeave
    | Training
    | Wfh
    | Volunteering
    | Remote
    | OnSite
    | Conference
    | JuryDuty
    | Amber
    | UnknownKind
    
    override x.ToString() = 
        match x with
            | Appointment -> "Appointment"
            | Compassionate -> "Compassionate Leave"
            | Holiday -> "Holiday"
            | Sick -> "Sick Leave"
            | StudyLeave -> "Study Leave"
            | Training -> "Training"
            | Wfh -> "Working from Home"
            | Volunteering -> "Volunteering"
            | Conference -> "Conference"
            | JuryDuty -> "Jury Duty"
            | Remote -> "Remote"
            | OnSite -> "On Site"
            | Amber -> "Amber"
            | UnknownKind -> "Unknown reason"


type Employee = {
    firstName : string
    lastName : string
    department : string
}


type PartOfDay =
    | Am
    | Pm


type Duration =
    | Days of decimal
    | LessThanADay of PartOfDay
    | UnknownDuration
    
    override x.ToString() = 
        match x with
            | Days count -> sprintf "%M days" count
            | LessThanADay Am -> "Part-day (AM)"
            | LessThanADay Pm -> "Part-day (PM)"
            | UnknownDuration -> "Unknown duration"


type Absence = 
    {
        kind : KindOfAbsense
        duration : Duration
        employee : Employee
    }
    override x.ToString() = 
        sprintf "%s %s - %s - %s"
            (x.employee.firstName |> removeAccents) 
            (x.employee.lastName |> removeAccents)
            (x.kind.ToString()) 
            (x.duration.ToString())

type Logger = (string -> unit)
