module BobApi

open System.Net.Http.Headers
open Errors
open Config
open Helpers
open Dto
open System
open FSharp.Data
open JsonHelpers
open System.Net.Http
open Infrastructure

type BobApiHttpClient = HttpClient
let getClient apiUsername apiPassword = 
    let c = new BobApiHttpClient()
    let credentials = System.Text.Encoding.UTF8.GetBytes $"%s{apiUsername}:%s{apiPassword}"
    c.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials))
    c

let getAbsenceList (client : BobApiHttpClient) apiUrl (today:DateTime) = async {
    let! response = 
        sprintf "%s/timeoff/whosout?from=%s&to=%s" 
            apiUrl 
            (today.ToString "yyyy-MM-dd") 
            (today.ToString "yyyy-MM-dd")
        |> client.GetAsync
        |> Async.AwaitTask

    let! content = 
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask

    return deserialiseToAbsencesDto content
}

let getEmployeeDetails (client : BobApiHttpClient) apiUrl _ = async {
    let! response = 
        sprintf "%s/people?humanReadable=true" apiUrl 
        |> client.GetAsync
        |> Async.AwaitTask

    let! content = 
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask  

    return deserialiseToEmployeeDetailsDto content
}


type BobApiClient =
    { GetAbsenceList : GetAbsenceList
      GetEmployeeDetails : GetEmployeeDetails }
let buildBobApiClient client apiUrl =
    { GetAbsenceList = getAbsenceList client apiUrl 
      GetEmployeeDetails = getEmployeeDetails client apiUrl }

let buildBobApiClient2 client apiUrl =
    { GetAbsenceList = fun _ -> async { return Ok { Outs = [] } }
      GetEmployeeDetails = fun _ -> async { return Ok { Employees = [] } } }