module PeopleHrSickResponseParsingTests


open CoreModels
open Expecto
open Helpers
open PeopleHrApi



[<Tests>]
let tests =
    let sickParser = Sick.parseResponseBody ignore
    
    testList "People HR API Sick Response parsing" [
        test "Parses a single day sickness" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E1",
                        "First Name": "Edward",
                        "Last Name": "Dewhurst",
                        "Department": "Development",
                        "Sick (AM/PM)": null,
                        "Sick Duration (Days)": 1,
                        "Sick Start Date": "2019/09/02",
			            "Sick End Date": "2019/09/02"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = Employee.create "E1" "Edward" "Dewhurst" "Development"
                    kind = Sick
                    duration = Days 1m
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (sickParser json)
        }

        test "Parses a morning sickness" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Elmer",
                        "Last Name": "Fudd",
                        "Department": "Wabbit Hunting",
                        "Sick (AM/PM)": "AM",
                        "Sick Duration (Days)": null,
                        "Sick Start Date": "2019/09/02",
			            "Sick End Date": "2019/09/02"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = Employee.create "E2" "Elmer" "Fudd" "Wabbit Hunting"
                    kind = Sick
                    duration = LessThanADay Am
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a morning absence" (sickParser json)
        }

        test "Parses a afternoon sickness" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Foghorn",
                        "Last Name": "Leghorn",
                        "Department": "I said",
                        "Sick (AM/PM)": "PM",
                        "Sick Duration (Days)": null,
                        "Sick Start Date": "2019/09/02",
			            "Sick End Date": "2019/09/02"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = Employee.create "E2" "Foghorn" "Leghorn" "I said"
                    kind = Sick
                    duration = LessThanADay Pm
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a afternoon absence" (sickParser json)
        }

        test "Errors because of unexpected \"Sick (AM/PM)\" value" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Foghorn",
                        "Last Name": "Leghorn",
                        "Department": "I said",
                        "Sick (AM/PM)": "foobar",
                        "Sick Duration (Days)": null,
                        "Sick Start Date": "2019/09/02",
			            "Sick End Date": "2019/09/02"
                    }
                ]
            }"""

            Expect.isError (sickParser json) "Expected \"foobar\" to cause an error when determining the sick duration"
        }

        test "Parses empty/\"No records found\" response in to empty collection" {
            let json = """
                {
                    "isError":false,
                    "Status":10,
                    "Message":"No records found.",
                    "Result":""
                }"""
            
            expectAbsences [] "Expected a empty collection of absences" (sickParser json)
        }
    ]