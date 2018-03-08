module UtilModule

// Effectively List.filter, but flips the predicate to become List.filterNot 
// similar to what is available in Scala collections
let setFilterNot (predicate: 'a -> bool) (elems: 'a Set) : 'a Set =
    let flippedPredicate: 'a -> bool = fun a -> not (predicate a)
    Set.filter flippedPredicate elems


let setMinBy (ordering: 'a -> float) (elems: 'a Set) : 'a =
    elems
    |> List.ofSeq
    |> List.minBy ordering


// Math helpers
let square x: float = x * x

// Divide the second argument by the first argument (useful as a pipeline operator)
let divideBy y x : float = x / y

// Multiply the second argument by the first argument (useful as a pipeline operator)
let multiplyBy x y : float = x * y
