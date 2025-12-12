module Program

open System
open System.IO
open System.Reflection
open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.FileProviders
open DictionaryApp
open DictionaryApp.Crud
open DictionaryApp.Search

let builder = WebApplication.CreateBuilder()
let app = builder.Build()

let asmLocation = Assembly.GetExecutingAssembly().Location
let exeDir =
    if String.IsNullOrEmpty asmLocation then AppContext.BaseDirectory
    else Path.GetDirectoryName(asmLocation)

let wwwRootCandidate = Path.Combine(exeDir, "wwwroot")
let wwwRootPath =
    if Directory.Exists(wwwRootCandidate) then wwwRootCandidate
    else Path.Combine(AppContext.BaseDirectory, "wwwroot")

let provider = PhysicalFileProvider(wwwRootPath)
app.UseStaticFiles(StaticFileOptions(FileProvider = provider)) |> ignore

let badRequest (ctx: HttpContext) msg =
    ctx.Response.StatusCode <- 400
    ctx.Response.WriteAsJsonAsync(box {| error = msg |}) :> Task

let notFound (ctx: HttpContext) msg =
    ctx.Response.StatusCode <- 404
    ctx.Response.WriteAsJsonAsync(box {| error = msg |}) :> Task

app.MapGet("/words", Func<HttpContext, Task>(fun ctx ->
    ctx.Response.WriteAsJsonAsync(Crud.listAll()) :> Task)) |> ignore

app.MapGet("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    match Crud.getWord term with
    | Ok w -> ctx.Response.WriteAsJsonAsync(w) :> Task
    | Error _ -> notFound ctx "Word not found")) |> ignore

app.MapPost("/word", Func<HttpContext, Task>(fun ctx ->
    task {
        try
            let! body = ctx.Request.ReadFromJsonAsync<JsonElement>()

            if not (body.TryGetProperty("term", &Unchecked.defaultof<_>)) then
                return! badRequest ctx "Missing 'term'"

            let term = body.GetProperty("term").GetString()

            if String.IsNullOrWhiteSpace term then
                return! badRequest ctx "Empty term is not allowed"

            let def =
                if body.TryGetProperty("definition", &Unchecked.defaultof<_>) then
                    body.GetProperty("definition").GetString()
                else ""

            let tags =
                if body.TryGetProperty("tags", &Unchecked.defaultof<_>) then
                    body.GetProperty("tags").EnumerateArray()
                        |> Seq.map (fun x -> x.GetString())
                        |> Seq.toList
                else []

            match Crud.addWord term def tags with
            | Ok _ ->
                ctx.Response.StatusCode <- 201
                do! ctx.Response.WriteAsJsonAsync(box {| message = "created" |})
            | Error e -> return! badRequest ctx e
        with _ ->
            return! badRequest ctx "Invalid JSON"
    } :> Task)) |> ignore

app.MapPut("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    task {
        try
            let! body = ctx.Request.ReadFromJsonAsync<JsonElement>()
            let def =
                if body.TryGetProperty("definition", &Unchecked.defaultof<_>) then
                    body.GetProperty("definition").GetString()
                else ""

            let tags =
                if body.TryGetProperty("tags", &Unchecked.defaultof<_>) then
                    body.GetProperty("tags").EnumerateArray()
                    |> Seq.map (fun x -> x.GetString())
                    |> Seq.toList
                else []

            match Crud.updateWord term def tags with
            | Ok _ -> do! ctx.Response.WriteAsJsonAsync(box {| message="updated" |})
            | Error _ -> return! notFound ctx "Word not found"
        with _ ->
            return! badRequest ctx "Invalid JSON"
    } :> Task)) |> ignore

app.MapDelete("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    match Crud.deleteWord term with
    | Ok _ -> ctx.Response.WriteAsJsonAsync(box {| message = "deleted" |}) :> Task
    | Error _ -> notFound ctx "Word not found")) |> ignore

app.MapGet("/search", Func<HttpContext,Task>(fun ctx ->
    task {
        let q = ctx.Request.Query.["q"].ToString()
        let res = Search.search q
        do! ctx.Response.WriteAsJsonAsync(res)
    } :> Task)) |> ignore

app.MapGet("/", Func<HttpContext,Task>(fun ctx ->
    ctx.Response.Redirect("/index.html")
    Task.CompletedTask)) |> ignore

printfn "Server running at http://localhost:5000"
app.Urls.Add("http://localhost:5000")
app.Run()
