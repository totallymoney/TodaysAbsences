module PeopleHrApi


open CoreModels
open FSharp.Data


[<Literal>]
let private holidayResponseSample = """{
    "isError": false,
    "Result": [
        {
            "Employee Id": "TM0076",
            "First Name": "Joe",
            "Last Name": "Bloggs",
            "Department": "Development",
            "Holiday Start Date": "2018/01/23",
            "Holiday End Date": "2018/01/23",
            "Part of the Day": "PM",
            "Holiday Duration (Days)": 0.5
        }
    ]
}"""

type private HolidayResponse = JsonProvider<holidayResponseSample, RootName="Holiday">


let parseHolidayResponse json = {
        employee = { firstName = ""; lastName = ""; department = "" }
        kind = Holiday
        duration = Days 0
    }
