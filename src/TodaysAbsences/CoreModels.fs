module CoreModels

open Helpers
open Chiron

type KindOfAbsense =
    | Appointment
    | Compassionate
    | Holiday
    | Sick
    | StudyLeave
    | Training
    | Wfh
    | Volunteering
    | Conference
    | JuryDuty
    | UnknownKind
    
    override x.ToString() = 
        match x with
            | Appointment -> "Appointment"
            | Compassionate -> "Compassionate Leave"
            | Holiday -> "Holiday"
            | Sick -> "Sick Leave"
            | StudyLeave -> "Study Leave"
            | Training -> "Training"
            | Wfh -> "Working from Home"
            | Volunteering -> "Volunteering"
            | Conference -> "Conference"
            | JuryDuty -> "Jury Duty"
            | UnknownKind -> "Unknown reason"


type Employee = 
    { employeeId : string
      firstName : string
      lastName : string
      department : string }

module Employee = 
    let create id firstName lastName department = 
        { employeeId = id
          firstName = firstName
          lastName = lastName
          department = department }
          
(*
    holiday - start - end
        	"Holiday Start Date": "2019/09/02",
			"Holiday End Date": "2019/09/06",

        ### duratino = endDate = startDate, + startDate


    other - Other Events Start Date, 
    "Other Events Start Date": "2019/09/04",
			"Other Events Start Time": {
				"Ticks": 540000000000,
				"Days": 0,
				"Hours": 15,
				"Milliseconds": 0,
				"Minutes": 0,
				"Seconds": 0,
				"TotalDays": 0.625,
				"TotalHours": 15,
				"TotalMilliseconds": 54000000,
				"TotalMinutes": 900,
				"TotalSeconds": 54000
			},
			"Other Events Total Duration (Days)": 0.00,
            "Other Events Total Duration (Hrs)": "1:00"

        ### duration = days / hours, + startDate
            
    sick
        	"Sick Start Date": "2019/09/02",
			"Sick End Date": "2019/09/02",
			"Sick (AM/PM)": "PM",
            "Sick Duration (Days)": 0.50
            
            "Sick Start Date": "2019/09/03",
			"Sick End Date": "2019/09/03",
			"Sick (AM/PM)": null,
            "Sick Duration (Days)": 1.00
            

       ### duration = days or hours + startDate
*)

type PartOfDay = Am | Pm
    
type Duration =
    | Days of decimal
    | LessThanADay of PartOfDay
    | UnknownDuration
    
    override x.ToString() = 
        match x with
        | Days count -> sprintf "%M days" count
        | LessThanADay Am -> "Part-day (AM)"
        | LessThanADay Pm -> "Part-day (PM)"
        | UnknownDuration -> "Unknown duration"

type Absence = 
    {
        kind : KindOfAbsense
        duration : Duration
        employee : Employee
    }
    override x.ToString() = 
        sprintf "%s %s - %s - %s"
            (x.employee.firstName |> removeAccents) 
            (x.employee.lastName |> removeAccents)
            (x.kind.ToString()) 
            (x.duration.ToString())

type Logger = (string -> unit)

type SquadMember =
    { Name : string
      EmployeeID : string } 

type Squad =
    | Ducktales of SquadMember list
    | Scone of SquadMember list
    | Sequel of SquadMember list
    | Shield of SquadMember list
    | NoSnow of SquadMember list

module PeopleHrModels = 
    open System

    module Shared = 
        type Employee = 
            { EmployeeId : string
              FirstName : string
              LastName : string
              Department : string }

        type MappingErrors = 
            | HolidayMappingError of string
            | OtherEventMappingError of string
            | SicknessMappingError of string

    module OtherEventDomainTypes = 
        open Shared
        
        type OtherEventReasonType = 
            | Appointment
            | Compassionate
            | StudyLeave
            | Training
            | Wfh
            | Volunteering
            | Conference
            | JuryDuty
            | UnknownReason

        type Hour = int
        type Day = decimal
           
        type DurationType = 
            | Days of Day
            | Hours of Hour
            | PartDay of PartOfDay

        type TimeWindow = 
            { DurationType : DurationType
              StartDate : DateTime }
        
        type OtherEventsModel = 
            { Employee : Employee
              Reason : OtherEventReasonType
              TimeWindow : TimeWindow }

    module HolidayDomainTypes = 
        open Shared
        type HolidayTimeWindow = 
            { StartDate : DateTime 
              EndDate : DateTime 
              Duration : decimal }

        type HolidayIntervalType = 
            | Days of HolidayTimeWindow
            | Hours of (PartOfDay * HolidayTimeWindow)

        type HolidayModel =
            { Employee : Employee
              HolidayIntervalType : HolidayIntervalType
              Status : HolidayStatus }
        and HolidayStatus = Rejected | Submitted | Approved

    module SickDomainTypes = 
        open Shared

        type SickTimeWindow = 
            { StartDate : DateTime
              EndDate : DateTime
              Duration : decimal }

        type SickIntervalType = 
            | Days of SickTimeWindow
            | Hours of (PartOfDay * SickTimeWindow)

        type SickModel = 
            { Employee : Employee
              SickIntervalType : SickIntervalType }

