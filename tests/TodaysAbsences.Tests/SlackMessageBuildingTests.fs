module SlackMessageBuildingTests

open Dto
open Domain
open Expecto
open Expecto.Flip
open SlackApi
open System

let logger _ = () 

let private baseBlocks = seq [
        Block.section "Today's Absences and Holidays, from <https://app.hibob.com/home|Bob>"  
        Block.divider
    ]
let private emptyEmployeeDetails = 
    { FullName = ""
      Work = 
        { Custom = 
            Some { Squad_5Gqot = None }
          Department = "" }
      Personal =
        { ShortBirthDate = "" }
      Id = ""
    }

[<Tests>]
let tests =
    testList "Slack Message Tests" [

        testList "buildMessageBlocks" [

            test "Creates blocks with base message content when no absences or birthdays" {
                Expect.sequenceEqual "" (buildMessageBlocks [] []) baseBlocks
            }

            test "Creates blocks with absence and birthday blocks when present" {
                let absences = [
                    { Employee = { DisplayName = EmployeeDisplayName "Joe Bloggs"; 
                                   Department = Department.Tech; 
                                   Squad = None;
                                   Id = "" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 4m }
                    }
                ]
                let birthdays = [
                    { Employee = { emptyEmployeeDetails with FullName = "Joe Bloggs" }
                      Day = None }
                ]

                let expected = Seq.concat [ 
                    baseBlocks 
                    [ Block.section "*Tech*\nJoe Bloggs - Holiday - 4 days"
                      Block.divider
                      Block.section "*Birthdays*"
                      Block.section ":birthday: Joe Bloggs" ]
                ]

                Expect.sequenceEqual "" (buildMessageBlocks absences birthdays) expected
            }

        ]

        testList "absenceBlocks" [

            test "builds an absence block for a single department" {
                let absences = [
                    { Employee = { DisplayName = EmployeeDisplayName "Joe Bloggs"; 
                                   Department = Department.Tech; 
                                   Squad = None;
                                   Id = "" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.CompassionateLeave; 
                                  Duration = Days 4m }
                    }
                ]
                let expected = [
                    Block.section "*Tech*\nJoe Bloggs - Leave - 4 days"
                ]

                Expect.sequenceEqual "" (absenceBlocks absences) expected
            }

            test "correctly orders employees within a department block" {
                let absences = [
                    { Employee = { DisplayName = EmployeeDisplayName "John Smith"; 
                                   Department = Department.Tech; 
                                   Squad = None;
                                   Id = "" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.WorkingFromHome; 
                                  Duration = Days 1m }
                    }
                    { Employee = { DisplayName = EmployeeDisplayName "Joe Bloggs"; 
                                   Department = Department.Tech; 
                                   Squad = None;
                                   Id = "" |> EmployeeId }
                      Details = { Policy = AbsencePolicy.Holiday; 
                                  Duration = Days 4m }
                    }
                ]
                let expected = [
                    Block.section "*Tech*\nJoe Bloggs - Holiday - 4 days\nJohn Smith - WFH - 1 day"
                ]

                Expect.sequenceEqual "" (absenceBlocks absences) expected
            }

            test "builds ordered absence blocks given multiple departments" {
                let absences = [
                    { Employee = 
                        { DisplayName = EmployeeDisplayName "Joe Bloggs"; 
                          Department = Department.Tech; 
                          Squad = None;
                          Id = "" |> EmployeeId }
                      Details = 
                        { Policy = AbsencePolicy.Holiday; 
                          Duration = Days 4m }
                    }
                    { Employee = 
                        { DisplayName = EmployeeDisplayName "John Smith"; 
                          Department = Department.Commercial; 
                          Squad = None;
                          Id = "" |> EmployeeId }
                      Details = 
                        { Policy = AbsencePolicy.WorkingFromHome; 
                          Duration = Days 1m }
                    }
                ]
                let expected = [
                    Block.section "*Commercial*\nJohn Smith - WFH - 1 day"
                    Block.section "*Tech*\nJoe Bloggs - Holiday - 4 days"
                ]

                Expect.sequenceEqual "" (absenceBlocks absences) expected
            }

        ]

        testList "birthdayBlocks" [

            test "builds a birthday block for a single employee" {
                let birthdays = [
                    { Employee = { emptyEmployeeDetails with FullName = "Joe Bloggs" }
                      Day = None }
                ]
                let expected = [
                    Block.section ":birthday: Joe Bloggs"
                ]

                Expect.sequenceEqual "" (birthdayBlocks birthdays) expected
            }

            test "includes the day when provided" {
                let birthdays = [
                    { Employee = { emptyEmployeeDetails with FullName = "Joe Bloggs" }
                      Day = Some "Saturday" }
                ]
                let expected = [
                    Block.section ":birthday: Joe Bloggs (on Saturday)"
                ]

                Expect.sequenceEqual "" (birthdayBlocks birthdays) expected
            }

            test "builds correctly ordered birthday blocks for multiple employees" {
                let birthdays = [
                    { Employee = { emptyEmployeeDetails with FullName = "John Smith" }
                      Day = None }
                    { Employee = { emptyEmployeeDetails with FullName = "Joe Bloggs" }
                      Day = None }
                ]
                let expected = [
                    Block.section ":birthday: Joe Bloggs"
                    Block.section ":birthday: John Smith"
                ]

                Expect.sequenceEqual "" (birthdayBlocks birthdays) expected
            }

        ]

    ]
