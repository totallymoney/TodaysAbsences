module Domain

open CoreModels.PeopleHrModels
open Infrastructure.PeopleHrApi
open CoreModels.PeopleHrApiDto
open Core

type FetchAbsences = FetchAbsences of (ApiKey * Result<ApiResponseAggregate, string>)
type MapDto = MapDto of (ApiResponseAggregate -> Result<DtoAggregate, string>)

type PeopleHrApiFunctions = 
    { FetchHolidays = }

let handleNaming
    (FetchAbsences fetchAbsences) 
    (MapDto dtoMapper) =
    
    let g employee = result {
        let! stuff = Mapper.Holiday.tryMapHolidayDto employee
    
        return 0

    }
        

    0

let g = 0
(*
    base:
    fetch peopleHRApi (record type with functions)
    deserialize them into DTO types
    map into Model types
    filter absences with UnknownKind
    filter absences with UnknownDuration

    daily: create daily slack message format
    weekly: create weekly slack message format
*)