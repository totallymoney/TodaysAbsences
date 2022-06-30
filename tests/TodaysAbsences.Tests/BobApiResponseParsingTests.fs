module BobHAbsenceResponseParsingTests

open Expecto
open Expecto.Flip
open Helpers
open BobApi
open Domain
open Errors
open System
open Dto
open Workflow
open AppContext
open Config

let today = DateTime.Parse "2020-01-01"
let tomorrow = DateTime.Parse "2020-01-02"
let absencesParser = deserialiseToAbsencesDto >> Result.getOk
let detailsParser = deserialiseToEmployeeDetailsDto >> Result.getOk

let emptyDetails = 
    { Employees = [] }

let testConfig = 
    { BobApiUrl = ""
      BobApiKey = ""
      SlackWebhookUrl = "" }
let testContext absences details =
    { Config = testConfig
      Log = fun _ -> ()
      Today = today 
      BobApiClient = 
        { GetAbsenceList = fun _ -> async { return Ok absences }
          GetEmployeeDetails = fun _ -> async { return Ok details } } }

[<Tests>]
let tests =
    testList "Bob API Response parsing" [
        testList "Absences response" [
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 1m } } ]

                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected the JSON to be parsed in to a 1 day absence" 
                    expected
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = PartOfDay Afternoon } } ]

                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected the JSON to be parsed in to a afternoon absence (PM)" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = PartOfDay Morning } } ]
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected the JSON to be parsed in to a morning absence (AM)" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 10.0m } } ]
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected a single multi-day absence to be parsed"
                    expected
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 9.0m } }
                    { Employee = { DisplayName = EmployeeDisplayName "Daffy Duck"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "2" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 8.5m } } ]
                
                { testContext absences emptyDetails with Today = tomorrow } 
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected multi-day absences' duration to be changed" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = PartOfDay Afternoon } } ]
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected today's portion of a multi-day absence" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 10.0m } }
                    { Employee = { DisplayName = EmployeeDisplayName "Daffy Duck"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "2" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = PartOfDay Afternoon } } ] 
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected multiple multi-day absences to be parsed" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Unknown "good morning" } } ]
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected an unknown duration in the absence" 
                    expected 
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
                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny"; 
                                   Department = Department.Other; 
                                   Squad = None;
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 10m } } ]
                
                testContext absences emptyDetails
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected a fallback to all_day portions in the absence" 
                    expected 
            }
        ]

        testList "Details response" [
            test "Can match details responses to absences" {
                let details = detailsParser """{
                    "employees": [
                        {
                            "work": {
                                "custom": {
                                    "Squad_5Gqot": "Native App"
                                },
                                "department": "Tech"
                            },
                            "id": "1"
                        }
                    ]
                }"""
                let absences =
                    { Outs = [
                        { EmployeeId = "1"
                          PolicyTypeDisplayName = "Holiday"
                          EmployeeDisplayName = "Bugs Bunny"
                          StartDate = "2020-01-01"
                          StartPortion = "good morning"
                          EndDate = "2020-01-10"
                          EndPortion = "good evening" } ] }


                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny" 
                                   Department = Department.Tech;
                                   Squad = Some (Squad "Native App")
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday
                                  Duration = Days 10m } } ]

                
                testContext absences details
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected the JSON to be parsed in to a 1 day absence" 
                    expected
            }

            test "Returns Other department when no details match" {
                let details = detailsParser """{
                    "employees": [
                    ]
                }"""
                let absences =
                    { Outs = [
                        { EmployeeId = "1"
                          PolicyTypeDisplayName = "Holiday"
                          EmployeeDisplayName = "Bugs Bunny"
                          StartDate = "2020-01-01"
                          StartPortion = "good morning"
                          EndDate = "2020-01-10"
                          EndPortion = "good evening" } ] }


                let expected = [
                    { Employee = { DisplayName = EmployeeDisplayName "Bugs Bunny" 
                                   Department = Department.Other;
                                   Squad = None
                                   Id = "1" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday
                                  Duration = Days 10m } } ]

                
                testContext absences details
                |> getAbsences
                |> Expect.wantOk "" 
                |> Expect.equal 
                    "Expected the JSON to be parsed in to a 1 day absence" 
                    expected
            }
        ]
    ]
    
