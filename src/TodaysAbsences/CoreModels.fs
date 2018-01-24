module CoreModels


type KindOfAbsense =
    | Holiday
    | Sick
    | Wfh
    | Appointment


type Employee = {
    firstName : string
    lastName : string
    department : string
}


type Duration =
    | Hours of int
    | Days of int


type Absence = {
    kind : KindOfAbsense
    duration : Duration
    employee : Employee
}