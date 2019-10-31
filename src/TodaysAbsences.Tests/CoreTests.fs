module CoreTests

open Expecto
open Expecto.Flip
open Core
open System
open Helpers
open Newtonsoft.Json
open Newtonsoft.Json.Linq

[<Tests>]
let tests =
    testList "People HR API Holiday Response parsing" [
        test "Get days of week on Wednesday" {
            let actual = weekDaysOfWeek (DateTime(2019, 9, 4))
            let expected = [
                DateTime(2019, 9, 2)
                DateTime(2019, 9, 3)
                DateTime(2019, 9, 4)
                DateTime(2019, 9, 5)
                DateTime(2019, 9, 6)
            ]

            Expect.sequenceContainsOrder "Should be equal in the same order" expected actual
        }

        test "Get days of week on Monday" {
            let actual = weekDaysOfWeek (DateTime(2019, 9, 2))
            let expected = [
                DateTime(2019, 9, 2)
                DateTime(2019, 9, 3)
                DateTime(2019, 9, 4)
                DateTime(2019, 9, 5)
                DateTime(2019, 9, 6)
            ]

            Expect.sequenceContainsOrder "Should be equal in the same order" expected actual
        }

        test "Get days of week on Friday" {
            let actual = weekDaysOfWeek (DateTime(2019, 9, 6))
            let expected = [
                DateTime(2019, 9, 2)
                DateTime(2019, 9, 3)
                DateTime(2019, 9, 4)
                DateTime(2019, 9, 5)
                DateTime(2019, 9, 6)
            ]

            Expect.sequenceContainsOrder "Should be equal in the same order" expected actual
        }

        //TODO: move this to another file
        test "Change value of Result property to array if it's empty string" {
                let json = """
                {
                    "isError": false,
                    "Message":"The requested processed successfully.",
                    "Result": ""
                }"""

                let expectedJson = """
                {
                    "isError": false,
                    "Message":"The requested processed successfully.",
                    "Result": []
                }"""

                Expect.equal "Result property should be []" 
                    (expectedJson |> JObject.Parse |> fun exp -> exp.ToString(Formatting.Indented))
                    (json |> ensureEmptyArrayValueForResultProperty)
            }
    ]