module PeopleHrApi

open CoreModels
open Chiron
open Http
open Aether.Optics
open Core
open Core.Result
open System
open System.Globalization

let private foldIntoSingleResult results =
    let folder (state:Result<Absence list, string>) (result:Result<Absence, string>) =
        match result with
        | Ok absence -> Result.map (fun absences -> absence :: absences) state
        | Error message -> Result.Error message

    Array.fold folder (Ok []) results

let logObj logger objectToLog = 
    sprintf "%A" objectToLog |> logger
    objectToLog

type HolidayResponse =
    { EmployeeId : string
      FirstName : string
      LastName : string
      Department : string
      PartOfTheDay : string option
      HolidayDurationDays : decimal 
      HolidayStart : DateTime
      HolidayEnd : DateTime }

    static member public FromJson (_ : HolidayResponse) = json {
        let! employeeId = Json.read "Employee Id"
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! partOfTheDay = Json.read "Part of the Day"
        let! holidayDurationDays = Json.read "Holiday Duration (Days)"
        let! holidayStart = Json.read "Holiday Start Date"
        let! holidayEnd = Json.read "Holiday End Date"

        let holidayStartString : string = holidayStart
        let holidayEndString : string = holidayEnd

        let parseDateTime str =
            DateTime.ParseExact(str, "yyyy/MM/dd", CultureInfo.InvariantCulture)

        return
            { EmployeeId = employeeId
              FirstName = firstName
              LastName = lastName
              Department = department
              PartOfTheDay = partOfTheDay
              HolidayDurationDays = holidayDurationDays 
              HolidayStart = holidayStartString |> parseDateTime
              HolidayEnd = holidayEndString |> parseDateTime }
    }

let inline safeJsonTryRead key =
    try
        Json.tryRead key 
    with _ ->
        Json.init None

type HolidayResponseWrapper =
    {
        isError : bool
        Message : string
        Result : HolidayResponse []
    }
    static member FromJson (_ : HolidayResponseWrapper) = json {
        let! isError = Json.read "isError"
        let! message = Json.read "Message"
        
        let emptyResult = 
            { isError = isError
              Message = message
              Result = [||] }
        
        //This is needed to done like this, given PeopleHR sends back an empty string 
        //for an array type and Chiron can't handle it
        if message = "No records found." then return emptyResult
        else 
            let! result = Json.tryRead "Result"
            return 
                result 
                |> Option.map (fun res -> { emptyResult with Result = res })
                |> Option.defaultValue emptyResult
    }

module Holiday =
    let private duration (r:HolidayResponse) =
        match r.PartOfTheDay with
        | Some value ->
            match value with
            | "" -> r.HolidayDurationDays |> Days |> Ok
            | "AM" -> LessThanADay Am |> Ok
            | "PM" -> LessThanADay Pm |> Ok
            | unexpected -> sprintf "Unpected value for \"Part of the Day\": %s" unexpected |> Error
        | None ->
            r.HolidayDurationDays |> Days |> Ok

    let private employee (r : HolidayResponse) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department; employeeId = r.EmployeeId }

    let private mapToAbsences =
        let mapper (r:HolidayResponse) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Holiday; duration = d }
            | Error err -> Error err

        Array.map mapper

    let parseResponseBody (logger:Logger) json =
        try 
            Json.parse json
            |> Json.deserialize 
            |> (fun x -> x.Result) 
            |> mapToAbsences 
            |> foldIntoSingleResult
            |> logObj logger
        with exn ->
            sprintf "%s%s" (exn.ToString()) json |> logger
            exn.ToString() |> Error


type SickResponse = 
    { EmployeeId : string
      FirstName : string
      LastName : string
      Department : string
      SickAmPm : string option
      StartDate : string
      EndDate : string
      SickDurationDays : decimal option }
    static member FromJson (_ : SickResponse) = json {
        let! employeeId = Json.read "Employee Id"
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! sickAmPm = Json.read "Sick (AM/PM)"
        let! sickDurationDays = Json.read "Sick Duration (Days)"
        let! startDate = Json.read "Sick Start Date"
        let! endDate = Json.read "Sick End Date"

        return
            { EmployeeId = employeeId
              FirstName = firstName
              LastName = lastName
              Department = department
              SickAmPm = sickAmPm
              StartDate = startDate
              EndDate = endDate
              SickDurationDays = sickDurationDays }
    }

