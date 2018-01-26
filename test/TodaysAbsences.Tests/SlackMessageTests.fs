module SlackMessageTests


open Expecto
open SlackMessage


[<Tests>]
let tests =
    testList "Slack Message Tests" [

        test "Creates only static message content with no fields" {
            let absences = []
                // {
                //     employee = { firstName = "Joe"; lastName = "Blogs"; department = "Development" }
                //     kind = Holiday
                //     duration = Days 4
                // }
                // {
                //     employee = { firstName = "John"; lastName = "Smith"; department = "Design" }
                //     kind = Wfh
                //     duration = Days 1
                // }
            let attachment = 
                {
                    fallback = "Today\'s absences and holidays, from PeopleHR"
                    color = "#34495e"
                    pretext = "Today's Absences and Holidays, from <https://totallymoney.peoplehr.net|PeopleHR>"
                    text = "Sorted by Department, then by first name within departments"
                    fields = []
                }
            let expected = { attachments = [ attachment ] }

            Expect.equal (messageJson absences) expected "Expected a empty"
        }
    ]
