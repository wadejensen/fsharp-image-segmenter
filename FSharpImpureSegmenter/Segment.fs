module Segment

open System
open System.Collections.Immutable
open Util

type Coord = { x: int; y: int } 

type Pixel = { x: int; y: int; subPixels: byte array }

// Project a Pixel down to just a Coord
let getCoordFromPixel (pixel: Pixel) : Coord = 
    {x = pixel.x; y = pixel.x}

// A Segment is just a collection of pixels
type Segment = Pixel array

// Default Segment constructor
let Segment (pixel: Pixel) : Segment = [| pixel |] 

// Alternative Segment constructor
let createSegment (segment1: Segment) (segment2: Segment) : Segment = 
    Array.concat [segment1; segment2]
    

// Calculates the statistical standard deviation of a vector of numbers
let standardDeviation (elems: float array) : float = 
    elems
    |> Array.map (fun x -> x - (Array.average elems) )
    |> Array.map square
    |> Array.sum
    |> divideBy (float(Array.length elems))
    |> sqrt


// return a vector of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment) = 
    let numPixels = segment.Length
    let numBands = (Array.last segment).subPixels.Length
    
    // get all subpixels bytes, grouped by subpixel, expected structure:
    // [ [r0; r1; r2; r3; ... rN]; [g0; g1; g2; g3; ... gN]; [b0; b1; b2; b3; ... bN] ]
    let subPixelBands: byte array array = init2d numPixels numBands (fun (x: int) (y: int) -> segment.[y].subPixels.[x])

    let stdDevBands =
        subPixelBands
        |> Array.map (Array.map (fun x -> float x) ) // convert byte to float
        |> Array.map standardDeviation
    
    stdDevBands


// Calculate the contribution of a segment to the merge cost of two segments
let calculateSegmentWeight (segment: Segment) : float =
    let subPixelStdDevs: float array = stddev(segment)
    
    subPixelStdDevs
    |> Array.sum
    |> multiplyBy (float(segment.Length))


// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments,
// weighted by their respective sizes and summed over all colour bands
let mergeCost (segment1: Segment) (segment2: Segment) : float =
    let combinedWeight: float  = calculateSegmentWeight(createSegment segment1 segment2)
    let segment1Weight: float  = calculateSegmentWeight segment1
    let segment2Weight: float  = calculateSegmentWeight segment2

    combinedWeight - segment1Weight - segment2Weight;
