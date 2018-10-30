module Helpers

open System
open System.Globalization
open System.Text

//https://stackoverflow.com/questions/3288114/what-does-nets-string-normalize-do/3288164#3288164
let removeAccents (s:string) = 
    s.Normalize(NormalizationForm.FormD).ToCharArray()
    |> Seq.where (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
    |> fun x -> new String(x |> Seq.toArray)

