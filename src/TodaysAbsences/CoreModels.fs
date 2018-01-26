module CoreModels


type KindOfAbsense =
    | Appointment
    | Compassionate
    | Holiday
    | Sick
    | StudyLeave
    | Training
    | Wfh



type Employee = {
    firstName : string
    lastName : string
    department : string
}


type PartOfDay =
    | Am
    | Pm


type Duration =
    | Days of int
    | LessThanADay of PartOfDay


type Absence = {
    kind : KindOfAbsense
    duration : Duration
    employee : Employee
}
