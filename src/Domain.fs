module Domain

open System
open BobApi
open Dto
open Helpers
open Errors

type EmployeeId = 
    | EmployeeId of string
    static member unwrap (EmployeeId id) = id

type EmployeeDisplayName = 
    | EmployeeDisplayName of string
    static member unwrap (EmployeeDisplayName name) = name

type Department = 
    | Commercial
    | Corporate
    | Marketing
    | Product
    | Tech
    | Finance
    | People
    | LegalAndCompliance
    | Operations
    | StrategicOperations
    | ProductDesign
    | Other

    static member unwrap = 
        function
        | Commercial -> "Commercial"
        | Corporate -> "Corporate"
        | Marketing -> "Marketing"
        | Product -> "Product"
        | Tech -> "Tech"
        | Finance -> "Finance"
        | People -> "People"
        | LegalAndCompliance -> "Legal & Compliance"
        | Operations -> "Operations"
        | StrategicOperations -> "Strategic Operations"
        | ProductDesign -> "Product Design"
        | Other -> "Other"
    
    static member create = 
        function
        | "Commercial" -> Commercial
        | "Corporate" -> Corporate
        | "Marketing" -> Marketing
        | "Product" -> Product
        | "Tech" -> Tech
        | "Finance" -> Finance
        | "People" -> People
        | "Legal" -> LegalAndCompliance
        | "Legal & Compliance" -> LegalAndCompliance
        | "Operations" -> Operations
        | "Strategic Operations" -> StrategicOperations
        | "Product Design" -> ProductDesign
        | x -> printf "Other department: %s" x
               Other

type Squad = Squad of string
type EmployeeWorkDetails = 
    { Department : Department
      Squad : Squad option }

    static member create = function
        | Some details ->
            {  Department = details.Work.Department |> Department.create
               Squad = details.Work.Custom |> Option.bind (fun x -> x.Squad_5Gqot) |> Option.map Squad }
        | None -> 
            { Department = Other
              Squad = None }

type Employee = 
    { Id : EmployeeId
      DisplayName : EmployeeDisplayName
      Department : Department
      Squad : Squad option }

type AbsencePolicy =
    | Holiday
    | WorkingFromHome
    | Sick
    | Appointment
    | CompassionateLeave
    | UnpaidLeave
    | Conference
    | Training
    | Volunteering
    | PaternityLeave
    | MaternityLeave
    | WorkingAbroad
    | StudyLeave
    | Other

    static member unwrap = 
        function
        | Holiday -> "Holiday"
        | WorkingFromHome -> "WFH"
        | Sick -> "Sick Leave"
        | Appointment -> "Appointment"
        | CompassionateLeave -> "Leave"
        | UnpaidLeave -> "Unpaid Leave"
        | Conference -> "Conference"
        | Training -> "Training"
        | Volunteering -> "Volunteering"
        | PaternityLeave -> "Paternity Leave"
        | MaternityLeave -> "Maternity Leave"
        | WorkingAbroad -> "Working Abroad"
        | StudyLeave -> "Study Leave"
        | Other -> "Other"

    static member create = 
        function
        | "Holiday" -> Holiday
        | "WFH" -> WorkingFromHome
        | "Sick" -> Sick
        | "Appointment" -> Appointment
        | "Compassionate Leave" -> CompassionateLeave
        | "Unpaid Leave" -> UnpaidLeave
        | "Conference" -> Conference
        | "Training" -> Training
        | "Volunteering" -> Volunteering
        | "Paternity Leave" -> PaternityLeave
        | "Maternity Leave" -> MaternityLeave
        | "Working Abroad" -> WorkingAbroad
        | "Study Leave" -> StudyLeave
        | _ -> Other

type PartOfDay = Morning | Afternoon | AllDay
let toPartOfDay = 
    function
    | "morning" -> Ok Morning
    | "afternoon" -> Ok Afternoon
    | "all_day" -> Ok AllDay
    | other -> Error other

type AbsenceDuration = 
    | Days of decimal
    | PartOfDay of PartOfDay
    | Unknown of string
    
    static member unwrap = 
        function
        | Days days -> sprintf (if days = 1m then "%g day" else "%g days") days
        | PartOfDay Morning -> "Part-day (AM)"
        | PartOfDay Afternoon -> "Part-day (PM)"
        | PartOfDay AllDay -> "All day"
        | Unknown reason -> "Unknown duration"

type AbsenceDetails = 
    { Policy: AbsencePolicy
      Duration: AbsenceDuration }

type Absence = 
    { Employee : Employee
      Details: AbsenceDetails }

    static member toString a = 
        sprintf "%s - %s - %s" 
            (match a.Employee.Squad with
             | Some (Squad squad) -> sprintf "%s *(%s)*" 
                                         (a.Employee.DisplayName |> EmployeeDisplayName.unwrap |> removeAccents) 
                                         (string squad)
             | None -> a.Employee.DisplayName |> EmployeeDisplayName.unwrap |> removeAccents) 
            (a.Details.Policy |> AbsencePolicy.unwrap)
            (a.Details.Duration |> AbsenceDuration.unwrap)

type Birthday =
    { Employee : EmployeeDetailsDto
      Day : string option }

let getDuration logger (today : DateTime) absence = 
    let startPortion = toPartOfDay absence.StartPortion
    let endPortion = toPartOfDay absence.EndPortion
    match tryParse absence.StartDate, tryParse absence.EndDate with
    | Ok startDate, Ok endDate -> 
        if today = endDate then
            match startPortion with
            | Ok AllDay -> Days 1m
            | Ok portion -> PartOfDay portion
            | Error reason -> Unknown reason
        else if today = startDate then
            if startPortion = Ok Afternoon then PartOfDay Afternoon // just for clarity - the PeopleHR version did the same
            else
                let offset = match startPortion, endPortion with
                             | Ok Afternoon, Ok Morning -> 0.0m
                             | Ok Afternoon, Ok AllDay -> 0.5m
                             | Error e, _
                             | _, Error e -> sprintf "Unknown part of day %s" e |> logger
                                             1.0m // if we have an unknown part of day, default to all day
                             | _ -> 1.0m
                let duration = (decimal (endDate - today).TotalDays) + offset
                Days duration
        else // we are in the middle of the holiday
            let offset = match endPortion with
                         | Ok Morning -> 0.5m
                         | Error e -> sprintf "Unknown part of day %s" e |> logger
                                      1.0m // if we have an unknown part of day, default to all day
                         | _ -> 1.0m 
            let duration = (decimal (endDate - today).TotalDays) + offset
            Days duration
    | _, Error e
    | Error e, _ -> 
        logger e
        Unknown e

let toAbsence logger today absence details = 
    let employeeDetails = EmployeeWorkDetails.create details
    { Employee = 
        { Id = absence.EmployeeId |> EmployeeId
          DisplayName = absence.EmployeeDisplayName |> removeAccents |> EmployeeDisplayName
          Department = employeeDetails.Department
          Squad = employeeDetails.Squad }
      Details =
        { Policy = absence.PolicyTypeDisplayName |> AbsencePolicy.create
          Duration = getDuration logger today absence } }