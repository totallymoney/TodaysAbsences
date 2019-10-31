module PeopleHrOtherEventResponseParsingTests

open CoreModels.PeopleHrModels.Shared
open CoreModels.PeopleHrApiDto
open CoreModels.PeopleHrModels.OtherEventDomainTypes
open Expecto
open Expecto.Flip
open Helpers
open Core

let testStartDateTime = date 2018 1 15
let testStartDateTimeString = "2018/01/15"

let testEmployee = 
    { EmployeeId = "E1"
      FirstName = "Bugs"
      LastName = "Bunny"
      Department = "Department" }

let testTimeWindow = 
    { DurationType = Days 1m
      StartDate = testStartDateTime }

let validOtherEventDtoWithDays : OtherEventResponseDto = 
    { EmployeeId = "E1"
      FirstName = "Bugs"
      LastName = "Bunny"
      Department = "Department"
      OtherEventsDurationType = "Days"
      OtherEventsReason = "Appointment"
      StartDate = testStartDateTimeString
      OtherEventsStartTime = None
      OtherEventsTotalDurationDays = 1m }

let validOtherEventDtoWithHours = 
    { EmployeeId = "E1"
      FirstName = "Bugs"
      LastName = "Bunny"
      Department = "Department"
      OtherEventsDurationType = "Hours"
      OtherEventsReason = "Appointment"
      StartDate = testStartDateTimeString
      OtherEventsStartTime = Some { Hours = 3 }
      OtherEventsTotalDurationDays = 0m }

let assertMapOtherEventDtoResultIsOk actual expected =
    Expect.isOk "Should be OK" actual

    actual
    |> Result.get
    |> Expect.equal "" expected

let assertMapOtherEventDtoResultIsError actual expected = 
    Expect.isError "Should be OK" actual

    actual 
    |> Result.getError 
    |> Expect.equal "Should be OtherEventMappingError" expected

