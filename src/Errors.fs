module Errors

type Error = 
| BobApiError of string
| DeserialisationError of string
| SlackApiError of string

let unwrap = function
    | (BobApiError s)
    | (DeserialisationError s)
    | (SlackApiError s) -> s