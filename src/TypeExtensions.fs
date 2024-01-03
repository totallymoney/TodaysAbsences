[<AutoOpen>]
module TypeExtensions

open System
open System.Text.RegularExpressions

let tee f x =
    f x
    x
    
let (|Regex|_|) pattern options input =
   let m = Regex.Match(input, pattern, options)
   if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
   else None

module Result =
    let retn x = Ok x

    /// Predicate that returns true on success
    let isOk =
        function
        | Ok _ -> true
        | Error _ -> false

    /// Predicate that returns true on failure
    let isError xR =
        xR |> isOk |> not

    let either fOk fError = function
        | Ok x -> fOk x
        | Error err -> fError err

    let sideEffect (onOk:_ -> unit) (onError:_ -> unit) result =
        match result with
        | Ok v as s -> onOk v; s
        | Error err as e -> onError err; e

    let getOk = function
        | Ok x -> x
        | err -> failwithf "Expected Ok state, but result had error: %A" err

    let getOrFail fOk = function
        | Ok x -> x |> fOk
        | Error err -> failwith (sprintf "ERROR: %A" err)

    let getErrorOrFail fError = function
        | Ok _ -> failwith "This is an OK"
        | Error err -> err |> fError

    let apply f xResult =
        match f, xResult with
        | Ok f, Ok x -> Ok (f x)
        | Error errs, _ -> Error errs
        | _, Error errs -> Error errs

    let tee f x =
        f x
        retn x

    /// Convert a Result into an Option
    let toOption xR =
        match xR with
        | Ok v -> Some v
        | Error _ -> None

    let ofOption err = function
        | Some s -> Ok s
        | None -> Error err

    let ofChoice = function
        | Choice1Of2 x -> Ok x
        | Choice2Of2 err -> Error err

    let catch f = 
        try Ok f with err -> Error err

    type ResultsBuilder () =
        member __.Bind(a, f) = Result.bind f a
        member __.Return(a) = Ok a
        //Used when return! is called
        member __.ReturnFrom(a) = a
        member __.Zero() = __.Return ()
        member __.TryFinally(body, compensation) =
            try __.ReturnFrom(body())
            finally compensation()

        member __.Using(disposable:#System.IDisposable, body) =
            let body' = fun () -> body disposable
            __.TryFinally(body', fun () ->
                match disposable with
                    | null -> ()
                    | disp -> disp.Dispose())

let result = Result.ResultsBuilder()

module List = 
    let zipInto l1 l2 f = 
        List.zip l1 l2 |> List.map (fun (a, b) -> f a b)

module Async =
    let retn x = async.Return x

    let bind f xAsync = async.Bind (xAsync, f)

    let map f xAsync = async {
        let! x = xAsync
        return f x }

    let apply fA xAsync = async {
        let! fChild = Async.StartChild fA // run in parallel
        let! x = xAsync
        // wait for the result of the first one
        let! f = fChild
        return f x
    }

    let catch (xAsync : Async<'a>) =
        xAsync
        |> Async.Catch
        |> map Result.ofChoice

module String =
    let until (c : char) (s : string) =
        s.Substring(0, s.IndexOf c)