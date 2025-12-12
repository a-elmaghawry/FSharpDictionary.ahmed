module DictionaryApp.Search

open System
open DictionaryApp
open DictionaryApp.Storage

let search (q:string) : Word[] =
    let ql = (if isNull q then "" else q).ToLowerInvariant().Trim()
    if String.IsNullOrWhiteSpace(ql) then
        listAll()
    else
        listAll()
        |> Array.filter (fun w ->
            (if not (isNull w.Term) then w.Term.ToLowerInvariant().Contains(ql) else false)
            || (if not (isNull w.Definition) then w.Definition.ToLowerInvariant().Contains(ql) else false)
            || (w.Tags |> List.exists (fun (t:string) -> not (isNull t) && t.ToLowerInvariant().Contains(ql))))
