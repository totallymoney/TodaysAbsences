module Helpers

open System
open System.Globalization
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq

//https://stackoverflow.com/questions/3288114/what-does-nets-string-normalize-do/3288164#3288164
let removeAccents (s : string) = 
    s.Normalize(NormalizationForm.FormD)
    |> fun norm -> norm.ToCharArray()
    |> Seq.where (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
    |> (Seq.toArray >> String)

//The type of Result property is array but when there is no value represented, 
// instead of returning with an empty array the API returns with an empty string
let ensureEmptyArrayValueForResultProperty json = 
    let parsedJObject = JObject.Parse(json)

    let changeEmptyStringToArray str = 
        if str |> String.IsNullOrEmpty then "[]"
        else str
    
    parsedJObject.["Result"] <- parsedJObject.["Result"].ToString() |> changeEmptyStringToArray |> JToken.Parse
    parsedJObject.ToString(Formatting.Indented)