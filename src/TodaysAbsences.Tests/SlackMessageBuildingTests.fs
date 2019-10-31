module SlackMessageBuildingTests


open CoreModels
open Expecto
open SlackApi


let private baseAttachment = {
    fallback = "Today\'s absences and holidays, from PeopleHR"
    color = "#34495e"
    pretext = "Today's Absences and Holidays, from <https://totallymoney.peoplehr.net|PeopleHR>"
    text = "Sorted by Department, then by first name within departments"
    fields = []
}


[<Tests>]
let tests =
    testList "Slack Message Tests" [

        test "Creates a single attachment with base message content and no fields, given no absences" {
            let expected = { attachments = [ baseAttachment ] }

            Expect.equal (messageJson []) expected "Expected a single attachment, with no fields"
        }

        testList "Creates a single attachment with base message content an a field per department" [

            test "1 department" {
                let absences = [
                    {
                        employee = Employee.create "id" "Joe" "Bloggs" "Development"
                        kind = Holiday
                        duration = Days 4m
                    }
                    {
                        employee = Employee.create "id" "John" "Smith" "Development"
                        kind = Wfh
                        duration = Days 1m
                    }
                ]
                let expectedFields = [
                    {
                        title = "Development"
                        value = "Joe Bloggs - Holiday - 4 days\nJohn Smith - Working from Home - 1 days"
                    }
                ]
                let expected = { attachments = [ { baseAttachment with fields = expectedFields } ] }

                Expect.equal (messageJson absences) expected "Expected a single attachment with a single field"
            }

            test "2 departments" {
                let absences = [
                    {
                        employee = Employee.create "id" "Steve" "Wozniak" "Development"
                        kind = Appointment
                        duration = LessThanADay Am
                    }
                    {
                        employee = Employee.create "id" "Steve" "Jobs" "Marketing"
                        kind = Training
                        duration = Days 2m
                    }
                ]
                let expectedFields = [
                    {
                        title = "Development"
                        value = "Steve Wozniak - Appointment - Part-day (AM)"
                    }
                    {
                        title = "Marketing"
                        value = "Steve Jobs - Training - 2 days"
                    }
                ]
                let expected = { attachments = [ { baseAttachment with fields = expectedFields } ] }

                Expect.equal (messageJson absences) expected "Expected a single attachment with a two fields"
            }

            test "Fields are sorted by department name" {
                let absences = [
                    {
                        employee = Employee.create "id" "Julius" "Caesar" "Department D"
                        kind = Training
                        duration = Days 2m
                    }
                    {
                        employee = Employee.create "id" "Dead" "Mau5" "Department C"
                        kind = Training
                        duration = Days 2m
                    }
                    {
                        employee = Employee.create "id" "Mark" "Antony" "Department A"
                        kind = Training
                        duration = Days 2m
                    }
                    {
                        employee = Employee.create "id" "James" "Bond" "Department B"
                        kind = Training
                        duration = Days 2m
                    }
                ]
                let expectedFields = [
                    {
                        title = "Department A"
                        value = "Mark Antony - Training - 2 days"
                    }
                    {
                        title = "Department B"
                        value = "James Bond - Training - 2 days"
                    }
                    {
                        title = "Department C"
                        value = "Dead Mau5 - Training - 2 days"
                    }
                    {
                        title = "Department D"
                        value = "Julius Caesar - Training - 2 days"
                    }
                ]
                let expected = { attachments = [ { baseAttachment with fields = expectedFields } ] }

                Expect.equal (messageJson absences) expected "Expected a single attachment with a field for each department, sorted alphabetically"
            }
        ]

        test "Creates a single attachment with base content and employees sorted by first name within departments" {
            let absences = [
                {
                    employee = Employee.create "id" "Alice" "of Wonderland" "Sales"
                    kind = Appointment
                    duration = Days 1m
                }
                {
                    employee = Employee.create "id" "Trevor" "McTest" "Design"
                    kind = Holiday
                    duration = Days 1m
                }
                {
                    employee = Employee.create "id" "Edward" "Sheeran" "Design"
                    kind = Holiday
                    duration = Days 1m
                }
                {
                    employee = Employee.create "id" "Charles" "Chaplin" "Sales"
                    kind = Appointment
                    duration = Days 1m
                }
                {
                    employee = Employee.create "id" "Bob" "Feynman" "Sales"
                    kind = Appointment
                    duration = Days 1m
                }
            ]
            let expectedFields = [
                {
                    title = "Design"
                    value = "Edward Sheeran - Holiday - 1 days\nTrevor McTest - Holiday - 1 days"
                }
                {
                    title = "Sales"
                    value = "Alice of Wonderland - Appointment - 1 days\nBob Feynman - Appointment - 1 days\nCharles Chaplin - Appointment - 1 days"
                }
            ]
            let expected = { attachments = [ { baseAttachment with fields = expectedFields } ] }

            Expect.equal (messageJson absences) expected "Expected a single attachment with employees sorted by first name within department"
        }

        test "Creates a JSON string that matches the content of the given model" {
            let fields = [
                {
                    title = "Sales"
                    value = "Carl Sagan - Holiday - 1 days"
                }
            ]
            let message = { attachments = [ { baseAttachment with fields = fields } ] }
            let expectedJson = """{"attachments":[{"color":"#34495e","fallback":"Today's absences and holidays, from PeopleHR","fields":[{"title":"Sales","value":"Carl Sagan - Holiday - 1 days"}],"pretext":"Today's Absences and Holidays, from <https://totallymoney.peoplehr.net|PeopleHR>","text":"Sorted by Department, then by first name within departments"}]}"""
            Expect.equal (messageJsonString message) expectedJson "Expected the returned json to match the expected JSON"
        }

    ]
