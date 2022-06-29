﻿open Akka.FSharp
open Akka.Actor

type ServerMsg = 
    | AA 
    | BB

type ServerState = {
    SomeState: string
}

let config =  
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 0
            }
        }"


let createAkkaMailbox<'MsgType, 'State>(actorSystem, actorName: string, handleMsg: Actor<'MsgType> -> 'MsgType -> 'State -> 'State, initialState: 'State) : IActorRef =
    spawn actorSystem actorName <| fun (mailbox: Actor<'MsgType>) ->
        let rec loop (state: 'State) = actor {
            let! (msg:'MsgType) = mailbox.Receive()

            printfn "Received in AkkaMailbox %s Msg: %A" actorName msg

            let newState = handleMsg mailbox msg state
            return! loop newState
        }
        loop initialState

let handleRemoteMsg<'Data>(mailbox: Actor<ServerMsg>) (serverMsg:ServerMsg) (serverState:ServerState) =
    match serverMsg with
    | AA -> 
        mailbox.Self.Path.Elements
        |> Seq.iteri(fun i el -> printfn "%d: %s\n" i el)
        let kk1 = mailbox.Context.System.Settings.Home
        // mailbox.Context.System.Settings.Config.AsEnumerable()
        // |> Seq.iter(fun kvp -> 
        //     printfn "%s: %s\n" (kvp.Key) (kvp.Value.ToString())
        // )
        

        // let kk2 = 
        //     match mailbox.Self.Path.Address.Port.HasValue with
        //     | true ->
        //         "no port"
        //     | false -> 
        //         //mailbox.Self.Path.Address.Port.Value.ToString()
        //         "says it has port but.."

        // let kk3 = mailbox.Self.Path.Address.System
        // let kk4 = mailbox.Self.Path.Address.HostPort()
        // let kk5 = mailbox.Self.Path.Address.ToString()
        let kk2 = mailbox.Context.System.Settings.ProviderClass.ToLower()
        let jj = mailbox.Self.Path.ToStringWithAddress()
        // let jjg = mailbox.Self.Path.ToSerializationFormatWithAddress()


        // printfn "%s\n" zz
        printfn "%s\n" kk1
        printfn "%s\n" jj
        // printfn "%s\n" jjg
        // printfn "%s\n" kk4
        // printfn "%s\n" kk5
        serverState
    | BB -> serverState
        
[<EntryPoint>]
let main args =  
    use system = System.create "remote-system" config
    let jj = createAkkaMailbox<ServerMsg, ServerState>(system, "jojo",handleRemoteMsg, { SomeState = "hh"})
    jj.Tell AA
    ignore <| System.Console.ReadLine()
    0