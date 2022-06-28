
let start (filePath:string) =
    System.Diagnostics.Process.Start(filePath) |> ignore

let piServer = __SOURCE_DIRECTORY__ + @"\bin\Debug\net6.0\Server.exe"

printfn "%s" piServer


piServer |> start



