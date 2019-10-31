module Infrastructure

open Core

module Http =
    open FSharp.Data
    open System

    let private bodyString = function
        | Text s -> s
        | Binary bytes -> Text.Encoding.UTF8.GetString bytes

    let private responseBody (response : HttpResponse) =
        if response.StatusCode >= 200 && response.StatusCode <= 299 then
            Ok (response.Body)
        else
            Error (sprintf "Non-success status code %i for %s" response.StatusCode response.ResponseUrl)

    let private postJsonInner url body =
        Http.Request
            ( url,
              httpMethod = "POST",
              headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json ],
              body = TextRequest body )

    let postJson url =
        postJsonInner url >> responseBody >> Result.bind (bodyString >> Ok)

module PeopleHrApi = 
    type QueryName = string 
    type ApiKey = string 

    type ApiResponseAggregate = 
        { Holidays : string
          Sick : string
          OtherEvents : string }

    type IntervalType =
        | Today
        | ThisWeek

    type QueryType =
        | Holidays
        | AbsentOrSick 
        | OtherEvents

    module QueryType = 
        let toQueryName queryType intervalType = 
            match queryType, intervalType with
            | Holidays, Today -> "Employees on holiday today"
            | AbsentOrSick, Today -> "Employees absent/sick today"
            | OtherEvents, Today -> "Employees with other events today"
            | Holidays, ThisWeek-> "Employees on holiday this week"
            | AbsentOrSick, ThisWeek-> "Employees absent/sick this week"
            | OtherEvents, ThisWeek -> "Employees with other events this week"

    let private queryRequestPayload (apiKey : ApiKey) (queryName : QueryName) =
        sprintf """{"APIKey":"%s","Action":"GetQueryResultByQueryName","QueryName":"%s"}""" apiKey queryName

    let private executeQuery apiKey queryType intervalType  =
        //TODO: move URL this to SSM
        (queryType, intervalType)
        ||> QueryType.toQueryName
        |> queryRequestPayload apiKey
        |> Http.postJson "https://api.peoplehr.net/Query"

    let private getEmployeesOnHoliday apiKey = 
        executeQuery apiKey Holidays

    let private getEmployeesWithSick apiKey = 
        executeQuery apiKey AbsentOrSick

    let private getEmployeesWithOtherEvent apiKey =
        executeQuery apiKey OtherEvents

    let private getAbsences intervalType apiKey = 
        result {
            let! holidays = getEmployeesOnHoliday apiKey intervalType
            let! sicks = getEmployeesWithSick apiKey intervalType
            let! otherEvents = getEmployeesWithOtherEvent apiKey intervalType

            return
                { Holidays = holidays
                  Sick = sicks
                  OtherEvents = otherEvents }
        }
        
    let getAbsencesToday =
        getAbsences Today

    let getAbsencesThisWeek =
        getAbsences ThisWeek