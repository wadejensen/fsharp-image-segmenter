module Segment

open FSharpx.Collections
open Vector
open Util

type Coord = { x: int; y: int } 

type Pixel = { x: int; y: int; subPixels: byte vector }


let test : Pixel Set = 
    let p1: Pixel = {x=1; y=1}
    let p2: Pixel = {x=2; y=2}
    
    Set [p1; p2]

// A Segment is just a collection of pixels
type Segment = Pixel vector

// Default Segment constructor
let Segment (pixel: Pixel) : Segment = Vector.ofSeq [pixel]

// Alternative Segment constructor
let createSegment (segment1: Segment) (segment2: Segment) : Segment = Vector.append segment1 segment2
    

// Transform a vector of vectors with dimensions n x m into a vector of vectors with dimensions m x n
let transpose (matrix: 'a vector vector, rows, cols) : 'a vector vector  =
    // initialise a new 2d vector by reversing row - column access order
    init2d rows cols (fun (x: int) (y: int) -> Vector.get y x matrix)
    

// Calculates the statistical standard deviation of a vector of numbers
let standardDeviation (elems: float vector) : float = 
    elems
    |> Vector.map (fun x -> x - (Vector.average elems) )
    |> Vector.map square
    |> Vector.sum
    |> divideBy (float(Vector.length elems))
    |> sqrt


// return a vector of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment) = 
    let numPixels = Vector.length segment
    let numBands = Vector.length (Vector.last segment).subPixels
    
    // Get all subpixel bytes of the segment, indexed by pixel
    // expected structure: [ [r0; g0; b0]; [r1; g1; b1]; [r2; g2; b2]; [r3; g3; b3]; ... [rN; gN; bN] ]
    let pixelsColours = Vector.map (fun (pixel: Pixel) -> pixel.subPixels) segment
    
    // get all subpixels bytes, grouped by subpixel, expected structure:
    // [ [r0; r1; r2; r3; ... rN]; [g0; g1; g2; g3; ... gN]; [b0; b1; b2; b3; ... bN] ]
    let subPixelBands: byte vector vector = transpose(pixelsColours, numPixels, numBands)
    
    let stdDevBands =
        subPixelBands
        |> Vector.map (Vector.map (fun x -> float x) ) // convert byte to float
        |> Vector.map standardDeviation
    
    stdDevBands


// Calculate the contribution of a segment to the merge cost of two segments
let calculateSegmentWeight (segment: Segment) : float =
    let subPixelStdDevs: float vector = stddev(segment)
    
    subPixelStdDevs
    |> Vector.sum
    |> multiplyBy (float(Vector.length segment))


// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments,
// weighted by their respective sizes and summed over all colour bands
let mergeCost (segment1: Segment) (segment2: Segment) : float =
    let combinedWeight: float  = calculateSegmentWeight(createSegment segment1 segment2)
    let segment1Weight: float  = calculateSegmentWeight segment1
    let segment2Weight: float  = calculateSegmentWeight segment2

    combinedWeight - segment1Weight - segment2Weight;
