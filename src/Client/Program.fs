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
    Configuration.parse """
        akka {
            log-config-on-start = on
            stdout-loglevel = DEBUG
            loglevel = DEBUG
            actor {
                provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
                serializers {
                    hyperion = "Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion"
                }
                serialization-bindings {
                    "System.Object" = hyperion
                }
            }
            akka.actor.serialization-settings.hyperion.cross-platform-package-name-overrides = {
                netfx = [
                {
                    fingerprint = "System.Private.CoreLib,%core%",
                    rename-from = "System.Private.CoreLib,%core%",
                    rename-to = "mscorlib,%core%"
                }]
                netcore = [
                {
                    fingerprint = "mscorlib,%core%",
                    rename-from = "mscorlib,%core%",
                    rename-to = "System.Private.CoreLib,%core%"
                }]
                net = [
                {
                    fingerprint = "mscorlib,%core%",
                    rename-from = "mscorlib,%core%",
                    rename-to = "System.Private.CoreLib,%core%"
                }]
            }
            remote {
                helios.tcp {
                    transport-class = "Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote"
                    applied-adapters = []
                    transport-protocol  = tcp
                    port = 0
                    hostname = localhost
                }
            }
        }
        """


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
                let rec loop(state: API.APIState): Cont<API.StreamAPI<float>, API.APIState> = 
                    actor { 
                        let! msg = mailbox.Receive()
                        let newState = 
                            API.handleAPIMsg mailbox msg state
                        return! loop(newState)
                    }
                loop({API.APIState.RequesterMap = Map.ofList([("hello", "world")])})
                @> [ SpawnOption.Deploy(remoteDeploy remoteSystemAddress) ]
    let m = (API.StreamAPI.Unsubscribe "ss")
        
    remoter.Tell m


    ignore <| System.Console.ReadLine()
    0 // return an integer exit code