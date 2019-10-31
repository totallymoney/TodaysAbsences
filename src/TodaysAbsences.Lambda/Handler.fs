module Handler

open Amazon.Lambda.Core
open PeopleHrApi
open SlackApi.Http
open System
open CoreModels
open Core
open Infrastructure

[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
do ()

type SquadMemberAbsences = 
    { MemberName : string 
      Absences : Absence list }

let environment = Environment.GetEnvironmentVariables ()

let getEnvVarOrThrowIfMissing key =
    if key |> environment.Contains |> not then
        invalidOp (sprintf "The environment varaible %s was not found" key)
    else
        environment.Item key :?> string

let handler (lambdaContext : ILambdaContext) =
    let peopleHrApiKey = getEnvVarOrThrowIfMissing "PEOPLEHR_API_KEY"
    let slackWebhookUrl = getEnvVarOrThrowIfMissing "SLACK_WEBHOOK_URL"

    let logger = lambdaContext.Logger.Log

    PeopleHrApi.getAbsencesToday peopleHrApiKey 
    // >>= sendMessage slackWebhookUrl
    // |> function
    //     | Ok message -> sprintf "Ok: %s" message
    //     | Error message -> sprintf "Error: %s" message

let weeklyAbsencesHandler (lambdaContext:ILambdaContext) =
    let peopleHrApiKey = getEnvVarOrThrowIfMissing "PEOPLEHR_API_KEY"
    //TODO: add to SSM
    let shieldSlackChannelId = getEnvVarOrThrowIfMissing "SHIELD_SLACK_CHANNEL_ID"
    let logger = lambdaContext.Logger.Log

    //TODO: this will go to DynamoDb
    let shield = 
        [ { Name = "Nick"; EmployeeID = "TM0072" }
          { Name = "Tom "; EmployeeID = "TM0092" }
          { Name = "Aaron"; EmployeeID = "PW26" }
          { Name = "Brian"; EmployeeID = "TM0093" }
          { Name = "Craig"; EmployeeID = "PW21" }
          { Name = "Jeremy"; EmployeeID = "PW19" }
          { Name = "Jorge"; EmployeeID = "TM0064" }
          { Name = "Neil"; EmployeeID = "PW43" }
          { Name = "Nick"; EmployeeID = "TM0072" }
          { Name = "Peter"; EmployeeID = "PW38" }
          { Name = "Tristian"; EmployeeID = "PW40" }
          { Name = "Imre"; EmployeeID = "TM0094" } ]
        // |> Shield
    
    // stuff!
    let filterSquad (squad : SquadMember list) (absences : Absence list)  = 
        // let isInSquad employeeId =
        //     squad |> List.exists (fun x -> x.EmployeeID = employeeId)

        let absencesOfSquadMember (squadMember : SquadMember) = 
            absences |> List.filter (fun a -> a.employee.employeeId = squadMember.EmployeeID)

        squad
        |> List.map (fun x -> { MemberName = x.Name; Absences = absencesOfSquadMember x })
        |> Ok

    let filterForWeek (weekdays : DateTime list) (absencesBySquadMembers : SquadMemberAbsences list) = 
        let folder 
            (acc : Map<string, (DateTime * KindOfAbsense) list>) 
            (memberAbsences : SquadMemberAbsences) = 

            0

            (*
                Other 


                Sick

                Holiday

            *)

            // let getAbsenceForDay (day : DateTime) (absence : Absence) =
            //     let getDay = 
            //         match absence.duration with
            //         | Days d -> ""
            //         | LessThanADay partOfDay -> ""
            //         | UnknownDuration -> ""

            //     None


            // if acc.ContainsKey memberAbsences.MemberName then acc
            // else 
            //     let absencesForDays = 
            //         weekdays 
            //         |> List.map (fun day -> memberAbsences.Absences |> List.tryPick (fun x -> x.duration.)) 

            


        // let g = List.fold folder Map.empty squadAbsences
        "" |> Ok
    (*
        getWeeklyAbsences - done
        Filter absences per team - done
        create message model
        generate table
        generate slack message
        send message
    *)

    // getAbsencesThisWeek logger peopleHrApiKey
    // >>= filterSquad shield
    // >>= filterForWeek (weekDaysOfWeek DateTime.Now)
    0
    // sendTodaysAbsencesMessage logger peopleHrApiKey slackWebhookUrl
    // |> function
    // | Ok message -> sprintf "Ok: %s" message
    // | Error message -> sprintf "Error: %s" message
