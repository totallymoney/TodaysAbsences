module Config

open System

let tryFetchEnvVar name =
    match Environment.GetEnvironmentVariable name with
    | var when not (String.IsNullOrEmpty var) -> Some var
    | _ -> None
    
let fetchEnvVar name = 
    tryFetchEnvVar name
    |> Option.defaultWith (fun _ -> failwith (sprintf "Environment variable %s not found" name))

type Config =
    { BobApiUrl : string
      BobApiUsername : string
      BobApiPassword : string
      SlackWebhookUrl : string
      BirthdayOptIns : string list }

let getConfig =
    { BobApiUrl = fetchEnvVar "BOB_API_URL"
      BobApiUsername = fetchEnvVar "BOB_API_USERNAME"
      BobApiPassword = fetchEnvVar "BOB_API_PASSWORD"
      SlackWebhookUrl = fetchEnvVar "SLACK_WEBHOOK_URL"
      BirthdayOptIns = (fetchEnvVar "BIRTHDAY_OPT_INS").Split('\n') 
                       |> List.ofArray 
                       |> List.map (String.until ':') }