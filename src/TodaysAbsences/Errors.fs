module Errors

type Error = 
| BobApiError of string
| DeserialisationError of string