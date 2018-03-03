module SegmentModule
open System
open System.Security.Cryptography

type Coordinate = (int * int) // x, y coordinate of a pixel
type Colour = byte list       // one entry for each colour band, typically: [red, green and blue]

type Segment =
    | Pixel of Coordinate * Colour
    | Parent of Segment * Segment

// determine with a Segment is a Pixel or a Parent
let isPixel (segment: Segment): bool =
    match segment with
    | Pixel (_, _) -> true
    | Parent (_, _) ->  false

let getPixelColourFromSegment (segment: Segment): Colour option =
    match segment with
    | Pixel (_, colour) -> Some colour
    | _ ->  None

// HOF helpers

// Catamorphisms

/// A function to perform breadth first traversal of a recursive data structure and collect all leaf elements into a list
let rec breadthFirstFold (descendTreeLayer: 'a list -> 'a list, isLeaf: 'a -> bool, state: 'a list, layer: 'a list): 'a list =
    let children: 'a list = descendTreeLayer(layer)
    let leaves, nextLayer = List.partition isLeaf children

    if ((List.length nextLayer) = 0) then List.concat [leaves; state]
    else breadthFirstFold(descendTreeLayer, isLeaf, List.concat [leaves; state], nextLayer)

/// Collect / Flatmap functions, the "descendTree" and "descendTreeLayer" respectively, to be used by DF and BF fold
let getChildSegments (segment: Segment): Segment list =
    match segment with
        | Pixel (_, _) as pixel -> [pixel]
        | Parent (segment1, segment2) -> [segment1; segment2]

let getLayerChildSegments (segments: Segment list): Segment list =
    List.collect getChildSegments segments

// Math helpers

let square x = x * x

// Divide the second argument by the first argument
let divideBy y x: float = x / y

// Calculates the statistical standard deviation of a list of numbers
let standardDeviation (elems: float list): float =
    elems
    |> List.map (fun x -> x - (List.average elems) )
    |> List.map square
    |> List.sum
    |> divideBy (float(List.length elems))
    |> Math.Sqrt
    
// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments,
// weighted by their respective sizes and summed over all colour bands
let mergeCost segment1 segment2 : float = 
    raise (System.NotImplementedException())
    // Fixme: add implementation here

let getIndexOfSubPixel (p: int * byte): int =
    match p with (i,_) -> i

// Transform a list of lists with dimensions n x m into a list of lists with dimensions m x n
let transpose (listOfLists: 'a list list): 'a list list  =
    listOfLists
    |> List.collect List.indexed
    |> List.groupBy fst
    |> List.map (snd >> List.map snd);;

// return a list of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment): float list =
    // Traverse the nested Segment data structure to extract all pixels at the leaves
    let leafSegments: Segment list = breadthFirstFold(getLayerChildSegments, isPixel, [], [segment])

    // get pixels as [ [r0; g0; b0]; [r1; g1; b1]; [r2; g2; b2]; [r3; g3; b3]; ... [rN; gN; bN] ]
    let coloursByPixel: byte list list =  List.choose getPixelColourFromSegment leafSegments

    // get pixels as [ [r0; r1; r2; r3; ... rN]; [g0; g1; g2; g3; ... gN]; [b0; b1; b2; b3; ... bN] ]
    let subPixelBands: byte list list = transpose(coloursByPixel)

    let stdDevBands =
        subPixelBands
        |> List.map (List.map (fun x -> float x) ) // convert byte to float
        |> List.map standardDeviation

    stdDevBands
