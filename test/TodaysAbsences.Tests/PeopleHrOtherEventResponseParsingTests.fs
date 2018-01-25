module PeopleHrOtherEventResponseParsingTests


open CoreModels
open Expecto
open Helpers
open PeopleHrApi


[<Tests>]
let tests =
    testList "People HR API Other Event Response parsing" [
        test "Parses a one day appointment" {
            let json = """
            {
                "isError": false,
                "Result": [
                    {
                        "Employee Id": "TM0078",
                        "First Name": "Santa",
                        "Last Name": "Claus",
                        "Department": "Commercial",
                        "Other Events Duration Type": "Hours",
                        "Other Events Reason": "Appointment",
                        "Other Events Start Date": "2018/01/15",
                        "Other Events Start Time": {
                          "Ticks": 540000000000,
                          "Days": 0,
                          "Hours": 15,
                          "Milliseconds": 0,
                          "Minutes": 0,
                          "Seconds": 0,
                          "TotalDays": 0.625,
                          "TotalHours": 15,
                          "TotalMilliseconds": 54000000,
                          "TotalMinutes": 900,
                          "TotalSeconds": 54000
                        },
                        "Other Events Total Duration (Days)": 0.25
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Santa"; lastName = "Claus"; department = "Commercial" }
                    kind = Appointment
                    duration = Days 1
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (OtherEvent.parseResponseBody json)
        }
    ]