module PeopleHrHolidayTests

open Expecto
open Helpers
open Core
open Expecto.Flip
open CoreModels
open CoreModels.PeopleHrApiDto
open Microsoft.FSharpLu.Json

let testHolidayDto = 
    { EmployeeId = "E1"
      FirstName = "Bugs"
      LastName = "Bunny"
      Department = "Department"
      PartOfTheDay = None
      HolidayDurationDays = 1m
      HolidayStart = "2019/09/02"
      HolidayEnd = "2019/09/02"
      Status = "Approved" }

let assertMapHolidayDtoResultIsOk actual expected =
    Expect.isOk "Should be OK" actual

    actual
    |> Result.get
    |> Expect.equal "" expected

let assertMapHolidayDtoResultIsError actual expected = 
    Expect.isError "Should be OK" actual

    actual 
    |> Result.getError 
    |> Expect.equal "Should be HolidayMappingError" expected

module ParsingTests = 
    [<Tests>]
    let tests =
        // let holidayParser = Holiday.parseResponseBody ignore
        
        testList "API response to DTO mapping" [

            test "when somebody goes for a 1 day holiday" {
                // let json = """
                // {
                //     "isError": false,
                //     "Message":"The requested processed successfully.",
                //     "Result": [
                //         {
                //             "Employee Id": "E1",
                //             "First Name": "Bugs",
                //             "Last Name": "Bunny",
                //             "Department": "Development",
                //             "Part of the Day": null,
                //             "Holiday Start Date": "2019/09/02",
                //             "Holiday End Date": "2019/09/03",
                //             "Holiday Duration (Days)": 1
                //         }
                //     ]
                // }"""
                let json =     
                    """{
                        "Employee Id": "E1",
                        "First Name": "Bugs",
                        "Last Name": "Bunny",
                        "Department": "Department",
                        "Part of the Day": null,
                        "Holiday Start Date": "2019/09/02",
    		            "Holiday End Date": "2019/09/02",
                        "Holiday Duration (Days)": 1,
                        "Holiday Status" : "Approved"
                    }"""

                let actual = json |> parseJsonAndDeserialize ()

                let expected = 
                    { testHolidayDto with 
                        HolidayDurationDays = 1.0m }

                Expect.equal "Should be equal" expected actual
                // let expected = [
                //     {
                //         employee = Employee.create "E1" "Bugs" "Bunny" "Development"
                //         kind = Holiday
                //         duration = Days 1m
                //     }
                // ]

                // expectAbsences expected "Expected the JSON to be parsed in to a 1 day absence" (holidayParser json)
            }

            test "when somebody goes for a half day holiday in the afternoon" {              
                let json = 
                    """{
                        "Employee Id": "E1",
                        "First Name": "Bugs",
                        "Last Name": "Bunny",
                        "Department": "Department",
                        "Part of the Day": "PM",
                        "Holiday Start Date": "2019/09/02",
    		            "Holiday End Date": "2019/09/02",
                        "Holiday Duration (Days)": 0.5,
                        "Holiday Status" : "Approved"
                    }"""

                let actual = json |> parseJsonAndDeserialize ()

                let expected = 
                    { testHolidayDto with 
                        HolidayDurationDays = 0.5m 
                        PartOfTheDay = Some "PM"}

                Expect.equal "Should be equal" expected actual
            }

            test "when somebody goes for a 10 days long holiday" {
                let json = 
                    """{
                        "Employee Id": "E1",
                        "First Name": "Bugs",
                        "Last Name": "Bunny",
                        "Department": "Department",
                        "Part of the Day": null,
                        "Holiday Start Date": "2019/09/02",
    		            "Holiday End Date": "2019/09/12",
                        "Holiday Duration (Days)": 10,
                        "Holiday Status" : "Approved"
                    }"""

                let actual = json |> parseJsonAndDeserialize ()

                let expected = 
                    { testHolidayDto with 
                        HolidayDurationDays = 10m
                        PartOfTheDay = None 
                        HolidayStart = "2019/09/02"
                        HolidayEnd = "2019/09/12" }

                Expect.equal "Should be equal" expected actual
            }

            test "tesztelo1" {
                let json = """
                {
                    "isError": false,
                    "Message":"The requested processed successfully.",
                    "Result": []
                }"""

                let actual = json |> Default.deserialize<ResponseWrapper<SickResponseDto>>

                let expected = 
                    {
                        IsError = false
                        Message = "The requested processed successfully."
                        Result = [||]
                    }

                Expect.equal "" expected actual    
            }
            
            // test "Errors because of unexpected \"Part of the Day\" value" {
            //     let json = """
            //     {
            //         "isError": false,
            //         "Message":"The requested processed successfully.",
            //         "Result": [
            //             {
            //                 "Employee Id": "E1",
            //                 "First Name": "Bugs",
            //                 "Last Name": "Bunny",
            //                 "Department": "Department",
            //                 "Part of the Day": null,
            //                 "Holiday Start Date": "2019/09/02",
            // 	            "Holiday End Date": "2019/09/12",
            //                 "Holiday Duration (Days)": 10,
            //                 "Holiday Status" : "Approved"
            //             }
            //         ]
            //     }"""

            //     let actual =
            //         json |> parseJsonAndDeserialize () 
                    
            //     let expected =
            //         let dto =
            //             { testHolidayDto with 
            //                 HolidayDurationDays = 10m
            //                 PartOfTheDay = None 
            //                 HolidayStart = "2019/09/02"
            //                 HolidayEnd = "2019/09/12" }                    

            //         { IsError = false
            //           Message = "The requested processed successfully."
            //           Result = [||] }
                     
            //     Expect.equal "" expected actual
            // }

            // test "Errors because of unexpected \"Part of the Day\" value" {
            //     let json = """
            //     {
            //         "isError": false,
            //         "Message":"The requested processed successfully.",
            //         "Result": [
            //             {
            //                 "Employee Id": "E3",
            //                 "First Name": "Damo",
            //                 "Last Name": "Winto",
            //                 "Department": "Dota",
            //                 "Part of the Day": "the spanish inquisition",
            //                 "Holiday Start Date": "2019/09/02",
            // 	            "Holiday End Date": "2019/09/11",
            //                 "Holiday Duration (Days)": 9.0
            //             }
            //         ]
            //     }"""

            //     Expect.isError (holidayParser json) "Expected \"the spanish inquisition\" to cause can error when determining holiday duration"
            // }

            // test "Parses empty/\"No records found\" response in to empty collection" {
            //     let json = """
            //         {
            //             "isError":false,
            //             "Status":10,
            //             "Message":"No records found.",
            //             "Result": ""
            //         }"""

            //     expectAbsences [] "Expected a empty collection of absences" (holidayParser json)
            // }
        ]

