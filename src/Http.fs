module Http

open FSharp.Data
open System
open Errors

let private bodyString = function
    | Text s -> s
    | Binary bytes -> Text.Encoding.UTF8.GetString bytes


let private responseBody (response:HttpResponse) =
    if response.StatusCode >= 200 && response.StatusCode <= 299 then
        Ok (bodyString response.Body)
    else
        Error (SlackApiError <| sprintf "Non-success status code %i for %s" response.StatusCode response.ResponseUrl)


let private postJsonInner url body =
    Http.Request
        ( url,
          httpMethod = "POST",
          headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json ],
          body = TextRequest body )


let postJson url =
    postJsonInner url >> responseBody
