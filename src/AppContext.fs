module AppContext

open System
open Config
open BobApi

type Context =
    { Config : Config
      Log : string -> unit
      Today : DateTime
      BobApiClient : BobApiClient }
let getContext log today (config : Config) =
    let bobApiClient = BobApi.getClient config.BobApiUsername config.BobApiPassword

    { Config = config
      Log = log
      Today = today
      BobApiClient = buildBobApiClient bobApiClient config.BobApiUrl }