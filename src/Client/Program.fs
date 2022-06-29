open Akka.FSharp
open Akka.Actor 
open Shared
// return Deploy instance able to operate in remote scope
let deployRemotely address = Deploy(RemoteScope (Address.Parse address))  
let spawnRemote systemOrContext remoteSystemAddress actorName expr =  
    spawne systemOrContext actorName expr [SpawnOption.Deploy (deployRemotely remoteSystemAddress)]


let tryParseAddress addressPath = 
    try
        let jj = Address.Parse addressPath    
        Some jj
    with 
        | _e -> None

let remoteDeploy systemPath = 
    let address = 
        match  tryParseAddress systemPath with
        | None -> 
            let msg = sprintf "ActorPath address cannot be parsed: %s" systemPath 
            failwith msg
        | Some a -> a
    Deploy(RemoteScope(address))

[<Literal>]
let REQ = 1

[<Literal>]
let RES = 2

    
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
    let remoteSystemAddress = "akka.tcp://remote-system@localhost:9001/"
    let initialState: API.APIState = {API.APIState.RequesterMap = Map.empty}
    let remoter = 
    // as long as actor receive logic is serializable F# Expr, there is no need for sharing any assemblies 
    // all code will be serialized, deployed to remote system and there compiled and executed
        spawne system "remote" 
            <@ 
                fun mailbox -> 
                let rec loop(state: API.APIState): Cont<int * string, API.APIState> = 
                    actor { 
                        let! msg = mailbox.Receive()
                        match msg with
                        | (REQ, m) -> 
                            mailbox.Self.Path.Elements
                            |> Seq.iteri(fun i el -> printfn "%d: %s\n" i el)
                            mailbox.Sender().Path.Elements
                            |> Seq.iteri(fun i el -> printfn "%d: %s\n" i el)
                            let kk1 = mailbox.Self.Path.Address.Host
                            let kk2 = mailbox.Context.System.Settings.ProviderClass.ToLower()
                            let kk3 = mailbox.Self.Path.Address.System
                            let kk4 = mailbox.Self.Path.Address.HostPort()
                            let kk5 = mailbox.Self.Path.Address.ToString()
                            let kk6 = mailbox.Self.Path.Address.ToString()
                            


                            // printfn "%s\n" zz
                            printfn "%s\n" kk1
                            // printfn "%d\n" kk2
                            printfn "%s\n" kk3
                            printfn "%s\n" kk4
                            printfn "%s\n" kk5


                            let uu = mailbox.Self.Path.Elements
                            let x = Map.find "hello" state.RequesterMap
                            printfn "Remote actor received: %A" x
                            mailbox.Sender() <! (RES, "ECHO " + m)
                        | _ -> logErrorf mailbox "Received unexpected message: %A" msg
                        return! loop(state)
                    }
                loop({API.APIState.RequesterMap = Map.ofList([("hello", "world")])})
                @> [ SpawnOption.Deploy(remoteDeploy remoteSystemAddress) ]
    async { 
        let! msg = remoter <? (REQ, "hello")
        match msg with
        | (RES, m) -> printfn "Remote actor responded: %s" m
        | _ -> printfn "Unexpected response from remote actor"
    }
    |> Async.RunSynchronously


    ignore <| System.Console.ReadLine()
    0 // return an integer exit code