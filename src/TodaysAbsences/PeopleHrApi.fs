module PeopleHrApi


open CoreModels
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


    let private duration (r:OtherEventResponse.Result) =
        match r.OtherEventsDurationType with
        | "Days" -> r.OtherEventsTotalDurationDays |> Decimal.ToInt32 |> Days |> Ok
        | "Hours" ->
            if r.OtherEventsStartTime.Hours <= 12 then
                LessThanADay Am |> Ok
            else
                LessThanADay Pm |> Ok
        | _ -> invalidOp "SHIT"

    
    let private employee (r:OtherEventResponse.Result) =
        { firstName = r.FirstName; lastName = r.LastName; department = r.Department }


    let private kind (r:OtherEventResponse.Result) =
        match r.OtherEventsReason with
        | "Appointment" -> Ok Appointment
        | "Compassionate" -> Ok Compassionate
        | "Study Leave" -> Ok StudyLeave
        | "Training" -> Ok Training
        | "Working from Home" -> Ok Wfh
        | unexpected -> Error (sprintf "Unexpected \"Other Events Reason\" value: %s" unexpected)


    let private mapToAbsences =
        let mapper (r:OtherEventResponse.Result) =
            match duration r with
            | Ok d ->
                match kind r with
                | Ok k -> Ok { employee = employee r; kind = k; duration = d }
                | Error message -> Error message
            | Error message -> Error message
        
        Array.map mapper


    let parseResponseBody =
        OtherEventResponse.Parse >> (fun x -> x.Result) >> mapToAbsences >> foldIntoSingleResult
