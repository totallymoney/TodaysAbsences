module PeopleHrApi


open CoreModels
open Chiron
open Http
open Aether.Optics
open Core
open Core.Result

let private results = ResultsBuilder()

let private foldIntoSingleResult results =
    let folder (state:Result<Absence list, string>) (result:Result<Absence, string>) =
        match result with
        | Ok absence -> Result.map (fun absences -> absence :: absences) state
        | Result.Error message -> Result.Error message

    Array.fold folder (Ok []) results

let logObj logger objectToLog = 
    sprintf "%A" objectToLog |> logger
    objectToLog

type HolidayResponse =
    {
        FirstName : string
        LastName : string
        Department : string
        PartOfTheDay : string option
        HolidayDurationDays : decimal
    }

    static member public FromJson (_:HolidayResponse) = json {
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! partOfTheDay = Json.read "Part of the Day"
        let! holidayDurationDays = Json.read "Holiday Duration (Days)"
        return
            {
                FirstName = firstName
                LastName = lastName
                Department = department
                PartOfTheDay = partOfTheDay
                HolidayDurationDays = holidayDurationDays
            }
    }



type HolidayResponseWrapper =
    {
        isError : bool
        Message : string
        Result : HolidayResponse array
    }

    static member FromJson (_:HolidayResponseWrapper) = json {
        let! isError = Json.read "isError"
        let! message = Json.read "Message"

        if message = "No records found." then
            return
                {
                    isError = isError
                    Message = message
                    Result = [||]
                }
        else
            let! result = Json.read "Result"

            match result with
            | Some r ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = r
                    }
            | None ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = [||]
                    }
    }


module Holiday =


    let private duration (r:HolidayResponse) =
        match r.PartOfTheDay with
        | Some value ->
            match value with
            | "" ->
                r.HolidayDurationDays |> Days |> Ok
            | "AM" ->
                LessThanADay Am |> Ok
            | "PM" ->
                LessThanADay Pm |> Ok
            | unexpected ->
                Result.Error <| sprintf "Unpected value for \"Part of the Day\": %s" unexpected
        | None ->
            r.HolidayDurationDays |> Days |> Ok


    let private employee (r:HolidayResponse) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private mapToAbsences =
        let mapper (r:HolidayResponse) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Holiday; duration = d }
            | Result.Error message -> Result.Error message

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
    {
        FirstName : string
        LastName : string
        Department : string
        SickAmPm : string option
        SickDurationDays : decimal option
    }

    static member FromJson (_:SickResponse) = json {
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! sickAmPm = Json.read "Sick (AM/PM)"
        let! sickDurationDays = Json.read "Sick Duration (Days)"
        return
            {
                FirstName = firstName
                LastName = lastName
                Department = department
                SickAmPm = sickAmPm
                SickDurationDays = sickDurationDays
            }
    }


type SickResponseWrapper =
    {
        isError : bool
        Message : string
        Result : SickResponse array
    }

    static member FromJson (_:SickResponseWrapper) = json {
        let! isError = Json.read "isError"
        let! message = Json.read "Message"

        if message = "No records found." then
            return
                {
                    isError = isError
                    Message = message
                    Result = [||]
                }
        else
            let! result = Json.read "Result"

            match result with
            | Some r ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = r
                    }
            | None ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = [||]
                    }
    }
    


module Sick =


    let private employee (r:SickResponse) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private duration (r:SickResponse) =
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
                Result.Error <| sprintf "Unpected value for \"Sick (AM/PM)\": %s" unexpected
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
    {
        Hours : int
    }

    static member FromJson (_:OtherEventsStartTime) = json {
        let! hours = Json.read "Hours"
        return { Hours = hours }
    }


type OtherEventResponse =
    {
        FirstName : string
        LastName : string
        Department : string
        OtherEventsDurationType : string option
        OtherEventsReason : string option
        OtherEventsStartTime : OtherEventsStartTime option
        OtherEventsTotalDurationDays : decimal
    }

    static member FromJson (_:OtherEventResponse) = json {
        let! firstName = Json.read "First Name"
        let! lastName = Json.read "Last Name"
        let! department = Json.read "Department"
        let! otherEventsDurationType = Json.read "Other Events Duration Type"
        let! otherEventsReason = Json.read "Other Events Reason"
        let! otherEventsStartTime = Json.read "Other Events Start Time"
        let! otherEventsTotalDurationDays = Json.read "Other Events Total Duration (Days)"
        return
            {
                FirstName = firstName
                LastName = lastName
                Department = department
                OtherEventsDurationType = otherEventsDurationType
                OtherEventsReason = otherEventsReason
                OtherEventsStartTime = otherEventsStartTime
                OtherEventsTotalDurationDays = otherEventsTotalDurationDays
            }
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

        if message = "No records found." then
            return
                {
                    isError = isError
                    Message = message
                    Result = [||]
                }
        else
            let! result = Json.read "Result"

            match result with
            | Some r ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = r
                    }
            | None ->
                return
                    {
                        isError = isError
                        Message = message
                        Result = [||]
                    }
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
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private kind (r:OtherEventResponse) =
        match r.OtherEventsReason with
        | Some reason ->
            match reason with
            | "Appointment" -> Ok Appointment
            | "Compassionate" -> Ok Compassionate
            | "Study Leave" -> Ok StudyLeave
            | "Training" -> Ok Training
            | "Working from Home" -> Ok Wfh
            | _ -> Ok UnknownKind
        | None ->
            Ok UnknownKind


    let private mapToAbsences =
        let mapper (r:OtherEventResponse) =
            match duration r with
            | Ok d ->
                match kind r with
                | Ok k -> Ok { employee = employee r; kind = k; duration = d }
                | Result.Error message -> Result.Error message
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


    let private queryRequestBody queryName apiKey =
        sprintf
            """{"APIKey":"%s","Action":"GetQueryResultByQueryName","QueryName":"%s"}"""
            apiKey
            queryName


    let private query queryName =
        queryRequestBody queryName >> postJson "https://api.peoplehr.net/Query"


    let private getEmployeesWithHolidayToday logger =
        query "Employees on holiday today" >> Result.bind (Holiday.parseResponseBody logger)


    let private getEmployeesWithSickToday logger =
        query "Employees absent/sick today" >> Result.bind (Sick.parseResponseBody logger)


    let private getEmployeesWithOtherEventToday logger =
        query "Employees with other events today" >> Result.bind (OtherEvent.parseResponseBody logger)

    let getAbsences (logger:Logger) apiKey =
        results {
            let! holidays = getEmployeesWithHolidayToday logger apiKey
            let! sicks = getEmployeesWithSickToday logger apiKey
            let! otherEvents = getEmployeesWithOtherEventToday logger apiKey    

            return
                holidays @ sicks @ otherEvents
        }