module PeopleHrApiDto = 
    type HolidayDto =
        { EmployeeId : string
          FirstName : string
          LastName : string
          Department : string
          PartOfTheDay : string option
          HolidayDurationDays : decimal
          HolidayStart : string
          HolidayEnd : string
          Status : string }
    
        static member public FromJson (_ : HolidayDto) = json {
            let! employeeId = Json.read "Employee Id"
            let! firstName = Json.read "First Name"
            let! lastName = Json.read "Last Name"
            let! department = Json.read "Department"
            let! partOfTheDay = Json.read "Part of the Day"
            let! holidayDurationDays = Json.read "Holiday Duration (Days)"
            let! holidayStart = Json.read "Holiday Start Date"
            let! holidayEnd = Json.read "Holiday End Date"
            let! holidayStatus = Json.read "Holiday Status"
    
            return
                { EmployeeId = employeeId
                  FirstName = firstName
                  LastName = lastName
                  Department = department
                  PartOfTheDay = partOfTheDay
                  HolidayDurationDays = holidayDurationDays 
                  HolidayStart = holidayStart
                  HolidayEnd = holidayEnd
                  Status = holidayStatus }
        }

    type SickDto = 
        { EmployeeId : string
          FirstName : string
          LastName : string
          Department : string
          SickAmPm : string option
          StartDate : string
          EndDate : string
          Duration : decimal }
        static member FromJson (_ : SickDto) = json {
            let! employeeId = Json.read "Employee Id"
            let! firstName = Json.read "First Name"
            let! lastName = Json.read "Last Name"
            let! department = Json.read "Department"
            let! sickAmPm = Json.read "Sick (AM/PM)"
            let! duration = Json.read "Sick Duration (Days)"
            let! startDate = Json.read "Sick Start Date"
            let! endDate = Json.read "Sick End Date"
    
            return
                { EmployeeId = employeeId
                  FirstName = firstName
                  LastName = lastName
                  Department = department
                  SickAmPm = sickAmPm
                  StartDate = startDate
                  EndDate = endDate
                  //TODO: check the possible values
                  //TODO: add tests
                  Duration = duration }
        }

    type OtherEventsStartTime =
        { Hours : int }
    
        static member FromJson (_ : OtherEventsStartTime) = json {
            let! hours = Json.read "Hours"
            return { Hours = hours }
        }

    type OtherEventDto =
        { EmployeeId : string
          FirstName : string
          LastName : string
          Department : string
          OtherEventsDurationType : string
          OtherEventsReason : string
          StartDate : string
          OtherEventsStartTime : OtherEventsStartTime option
          OtherEventsTotalDurationDays : decimal }
    
        static member FromJson (_ : OtherEventDto) = json {
            let! employeeId = Json.read "Employee Id"
            let! firstName = Json.read "First Name"
            let! lastName = Json.read "Last Name"
            let! department = Json.read "Department"
            let! otherEventsDurationType = Json.read "Other Events Duration Type"
            let! otherEventsReason = Json.read "Other Events Reason"
            let! otherEventStartDate = Json.read "Other Events Start Date"
            let! otherEventsStartTime = Json.read "Other Events Start Time"
            let! otherEventsTotalDurationDays = Json.read "Other Events Total Duration (Days)"
    
            return
                { EmployeeId = employeeId
                  FirstName = firstName
                  LastName = lastName
                  Department = department
                  OtherEventsDurationType = otherEventsDurationType
                  OtherEventsReason = otherEventsReason
                  StartDate = otherEventStartDate
                  OtherEventsStartTime = otherEventsStartTime
                  OtherEventsTotalDurationDays = otherEventsTotalDurationDays }
        }

    type DtoAggregate = 
        { Holidays : HolidayDto list
          Sick : SickDto list
          OtherEvents : OtherEventDto list }

    type ResponseWrapper<'a> =
        { IsError : bool
          Message : string
          Result : 'a [] }

    module OtherEventResponseDto =
        let empty = 
            { EmployeeId = ""
              FirstName = ""
              LastName = ""
              Department = ""
              OtherEventsDurationType = ""
              OtherEventsReason = ""
              StartDate = ""
              OtherEventsStartTime = None
              OtherEventsTotalDurationDays = 0.0m }