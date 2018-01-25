module Helpers


open Expecto
open CoreModels


let expectAbsences (expected:Absence list) message result =
    let expector absences = Expect.equal absences expected message
    Expect.isOk result "Expected the parsing to be successful"
    Result.map expector result |> ignore
