module SegmentationModule

open UtilModule
open SegmentModule

// Maps segments to their immediate parent segment that they are contained within (if any) 
type Segmentation = Map<Segment, Segment>

// Find the largest/top level segment that the given segment is a part of (based on the current segmentation)
let rec findRoot (segmentation: Segmentation) (segment : Segment) =
    let maybeParent: Segment option = Map.tryFind segment segmentation
    // Recursely traverse map until the Segmentation does not contain the parent, then we can return the segment as
    // it must be the root
    match maybeParent with
    | Some parent -> findRoot segmentation parent
    | None -> segment
    

// Initially, every pixel/coordinate in the image is a separate Segment
// Note: this is a higher order function which given an image, 
// returns a function which maps each coordinate to its corresponding (initial) Segment (of kind Pixel)
let createPixelMap (image:TiffModule.Image) : (Coordinate -> Segment) =
    fun (coord: Coordinate) -> Pixel(coord, TiffModule.getColourBands image coord)
    

// Determine whether a given segment is the highest level parent of another segment using a Segmentation map
let segmentIsRootOf (segmentation: Segmentation) (expectedRoot: Segment) (child: Segment) : bool = 
    let root = findRoot segmentation child
    root = expectedRoot


// Ensure a coordinate lies within the bounds of an image given a max coordinate value    
let isCoordWithinImage (max : int) (coord : Coordinate) : bool =
    match coord with (x,y) -> (0 <= x) && (x <= max) && (0 <= y) && (y <= max)


// Return the vertically and horizontally adjacent pixel coordinates (no diagonals)
let getAdjacentCoords (maxPixelCoord : int) (coord : Coordinate) : Coordinate list =
    let adjacentsCoords = match coord with (x,y) -> [(x+1, y); (x-1, y); (x, y+1); (x, y-1)]
    List.filter (isCoordWithinImage maxPixelCoord) adjacentsCoords


// Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
// Note: this is a higher order function which given a pixelMap function and a size N, 
// returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
let createNeighboursFunction (pixelMap:Coordinate->Segment) (N:int) : (Segmentation -> Segment -> Set<Segment>) =
    (fun (segmentation : Segmentation) (segment: Segment) -> 
        // get all bottom level child segments
        let leafSegments = foldSegment(segment)
        // safely convert to pixels
        let coords = List.choose getPixelCoordFromSegment leafSegments
        
        // get list of 4 adjacent pixels for each pixel and apply distinct operator via Set constructor
        let maxPixelCoord = int(2.0 ** float(N)-1.0)
        let possibleAdjacentCoords = List.collect (getAdjacentCoords maxPixelCoord) coords |> Set.ofList
        let possibleAdjacentSegments: Segment Set = Set.map pixelMap possibleAdjacentCoords
        
        // Takes two Segments and determines whether the second Segment has the first as its root Segment
        let hasRoot : (Segment -> Segment -> bool) = segmentIsRootOf segmentation
        
        let adjacentSegments = setFilterNot (hasRoot segment) possibleAdjacentSegments
        Set.map (findRoot segmentation) adjacentSegments
     )

 // Find the neighbour(s) of the given segment that has the (equal) best merge cost
 // (exclude neighbours if their merge cost is greater than the threshold)
let createBestNeighbourFunction (neighbours:Segmentation->Segment->Set<Segment>) (threshold:float) : (Segmentation->Segment->Set<Segment>) =
    (fun (segmentation : Segmentation) (segment : Segment) ->
        let segmentNeighbours: Segment Set = neighbours segmentation segment
        
        // create a tuple of neighbours and their merge costs
        let segmentNeighboursAndMergeCosts: (Segment * float) Set = 
            segmentNeighbours
            |> Set.map (fun (neighbour: Segment) -> (neighbour, (mergeCost neighbour segment)) )
        
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
    )                           

// Determine whether segmentA is one of the best neighbours of segmentB
let bestNeighbourChecker (bestNeighbours : Segmentation->Segment->Set<Segment>) (segmentation : Segmentation) : Segment-> Segment->bool =
    fun (segmentA: Segment) (segmentB: Segment) -> 
        let bestNeighboursOfSegmentB = bestNeighbours segmentation segmentB
        Set.contains segmentA bestNeighboursOfSegmentB   
    
