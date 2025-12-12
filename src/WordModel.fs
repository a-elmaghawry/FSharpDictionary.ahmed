namespace DictionaryApp

open System.Text.Json.Serialization

type Word =
    { [<JsonPropertyName("term")>]
      Term: string
      [<JsonPropertyName("definition")>]
      Definition: string
      [<JsonPropertyName("tags")>]
      Tags: string list }