type SickResponseWrapper =
    { isError : bool
      Message : string
      Result : SickResponse array }
    static member FromJson (_ : SickResponseWrapper) = json {
        let! isError = Json.read "isError"
        let! message = Json.read "Message"
        
        let emptyResult = 
            { isError = isError
              Message = message
              Result = [||] }
        
        //This is needed to done like this, given PeopleHR sends back an empty string 
        //for an array type and Chiron can't handle it
        if message = "No records found." then return emptyResult
        else 
            let! result = Json.tryRead "Result"
            return 
                result 
                |> Option.map (fun res -> { emptyResult with Result = res })
                |> Option.defaultValue emptyResult
    }
    
module Sick =

    let private employee (r : SickResponse) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department; employeeId = r.EmployeeId }

    let private duration (r : SickResponse) =
        match r.SickAmPm with
        | Some value ->
            match value with
            | "" ->
                match r.SickDurationDays with
                | Some days ->
                    Days days |> Ok
                | None ->
                    invalidOp "test me!"
            | "AM" ->
                LessThanADay Am |> Ok
            | "PM" ->
                LessThanADay Pm |> Ok
            | unexpected ->
                sprintf "Unpected value for \"Sick (AM/PM)\": %s" unexpected |> Error
        | None ->
            match r.SickDurationDays with
            | Some days ->
                Days days |> Ok
            | None ->
                invalidOp "test me!"

    let private mapToAbsences =
        let mapper (r:SickResponse) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Sick; duration = d}
            | Result.Error message -> Result.Error message

        Array.map mapper
        
    let parseResponseBody logger json =
        try 
            Json.parse json
            |> Json.deserialize 
            |> (fun x -> x.Result) 
            |> mapToAbsences 
            |> foldIntoSingleResult
            |> logObj logger
        with exn -> 
            sprintf "%s%s" (exn.ToString()) json |> logger
            exn.ToString() |> Error


type OtherEventsStartTime =
    { Hours : int }

    static member FromJson (_ : OtherEventsStartTime) = json {
        let! hours = Json.read "Hours"
        return { Hours = hours }
    }

type OtherEventResponse =
    { EmployeeId : string
      FirstName : string
      LastName : string
      Department : string
      OtherEventsDurationType : string option
      OtherEventsReason : string option
      StartDate : string
      OtherEventsStartTime : OtherEventsStartTime option
      OtherEventsTotalDurationDays : decimal }
    (*
    duration type == hours
        "Other Events Total Duration (Hrs)"

    duration type == days
         "Other Events Total Duration (Days)"
    *)

    static member FromJson (_ : OtherEventResponse) = json {
        let! employeeId = Json.read "Employee Id"
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! otherEventsDurationType = Json.read "Other Events Duration Type"
        let! otherEventsReason = Json.read "Other Events Reason"
        let! otherEventStartDate = Json.read "Other Events Start Date"
        let! otherEventsStartTime = Json.read "Other Events Start Time"
        let! otherEventsTotalDurationDays = Json.read "Other Events Total Duration (Days)"

        return
            { EmployeeId = employeeId
              FirstName = firstName
              LastName = lastName
              Department = department
              OtherEventsDurationType = otherEventsDurationType
              OtherEventsReason = otherEventsReason
              StartDate = otherEventStartDate
              OtherEventsStartTime = otherEventsStartTime
              OtherEventsTotalDurationDays = otherEventsTotalDurationDays }
    }

type OtherEventResponseWrapper =
    {
        isError : bool
        Message : string
        Result : OtherEventResponse array
    }

    static member FromJson (_:OtherEventResponseWrapper) = json {
        let! isError = Json.read "isError"
        let! message = Json.read "Message"
        
        let emptyResult = 
            { isError = isError
              Message = message
              Result = [||] }
        
        //This is needed to done like this, given PeopleHR sends back an empty string 
        //for an array type and Chiron can't handle it
        if message = "No records found." then return emptyResult
        else 
            let! result = Json.tryRead "Result"
            return 
                result 
                |> Option.map (fun res -> { emptyResult with Result = res })
                |> Option.defaultValue emptyResult
    }

