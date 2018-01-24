module Handler


open Amazon.Lambda.Core


[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
do ()


let handler (input:string, _:ILambdaContext) = input.ToUpper()

