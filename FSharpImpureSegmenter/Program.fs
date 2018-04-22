module Program

open FSharpx.Collections
open FSharpx.Collections.PersistentVector

[<EntryPoint>]
let main argv =
//    let x = 5
//    printfn "%A" x

    let twoDeeArr: PersistentVector<PersistentVector<int>> = ofSeq [ ofSeq [1;2;3]; ofSeq [4;5;6]; ofSeq [7;8;9]]
    
    for elem in twoDeeArr do
        for e in elem do
            printfn "%A" e
    
    //printfn "%A" "" |> ignore
    
    
    let myArrayofArrays = PersistentVector.init 3 (fun x -> PersistentVector.init 3 (fun y -> PersistentVector.nthNth y x twoDeeArr))
    
    let xs = [1 .. 3]
    let ys = [1 .. 3]

    for x in xs do
        for y in ys do
            let value = myArrayofArrays.[x,y]
            printfn "%A" value
    
    for elem in myArrayofArrays do
        for e in elem do
            printfn "%A" e    
        
    //let v: int IPersistentVector = [1,2,3,4]
    
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    0 // return an integer exit code
