module PeopleHrOtherEventResponseParsingTests


open CoreModels
open Expecto
open Helpers
open PeopleHrApi


[<Tests>]
let tests =
    let otherEventParser = OtherEvent.parseResponseBody ignore
    
    testList "People HR API Other Event Response parsing" [

        test "Parses a one day appointment" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "Employee Id": "E999",
                        "First Name": "Santa",
                        "Last Name": "Claus",
                        "Department": "Commercial",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Appointment",
                        "Other Events Start Date": "2018/01/15",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 1.0
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Santa"; lastName = "Claus"; department = "Commercial" }
                    kind = Appointment
                    duration = Days 1m
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (otherEventParser json)
        }

        test "Parses a morning Study Leave" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "John",
                        "Last Name": "Smith",
                        "Department": "Commercial",
                        "Other Events Duration Type": "Hours",
                        "Other Events Reason": "Study Leave",
                        "Other Events Start Time": {
                            "Hours": 10
                        },
                        "Other Events Total Duration (Days)": 0.25
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "John"; lastName = "Smith"; department = "Commercial" }
                    kind = StudyLeave
                    duration = LessThanADay Am
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a morning study leave (AM)" (otherEventParser json)
        }

        test "Parses a afternoon compassionate leave" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "John",
                        "Last Name": "Smith",
                        "Department": "Commercial",
                        "Other Events Duration Type": "Hours",
                        "Other Events Reason": "Compassionate",
                        "Other Events Start Time": {
                            "Hours": 15
                        },
                        "Other Events Total Duration (Days)": 0.25
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "John"; lastName = "Smith"; department = "Commercial" }
                    kind = Compassionate
                    duration = LessThanADay Pm
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a morning compassionate leave (AM)" (otherEventParser json)
        }

        test "Parses a two day training" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Albert",
                        "Last Name": "Camus",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Training",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 2
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Albert"; lastName = "Camus"; department = "Development" }
                    kind = Training
                    duration = Days 2m
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a two day training absence" (otherEventParser json)
        }

        test "Parses a three day working from home" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Working from Home",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 3
                    }
                ]
            }"""
            let expected = [
                {
                    employee = { firstName = "Martin"; lastName = "Heidigger"; department = "Development" }
                    kind = Wfh
                    duration = Days 3m
                }
            ]

            expectAbsences expected "Expected the JSON to be parsed in to a three day training absence" (otherEventParser json)
        }

        test "Ignores event with null \"Other Events Reason\"" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": null,
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 3
                    }
                ]
            }"""

            expectAbsences [] "Expected the JSON to be parsed, and the event with invalid data to be ignored" (otherEventParser json)
        }

        test "Ignores event with null \"Other Events Duration Type\"" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": null,
                        "Other Events Reason": "Appointment",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 3
                    }
                ]
            }"""

            expectAbsences [] "Expected the JSON to be parsed, and the event with invalid data to be ignored" (otherEventParser json)
        }

        test "Parses empty/\"No records found\" response in to empty collection" {
            let json = """
                {
                    "isError":false,
                    "Status":10,
                    "Message":"No records found.",
                    "Result":""
                }"""
            
            expectAbsences [] "Expected a empty collection of absences" (otherEventParser json)
        }

        test "Parses volunteering" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Volunteering",
                        "Other Events Start Date": "2018/12/21",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 1.00
                    }
                ]
            }"""
            
            let expected = [
                {
                    employee = { firstName = "Martin"; lastName = "Heidigger"; department = "Development" }
                    kind = Volunteering
                    duration = Days 1m
                }
            ]

            expectAbsences expected "Volunteering event should be parsed" (otherEventParser json)
        }

        test "Parses conference event type" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Conference",
                        "Other Events Start Date": "2018/12/21",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 1.00
                    }
                ]
            }"""
            
            let expected = [
                {
                    employee = { firstName = "Martin"; lastName = "Heidigger"; department = "Development" }
                    kind = Conference
                    duration = Days 1m
                }
            ]

            expectAbsences expected "Conference event should be parsed" (otherEventParser json)
        }

        test "Parses JuryDuty event type" {
            let json = """
            {
                "isError": false,
                "Message":"The requested processed successfully.",
                "Result": [
                    {
                        "First Name": "Martin",
                        "Last Name": "Heidigger",
                        "Department": "Development",
                        "Other Events Duration Type": "Days",
                        "Other Events Reason": "Jury Duty",
                        "Other Events Start Date": "2018/12/21",
                        "Other Events Start Time": null,
                        "Other Events Total Duration (Days)": 1.00
                    }
                ]
            }"""
            
            let expected = [
                {
                    employee = { firstName = "Martin"; lastName = "Heidigger"; department = "Development" }
                    kind = JuryDuty
                    duration = Days 1m
                }
            ]

            expectAbsences expected "JuryDuty event should be parsed" (otherEventParser json)
        }
    ]
