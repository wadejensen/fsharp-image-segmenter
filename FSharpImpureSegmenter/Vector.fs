module Vector

open FSharpx.Collections

// An alias to FSharpx.Collections.PersistentVector, a hash array mapped trie (HAMT).
// PersistentVector is a high performance functional data structure which provides
// O(log32(n)) indexed array read and writes, which is close enough to O(1) for most sane 
// practical purposes.
// Writes are done in an immutable way by returning a new trie which reuses the structure 
// of the previous trie, and simply changes the pointers for updated leaves.
// See https://hypirion.com/musings/understanding-persistent-vector-pt-1
type vector<'a when 'a : comparison> = PersistentVector<'a>

let ofSeq(elems: 'a seq): 'a vector = PersistentVector.ofSeq elems

let append = PersistentVector.append

let map = PersistentVector.map

let sum = PersistentVector.fold (+) 0.0

let length = PersistentVector.length

let toSeq = PersistentVector.toSeq

let average(v: 'a PersistentVector) = sum v / float(length v)

let last = PersistentVector.last

let toSet (v: 'a PersistentVector): 'a Set = PersistentVector.toSeq v |> Set.ofSeq

//let contains (elem: 'a) (v: 'a PersistentVector) : bool = 
//    PersistentVector.map

//let distinct (v: 'a PersistentVector): 'a PersistentVector = 
//    PersistentVector.init v.Length (fun i: int -> PersistentVector.nth i 

// alias 2d array access to be `get x y`
let get = PersistentVector.nthNth 

// initialiser for a 2D PersistentVector data structure
let init2d (width: int) (height: int) (f: int -> int -> 'a) : 'a PersistentVector PersistentVector = 
    PersistentVector.init width (fun x -> 
        PersistentVector.init height (fun y -> 
            f x y))
