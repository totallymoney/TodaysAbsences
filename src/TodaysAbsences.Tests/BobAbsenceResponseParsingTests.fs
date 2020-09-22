module BobHAbsenceResponseParsingTests

open Expecto
open Helpers
open BobApi
open Domain
open Errors
open System
open Dto

let logger _ = () 
let today = DateTime.Parse "2020-01-01"
let tomorrow = DateTime.Parse "2020-01-02"
let absencesParser json _ = Dto.deserialiseToAbsencesDto json
let getDetails (department:Department) _ = Ok { 
    EmployeeDetailsResponseDto.Work = { Department = string department
                                        Custom = { Squad_5Gqot = None } } 
    }

[<Tests>]
let tests =
    testList "Bob API Absence Response parsing" [

        test "Parses a 1 day absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "all_day",
                            "endDate": "2020-01-01",
                            "endPortion": "all_day",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Commercial
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 1m }
                }
            ]

            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected the JSON to be parsed in to a 1 day absence" 
        }

        test "Parses afternoon absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "afternoon",
                            "endDate": "2020-01-01",
                            "endPortion": "afternoon",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = PartOfDay Afternoon }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected the JSON to be parsed in to a afternoon absence (PM)" 
        }

        test "Parses morning absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "morning",
                            "endDate": "2020-01-01",
                            "endPortion": "morning",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = PartOfDay Morning }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected the JSON to be parsed in to a morning absence (AM)" 
        }

        test "Parses a multi-day absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "all_day",
                            "endDate": "2020-01-10",
                            "endPortion": "all_day",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 10.0m }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected
                "Expected a single multi-day absence to be parsed" 
        }

        test "Changes remaining multi-day absence duration based on day" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "afternoon",
                            "endDate": "2020-01-10",
                            "endPortion": "all_day",
                            "type": "days"
                        },
                        {
                            "employeeId": "2",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 2,
                            "status": "approved",
                            "employeeDisplayName": "Daffy Duck",
                            "startDate": "2020-01-01",
                            "startPortion": "afternoon",
                            "endDate": "2020-01-10",
                            "endPortion": "morning",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 9.0m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Daffy Duck"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "2" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 8.5m }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) tomorrow) 
                expected 
                "Expected multi-day absences' duration to be changed" 
        }

        test "Returns today's portion of a multi-day absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "afternoon",
                            "endDate": "2020-01-10",
                            "endPortion": "all_day",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = PartOfDay Afternoon }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected today's portion of a multi-day absence" 
        }

        test "Parses multiple multi-day absences" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "all_day",
                            "endDate": "2020-01-10",
                            "endPortion": "all_day",
                            "type": "days"
                        },
                        {
                            "employeeId": "2",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 2,
                            "status": "approved",
                            "employeeDisplayName": "Daffy Duck",
                            "startDate": "2020-01-01",
                            "startPortion": "afternoon",
                            "endDate": "2020-01-10",
                            "endPortion": "all_day",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 10.0m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Daffy Duck"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "2" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = PartOfDay Afternoon }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected multiple multi-day absences to be parsed" 
        }

        test "Gracefully handles unknown part of the day for single-day absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "good morning",
                            "endDate": "2020-01-01",
                            "endPortion": "good evening",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Unknown "good morning" }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected an unknown duration in the absence" 
        }

        test "Gracefully handles unknown part of the day for multi-day absence" {
            let absences = absencesParser """
                {
                    "outs": [
                        {
                            "employeeId": "1",
                            "policyType": "type1",
                            "policyTypeDisplayName": "Holiday",
                            "requestId": 1,
                            "status": "approved",
                            "employeeDisplayName": "Bugs Bunny",
                            "startDate": "2020-01-01",
                            "startPortion": "good morning",
                            "endDate": "2020-01-10",
                            "endPortion": "good evening",
                            "type": "days"
                        }
                    ]
                }"""
            let department = Marketing
            let expected = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                 Department = department; 
                                 Squad = None;
                                 Id = "1" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday; 
                                Duration = Days 10m }
                }
            ]
            
            Expect.equal 
                (getAbsences logger absences (getDetails department) today) 
                expected 
                "Expected a fallback to all_day portions in the absence" 
        }

        //test "Parses empty/\"No records found\" response in to empty collection" {
        //    let json = """
        //        {
        //            "isError":false,
        //            "Status":10,
        //            "Message":"No records found.",
        //            "Result":""
        //        }"""
        //    
        //    expectAbsences [] "Expected a empty collection of absences" (holidayParser json)
        //}
    ]