[<Tests>]
let tests = 
    testList "API response to DTO mapping" [
        test "when somebody has a one day event" {
            let json = 
                """{
                    "Employee Id": "E1",
                    "First Name": "Bugs",
                    "Last Name": "Bunny",
                    "Department": "Department",
                    "Other Events Duration Type": "Days",
                    "Other Events Reason": "Appointment",
                    "Other Events Start Date": "2018/01/15",
                    "Other Events Start Time": null,
                    "Other Events Total Duration (Days)": 1.0
                }"""

            let actual = json |> parseJsonAndDeserialize ()

            let expected = 
                { validOtherEventDtoWithDays with 
                    StartDate = "2018/01/15" }

            Expect.equal "Should be equal" expected actual
        }

        test "when somebody has a two day event" {
            let json = 
                """{
                    "Employee Id": "E1",
                    "First Name": "Bugs",
                    "Last Name": "Bunny",
                    "Department": "Department",
                    "Other Events Duration Type": "Days",
                    "Other Events Reason": "Appointment",
                    "Other Events Start Date": "2018/01/15",
                    "Other Events Start Time": null,
                    "Other Events Total Duration (Days)": 2.0
                }"""

            let actual = json |> parseJsonAndDeserialize ()

            let expected = 
                { validOtherEventDtoWithDays with 
                    StartDate = "2018/01/15" 
                    OtherEventsTotalDurationDays = 2.0m }

            Expect.equal "Should be equal" expected actual
        }

        // test "Parses a three day working from home" {
        //     let json = """
        //     {
        //         "isError": false,
        //         "Message":"The requested processed successfully.",
        //         "Result": [
        //             {
        //                 "Employee Id": "E999",
        //                 "First Name": "Martin",
        //                 "Last Name": "Heidigger",
        //                 "Department": "Development",
        //                 "Other Events Duration Type": "Days",
        //                 "Other Events Reason": "Working from Home",
        //                 "Other Events Start Time": null,
        //                 "Other Events Total Duration (Days)": 3,
        //                 "Other Events Start Date": "2019/09/04"
        //             }
        //         ]
        //     }"""
        //     let expected = [
        //         {
        //             employee = Employee.create "E999" "Martin" "Heidigger" "Development" 
        //             kind = Wfh
        //             duration = Days 3m
        //         }
        //     ]

        //     expectAbsences expected "Expected the JSON to be parsed in to a three day training absence" (otherEventParser json)
        // }

        // test "Ignores event with null \"Other Events Reason\"" {
        //     let json = 
        //         """{
        //             "Employee Id": "E1",
        //             "First Name": "Bugs",
        //             "Last Name": "Bunny",
        //             "Department": "Department",
        //             "Other Events Duration Type": "",
        //             "Other Events Reason": "Appointment",
        //             "Other Events Start Date": "2018/01/15",
        //             "Other Events Start Time": null,
        //             "Other Events Total Duration (Days)": 1.0
        //         }"""

        //     let actual = json |> parseJsonAndDeserialize ()

        //     let expected = 
        //         { testOtherEventDto with 
        //             OtherEventsDurationType = "Days"
        //             OtherEventsReason = "Appointment"
        //             OtherEventsStartTime = None
        //             StartDate = "2018/01/15" 
        //             OtherEventsTotalDurationDays = 1.0m }

        //     Expect.equal "Should be equal" expected actual

        //     let json = """
        //     {
        //         "isError": false,
        //         "Message":"The requested processed successfully.",
        //         "Result": [
        //             {
        //                 "Employee Id": "E999",
        //                 "First Name": "Martin",
        //                 "Last Name": "Heidigger",
        //                 "Department": "Development",
        //                 "Other Events Duration Type": "Days",
        //                 "Other Events Reason": null,
        //                 "Other Events Start Time": null,
        //                 "Other Events Total Duration (Days)": 3,
        //                 "Other Events Start Date": "2019/09/04"
        //             }
        //         ]
        //     }"""

        //     expectAbsences [] "Expected the JSON to be parsed, and the event with invalid data to be ignored" (otherEventParser json)
        // }

        // test "Ignores event with null \"Other Events Duration Type\"" {
        //     let json = """
        //     {
        //         "isError": false,
        //         "Message":"The requested processed successfully.",
        //         "Result": [
        //             {
        //                 "Employee Id": "E999",
        //                 "First Name": "Martin",
        //                 "Last Name": "Heidigger",
        //                 "Department": "Development",
        //                 "Other Events Duration Type": null,
        //                 "Other Events Reason": "Appointment",
        //                 "Other Events Start Time": null,
        //                 "Other Events Total Duration (Days)": 3,
        //                 "Other Events Start Date": "2019/09/04"
        //             }
        //         ]
        //     }"""

        //     expectAbsences [] "Expected the JSON to be parsed, and the event with invalid data to be ignored" (otherEventParser json)
        // }

        // test "Parses empty/\"No records found\" response in to empty collection" {
        //     let json = """
        //         {
        //             "isError":false,
        //             "Status":10,
        //             "Message":"No records found.",
        //             "Result":""
        //         }"""
            
        //     expectAbsences [] "Expected a empty collection of absences" (otherEventParser json)
        // }

        test "when duration type is Hours" {
            let json = 
                """{
                    "Employee Id": "E1",
                    "First Name": "Bugs",
                    "Last Name": "Bunny",
                    "Department": "Department",
                    "Other Events Duration Type": "Hours",
                    "Other Events Reason": "Appointment",
                    "Other Events Start Date": "2018/01/15",
                    "Other Events Start Time": {
            			"Ticks": 522000000000,
            			"Days": 0,
            			"Hours": 14,
            			"Milliseconds": 0,
            			"Minutes": 30,
            			"Seconds": 0,
            			"TotalDays": 0.60416666666666663,
            			"TotalHours": 14.5,
                        "TotalMilliseconds": 52200000,
            			"TotalMinutes": 870,
            			"TotalSeconds": 52200
            		},
                    "Other Events Total Duration (Days)": 0.0
                }"""

            let actual = json |> parseJsonAndDeserialize ()

            let expected = 
                { validOtherEventDtoWithDays with 
                    StartDate = "2018/01/15" 
                    OtherEventsDurationType = "Hours"
                    OtherEventsStartTime = Some { Hours = 14 } 
                    OtherEventsTotalDurationDays = 0.0m }

            Expect.equal "Should be equal" expected actual
        }
    ]

