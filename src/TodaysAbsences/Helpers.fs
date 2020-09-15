module Helpers

open System
open System.Globalization
open System.Text

//https://stackoverflow.com/questions/3288114/what-does-nets-string-normalize-do/3288164#3288164
let removeAccents (s:string) = 
    s.Normalize(NormalizationForm.FormD).ToCharArray()
    |> Seq.where (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
    |> fun x -> new String(x |> Seq.toArray)


type ResultBuilder () =
    member __.Return (x) = Ok x
    member __.ReturnFrom (m: Result<_, _>) = m
    member __.Bind (m, f) = Result.bind f m

let result = ResultBuilder ()

let tryParse (date:string) = 
    match DateTime.TryParse date with
    | true, out -> Ok out
    | false, _ -> sprintf "Couldn't parse date: %s" date |> Error

module List = 
    let zipInto l1 l2 f = 
        List.zip l1 l2 |> List.map (fun (a, b) -> f a b)