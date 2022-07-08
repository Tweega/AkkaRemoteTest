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

    let handleAPIMsg(mailbox: Actor<StreamAPI<'T>>) (streamMsg:StreamAPI<'T>) (apiState:APIState) =
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


    let getInitialState() = 
        {APIState.RequesterMap = Map.ofList([("hello", "world")])}

    let getAkkaMailbox<'T>() = 
        let initialState = getInitialState()
        fun (mailbox: Actor<StreamAPI<'T>>) -> 
            let rec loop(state: APIState): Cont<StreamAPI<'T>, APIState> = 
                actor { 
                    let! msg = mailbox.Receive()
                    let newState = 
                        handleAPIMsg mailbox msg state
                    return! loop(newState)
                }
            loop(initialState)

    let dispatchMessage<'T> (iActorRef: IActorRef) (m: StreamAPI<'T>) =

        iActorRef.Tell m