module DomainTypeMappingTests = 
    open Mapper.OtherEvents

    let assertReasonType dtoValue expectedModelValue = 
        let expected = 
            { Employee = testEmployee
              Reason = expectedModelValue
              TimeWindow = testTimeWindow }

        let actual = 
            { validOtherEventDtoWithDays with
                OtherEventsReason = dtoValue}
            |> tryMapOtherEventsDtoToModel

        assertMapOtherEventDtoResultIsOk actual expected

    [<Tests>]
    let tests =
        testList "Map other event DTO to domain type when" [
            test "duration type is Days, should return with Ok" {
                let expected = 
                    let timeWindow = 
                        { DurationType = Days 42m
                          StartDate = testStartDateTime }

                    { Employee = testEmployee
                      Reason = Appointment
                      TimeWindow = timeWindow }

                let actual = 
                    { validOtherEventDtoWithDays with 
                        OtherEventsTotalDurationDays = 42m }
                    |> tryMapOtherEventsDtoToModel

                assertMapOtherEventDtoResultIsOk actual expected
            }

            test "duration type is Hours and the value of Hours is provided, should return with Ok" { 
                let expected = 
                    let timeWindow = 
                        { DurationType = Hours 8
                          StartDate = testStartDateTime }

                    { Employee = testEmployee
                      Reason = Appointment
                      TimeWindow = timeWindow }

                let actual = 
                    { validOtherEventDtoWithDays with 
                        OtherEventsDurationType = "Hours"
                        OtherEventsStartTime = Some { Hours = 8 } }
                    |> tryMapOtherEventsDtoToModel

                assertMapOtherEventDtoResultIsOk actual expected
            }

            test "duration type is Hours but the value of Hours is not provided, should return with OtherEventMappingError" { 
                let expected = OtherEventMappingError "There should be a value for hours"

                let actual = 
                    { validOtherEventDtoWithDays with 
                        OtherEventsDurationType = "Hours" }
                    |> tryMapOtherEventsDtoToModel

                assertMapOtherEventDtoResultIsError actual expected
            }

            test "duration type is neither Days or Hours, should return with OtherEventMappingError" { 
                let expected = OtherEventMappingError "Unknown duration type: something unknown"

                let actual = 
                    { validOtherEventDtoWithDays with 
                        OtherEventsDurationType = "something unknown" }
                    |> tryMapOtherEventsDtoToModel

                assertMapOtherEventDtoResultIsError actual expected
            }

            test "reason type is Appointment" {
                assertReasonType "Appointment" Appointment
            }

            test "reason type is Compassionate" {
                assertReasonType "Compassionate" Compassionate
            }

            test "reason type is Study Leave" {
                assertReasonType "Study Leave" StudyLeave
            }

            test "reason type is Training" {
                assertReasonType "Training" Training
            }

            test "reason type is Working from Home" {
                assertReasonType "Working from Home" Wfh
            }

            test "reason type is Volunteering" {
                assertReasonType "Volunteering" Volunteering
            }

            test "reason type is Conference" {
                assertReasonType "Conference" Conference
            }

            test "reason type is Jury Duty" {
                assertReasonType "Jury Duty" JuryDuty
            }

            test "reason type is unknown, should map to UnkownReason" {
                assertReasonType "something new" UnknownReason
            }
        ]