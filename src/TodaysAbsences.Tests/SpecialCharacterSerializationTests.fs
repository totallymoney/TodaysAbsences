module SpecialCharacterSerializationTests

open Chiron.Mapping
open Expecto
open CoreModels
open Helpers
open Newtonsoft.Json.Linq

let assertEmployeeWithSpecialName nameWithAccent expected = 
    let emp = {
        firstName = nameWithAccent
        lastName = nameWithAccent;
        department = "Fun"
    }
                
    let absence = {
        kind = Holiday
        duration = Days 1.00M
        employee = emp 
    }            
               
    let namePart = 
        SlackApi.messageJson [absence] 
        |> SlackApi.messageJsonString
        |> JObject.Parse  
        |> fun x -> x.SelectToken("attachments[0].fields[0].value").ToString().Split("-").[0] 
    
    Expect.isTrue ((sprintf "%s %s " expected expected) = namePart) "Accents should have be removed"

[<Tests>]
let tests = 
    [
        testCase "Remove accents from first and last names " <| fun _ -> 
            assertEmployeeWithSpecialName "à" "a"
            assertEmployeeWithSpecialName "à" "a"  
    ] 
    |> testList "Serialize special characteres in names without accents"