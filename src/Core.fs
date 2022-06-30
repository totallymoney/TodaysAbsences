module Core

let (>>=) r f = Result.bind f r
let (|>>) r m = Result.map m r

type Logger = string -> unit

module Result = 
    let either fOk fError = function
        | Ok x -> fOk x
        | Error err -> fError err

    type ResultsBuilder () =
        member __.Bind(a, f) = Result.bind f a
        member __.Return(a) = Ok a
        member __.Zero(a:Result<_,_>) = a