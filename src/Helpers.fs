module Helpers

open System
open System.Globalization
open System.Text

//https://stackoverflow.com/questions/3288114/what-does-nets-string-normalize-do/3288164#3288164
let removeAccents (s:string) =
    s.Normalize(NormalizationForm.FormD)
    |> Seq.where (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
    |> String.Concat


let tryParse (date:string) = 
    match DateTime.TryParse date with
    | true, out -> Ok out
    | false, _ -> sprintf "Couldn't parse date: %s" date |> Error
