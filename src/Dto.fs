module Dto

open JsonHelpers
open Errors

type AbsenceDto = 
    { EmployeeId: string
      PolicyTypeDisplayName: string
      EmployeeDisplayName: string
      StartDate: string
      StartPortion: string
      EndDate: string
      EndPortion: string }
type AbsencesResponseDto = 
    { Outs: AbsenceDto list }
let deserialiseToAbsencesDto json = 
    try 
        JsonSerializer.deserialize<AbsencesResponseDto> json |> Ok
    with ex -> sprintf"%s\n%s" ex.Message json |> DeserialisationError  |> Error

type SquadDto = 
    { Squad_5Gqot : string option }
type EmployeeWorkDetailsDto = 
    {
        Custom: SquadDto
        Department : string
    }
type HumanReadableDto =
    {
        Work: EmployeeWorkDetailsDto
    }
type EmployeePersonalDetailsDto =
    { ShortBirthDate : string }
type EmployeeDetailsDto = 
    { HumanReadable: HumanReadableDto
      Work : EmployeeWorkDetailsDto
      Personal : EmployeePersonalDetailsDto
      Id : string
      FullName : string }
type EmployeeDetailsResponseDto =
    { Employees : EmployeeDetailsDto list }
let deserialiseToEmployeeDetailsDto json =
    try 
        JsonSerializer.deserialize<EmployeeDetailsResponseDto> json |> Ok
    with ex -> sprintf"%s\n%s" ex.Message json |> DeserialisationError  |> Error