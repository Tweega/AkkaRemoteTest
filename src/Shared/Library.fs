namespace Shared

module API =
    open Akka.FSharp
    open Akka.Actor
    open Akka.Remote
    open Akka.Routing

    type MessageHandler = float -> unit
    type StreamAPI = 
        | Subscribe of string * MessageHandler
        | Unsubscribe of string

    type APIState = {
        RequesterMap: Map<string, string>
    }

    let handleAPIMsg(mailbox: Actor<StreamAPI>) (streamMsg:StreamAPI) (apiState:APIState) =
        match streamMsg with 
        | Subscribe (s, f) -> 
            printfn "Got Subscribe message: %s" s
            apiState
        | Unsubscribe s -> 
            printfn "Got Unsubscribe message: %s" s

            let jj = 
                match Map.tryFind "hello" apiState.RequesterMap with 
                | Some s -> s
                | None -> "value for hello Not found"
            printfn "Map look up:: %s" jj
            apiState


    let getAkkaMailbox() = 
        fun (mailbox: Actor<StreamAPI>) -> 
            let rec loop(state: APIState): Cont<StreamAPI, APIState> = 
                actor { 
                    let! msg = mailbox.Receive()
                    let newState = 
                        handleAPIMsg mailbox msg state
                    return! loop(newState)
                }
            loop({APIState.RequesterMap = Map.ofList([("hello", "world")])})

