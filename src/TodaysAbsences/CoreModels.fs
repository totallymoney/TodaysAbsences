module CoreModels


type KindOfAbsense =
    | Appointment
    | Compassionate
    | Holiday
    | Sick
    | StudyLeave
    | Training
    | Wfh
    | UnknownKind


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


type Absence = {
    kind : KindOfAbsense
    duration : Duration
    employee : Employee
}

type Logger = (string -> unit)
