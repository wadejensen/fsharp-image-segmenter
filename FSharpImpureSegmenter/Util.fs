module Util

// Math helpers
let square x: float = x * x

// Divide the second argument by the first argument (useful as a pipeline operator)
let divideBy y x : float = x / y

// Multiply the second argument by the first argument (useful as a pipeline operator)
let multiplyBy x y : float = x * y

// Flatten a set of sets into a single set which is the union of all
let setFlatten (nestedSet: 'a Set Set) : 'a Set =
    nestedSet 
    |> Set.fold Set.union (Set.empty)

