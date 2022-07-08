module SlackApi


open Chiron
open Http
open Domain
open AppContext
open JsonHelpers

type Block =
    { ``type`` : string
      text : SubBlock option }
and SubBlock =
    { ``type`` : string
      text : string }

type Message = 
    { blocks : Block seq }

module Block =
    let divider : Block =
        { ``type`` = "divider"
          text = None }
    let section text : Block =
        { ``type`` = "section"
          text = Some
            { ``type`` = "mrkdwn"
              text = text } }


let private absenceStrings : seq<Absence> -> string =
    Seq.sortBy (fun a -> (a.Employee.DisplayName))
    >> Seq.map (fun abs -> abs |> Absence.toString) 
    >> String.concat "\n"

let departmentAbsenceString (department, absences) =
    absences
    |> absenceStrings
    |> sprintf "*%s*\n%s" department

let absenceBlocks : seq<Absence> -> seq<Block> = 
    Seq.groupBy (fun a -> a.Employee.Department |> Department.unwrap) 
    >> Seq.sortBy fst 
    >> Seq.map departmentAbsenceString
    >> Seq.map Block.section

let birthdayBlocks : seq<Birthday> -> seq<Block> =
    Seq.sortBy (fun (b : Birthday) -> b.Employee.FullName)
    >> Seq.map(fun b ->
        match b.Day with
        | Some day -> sprintf ":birthday: %s (on %s)" b.Employee.FullName day
        | None -> sprintf ":birthday: %s" b.Employee.FullName
        |> Block.section)

let buildMessageBlocks absences birthdays =
    seq {
        yield Block.section "Today's Absences and Holidays, from <https://app.hibob.com/home|Bob>"
        yield Block.divider
        yield! absenceBlocks absences

        if Seq.length birthdays > 0 then
            yield Block.divider
            yield Block.section "*Birthdays*"
            yield! birthdayBlocks birthdays
     }

let buildMessage absences birthdays =
    { blocks = buildMessageBlocks absences birthdays }

module Http =
    let sendMessage (context : Context) absences birthdays =
        buildMessage absences birthdays
        |> toJson
        |> tee (toJson >> context.Log)
        |> postJson context.Config.SlackWebhookUrl