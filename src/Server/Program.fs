open Akka.FSharp

let config =  
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 9001
            }
        }"

[<EntryPoint>]
let main args =  
    use system = System.create "remote-system" config
    ignore <| System.Console.ReadLine()
    0