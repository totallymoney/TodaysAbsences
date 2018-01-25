module PeopleHrApi


open CoreModels
open FSharp.Data
open System


[<Literal>]
let private holidayResponseSample = """{
    "isError": false,
    "Result": [
        {
            "Employee Id": "TM0076",
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

type private HolidayResponse = JsonProvider<holidayResponseSample, RootName="Holiday">


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


let private mapToAbsences  =
    let mapper (r:HolidayResponse.Result) =
        match duration r with
        | Ok d -> Ok { employee = employee r; kind = Holiday; duration = d }
        | Error message -> Error message

    Array.map mapper

let private foldIntoSingleResult results =
    let folder (state:Result<Absence list, string>) (result:Result<Absence, string>) =
        match result with
        | Ok absence -> Result.map (fun absences -> absence :: absences) state
        | Error message -> Error message

    Array.fold folder (Ok []) results

let parseHolidaysResponse =
    HolidayResponse.Parse >> (fun x -> x.Result) >> mapToAbsences >> foldIntoSingleResult
