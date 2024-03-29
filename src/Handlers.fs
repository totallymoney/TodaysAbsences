module Handlers

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json
open System
open Core
open Domain
open BobApi
open SlackApi.Http
open System.IO
open Workflow
open Config
open AppContext

[<LambdaSerializer(typeof<JsonSerializer>)>]
let SendAbsencesMessage (_ : Stream, lambdaContext : ILambdaContext) =
    result {
        let config = getConfig
        let context = getContext lambdaContext.Logger.Log DateTime.Today config
        let! (absences, details) = getBobData context

        let! absenceDetails = getAbsences context absences details
        let birthdays = getBirthdays context details
        do printf "%s" (JsonHelpers.JsonSerializer.serialize absenceDetails)
        do printf "%s" (JsonHelpers.JsonSerializer.serialize birthdays)
        
        return! sendMessage context absenceDetails birthdays
    }
    |> function
       | Ok message -> sprintf "Ok: %s" message
       | Error message -> sprintf "Error: %s" (Errors.unwrap message)