// Merge two segments by creating their parent and adding them to the segmentation
let mergeSegments (segmentation : Segmentation) (segmentA: Segment) (segmentB: Segment) : Segmentation =
    let parent = Parent(segmentA, segmentB)
    
    segmentation
    |> Map.add segmentA parent
    |> Map.add segmentB parent


// Try to find a neighbouring segmentB such that:
//     1) segmentB is one of the best neighbours of segment A, and 
//     2) segmentA is one of the best neighbours of segment B
// if such a mutally optimal neighbour exists then merge them,
// otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
let createTryGrowOneSegmentFunction (bestNeighbours:Segmentation->Segment->Set<Segment>) (pixelMap:Coordinate->Segment) : (Segmentation->Coordinate->Segmentation) =
    fun (segmentation : Segmentation) (coord : Coordinate) -> 
        let initialSegment = findRoot segmentation (pixelMap coord)
    
        let rec tryGrowOneSegment (segmentation: Segmentation) (segmentA: Segment) : Segmentation =              
            // The best neighbours of SegmentA
            let segmentABestNeighbours: Segment Set = bestNeighbours segmentation segmentA

            if segmentABestNeighbours.IsEmpty then 
                segmentation
            else
                // A function which returns true when SegmentA is one of a given Segment's best neighbours
                let checkBestNeighboursIncudesSegmentA = bestNeighbourChecker bestNeighbours segmentation segmentA
                
                // the mutually optimal neighbours of SegmentA
                let mutuallyOptimalNeighbours = Set.filter checkBestNeighboursIncudesSegmentA segmentABestNeighbours
                
                // if there is one mutually optimal neighbour, merge segments A and B
                // if there are none choose a best neighbour and try to grow it instead
                // if theres is more then one, pick one and try to and grow it instead
                match List.ofSeq mutuallyOptimalNeighbours with
                | [segmentB] -> mergeSegments segmentation segmentA segmentB
                | [] -> tryGrowOneSegment segmentation (List.ofSeq segmentABestNeighbours).Head
                | segment :: _ -> tryGrowOneSegment segmentation segment
                     
        tryGrowOneSegment segmentation initialSegment

// Try to grow the segments corresponding to every pixel on the image in turn 
// (considering pixel coordinates in special dither order)
let createTryGrowAllCoordinatesFunction (tryGrowPixel:Segmentation->Coordinate->Segmentation) (N:int) : (Segmentation->Segmentation) =
    fun (segmentation : Segmentation) ->
        // Get all the coordinates within the image in dither order
        let coordsInDitherOrder: (int * int) seq = DitherModule.coordinates N
        // Perform a single pass over the image and attempt to merge segments
        Seq.fold tryGrowPixel segmentation coordsInDitherOrder


// Keep growing segments as above until no further merging is possible
let createGrowUntilNoChangeFunction (tryGrowAllCoordinates:Segmentation->Segmentation) : (Segmentation->Segmentation) =
    let rec growUntilFullySegmented (segmentation : Segmentation) : Segmentation =
        // Perform one iteration of merging
        let newSegmentation = tryGrowAllCoordinates segmentation
        
        // if the result is the same as the previous segmentation then we have fully segmented
        if newSegmentation = segmentation then segmentation
        // otherwise continue merging
        else growUntilFullySegmented newSegmentation
    
    growUntilFullySegmented


// Segment the given image based on the given merge cost threshold, but only for the top left corner of the image of size (2^N x 2^N)
let segment (image:TiffModule.Image) (N: int) (threshold:float)  : (Coordinate -> Segment) =
    // Build the segmenter application logic by functional dependency injection
    let pixelMap: Coordinate -> Segment = createPixelMap image
    
    let neighbourFinder = createNeighboursFunction pixelMap N
    let bestNeighbourFinder = createBestNeighbourFunction neighbourFinder threshold
    let growSinglePixel = createTryGrowOneSegmentFunction bestNeighbourFinder pixelMap
    let tryGrowAllSegments = createTryGrowAllCoordinatesFunction growSinglePixel N
    let segmenter = createGrowUntilNoChangeFunction tryGrowAllSegments
    
    let emptySegmentation = Map.empty<Segment, Segment>
    let segmentation = segmenter emptySegmentation
    
    // return the root segment of the result segmentation which corresponds to the input pixel
    fun (coord: Coordinate) ->
        findRoot segmentation (pixelMap coord)
        
