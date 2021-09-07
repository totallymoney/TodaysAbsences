module BobApi

open Errors
open Helpers
open Dto
open System
open FSharp.Data
open JsonHelper
open System.Net.Http

let client (apiKey:string) = 
    let c = new HttpClient()
    c.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey) |> ignore
    c

let absencesUrl (fromDate:DateTime) (toDate:DateTime) =
    let dateFormat (date:DateTime) = date.ToString "yyyy-MM-dd"
    sprintf "https://api.hibob.com/v1/timeoff/whosout?from=%s&to=%s" (dateFormat fromDate) (dateFormat toDate)
let employeeDetailsUrl (employeeId:string ) = sprintf "https://api.hibob.com/v1/people/%s" employeeId

let getAbsenceList apiKey (today:DateTime) = 
    absencesUrl today today 
    |> (client apiKey).GetAsync
    |> fun response -> response.Result.Content.ReadAsStringAsync().Result
    |> deserialiseToAbsencesDto

let getEmployeeDetails apiKey (employeeId:string) = 
    employeeDetailsUrl employeeId
    |> (client apiKey).GetAsync
    |> fun response -> response.Result.Content.ReadAsStringAsync().Result
    |> deserialiseToEmployeeDetailsDto