module PeopleHrResponseParsingTests


open Expecto
open CoreModels
open PeopleHrApi


[<Tests>]
let tests =
    testList "People HR API Response parsing" [
        test "Parses employee holiday" {
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
            let expected = {
                employee = { firstName = "Bugs"; lastName = "Bunny"; department = "Development" }
                kind = Holiday
                duration = Hours 12
            }
            Expect.equal expected (parseHolidayResponse json) "Expected the JSON to be parsed in to a Absense"
        }
    ]
