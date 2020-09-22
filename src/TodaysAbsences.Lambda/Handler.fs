module Handler


open Amazon.Lambda.Core
open System
open Core
open Domain
open BobApi
open SlackApi.Http
open System.IO


[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
do ()

let environment = System.Environment.GetEnvironmentVariables ()


let getEnvVarOrThrowIfMissing key =
    if not (environment.Contains key) then
        invalidOp (sprintf "The environment variable %s was not found" key)
    else
        environment.Item key :?> string


let private sendTodaysAbsencesMessage (logger:Logger) apiKey webhookUrl =
    getAbsences logger (getAbsenceList apiKey) (getEmployeeDetails apiKey) DateTime.Today
    |> sendMessage logger webhookUrl

let handler (_:Stream, lambdaContext:ILambdaContext) =
    let bobApiKey = getEnvVarOrThrowIfMissing "BOB_API_KEY"
    let slackWebhookUrl = getEnvVarOrThrowIfMissing "SLACK_WEBHOOK_URL"

    let logger = lambdaContext.Logger.Log

    sendTodaysAbsencesMessage logger bobApiKey slackWebhookUrl
    |> function
       | Ok message -> sprintf "Ok: %s" message
       | Error message -> sprintf "Error: %s" message