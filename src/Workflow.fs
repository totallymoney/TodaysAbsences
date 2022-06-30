module Workflow

open AppContext
open Dto
open System
open Domain

let getAbsences (context : Context) = 
    let fetchBobData = async {
        let! absences = Async.StartChild (context.BobApiClient.GetAbsenceList context.Today)
        let! details = Async.StartChild (context.BobApiClient.GetEmployeeDetails())

        let! absencesData = absences
        let! detailsData = details

        return (absencesData, detailsData)
    }

    result {
        let (absencesData, detailsData) = fetchBobData |> Async.RunSynchronously
        let! absences = absencesData
        let! details = detailsData

        let detailedAbsences = 
            absences.Outs
            |> List.map (fun absence -> (absence, details.Employees |> List.tryFind (fun employee -> absence.EmployeeId = employee.Id)))
            |> List.map (fun (absence, detail) -> toAbsence context.Log context.Today absence detail)

        return detailedAbsences
    }