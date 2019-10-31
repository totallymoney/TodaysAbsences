module Mapper

open System
open System.Globalization
open CoreModels
open CoreModels.PeopleHrApiDto
open Core

let parseDateTimeString str = DateTime.ParseExact(str, "yyyy/MM/dd", CultureInfo.InvariantCulture)

module Holiday = 
    open CoreModels.PeopleHrModels.Shared
    open CoreModels.PeopleHrModels.HolidayDomainTypes

    let tryMapHolidayDto (holidayDto : HolidayDto) = 
        let mapEmployee = 
            { EmployeeId = holidayDto.EmployeeId
              FirstName = holidayDto.FirstName
              LastName = holidayDto.LastName
              Department = holidayDto.Department }

        let tryMapHolidayStatus = function
            | "Approved" -> Approved |> Ok
            | "Submitted" -> Submitted |> Ok 
            | "Rejected" -> Rejected |> Ok
            | s -> sprintf "Unknown holiday status: %s" s |> HolidayMappingError |> Error

        let tryMapPartOfDay = function
            | Some "AM" -> Some(Am) |> Ok
            | Some "PM" -> Some(Pm) |> Ok
            | Some s -> sprintf "Unknown part of day: %s" s |> HolidayMappingError |> Error
            | None ->  None |> Ok

        let holidayTimeWindow = 
            { StartDate = holidayDto.HolidayStart |> parseDateTimeString
              EndDate = holidayDto.HolidayEnd |> parseDateTimeString
              Duration = holidayDto.HolidayDurationDays }
  
        let tryMapHolidayIntervalType = 
            holidayDto.PartOfTheDay |> tryMapPartOfDay
            >>= function 
                | Some partOfDay -> (partOfDay, holidayTimeWindow) |> Hours |> Ok
                | None -> holidayTimeWindow |> Days |> Ok         

        result {
            let! holidayStatus = holidayDto.Status |> tryMapHolidayStatus
            let! holidayIntervalType = tryMapHolidayIntervalType

            return 
                { Employee = mapEmployee
                  HolidayIntervalType = holidayIntervalType
                  Status = holidayStatus }
        }
        

module OtherEvents = 
    open CoreModels.PeopleHrModels.Shared
    open CoreModels.PeopleHrModels.OtherEventDomainTypes

    let getReasonType = function    
        | "Appointment" -> Appointment
        | "Compassionate" -> Compassionate
        | "Study Leave" -> StudyLeave
        | "Training" -> Training
        | "Working from Home" -> Wfh
        | "Volunteering" -> Volunteering
        | "Conference" -> Conference
        | "Jury Duty" -> JuryDuty
        | _ -> UnknownReason

    // let hoursToPartDay = function
    //     | Some hour -> 
    //         if hour <= 12 then Some (LessThanADay Am)
    //         else Some (LessThanADay Pm)
    //     | None -> None

    //TODO: change part day type?
    let hoursToPartDay (hour : Hour) = 
        if hour <= 12 then (LessThanADay Am)
        else (LessThanADay Pm)

    let tryMapOtherEventsDtoToModel (otherEventsDto : OtherEventDto) = 
        let employee = 
            { EmployeeId = otherEventsDto.EmployeeId
              FirstName = otherEventsDto.FirstName
              LastName = otherEventsDto.LastName
              Department = otherEventsDto.Department }
            
        let tryMapTimeWindow = 
            let tryMapDurationType = 
                match otherEventsDto.OtherEventsDurationType with
                | "Days" ->
                    otherEventsDto.OtherEventsTotalDurationDays |> Days |> Ok
                | "Hours" -> 
                    match otherEventsDto.OtherEventsStartTime with
                    | Some startTime -> startTime.Hours |> Hours |> Ok
                    | None -> OtherEventMappingError "There should be a value for hours" |> Error
                | s -> OtherEventMappingError (sprintf "Unknown duration type: %s" s) |> Error

            tryMapDurationType
            >>= fun dt -> 
                { DurationType = dt
                  StartDate = otherEventsDto.StartDate |> parseDateTimeString } |> Ok
        
        tryMapTimeWindow
        >>= fun timeWindow -> 
            { Employee = employee
              Reason = otherEventsDto.OtherEventsReason |> getReasonType
              TimeWindow = timeWindow } |> Ok

module Sick = 
    open CoreModels.PeopleHrModels.Shared
    open CoreModels.PeopleHrModels.SickDomainTypes

    let mapSickDtoToModel (sickResponseDto : SickDto) = 
        let employee = 
            { EmployeeId = sickResponseDto.EmployeeId
              FirstName = sickResponseDto.FirstName
              LastName = sickResponseDto.LastName
              Department = sickResponseDto.Department }

        let timeWindow = 
            { StartDate = sickResponseDto.StartDate |> parseDateTimeString
              EndDate = sickResponseDto.EndDate |> parseDateTimeString
              Duration = sickResponseDto.Duration }
        
        let tryMapIntervalType =
            match sickResponseDto.SickAmPm with
            | Some value ->
                match value with
                | "AM" -> (Am, timeWindow) |> Hours |> Ok
                | "PM" -> (Pm, timeWindow) |> Hours |> Ok
                | unexpected ->
                    sprintf "Unpected value for \"Sick (AM/PM)\": %s" unexpected |> SicknessMappingError |> Error
            | None -> timeWindow |> Days |> Ok

        tryMapIntervalType
        >>= fun intervalType ->
            { Employee = employee 
              SickIntervalType = intervalType } |> Ok