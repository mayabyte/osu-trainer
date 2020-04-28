﻿module Events

open JunUtils
open System

type Background = 
    {
        startTime : int;
        filename  : string;
        xOffset   : int;
        yOffset   : int;
    }

type Video =
    {
        startTime : int;
        filename  : string;
        xOffset   : int;
        yOffset   : int;
    }

type Break =
    {
        startTime : int;
        endTime   : int;
    }

type BeatmapEvent =
    | Background of Background
    | Video      of Video
    | Break      of Break
    | Comment    of String

let isBackground          = function Background _ -> true          | _ -> false
let getBackgroundFilename = function Background x -> x.filename    | _ -> ""

// Background syntax: 0,0,filename,xOffset,yOffset
let tryParseBackground vals : Background option =
    if (typesMatch vals ["int"; "int"; "string"; "int"; "int";]) then 
        match vals with
        | ["0"; "0"; f; x; y;] when (isInt x) && (isInt y) ->
            Some({
                Background.startTime = 0;
                filename             = f;
                xOffset              = int x;
                yOffset              = int y;
            })
        | _ -> parseError vals
    else if (typesMatch vals ["int"; "int"; "string"]) then 
        match vals with
        | ["0"; "0"; f] ->
            Some({
                Background.startTime = 0;
                filename             = f;
                xOffset              = 0;
                yOffset              = 0;
            })
        | _ -> parseError vals
    else None

// Video syntax: Video,startTime,filename,xOffset,yOffset
let tryParseVideo vals : Video option =
    if (typesMatch vals ["any"; "int"; "string"; "int"; "int";]) then 
        match vals with
        | (["1"; "0"; f; x; y;] | ["Video"; "0"; f; x; y;]) when (isInt x) && (isInt y) ->
            Some({
                Video.startTime = 0;
                filename        = f;
                xOffset         = int x;
                yOffset         = int y;
            })
        | _ -> parseError vals
    else None

// Break syntax: 2,startTime,endTime
let tryParseBreak vals : Break option = 
    if (typesMatch vals ["any"; "int"; "int"]) then
        match vals with
        | ["2"; s; e;] | ["Break"; s; e;] ->
            Some({
                startTime = int s;
                endTime   = int e;
            })
        | _ -> parseError vals
    else None


let tryParseEvent line : BeatmapEvent option =
    let values = parseCsv line
    match values.[0] with

    // Background syntax: 0,0,filename,xOffset,yOffset
    | "0" ->
        match tryParseBackground values with
        | Some(bg) -> Some(Background(bg))
        | _        -> Some(Comment(line))

    // Video syntax: Video,startTime,filename,xOffset,yOffset
    | "1" | "Video" ->
        match tryParseVideo values with
        | Some(bg) -> Some(Video(bg))
        | _        -> Some(Comment(line))

    // Break syntax: 2,startTime,endTime
    | "2" ->
        match tryParseBreak values with
        | Some(br) -> Some(Break(br))
        | _        -> Some(Comment(line))

    | _ -> Some(Comment(line))

let eventToString ev =
    match ev with
    | Background bg   -> sprintf "0,0,\"%s\",%d,%d" bg.filename bg.xOffset bg.yOffset
    | Video vid       -> sprintf "Video,%d,\"%s\"" vid.startTime vid.filename
    | Break br        -> sprintf "2,%d,%d" br.startTime br.endTime
    | Comment comment -> comment

let parseEventSection : string list -> BeatmapEvent list = parseSectionUsing tryParseEvent