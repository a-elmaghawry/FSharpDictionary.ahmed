namespace DictionaryApp

module ErrorHandling =

    type AppError =
        | NotFound of string
        | AlreadyExists of string
        | InvalidInput of string

    let errorMessage err =
        match err with
        | NotFound msg -> msg
        | AlreadyExists msg -> msg
        | InvalidInput msg -> msg
