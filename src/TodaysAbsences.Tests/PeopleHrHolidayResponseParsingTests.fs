module PeopleHrHolidayResponseParsingTests


open Expecto
open CoreModels
open Helpers
open PeopleHrApi


[<Tests>]
let tests =
    let holidayParser = Holiday.parseResponseBody ignore
    
    testList "People HR API Holiday Response parsing" [

        test "Parses a 1 day holiday" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E1",
                        "First Name": "Bugs",
                        "Last Name": "Bunny",
                        "Department": "Development",
                        "Part of the Day": null,
                        "Holiday Duration (Days)": 1
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Bugs"; lastName = "Bunny"; department = "Development" }
                    kind = Holiday
                    duration = Days 1m
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (holidayParser json)
        }

        test "Parses afternoon holiday" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E4",
                        "First Name": "Daffy",
                        "Last Name": "Duck",
                        "Department": "Marketing",
                        "Part of the Day": "PM",
                        "Holiday Duration (Days)": 0.5
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Daffy"; lastName = "Duck"; department = "Marketing" }
                    kind = Holiday
                    duration = LessThanADay Pm
                }
            ]
            
            expectAbsences expected "Expected the JSON to be parsed in to a afternoon holiday (PM)" (holidayParser json)
        }

        test "Parses morning holiday" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Lola",
                        "Last Name": "Bunny",
                        "Department": "Management",
                        "Part of the Day": "AM",
                        "Holiday Duration (Days)": 0.5
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Lola"; lastName = "Bunny"; department = "Management" }
                    kind = Holiday
                    duration = LessThanADay Am
                }
            ]
            
            expectAbsences expected "Expected the JSON to be parsed in to a afternoon holiday (PM)" (holidayParser json)
        }

        test "Parses a multi-day holiday" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E3",
                        "First Name": "Damo",
                        "Last Name": "Winto",
                        "Department": "Dota",
                        "Part of the Day": null,
                        "Holiday Duration (Days)": 9.0
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Damo"; lastName = "Winto"; department = "Dota" }
                    kind = Holiday
                    duration = Days 9m
                }
            ]

            expectAbsences expected "Expected the single multi-day holiday to be parsed" (holidayParser json)
        }

        test "Errors because of unexpected \"Part of the Day\" value" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E3",
                        "First Name": "Damo",
                        "Last Name": "Winto",
                        "Department": "Dota",
                        "Part of the Day": "the spanish inquisition",
                        "Holiday Duration (Days)": 9.0
                    }
                ]
            }"""

            Expect.isError (holidayParser json) "Expected \"the spanish inquisition\" to cause can error when determining holiday duration"
        }

        test "Parses empty/\"No records found\" response in to empty collection" {
            let json = """
                {
                    "isError":false,
                    "Status":10,
                    "Message":"No records found.",
                    "Result":""
                }"""
            
            expectAbsences [] "Expected a empty collection of absences" (holidayParser json)
        }
    ]
