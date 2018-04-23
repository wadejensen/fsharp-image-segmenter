module Util

/// Missing standard library functions

// Flatten a set of sets into a single set which is the union of all
let setFlatten (nestedSet: 'a Set Set) : 'a Set =
    nestedSet 
    |> Set.fold Set.union (Set.empty)


// functional initialiser for a 2D array
let init2d (width: int) (height: int) (f: int -> int -> 'a) : 'a array array = 
    Array.init width (fun x -> 
        Array.init height (fun y -> 
            f x y))

let setSafeMin (elems: 'a Set): 'a option =
    let elemsSeq = List.ofSeq elems
    match elemsSeq.Length with
    | 0 -> None
    | _ -> Some (Set.minElement elems)


/// Math helpers

let square x: float = x * x

// Divide the second argument by the first argument (useful as a pipeline operator)
let divideBy y x : float = x / y

// Multiply the second argument by the first argument (useful as a pipeline operator)
let multiplyBy x y : float = x * y
