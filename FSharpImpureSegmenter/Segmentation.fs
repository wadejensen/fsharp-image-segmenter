module Segmentation

open DitherModule
open Util
open Segment
open Tiff


type Segmentation = Map<Coord, Segment>

// Updates a Segmentation to reflect when a new Segment is merged
// Uses a fold to ensure tail recursion and not cause a stack overflow
let updateSegmentation (segmentation: Segmentation) (newSegment: Segment) : Segmentation =
    
    // update a single pixel of a Segmentation and return the new Segmentation
    let updateCoordOfSegmentation (segmentation: Segmentation) (pixel: Pixel) : Segmentation = 
        Map.add (getCoordFromPixel pixel) newSegment segmentation
    
    Array.fold updateCoordOfSegmentation segmentation newSegment


// Ensure a pixel lies within the bounds of an image given the size of a segmentation
let isWithinSegmentation (size: int) (coord: Coord) : bool = 
    (0 <= coord.x) && (coord.x < size) && (0 <= coord.y) && (coord.y < size)


// Return the vertically and horizontally adjacent pixel coordinates (no diagonals)
let getAdjacentCoords (maxPixelCoord : int) (coord : Coord) : Coord Set =
    let x = coord.x
    let y = coord.y
    
    let potentialNeighbourCoords: Coord Set = 
        Set.ofList [ 
              {x = x+1; y = y  }; 
              {x = x-1; y = y  }; 
              {x = x;   y = y+1};
              {x = x;   y = y-1}]
    
    Set.filter (isWithinSegmentation maxPixelCoord) potentialNeighbourCoords
   

let findNeighbours (segmentation: Segmentation) (size: int) (segment: Segment) : Segment Set =  
    let coords: Coord Set = 
        Array.map getCoordFromPixel segment 
        |> Set.ofArray

    let adjacentCoords: Coord Set = 
        Set.map (getAdjacentCoords size) coords
        |> setFlatten
    
    let adjacentSegments: Segment Set = 
        Set.map (fun (coord: Coord) -> Map.find coord segmentation) adjacentCoords
    
    adjacentSegments


let findBestNeighbours (segmentation: Segmentation) (size: int) (threshold: float) (segment: Segment) : Segment Set = 
    let segmentNeighbours: Segment Set = findNeighbours segmentation size segment
    
     // create a tuple of neighbours and their merge costs
    let segmentNeighboursAndMergeCosts: (Segment * float) Set = 
        segmentNeighbours
        |> Set.map (fun (neighbour: Segment) -> (neighbour, (mergeCost neighbour segment)))
    
    let mergeCosts = segmentNeighboursAndMergeCosts |> Set.map snd
    
    // get min merge cost or None
    let maybeBestCost = setSafeMin mergeCosts
    
    match maybeBestCost with
    // If merge cost exists, return neighbours with equal best cost
    | Some bestCost -> segmentNeighboursAndMergeCosts
                       |> Set.filter (fun tuple -> (snd tuple) = bestCost)
                       |> Set.filter (fun tuple -> (snd tuple) <= threshold)
                       |> Set.map fst
    // otherwise no best neighbours
    | None -> Set.empty


