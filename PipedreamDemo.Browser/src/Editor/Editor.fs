﻿[<RequireQualifiedAccess>]
module PipedreamDemo.Browser.Editor

open Browser
open Elmish
open PipedreamDemo
open PipedreamDemo.GraphManagement
open PipedreamDemo.LayoutManagement

type State =
    {
        Graph: NodeGraph
        Layout: GraphLayout
        Inputs: InputValue list
        ClickedNodeIndex: NodeIndex option
        ClickedOutputSlot: SlotAddress option
    }

[<RequireQualifiedAccess>]
type Msg =
    | InputChanged of InputIndex * float
    | NodeClicked of NodeIndex
    | OutputClicked of SlotAddress
    | InputClicked of SlotAddress
    | MouseUpOnInput of SlotAddress
    | MouseUp
    | MouseDragged of Vector
    | AddPipe of Pipe

let initialState =
    {
        Graph = fromNodes [ Input; Input; Output ]
        Layout = Positions [ XY(100., 100.); XY(100., 200.); XY(300., 150.) ]
        Inputs = [ 0.; 0. ]
        ClickedNodeIndex = None
        ClickedOutputSlot = None
    }

let getCenterOfScreen () =
    XY(Dom.window.innerWidth / 2., Dom.window.innerHeight / 2.)

let setInputs inputs state = { state with Inputs = inputs }

let mapInputs mapper state = state |> setInputs (state.Inputs |> mapper)

let mapGraph mapper state = { state with Graph = state.Graph |> mapper }

let mapLayout mapper state = { state with Layout = state.Layout |> mapper }

let clickNode nodeIndex state = { state with ClickedNodeIndex = Some nodeIndex }

let clickOutput address state = { state with ClickedOutputSlot = Some address }

let clickInput address state =
    if state.Graph |> hasLinkInto address then
        state |> mapGraph (removeLinkInto address)
    else
        state

let unclick state =
    { state with
        ClickedNodeIndex = None
        ClickedOutputSlot = None
    }

let tryAddLinkFromSelectedOutputTo input state =
    match state.ClickedOutputSlot with
    | Some output -> state |> mapGraph (tryConnect output input)
    | None -> state

let moveNodeWithIndexTo index newPos state =
    { state with
        Layout = state.Layout |> moveNode index newPos
    }

let moveClickedNodeTo newPos state =
    match state.ClickedNodeIndex with
    | Some index -> state |> moveNodeWithIndexTo index newPos
    | None -> state

let addPipeCallNodeFor pipe state =
    state
    |> mapGraph (addCallTo pipe)
    |> mapLayout (addPosition (getCenterOfScreen ()))

let init _ = initialState, Cmd.none

let update msg state =
    match msg with
    | Msg.InputChanged (index, value) ->
        state |> mapInputs (replaceAtIndex value index), Cmd.none
    | Msg.NodeClicked index -> state |> clickNode index, Cmd.none
    | Msg.OutputClicked address -> state |> clickOutput address, Cmd.none
    | Msg.InputClicked address -> state |> clickInput address, Cmd.none
    | Msg.MouseUp -> state |> unclick, Cmd.none
    | Msg.MouseUpOnInput address ->
        state |> tryAddLinkFromSelectedOutputTo address, Cmd.none
    | Msg.MouseDragged newPos -> state |> moveClickedNodeTo newPos, Cmd.none
    | Msg.AddPipe pipe -> state |> addPipeCallNodeFor pipe, Cmd.none
