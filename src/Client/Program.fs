open Akka.FSharp
open Akka.Actor  
// return Deploy instance able to operate in remote scope
let deployRemotely address = Deploy(RemoteScope (Address.Parse address))  
let spawnRemote systemOrContext remoteSystemAddress actorName expr =  
    spawne systemOrContext actorName expr [SpawnOption.Deploy (deployRemotely remoteSystemAddress)]
    
let config =  
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 0
            }
        }"

[<EntryPoint>]
let main argv = 
    let system = System.create "local-system" config  
    let aref =  
        spawnRemote system "akka.tcp://remote-system@localhost:9001/" "hello"
           // actorOf wraps custom handling function with message receiver logic
           <@ actorOf (fun msg -> printfn "received '%s'" msg) @>

    // send example message to remotely deployed actor
    aref <! "Hello world"

    // thanks to location transparency, we can select 
    // remote actors as if they where existing on local node
    let sref = select "akka://local-system/user/hello" system  
    sref <! "Hello again"

    // we can still create actors in local system context
    let lref = spawn system "local" (actorOf (fun msg -> printfn "local '%s'" msg))  
    // this message should be printed in local application console
    lref <! "Hello locally"  
    ignore <| System.Console.ReadLine()
    0 // return an integer exit code