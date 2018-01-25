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
            "Employee Id": "E1",
            "First Name": "Joe",
            "Last Name": "Bloggs",
            "Department": "Development",
            "Holiday Start Date": "2018/01/23",
            "Holiday End Date": "2018/01/23",
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
            "Employee Id": "E1",
            "First Name": "Edward",
            "Last Name": "Dewhurst",
            "Department": "Development",
            "Sick Start Date": "2018/01/18",
            "Sick End Date": "2018/01/18",
            "Sick Duration Type": "Full day",
            "Sick (AM/PM)": "AM",
            "Sick Duration (Days)": 1,
            "Sick Duration (Hrs)": "8:00"
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



    let private mapToSicks =
        let mapper (r:SickResponse.Result) =
            match duration r with
            | Ok d -> Ok { employee = employee r; kind = Sick; duration = d}
            | Error message -> Error message

        Array.map mapper

    let parseResponseBody =
        SickResponse.Parse >> (fun x -> x.Result) >> mapToSicks >> foldIntoSingleResult



module OtherEvent =


    let parseResponseBody json =
        Error "Not implemented"
