module Infrastructure

open Errors
open Dto
open System

type GetAbsenceList = (DateTime -> Async<Result<AbsencesResponseDto, Error>>)
type GetEmployeeDetails = (unit -> Async<Result<EmployeeDetailsResponseDto, Error>>)