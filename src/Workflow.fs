module Workflow

open System.Globalization
open AppContext
open Dto
open System
open Domain

let getBobData context = result {
    let fetchBobData context = async {
        let! absences = Async.StartChild (context.BobApiClient.GetAbsenceList context.Today)
        let! details = Async.StartChild (context.BobApiClient.GetEmployeeDetails())

        let! absencesData = absences
        let! detailsData = details

        return (absencesData, detailsData)
    }

    let (absencesData, detailsData) = fetchBobData context |> Async.RunSynchronously
    let! absences = absencesData
    let! details = detailsData
    return (absences, details)
}

let getAbsences (context : Context) absences details = 
    result {
        let detailedAbsences = 
            absences.Outs
            |> List.map (fun absence -> (absence, details.Employees |> List.tryFind (fun employee -> absence.EmployeeId = employee.Id)))
            |> List.map (fun (absence, detail) -> toAbsence context.Log context.Today absence detail)

        return detailedAbsences
    }
    
let getBirthdayDate (context : Context) (emp : EmployeeDetailsDto) =
    let birthdayString = $"{emp.Personal.ShortBirthDate}/{context.Today.Year}"
    match DateTime.TryParseExact(birthdayString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None) with
    | true, dt -> dt
    | false, _ ->
        DateTime.ParseExact(birthdayString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
    

let getBirthdays (context : Context) (details : EmployeeDetailsResponseDto) = 
        details.Employees
        |> List.filter (fun emp -> List.contains emp.Id context.Config.BirthdayOptIns)
        |> List.choose (fun emp -> 
            let birthday = getBirthdayDate context emp
            match context.Today.DayOfWeek, (birthday - context.Today).Days with
            | DayOfWeek.Friday, n when n = 1 || n = 2 -> 
                Some { Employee = emp
                       Day = Some (string birthday.DayOfWeek) }
            | _, 0 -> 
                Some { Employee = emp
                       Day = None}
            | _ -> None)
