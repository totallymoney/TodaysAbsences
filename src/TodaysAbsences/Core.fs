module Core
open System

let (>>=) r f = Result.bind f r
let (|>>) r m = Result.map m r

module Result = 
    let either fOk fError = function
        | Ok x -> fOk x
        | Error err -> fError err

    let get = function
        | Ok x -> x
        | Error e -> failwithf "Result Error: [%A]" (e.ToString())

    let getError = function
        | Ok o -> failwithf "Result Ok: [%A]" (o.ToString())
        | Error e -> e

    type ResultsBuilder () =
        member __.Bind(a, f) = Result.bind f a
        member __.Return(a) = Ok a
        member __.Zero(a:Result<_,_>) = a

let result = Result.ResultsBuilder()

let weekDaysOfWeek (checkDay : DateTime) = 
    let startOfWeek (startDayOfWeek : DayOfWeek) (dt : DateTime) =
        let diff = 
            (7 + (dt.DayOfWeek - startDayOfWeek |> int)) % 7
            |> (*) -1
            |> float
            
        dt.AddDays(diff).Date
    
    let weekFirstDay = startOfWeek DayOfWeek.Monday checkDay
    
    [0..4]
    |> List.map (float >> weekFirstDay.AddDays)

let date y m d = 
    DateTime(y, m, d)
    
module JsonHelper = 
    open Chiron
        let inline fromJson (str : string) =
            str |> Json.parse |> Json.deserialize 