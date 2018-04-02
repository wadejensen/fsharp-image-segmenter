module SegmentModule
open System
open System.Security.Cryptography
open UtilModule

type Coordinate = (int * int) // x, y coordinate of a pixel
type Colour = byte list       // one entry for each colour band, typically: [red, green and blue]

type Segment =
    | Pixel of Coordinate * Colour
    | Parent of Segment * Segment

// determine whether a Segment is a Pixel or a Parent
let isPixel (segment: Segment): bool =
    match segment with
    | Pixel (_, _) -> true
    | Parent (_, _) ->  false

// Destructure a Segment to get pixel colour
let getPixelColourFromSegment (segment: Segment): Colour option =
    match segment with
    | Pixel (_, colour) -> Some colour
    | _ ->  None

let getPixelCoordFromSegment (segment: Segment) : Coordinate option =
    match segment with
    | Pixel (coord, _) -> Some coord
    | _ ->  None

// Destructures a Segment into a list of its immediate children 
// Used as the collect / flatmap functions in `descendTree` and `descendTreeLayer` respectively,
// written to be used by the breath first fold method defined in this medule.
let getChildSegments (segment: Segment) : Segment list =
    match segment with
        | Pixel (_, _) as pixel -> [pixel]
        | Parent (segment1, segment2) -> [segment1; segment2]

// Collect / flatmap, all of the child pixels of a list of Segments
let collectChildSegments (segments: Segment list) : Segment list =
    List.collect getChildSegments segments

// A function to perform breadth first traversal of a recursive data structure and collect all leaf elements into a list
// @param descendTreeLayer A function which takes all elements of one layer of a tree structure and returns 
//                         all elements of the layer below
// @param isLeaf A function which determines whether a node in the tree is a leaf ie. does it have children?
// @param state Accumulator which holds leaf nodes already reached
// @param layer Initial layer of the tree. If providing a single root, simply wrap inside a list
let rec breadthFirstFold (descendTreeLayer: 'a list -> 'a list, isLeaf: 'a -> bool, state: 'a list, layer: 'a list) : 'a list =
    let children: 'a list = descendTreeLayer(layer)
    let leaves, nextLayer = List.partition isLeaf children

    if ((List.length nextLayer) = 0) then List.concat [leaves; state]
    else breadthFirstFold(descendTreeLayer, isLeaf, List.concat [leaves; state], nextLayer)

// Recursively traverse a Segment to collect all of its base children Segments into a list
let foldSegment (segment: Segment) : Segment list =
    breadthFirstFold(collectChildSegments, isPixel, [], [segment])

// Get all of the pixels contained within a segment
let getAllPixelsFromSegment(segment: Segment) : byte list list =
    let leafSegments = foldSegment(segment)
    List.choose getPixelColourFromSegment leafSegments


// Transform a list of lists with dimensions n x m into a list of lists with dimensions m x n
let transpose (listOfLists: 'a list list) : 'a list list  =
    listOfLists
    |> List.collect List.indexed
    |> List.groupBy fst
    |> List.map (snd >> List.map snd);;


// Calculates the statistical standard deviation of a list of numbers
let standardDeviation (elems: float list): float =
    elems
    |> List.map (fun x -> x - (List.average elems) )
    |> List.map square
    |> List.sum
    |> divideBy (float(List.length elems))
    |> Math.Sqrt


// return a list of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment) : float list =
    // Traverse the nested Segment data structure to extract all pixels at the leaves
    // expected structure: [ [r0; g0; b0]; [r1; g1; b1]; [r2; g2; b2]; [r3; g3; b3]; ... [rN; gN; bN] ]
    let pixels: byte list list = getAllPixelsFromSegment(segment)
    
    // group by subpixels, expected structure:
    // [ [r0; r1; r2; r3; ... rN]; [g0; g1; g2; g3; ... gN]; [b0; b1; b2; b3; ... bN] ]
    let subPixelBands: byte list list = transpose(pixels)

    let stdDevBands =
        subPixelBands
        |> List.map (List.map (fun x -> float x) ) // convert byte to float
        |> List.map standardDeviation

    stdDevBands

// Calculate the contribution of a segment to the merge cost of two segments
let calculateSegmentWeight(segment: Segment) : float =
    let pixels = getAllPixelsFromSegment(segment)
    let subPixelStdDevs = stddev(segment)
    
    subPixelStdDevs
    |> List.sum
    |> multiplyBy (float(List.length pixels))

// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments,
// weighted by their respective sizes and summed over all colour bands
let mergeCost segment1 segment2 : float =
    let combinedWeight = calculateSegmentWeight (Parent(segment1, segment2)) 
    let segment1Weight = calculateSegmentWeight segment1
    let segment2Weight = calculateSegmentWeight segment2
    
    combinedWeight - segment1Weight - segment2Weight
