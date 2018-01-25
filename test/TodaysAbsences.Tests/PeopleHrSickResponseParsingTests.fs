module PeopleHrSickResponseParsingTests


open CoreModels
open Expecto
open Helpers
open PeopleHrApi



[<Tests>]
let tests =
    testList "People HR API Sick Response parsing" [
        test "Parses a single day sickness" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E1",
                        "First Name": "Edward",
                        "Last Name": "Dewhurst",
                        "Department": "Development",
                        "Sick Start Date": "2018/01/18",
                        "Sick End Date": "2018/01/18",
                        "Sick Duration Type": "Full day",
                        "Sick (AM/PM)": null,
                        "Sick Duration (Days)": 1,
                        "Sick Duration (Hrs)": "8:00"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Edward"; lastName = "Dewhurst"; department = "Development" }
                    kind = Sick
                    duration = Days 1
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (Sick.parseResponseBody json)
        }

        test "Parses a morning sickness" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Elmer",
                        "Last Name": "Fudd",
                        "Department": "Wabbit Hunting",
                        "Sick Start Date": "2018/01/18",
                        "Sick End Date": "2018/01/18",
                        "Sick Duration Type": null,
                        "Sick (AM/PM)": "AM",
                        "Sick Duration (Days)": null,
                        "Sick Duration (Hrs)": "4:00"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Elmer"; lastName = "Fudd"; department = "Wabbit Hunting" }
                    kind = Sick
                    duration = LessThanADay Am
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a morning absence" (Sick.parseResponseBody json)
        }

        test "Parses a afternoon sickness" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Foghorn",
                        "Last Name": "Leghorn",
                        "Department": "I said",
                        "Sick Start Date": "2018/01/18",
                        "Sick End Date": "2018/01/18",
                        "Sick Duration Type": null,
                        "Sick (AM/PM)": "PM",
                        "Sick Duration (Days)": null,
                        "Sick Duration (Hrs)": "4:00"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Foghorn"; lastName = "Leghorn"; department = "I said" }
                    kind = Sick
                    duration = LessThanADay Pm
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a afternoon absence" (Sick.parseResponseBody json)
        }

        test "Errors because of unexpected \"Sick (AM/PM)\" value" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Foghorn",
                        "Last Name": "Leghorn",
                        "Department": "I said",
                        "Sick Start Date": "2018/01/18",
                        "Sick End Date": "2018/01/18",
                        "Sick Duration Type": null,
                        "Sick (AM/PM)": "foobar",
                        "Sick Duration (Days)": null,
                        "Sick Duration (Hrs)": "4:00"
                    }
                ]
            }"""

            Expect.isError (Sick.parseResponseBody json) "Expected \"foobar\" to cause an error when determining the sick duration"
        }
    ]