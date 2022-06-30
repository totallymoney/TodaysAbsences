module JsonHelpers

open Microsoft.FSharpLu.Json
open Newtonsoft.Json
open Errors

type JsonSettings =
    static member settings =
        let settings =
            JsonSerializerSettings(
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore)
        settings.Converters.Add(CompactUnionJsonConverter())
        settings
    static member formatting = Formatting.None

type JsonSerializer = With<JsonSettings>

let toJson = JsonSerializer.serialize