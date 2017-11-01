module Common

let flip f a b = f b a

let (=>) choice f =
    match choice with
    | Choice1Of2 x ->
        try
            Choice1Of2 <| f x
        with 
        | e -> Choice2Of2 e.Message
    | Choice2Of2 x -> Choice2Of2 x

module String =
    let endsWith (postfix: string) (s: string) = s.EndsWith(postfix) 

module Async =
    let asResult f =
        async {
            try
                do! f |> Async.Ignore
                return Ok ()
            with
            | e -> return Error e
        }
    let toResult t =
        async {
            try
                let! r = t
                return Ok r
            with
            | e -> return Error e
        }

module Domain =
    open System

    type Size = { width: int; height: int }
    type Rect = { size: Size; left: int; top: int }
    type SizeUri = { size: Size; uri: Uri }

    let fit src target =
        let rs = (float src.width) / (float src.height)
        let rt = (float target.width) / (float target.height)
        if rt > rs then
            let th = (float src.height) * (float target.width) / (float src.width) |> int
            { left = 0
              top = (target.height - th) / 2
              size = { width = target.width; height = th } }
        else 
            let tw = (float src.width) * (float target.height) / (float src.height) |> int
            { left = (target.width - tw) / 2
              top = 0
              size = { width = tw; height = target.height } }

    let private factor = 3;
    let tryNormalize x = 
        let mutable t = x.width
        let mutable n = 0

        while t >= factor do
            t <- t / factor
            n <- n + 1

        let nw = t * (int <| Math.Pow(float factor, float n))
        let nh = int <| (float nw) * (float x.height) / (float x.width)

        match nw = x.width with
        | true -> None
        | false -> Some { x with width = nw; height = nh }

module SkiaUtils =
    open SkiaSharp

    let toSKRect (rect: Domain.Rect) =
        SKRect(
            float32 <| rect.left,
            float32 <| rect.top,
            float32 <| rect.left + rect.size.width,
            float32 <| rect.top + rect.size.height)