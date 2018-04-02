module UtilModule

// Effectively Set.filter, but flips the predicate to become Set.filterNot
// which is not present in the F# standard libary.
// Similar to what is available in Scala collections.

let setFilterNot (predicate: 'a -> bool) (elems: 'a Set) : 'a Set =
    let flippedPredicate: 'a -> bool = fun a -> not (predicate a)
    Set.filter flippedPredicate elems


let setSafeMin (elems: 'a Set): 'a option =
    let elemsSeq = List.ofSeq elems
    match elemsSeq.Length with
    | 0 -> None
    | _ -> Some (Set.minElement elems)
 
 
let listSafeMinBy (ordering: 'a -> float) (elems: 'a list): 'a option =
    match elems.Length with 
    | 0 -> None
    | _ -> Some(List.minBy ordering elems) 


let setSafeMinBy (ordering: 'a -> float) (elems: 'a Set) : 'a option =
    elems
    |> List.ofSeq
    |> listSafeMinBy ordering

// Math helpers
let square x: float = x * x

// Divide the second argument by the first argument (useful as a pipeline operator)
let divideBy y x : float = x / y

// Multiply the second argument by the first argument (useful as a pipeline operator)
let multiplyBy x y : float = x * y
