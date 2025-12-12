module DictionaryApp.Storage

open System
open System.IO
open System.Text.Json
open System.Collections.Concurrent
open DictionaryApp

let private dataFile = Path.Combine(AppContext.BaseDirectory, "data.json")
let private db = ConcurrentDictionary<string, Word>(StringComparer.OrdinalIgnoreCase)

let load () =
    try
        if File.Exists(dataFile) then
            let txt = File.ReadAllText(dataFile)
            if not (String.IsNullOrWhiteSpace txt) then
                let arr = JsonSerializer.Deserialize<Word[]>(txt)
                if not (isNull arr) then
                    arr |> Array.iter (fun w -> db.[w.Term] <- w)
    with _ -> ()

let save () =
    try
        let arr = db.Values |> Seq.toArray
        let opts = JsonSerializerOptions(WriteIndented = true)
        let txt = JsonSerializer.Serialize(arr, opts)
        File.WriteAllText(dataFile, txt)
        true
    with _ -> false

let listAll () : Word[] = db.Values |> Seq.toArray

let tryGet (term:string) : Word option =
    match db.TryGetValue(term) with
    | true, v -> Some v
    | _ -> None

let add (w: Word) : bool =
    if db.ContainsKey(w.Term) then false
    else
        db.[w.Term] <- w
        save() |> ignore
        true

let update (term:string) (definition: string option) (tags: string list option) : bool =
    match db.TryGetValue(term) with
    | true, existing ->
        let newDef = defaultArg definition existing.Definition
        let newTags = defaultArg tags existing.Tags
        let updated = { existing with Definition = newDef; Tags = newTags }
        db.[existing.Term] <- updated
        save() |> ignore
        true
    | _ -> false

let delete (term:string) : bool =
    match db.TryRemove(term) with
    | true, _ -> save() |> ignore; true
    | _ -> false

do load()
