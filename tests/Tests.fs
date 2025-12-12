module Tests

open Expecto
open DictionaryApp

let sample =
    { Term = "hello"
      Definition = "a greeting"
      Tags = ["common"; "basic"] }

[<Tests>]
let crudTests =
  testList "CRUD Tests" [

      testCase "Add word" <| fun _ ->
          // ensure clean
          let _ = Storage.delete sample.Term
          let result = Crud.addWord sample.Term sample.Definition sample.Tags
          Expect.isOk result "Should add word successfully"

      testCase "Get word" <| fun _ ->
          let result = Crud.getWord sample.Term
          match result with
          | Ok w -> Expect.equal w.Term sample.Term "Term should match"
          | Error _ -> failwith "Word not found"

      testCase "Update word" <| fun _ ->
          let newDef = "updated greeting"
          let result = Crud.updateWord sample.Term newDef ["updated"]
          Expect.isOk result "Should update successfully"

      testCase "Delete word" <| fun _ ->
          let result = Crud.deleteWord sample.Term
          Expect.isOk result "Should delete successfully"
  ]

[<EntryPoint>]
let main args =
    runTestsInAssembly defaultConfig args
