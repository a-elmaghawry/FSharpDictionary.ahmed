module DictionaryApp.Crud

open DictionaryApp
open DictionaryApp.Storage

let addWord term def tags =
    let w = { Term = term; Definition = def; Tags = tags }
    if add w then Ok "created" else Error "exists"

let updateWord term def tags =
    if update term (Some def) (Some tags) then Ok "updated" else Error "not found"

let deleteWord term =
    if delete term then Ok "deleted" else Error "not found"

let getWord term =
    match tryGet term with
    | Some w -> Ok w
    | None -> Error "not found"

let listAll () = listAll()
