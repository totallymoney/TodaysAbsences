module BobApi

open Errors
open Helpers
open Dto
open System
open FSharp.Data
open JsonHelper

let unwrapHttpResponseBodyAsText = function
    | Text text -> text
    | Binary _ -> failwith "Should be Text format"

let absencesUrl (fromDate:DateTime) (toDate:DateTime) =
    let dateFormat (date:DateTime) = date.ToString "yyyy-MM-dd"
    sprintf "https://api.hibob.com/v1/timeoff/whosout?from=%s&to=%s" (dateFormat fromDate) (dateFormat toDate)
let employeeDetailsUrl (employeeId:string ) = sprintf "https://api.hibob.com/v1/people/%s" employeeId

let getAbsenceList apiKey (today:DateTime) = 
    Http.Request
        ( absencesUrl today today,
          httpMethod = "GET",
          headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json;
                      HttpRequestHeaders.Authorization apiKey ])
    |> (fun response -> response.Body |> unwrapHttpResponseBodyAsText)
    |> deserialiseToAbsencesDto

let getEmployeeDetails apiKey (employeeId:string) = 
    Http.Request
        ( employeeDetailsUrl employeeId,
          httpMethod = "GET",
          headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json;
                      HttpRequestHeaders.Authorization apiKey ])
    |> (fun response -> response.Body |> unwrapHttpResponseBodyAsText)
    |> deserialiseToEmployeeDetailsDto