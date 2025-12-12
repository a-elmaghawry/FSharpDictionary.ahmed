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
    if Directory.Exists(wwwRootCandidate) then
        wwwRootCandidate
    else
        Path.Combine(AppContext.BaseDirectory, "wwwroot")

let provider = PhysicalFileProvider(wwwRootPath)
app.UseStaticFiles(Microsoft.AspNetCore.Builder.StaticFileOptions(FileProvider = provider)) |> ignore

app.MapGet("/words", Func<HttpContext, Task>(fun ctx ->
    ctx.Response.WriteAsJsonAsync(Crud.listAll()) :> Task)) |> ignore

app.MapGet("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    match Crud.getWord term with
    | Ok w -> ctx.Response.WriteAsJsonAsync(w) :> Task
    | Error _ ->
        ctx.Response.StatusCode <- 404
        ctx.Response.WriteAsJsonAsync(box {| error = "not found" |}) :> Task)) |> ignore

app.MapPost("/word", Func<HttpContext, Task>(fun ctx ->
    task {
        let! body = ctx.Request.ReadFromJsonAsync<JsonElement>()
        let mutable je = Unchecked.defaultof<JsonElement>

        let term =
            if body.TryGetProperty("term",&je) then
                let s = je.GetString()
                if isNull s then "" else s
            else ""

        let def =
            if body.TryGetProperty("definition",&je) then
                let s = je.GetString()
                if isNull s then "" else s
            else ""

        let tags =
            if body.TryGetProperty("tags",&je) then
                je.EnumerateArray()
                |> Seq.map (fun j ->
                    let s = j.GetString()
                    if isNull s then "" else s)
                |> Seq.toList
            else []

        match Crud.addWord term def tags with
        | Ok _ ->
            ctx.Response.StatusCode <- 201
            do! ctx.Response.WriteAsJsonAsync(box {| message = "created" |})
        | Error e ->
            ctx.Response.StatusCode <- 409
            do! ctx.Response.WriteAsJsonAsync(box {| error = e |})
    } :> Task)) |> ignore

app.MapPut("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    task {
        let! body = ctx.Request.ReadFromJsonAsync<JsonElement>()
        let mutable je = Unchecked.defaultof<JsonElement>

        let def =
            if body.TryGetProperty("definition",&je) then
                let s = je.GetString()
                if isNull s then "" else s
            else ""

        let tags =
            if body.TryGetProperty("tags",&je) then
                je.EnumerateArray()
                |> Seq.map (fun j ->
                    let s = j.GetString()
                    if isNull s then "" else s)
                |> Seq.toList
            else []

        match Crud.updateWord term def tags with
        | Ok _ -> do! ctx.Response.WriteAsJsonAsync(box {| message = "updated" |})
        | Error _ ->
            ctx.Response.StatusCode <- 404
            do! ctx.Response.WriteAsJsonAsync(box {| error = "not found" |})
    } :> Task)) |> ignore

app.MapDelete("/word/{term}", Func<HttpContext,string,Task>(fun ctx term ->
    match Crud.deleteWord term with
    | Ok _ -> ctx.Response.WriteAsJsonAsync(box {| message = "deleted" |}) :> Task
    | Error _ -> ctx.Response.StatusCode <- 404; ctx.Response.WriteAsJsonAsync(box {| error = "not found" |}) :> Task)) |> ignore

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
