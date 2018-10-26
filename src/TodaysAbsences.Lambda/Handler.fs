module Handler


open Amazon.Lambda.Core
open PeopleHrApi.Http
open SlackApi.Http
open System.IO
open CoreModels


[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
do ()

let environment = System.Environment.GetEnvironmentVariables ()


let getEnvVarOrThrowIfMissing key =
    if not (environment.Contains key) then
        invalidOp (sprintf "The environment varaible %s was not found" key)
    else
        environment.Item key :?> string


let private sendTodaysAbsencesMessage (logger:Logger) apiKey webhookUrl =
    getAbsences logger apiKey |> Result.bind (sendMessage webhookUrl)

let handler (_:Stream, lambdaContext:ILambdaContext) =
    let peopleHrApiKey = getEnvVarOrThrowIfMissing "PEOPLEHR_API_KEY"
    let slackWebhookUrl = getEnvVarOrThrowIfMissing "SLACK_WEBHOOK_URL"

    let logger = lambdaContext.Logger.Log

    sendTodaysAbsencesMessage logger peopleHrApiKey slackWebhookUrl
    |> function
    | Ok message -> sprintf "Ok: %s" message
    | Error message -> sprintf "Error: %s" message