module OtherEvent =
    let private unexpectedValueMessage fieldName value =
        if isNull value then
            sprintf "Unexpected %s value: (null)" fieldName
        else
            sprintf "Unexpected %s value: %s" fieldName value


    let private duration (r:OtherEventResponse) = 
        match r.OtherEventsDurationType with
        | Some duration ->
            match duration with
            | "Days" ->
                r.OtherEventsTotalDurationDays |> Days |> Ok
            | "Hours" ->
                match r.OtherEventsStartTime with
                | Some time ->
                    if time.Hours <= 12 then
                        LessThanADay Am |> Ok
                    else
                        LessThanADay Pm |> Ok
                | None ->
                    Ok UnknownDuration
            | _ ->
                Ok UnknownDuration
        | None ->
            Ok UnknownDuration


    let private employee (r:OtherEventResponse) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department; employeeId = r.EmployeeId }


    let private kind (r:OtherEventResponse) =
        match r.OtherEventsReason with
        | Some reason ->
            match reason with
            | "Appointment" -> Appointment
            | "Compassionate" -> Compassionate
            | "Study Leave" -> StudyLeave
            | "Training" -> Training
            | "Working from Home" -> Wfh
            | "Volunteering" -> Volunteering
            | "Conference" -> Conference
            | "Jury Duty" -> JuryDuty
            | _ -> UnknownKind
        | None ->
            UnknownKind

    let private mapToAbsences =
        let mapper (r:OtherEventResponse) =
            match duration r with
            | Ok d -> r |> kind |> fun k -> Ok { employee = employee r; kind = k; duration = d }
            | Result.Error message -> Result.Error message
        
        Array.map mapper

    let private filterUnknownDurations =
        Result.map (List.filter (fun a -> match a.duration with | UnknownDuration -> false | _ -> true))

    let private filterUnknownKinds : (Result<Absence list, string> -> Result<Absence list, string>) =
        Result.map (List.filter (fun a -> match a.kind with | UnknownKind -> false | _ -> true))

    let parseResponseBody logger json =
        try 
            Json.parse json
            |> Json.deserialize 
            |> (fun x -> x.Result) 
            |> mapToAbsences 
            |> foldIntoSingleResult
            |> filterUnknownDurations
            |> filterUnknownKinds
            |> logObj logger
        with exn -> 
            sprintf "%s%s" (exn.ToString()) json |> logger
            exn.ToString() |> Error

module Http =
    type QueryName = string 
    type ApiKey = string 

    type QueryType =
        | HolidaysToday
        | AbsentOrSickToday 
        | OtherEventsToday
        | HolidaysThisWeek
        | AbsentOrSickThisWeek
        | OtherEventsThisWeek

    module QueryType = 
        let toQueryName = function
            | HolidaysToday -> "Employees on holiday today"
            | AbsentOrSickToday -> "Employees absent/sick today"
            | OtherEventsToday -> "Employees with other events today"
            | HolidaysThisWeek -> "Employees on holiday this week"
            | AbsentOrSickThisWeek -> "Employees absent/sick this week"
            | OtherEventsThisWeek -> "Employees with other events this week"

    type IntervalType =
        | Today
        | ThisWeek

    let private queryRequestBody (queryName : QueryName) (apiKey : ApiKey) =
        sprintf """{"APIKey":"%s","Action":"GetQueryResultByQueryName","QueryName":"%s"}""" apiKey queryName

    let private query queryName =
        //TODO: move this to SSM
        queryRequestBody queryName >> postJson "https://api.peoplehr.net/Query"

    let toQueryType = function
        | Today -> HolidaysToday
        | ThisWeek -> HolidaysThisWeek

    let private getEmployeesOnHoliday intervalType logger =
        let queryName = 
            intervalType
            |> toQueryType
            |> QueryType.toQueryName

        query queryName >> Result.bind (Holiday.parseResponseBody logger)

    let private getEmployeesWithSick intervalType logger = 
        let queryName = 
            intervalType
            |> toQueryType
            |> QueryType.toQueryName

        query queryName >> Result.bind (Sick.parseResponseBody logger)

    let private getEmployeesWithOtherEvent intervalType logger =
        let queryName = 
            intervalType
            |> toQueryType
            |> QueryType.toQueryName

        query queryName >> Result.bind (OtherEvent.parseResponseBody logger)

    let private getAbsences intervalType (logger : Logger) apiKey = 
        result {
            let! holidays = getEmployeesOnHoliday intervalType logger apiKey
            let! sicks = getEmployeesWithSick intervalType logger apiKey
            let! otherEvents = getEmployeesWithOtherEvent intervalType logger apiKey    

            return
                holidays @ sicks @ otherEvents
        }

    let getAbsencesToday logger apiKey =
        getAbsences Today logger apiKey

    let getAbsencesThisWeek logger apiKey =
        getAbsences ThisWeek logger apiKey
        