// Try to find a neighbouring segmentB such that:
//     1) segmentB is one of the best neighbours of segment A, and 
//     2) segmentA is one of the best neighbours of segment B
// if such a mutally optimal neighbour exists then merge them,
// otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
// Returns true if a change was made to the segmentation, ie. the
let tryGrowSegment (segmentation: Segmentation) (size: int) (threshold: float) (coord: Coord) : (Segmentation * bool) =
    let initialSegment = Map.find coord segmentation
    
    let rec tryGrowOneSegment (segmentation: Segmentation) (segmentA: Segment) : (Segmentation * bool) =
        // checkers whether segmentB is a best neighbour of segmentA
        let isBestNeighbour (segmentA: Segment) (segmentB: Segment) : bool = 
            let bestNeighboursOfSegmentB = findBestNeighbours segmentation size threshold segmentB
            Set.contains segmentA bestNeighboursOfSegmentB
        
        let segmentABestNeighbours: Segment Set = findBestNeighbours segmentation size threshold segmentA
        
        if segmentABestNeighbours.IsEmpty then (segmentation, false)
        else
            // A function which returns true when SegmentA is one of a given Segment's best neighbours
            let checkBestNeighboursIncudesSegmentA = isBestNeighbour segmentA
            
            // the mutually optimal neighbours of SegmentA
            let mutuallyOptimalNeighbours = Set.filter checkBestNeighboursIncudesSegmentA segmentABestNeighbours
            
            // if there is one mutually optimal neighbour, merge segments A and B
            // if there are none choose a best neighbour and try to grow it instead
            // if theres is more then one, pick one and try to and grow it instead
            if (Set.count mutuallyOptimalNeighbours = 1) then
                // create our merged Segment
                let mergedSegment = createSegment segmentA (Set.minElement mutuallyOptimalNeighbours)
                
                // We need to update the segmentation to reflect the merged Segments
                let newSegmentation = updateSegmentation segmentation mergedSegment
                
                (newSegmentation, true)
            else if (Set.count mutuallyOptimalNeighbours = 0) then
                tryGrowOneSegment segmentation (Set.minElement segmentABestNeighbours)
            else if (Set.count mutuallyOptimalNeighbours > 1) then
                tryGrowOneSegment segmentation (Set.minElement mutuallyOptimalNeighbours)
            else 
                failwith "Impossible number of mutually optimal neighbours encountered." 
    
    tryGrowOneSegment segmentation initialSegment

// Attempt to grow all segments in the segmentation.
// Returns the updated Segmentation as well as a boolean indicating if at least one segment was grown, 
// ie. the segmentation should continue
let tryGrowAllSegments (segmentation: Segmentation) (n: int) (threshold: float) : (Segmentation * bool) =
    let size = int(2.0 ** float(n))
    
    // Get all the coordinates within the image in dither order
    let coordsInDitherOrder: Coord seq = Dither.coordinates n
    
    // attempt to grow each coord in dither order, keeping track of whether any Segment has been grown
    let rec tryGrowAllCoords (segmentation: Segmentation) (hasAlreadyGrown: bool) (coords: Coord seq) : (Segmentation * bool) =         
        if Seq.length coords = 0 then 
            (segmentation, hasAlreadyGrown)
        else
            let (updatedSegmentation, hasGrown) = tryGrowSegment segmentation size threshold (Seq.head coords)
            tryGrowAllCoords updatedSegmentation (hasAlreadyGrown || hasGrown) (Seq.tail coords)
    
    tryGrowAllCoords segmentation false coordsInDitherOrder   

// Grow all segments until Segmentation is complete
let growUntilFullySegmented (segmentation: Segmentation) (n: int) (threshold: float) : Segmentation = 
    let rec growUntilFinished (segmentation : Segmentation) : Segmentation =
        let (updatedSegmentation, hasGrown) = tryGrowAllSegments segmentation n threshold
        
        if hasGrown then
            growUntilFinished updatedSegmentation
        else
            segmentation    
    
    growUntilFinished segmentation
   

let createSegmentationFromImage (image: Image) : Segmentation = 
    let addCoordToSegmentation (segmentation: Segmentation) (coord: Coord) : Segmentation = 
        let subPixels = getColourBands image (coord.x, coord.y)
        let pixel = {x=coord.x; y=coord.y; subPixels=subPixels }  
        
        Map.add coord (Segment pixel) segmentation
        
    // create a flat list of all coords
    let coords: Coord array = 
        (init2d image.width image.height (fun x y -> {x=x; y=y}))
        |> Array.concat  
        
    let segmentation = Array.fold addCoordToSegmentation Map.empty<Coord, Segment> coords
    
    segmentation


let segmentImage (image: Image) (n: int) (threshold: float) : Segmentation =
    let segmentation = createSegmentationFromImage image
    growUntilFullySegmented segmentation n threshold
    