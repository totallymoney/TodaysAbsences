module SlackMessageBuildingTests


open Domain
open Expecto
open SlackApi

let logger _ = () 

let private baseAttachment = {
    Fallback = "Today's absences and holidays, from Bob"
    Color = "#34495e"
    Pretext = "Today's Absences and Holidays, from <https://app.hibob.com/home|Bob>"
    Text = "Sorted by Department, then by first name within departments"
    Fields = []
}


[<Tests>]
let tests =
    testList "Slack Message Tests" [

        test "Creates a single attachment with base message content and no fields, given no absences" {
            let expected = { Attachments = [ baseAttachment ] }

            Expect.equal (messageJson []) expected "Expected a single attachment, with no fields"
        }

        testList "Creates a single attachment with base message content an a field per department" [

            test "1 department" {
                let absences = [
                    {
                        Employee = { DisplayName = EmployeeDisplayName "Joe Bloggs"; 
                                     Department = Department.Tech; 
                                     Squad = None;
                                     Id = "" |> EmployeeId }
                        Details = { Policy = AbsencePolicy.Holiday; 
                                    Duration = Days 4m }
                    }
                    {
                        Employee = { DisplayName = EmployeeDisplayName "John Smith"; 
                                     Department = Department.Tech; 
                                     Squad = None;
                                     Id = "" |> EmployeeId }
                        Details = { Policy = AbsencePolicy.WorkingFromHome; 
                                    Duration = Days 1m }
                    }
                ]
                let expectedFields = [
                    {
                        Title = "Tech"
                        Value = "Joe Bloggs - Holiday - 4 days\nJohn Smith - WFH - 1 day"
                    }
                ]
                let expected = { Attachments = [ { baseAttachment with Fields = expectedFields } ] }

                Expect.equal (messageJson absences) expected "Expected a single attachment with a single field"
            }

            test "2 departments" {
                let absences = [
                    {
                        Employee = { DisplayName = EmployeeDisplayName "Steve Wozniak"; 
                                     Department = Department.Tech; 
                                     Squad = None;
                                     Id = "" |> EmployeeId }
                        Details = { Policy = AbsencePolicy.Appointment; 
                                    Duration = PartOfDay Morning }
                    }
                    {
                        Employee = { DisplayName = EmployeeDisplayName "Steve Jobs"; 
                                     Department = Department.Marketing; 
                                     Squad = None;
                                     Id = "" |> EmployeeId }
                        Details = { Policy = AbsencePolicy.Training; 
                                    Duration = Days 2m }
                    }
                ]
                let expectedFields = [
                    {
                        Title = "Marketing"
                        Value = "Steve Jobs - Training - 2 days"
                    }
                    {
                        Title = "Tech"
                        Value = "Steve Wozniak - Appointment - Part-day (AM)"
                    }
                ]
                let expected = { Attachments = [ { baseAttachment with Fields = expectedFields } ] }

                Expect.equal (messageJson absences) expected "Expected a single attachment with a two fields"
            }
        ]

        test "Creates a single attachment with base content and employees sorted by first name within departments" {
            let absences = [
                {
                    Employee = { DisplayName = EmployeeDisplayName "Alice of Wonderland"; 
                                 Department = Department.Commercial; 
                                 Squad = None;
                                 Id = "" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Appointment; 
                                Duration = Days 1m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Trevor McTest"; 
                                 Department = Department.Product; 
                                 Squad = None;
                                 Id = "" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday;
                                Duration = Days 1m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Edward Sheeran"; 
                                 Department = Department.Product; 
                                 Squad = None;
                                 Id = "" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Holiday;
                                Duration = Days 1m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Charles Chaplin"; 
                                 Department = Department.Commercial; 
                                 Squad = None;
                                 Id = "" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Appointment; 
                                Duration = Days 1m }
                }
                {
                    Employee = { DisplayName = EmployeeDisplayName "Bob Feynman"; 
                                 Department = Department.Commercial; 
                                 Squad = None;
                                 Id = "" |> EmployeeId }
                    Details = { Policy = AbsencePolicy.Appointment; 
                                Duration = Unknown "couple of hours maybe more maybe less" }
                }
            ]
            let expectedFields = [
                {
                    Title = "Commercial"
                    Value = "Alice of Wonderland - Appointment - 1 day\nBob Feynman - Appointment - Unknown duration\nCharles Chaplin - Appointment - 1 day"
                }
                {
                    Title = "Product"
                    Value = "Edward Sheeran - Holiday - 1 day\nTrevor McTest - Holiday - 1 day"
                }
            ]
            let expected = { Attachments = [ { baseAttachment with Fields = expectedFields } ] }

            Expect.equal (messageJson absences) expected "Expected a single attachment with employees sorted by first name within department"
        }

        test "Creates a JSON string that matches the content of the given model" {
            let fields = [
                {
                    Title = "Sales"
                    Value = "Carl Sagan - Holiday - 1 day"
                }
            ]
            let message = { Attachments = [ { baseAttachment with Fields = fields } ] }
            let expectedJson = """{"attachments":[{"color":"#34495e","fallback":"Today's absences and holidays, from Bob","fields":[{"title":"Sales","value":"Carl Sagan - Holiday - 1 day"}],"pretext":"Today's Absences and Holidays, from <https://app.hibob.com/home|Bob>","text":"Sorted by Department, then by first name within departments"}]}"""
            Expect.equal (messageJsonToString message) expectedJson "Expected the returned json to match the expected JSON"
        }

    ]
