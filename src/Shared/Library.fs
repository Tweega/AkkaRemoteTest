namespace Shared

module API =
    open Akka.FSharp
    open Akka.Actor
    open Akka.Remote
    open Akka.Routing

    type MessageHandler<'T> = 'T -> unit
    type StreamAPI<'T> = 
        | Subscribe of string * MessageHandler<'T>
        | Unsubscribe of string

    type APIState = {
        RequesterMap: Map<string, string>
    }


    let handleAPIMsg(mailbox: Actor<StreamAPI<float>>) (streamMsg:StreamAPI<float>) (apiState:APIState) =
        match streamMsg with 
        | Subscribe (s, f) -> 
            printfn "Got Subscribe message: %s" s
            apiState
        | Unsubscribe s -> 
            printfn "Got Unsubscribe message: %s" s
            apiState
