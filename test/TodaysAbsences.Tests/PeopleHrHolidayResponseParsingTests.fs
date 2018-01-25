module PeopleHrHolidayResponseParsingTests


open Expecto
open CoreModels
open Helpers
open PeopleHrApi


[<Tests>]
let tests =
    testList "People HR API Holiday Response parsing" [

        test "Parses 1 day holiday" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E1",
                        "First Name": "Bugs",
                        "Last Name": "Bunny",
                        "Department": "Development",
                        "Holiday Start Date": "2018/01/23",
                        "Holiday End Date": "2018/01/23",
                        "Part of the Day": null,
                        "Holiday Duration (Days)": 1,
                        "Holiday Status": "Approved"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Bugs"; lastName = "Bunny"; department = "Development" }
                    kind = Holiday
                    duration = Days 1
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (Holiday.parseResponseBody json)
        }

        test "Parses afternoon holiday" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E4",
                        "First Name": "Daffy",
                        "Last Name": "Duck",
                        "Department": "Marketing",
                        "Holiday Start Date": "2018/01/23",
                        "Holiday End Date": "2018/01/23",
                        "Part of the Day": "PM",
                        "Holiday Duration (Days)": 0.5,
                        "Holiday Status": "Approved"
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
            
            expectAbsences expected "Expected the JSON to be parsed in to a afternoon holiday (PM)" (Holiday.parseResponseBody json)
        }

        test "Parses morning holiday" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E2",
                        "First Name": "Lola",
                        "Last Name": "Bunny",
                        "Department": "Management",
                        "Holiday Start Date": "2018/01/23",
                        "Holiday End Date": "2018/01/23",
                        "Part of the Day": "AM",
                        "Holiday Duration (Days)": 0.5,
                        "Holiday Status": "Approved"
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
            
            expectAbsences expected "Expected the JSON to be parsed in to a afternoon holiday (PM)" (Holiday.parseResponseBody json)
        }

        test "Parses a multi-day holiday" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E3",
                        "First Name": "Damo",
                        "Last Name": "Winto",
                        "Department": "Dota",
                        "Holiday Start Date": "2018/01/22",
                        "Holiday End Date": "2018/02/02",
                        "Part of the Day": null,
                        "Holiday Duration (Days)": 9.0,
                        "Holiday Status": "Approved"
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Damo"; lastName = "Winto"; department = "Dota" }
                    kind = Holiday
                    duration = Days 9
                }
            ]

            expectAbsences expected "Expected the single multi-day holiday to be parsed" (Holiday.parseResponseBody json)
        }

        test "Errors because of unexpected \"Part of the Day\" value" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "E3",
                        "First Name": "Damo",
                        "Last Name": "Winto",
                        "Department": "Dota",
                        "Holiday Start Date": "2018/01/22",
                        "Holiday End Date": "2018/02/02",
                        "Part of the Day": "the spanish inquisition",
                        "Holiday Duration (Days)": 9.0,
                        "Holiday Status": "Approved"
                    }
                ]
            }"""

            Expect.isError (Holiday.parseResponseBody json) "Expected \"the spanish inquisition\" to cause can error when determining holiday duration"
        }
    ]