module DomainTypeMappingTests = 
    open CoreModels.PeopleHrModels.HolidayDomainTypes
    open CoreModels.PeopleHrModels.Shared
    open Mapper.Holiday

    [<Tests>]
    let tests =
        testList "Map holiday DTO to domain type when" [
            test "part of the day is AM" {
                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        let timeWindow = 
                            { StartDate = (date 2019 9 2) 
                              EndDate = (date 2019 9 2)
                              Duration = 1m }
                        (Am, timeWindow) |> Hours
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Approved }
                   
                let actual = 
                    { testHolidayDto with 
                        PartOfTheDay = Some "AM" }
                    |> tryMapHolidayDto

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "part of the day is PM" {
                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        let timeWindow = 
                            { StartDate = (date 2019 9 2) 
                              EndDate = (date 2019 9 2)
                              Duration = 1m }
                        (Pm, timeWindow) |> Hours
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Approved }
                   

                let actual = 
                    { testHolidayDto with 
                        PartOfTheDay = Some "PM" }
                    |> tryMapHolidayDto

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "part of the day is not valid" {
                let expected = HolidayMappingError "Unknown part of day: NotValidPartOfDay"

                let actual = 
                    { testHolidayDto with 
                        PartOfTheDay = Some "NotValidPartOfDay" }
                    |> tryMapHolidayDto
                
                assertMapHolidayDtoResultIsError actual expected
            }

            test "part of the dayis null" {
                let actual = 
                    { testHolidayDto with 
                        HolidayStart = "2019/09/02"
                        HolidayEnd = "2019/09/05"
                        HolidayDurationDays = 4m
                        PartOfTheDay = None }
                    |> tryMapHolidayDto

                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        { StartDate = (date 2019 9 2) 
                          EndDate = (date 2019 9 5)
                          Duration = 4m }
                        |> Days
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Approved }

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "status is Approved" {
                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        let timeWindow = 
                            { StartDate = (date 2019 9 2) 
                              EndDate = (date 2019 9 2)
                              Duration = 1m }
                        (Pm, timeWindow) |> Hours
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Approved }
                   

                let actual = 
                    { testHolidayDto with 
                        Status = "Approved"
                        PartOfTheDay = Some "PM" }
                    |> tryMapHolidayDto

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "status is Submitted" {
                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        let timeWindow = 
                            { StartDate = (date 2019 9 2) 
                              EndDate = (date 2019 9 2)
                              Duration = 1m }
                        (Pm, timeWindow) |> Hours
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Submitted }
                   

                let actual = 
                    { testHolidayDto with 
                        Status = "Submitted"
                        PartOfTheDay = Some "PM" }
                    |> tryMapHolidayDto

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "status is Rejected" {
                let expected = 
                    let employee = 
                        { EmployeeId = "E1"
                          FirstName = "Bugs"
                          LastName = "Bunny"
                          Department = "Department" }

                    let holidayIntervalType = 
                        let timeWindow = 
                            { StartDate = (date 2019 9 2) 
                              EndDate = (date 2019 9 2)
                              Duration = 1m }
                        (Pm, timeWindow) |> Hours
                   
                    { Employee = employee
                      HolidayIntervalType = holidayIntervalType
                      Status = Rejected }
                   

                let actual = 
                    { testHolidayDto with 
                        Status = "Rejected"
                        PartOfTheDay = Some "PM" }
                    |> tryMapHolidayDto

                assertMapHolidayDtoResultIsOk actual expected
            }

            test "status is not valid" {
                let actual = 
                    { testHolidayDto with 
                        Status = "SomeNewState"
                        PartOfTheDay = Some "PM" }
                    |> tryMapHolidayDto
                
                let expected = HolidayMappingError "Unknown holiday status: SomeNewState"

                assertMapHolidayDtoResultIsError actual expected
            }
        ]