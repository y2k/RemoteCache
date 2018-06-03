module Downloader =
    open System
    open System.IO
    open System.Net.Http
    open Common

    type Request = AsyncReplyChannel<Result<unit, Exception>> * Uri * string

    let private download uri path (http : HttpClient) =
        async {
            use r = new HttpRequestMessage()
            r.RequestUri <- uri
            r.Headers.Referrer <- uri
            r.Headers.UserAgent.ParseAdd "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36"

            let! resp = http.SendAsync(r) |> Async.AwaitTask

            let tmp = Path.GetTempFileName()
            use tmpStream = File.OpenWrite(tmp)
            do! resp.Content.CopyToAsync(tmpStream) |> Async.AwaitTask
            tmpStream.Close()
            File.Move(tmp, path)
        }

    let agent =
        MailboxProcessor<Request>.Start(fun inbox ->
            let http = new HttpClient()
            let rec loop () = async {
                let! (reply, uri, path) = inbox.Receive()
                let! resp = Async.asResult (download uri path http)
                reply.Reply resp
                do! loop ()
            }
            loop ())

module Encoder =
    open System
    open System.IO
    open System.Diagnostics
    open Common

    type EncodeCmd = AsyncReplyChannel<Result<unit, exn>> * string * string

    let private getFFFMPEG () =
        let path = Environment.GetEnvironmentVariable("RC_FFMPEG_DIR")
        if isNull path then failwith "Env 'RC_FFMPEG_DIR' not found"
        Path.Combine(path, "ffmpeg")

    let private encode input output =
        async {
            let args = 
                sprintf "-i \"%s\" -preset medium -vprofile baseline -vcodec libx264 -acodec aac -strict -2 -g 30 -pix_fmt yuv420p -vf \"scale=trunc(in_w/2)*2:trunc(in_h/2)*2\" -f mp4 \"%s\""
                    input output

            let startInfo = ProcessStartInfo()
            startInfo.FileName <- getFFFMPEG()
            startInfo.Arguments <- args
            startInfo.UseShellExecute <- false

            let p = Process.Start(startInfo)
            do! p.Exited |> Async.AwaitEvent |> Async.Ignore
        }

    let encodeAgent =
        MailboxProcessor<EncodeCmd>.Start(fun inbox ->
            let rec loop () = async {
                let! (reply, input, output) = inbox.Receive()
                let! resp = Async.asResult (encode input output)
                reply.Reply resp
                do! loop ()
            }
            loop ())

module Resizer = 
    open System
    open System.IO
    open SkiaSharp
    open Common

    type RequestCmd = AsyncReplyChannel<Result<byte array, exn>> * string * int * int

    let private loadSized (path: string) (w: int) h =
        if not <| File.Exists(path) then failwith "file not found"
        let bitmap = SKBitmap.Decode(path)

        use resultBitmap = new SKBitmap(w, h)
        use canvas = new SKCanvas(resultBitmap)

        let rect = 
            Domain.fit
                { width = bitmap.Width; height = bitmap.Height }
                { width = w; height = h }
            |> SkiaUtils.toSKRect
        use paint = new SKPaint()
        paint.IsAntialias <- true
        paint.FilterQuality <- SKFilterQuality.High

        canvas.DrawBitmap(bitmap, rect, paint)

        let outStream = new SKDynamicMemoryWStream()
        resultBitmap.Encode(outStream, SKEncodedImageFormat.Jpeg, 90) |> ignore
        use data = outStream.DetachAsData()
        data.ToArray()

    let private generateAgent _ =
        MailboxProcessor<RequestCmd>.Start(fun inbox ->
            let rec loop () = async {
                let! (reply, path, w, h) = inbox.Receive()

                let imageResult =
                    try
                        loadSized path w h |> Ok
                    with
                    | e -> Error e

                reply.Reply imageResult
                do! loop ()
            }
            loop ())

    let private agents = Array.init Environment.ProcessorCount generateAgent
    
    let addWork path w h = 
        let agent = agents.[path.GetHashCode() % agents.Length |> Math.Abs]
        agent.PostAndAsyncReply (fun r -> r, path, w, h)

module SuaveRedirectGenerator =
    open Common.Domain
    open System
    open System.Text
    open System.Security.Cryptography

    let generate (x : SizeUri) =
        sprintf 
            "/cache/fit?url=%s&width=%d&height=%d" (Uri.EscapeDataString (x.uri.ToString())) 
            x.size.width x.size.height

    let calculateMD5Hash (uri : Uri) =
        Encoding.UTF8.GetBytes(uri.AbsoluteUri)
        |> MD5.Create().ComputeHash
        |> Array.map (fun x -> x.ToString("X2"))
        |> Array.reduce (+)

    let urlToPath rootPath uri =
        let hash = calculateMD5Hash uri
        sprintf "%s/%s/%s/%s" rootPath (hash.Substring(0, 1)) (hash.Substring(1, 2)) (hash.Substring(3))

module IOAction =
    open System.IO
    open Common.Domain

    let tryLoadImage path (r : SizeUri) = 
        async {
            let! imageFromCache = Resizer.addWork path r.size.width r.size.height
            match imageFromCache with
            | Ok x -> return Ok x
            | Error _ -> 
                return! async {
                    path |> Path.GetDirectoryName |> Directory.CreateDirectory |> ignore
                    do! Downloader.agent.PostAndAsyncReply (fun rp -> rp, r.uri, path) 
                        |> Async.Ignore
                    return! Resizer.addWork path r.size.width r.size.height
                }
        }

module WebApi =
    open System
    open System.Net
    open Suave
    open Suave.Model
    open Suave.Operators
    open Suave.Filters
    open Suave.RequestErrors
    open Suave.Redirection
    open Suave.ServerErrors
    open Suave.Successful
    open Suave.Writers
    open Common
    open Common.Domain

    let requestImage sizedUri ctx = 
        match tryNormalize sizedUri.size with
        | Some x -> FOUND (SuaveRedirectGenerator.generate { sizedUri with size = x }) ctx
        | None -> 
            async {
                let! imageResult =
                    Environment.CurrentDirectory + "/cache"
                    |> flip SuaveRedirectGenerator.urlToPath sizedUri.uri
                    |> flip IOAction.tryLoadImage sizedUri
                return!
                    match imageResult with
                    | Ok image -> 
                        (ok image) ctx 
                        >>= setMimeType "image/jpeg" 
                        >>= setHeader "Cache-Control" "public, max-age=60"
                    | Error e -> (INTERNAL_ERROR e.Message) ctx
            }

    let start () =
        let config = { defaultConfig with 
                           bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") 8080us  ] }
        let app =
            path "/cache/fit" >=> bindReq (fun r -> binding {
                let! width = r.queryParam "width" => int
                let! height = r.queryParam "height" => int
                let! uri = r.queryParam "url" => Uri
                return { uri = uri; size = { width = width; height = height } }
            }) requestImage BAD_REQUEST 
        startWebServer config app

[<EntryPoint>]
let main _ = 
    WebApi.start ()
    0