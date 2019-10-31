module RemoveAccentsTests

open Expecto
open Helpers

[<Tests>]
let tests = 
    [
        testCase "Parses à to a" <| fun _ -> 
            Expect.isTrue ("a" = (removeAccents "à")) "Should be 'a'"
            
        testCase "Parses é to e" <| fun _ ->
            Expect.isTrue ("e" = (removeAccents "é")) "Should be 'e'"
            
        //TODO: check he big versions of the accents? 
    ] 
    |> testList "Remove accents tests"

