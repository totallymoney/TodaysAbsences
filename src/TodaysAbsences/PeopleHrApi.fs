module PeopleHrApi


open CoreModels
open Http
open FSharp.Data
open System


let private foldIntoSingleResult results =
    let folder (state:Result<Absence list, string>) (result:Result<Absence, string>) =
        match result with
        | Ok absence -> Result.map (fun absences -> absence :: absences) state
        | Error message -> Error message

    Array.fold folder (Ok []) results


[<Literal>]
let private holidayResponseSample = """{
    "isError": false,
    "Result": [
        {
            "First Name": "Joe",
            "Last Name": "Bloggs",
            "Department": "Development",
            "Part of the Day": "PM",
            "Holiday Duration (Days)": 0.5
        }
    ]
}"""


type private HolidayResponse = JsonProvider<holidayResponseSample>


module Holiday =


    let private duration (r:HolidayResponse.Result) =
        match r.PartOfTheDay with
        | null
        | "" ->
            (Decimal.ToInt32 >> Days >> Ok) r.HolidayDurationDays
        | "AM" ->
            LessThanADay Am |> Ok
        | "PM" ->
            LessThanADay Pm |> Ok
        | unexpected ->
            Error <| sprintf "Unpected value for \"Part of the Day\": %s" unexpected


    let private employee (r:HolidayResponse.Result) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private mapToAbsences =
        let mapper (r:HolidayResponse.Result) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Holiday; duration = d }
            | Error message -> Error message

        Array.map mapper


    let parseResponseBody =
        HolidayResponse.Parse >> (fun x -> x.Result) >> mapToAbsences >> foldIntoSingleResult


[<Literal>]
let private sickResponseSample = """{
    "isError": false,
    "Result": [
        {
            "First Name": "Edward",
            "Last Name": "Dewhurst",
            "Department": "Development",
            "Sick (AM/PM)": "AM",
            "Sick Duration (Days)": 1
        }
    ]
}
"""


type private SickResponse = JsonProvider<sickResponseSample>


module Sick =


    let private employee (r:SickResponse.Result) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private duration (r:SickResponse.Result) =
        match r.SickAmPm with
        | null
        | "" ->
            (Days >> Ok) r.SickDurationDays
        | "AM" ->
            LessThanADay Am |> Ok
        | "PM" ->
            LessThanADay Pm |> Ok
        | unexpected ->
            Error <| sprintf "Unpected value for \"Sick (AM/PM)\": %s" unexpected



    let private mapToAbsences =
        let mapper (r:SickResponse.Result) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Sick; duration = d}
            | Error message -> Error message

        Array.map mapper

    let parseResponseBody =
        SickResponse.Parse >> (fun x -> x.Result) >> mapToAbsences >> foldIntoSingleResult


[<Literal>]
let private otherEventSample = """
{
    "isError": false,
    "Result": [
        {
            "First Name": "Michael",
            "Last Name": "Scott",
            "Department": "Design",
            "Other Events Duration Type": "Hours",
            "Other Events Reason": "Appointment",
            "Other Events Start Time": {
              "Hours": 10
            },
            "Other Events Total Duration (Days)": 0.4
        }
    ]
}
"""


type private OtherEventResponse = JsonProvider<otherEventSample>


module OtherEvent =


    let private unexpectedValueMessage fieldName value =
        if isNull value then
            sprintf "Unexpected %s value: (null)" fieldName
        else
            sprintf "Unexpected %s value: %s" fieldName value


    let private duration (r:OtherEventResponse.Result) =
        match r.OtherEventsDurationType with
        | "Days" ->
            r.OtherEventsTotalDurationDays |> Decimal.ToInt32 |> Days |> Ok
        | "Hours" ->
            if r.OtherEventsStartTime.Hours <= 12 then
                LessThanADay Am |> Ok
            else
                LessThanADay Pm |> Ok
        | _ ->
            Ok UnknownDuration

    
    let private employee (r:OtherEventResponse.Result) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private kind (r:OtherEventResponse.Result) =
        match r.OtherEventsReason with
        | "Appointment" -> Ok Appointment
        | "Compassionate" -> Ok Compassionate
        | "Study Leave" -> Ok StudyLeave
        | "Training" -> Ok Training
        | "Working from Home" -> Ok Wfh
        | _ -> Ok UnknownKind


    let private mapToAbsences =
        let mapper (r:OtherEventResponse.Result) =
            match duration r with
            | Ok d ->
                match kind r with
                | Ok k -> Ok { employee = employee r; kind = k; duration = d }
                | Error message -> Error message
            | Error message -> Error message
        
        Array.map mapper


    let private filterUnknownDurations =
        Result.map (List.filter (fun a -> match a.duration with | UnknownDuration -> false | _ -> true))


    let private filterUnknownKinds : (Result<Absence list, string> -> Result<Absence list, string>) =
        Result.map (List.filter (fun a -> match a.kind with | UnknownKind -> false | _ -> true))


    let parseResponseBody =
        OtherEventResponse.Parse
        >> (fun x -> x.Result)
        >> mapToAbsences
        >> foldIntoSingleResult 
        >> filterUnknownDurations
        >> filterUnknownKinds


module Http =


    let private queryRequestBody queryName apiKey =
        sprintf
            """{"APIKey":"%s","Action":"GetQueryResultByQueryName","QueryName":"%s"}"""
            apiKey
            queryName


    let private query queryName =
        queryRequestBody queryName >> postJson "https://api.peoplehr.net/Query"


    let private getEmployeesWithHolidayToday =
        query "Employees on holiday today" >> Result.bind Holiday.parseResponseBody


    let private getEmployeesWithSickToday =
        query "Employees absent/sick today" >> Result.bind Sick.parseResponseBody


    let private getEmployeesWithOtherEventToday =
        query "Employees with other events today" >> Result.bind OtherEvent.parseResponseBody


    let getAbsences apiKey =
        let holidaysR = getEmployeesWithHolidayToday apiKey
        let sicksR = getEmployeesWithSickToday apiKey
        let otherR = getEmployeesWithOtherEventToday apiKey

        match holidaysR with
        | Error message -> Error message
        | Ok holidays ->
            match sicksR with
            | Error message -> Error message
            | Ok sicks ->
                match otherR with
                | Error message -> Error message
                | Ok others ->
                    Ok (List.concat [ holidays; sicks; others ])
