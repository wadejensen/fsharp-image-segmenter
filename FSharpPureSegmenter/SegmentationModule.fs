module SegmentationModule

open UtilModule
open SegmentModule


// Maps segments to their immediate parent segment that they are contained within (if any) 
type Segmentation = Map<Segment, Segment>


// Find the largest/top level segment that the given segment is a part of (based on the current segmentation)
let rec findRoot (segmentation: Segmentation) (segment : Segment) =
    let maybeParent: Segment option = Map.tryFind segment segmentation
    // When the Segmentation does not contain the parent then we know we've found the root
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
    if root = expectedRoot then true
    else false

 
// Ensure a coordinate lies within the bounds of an image given a max coordinate value    
let boundsCheck (max: int) (coord: Coordinate) : bool =
    match coord with (x,y) -> (0 <= x) && (x <= max) && (0 <= y) && (y <= max)


// Return the vertically and horizontally adjacent pixel coordinates (no diagonals)
let getAdjacentCoords (N: int) (coord:Coordinate) : Coordinate list =
    let north = match coord with (x,y) -> (x, y-1)
    let east = match coord with (x,y) -> (x+1, y)
    let south = match coord with (x,y) -> (x, y+1)
    let west = match coord with (x,y) -> (x-1, y)
  
    // only return those coords with valid indices
    let isCoordWithinImage = boundsCheck (N*N) //: Coordinate -> bool = fun coord -> boundsCheck(N*N, coord)
    List.filter isCoordWithinImage [north; east; south; west]


// Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
// Note: this is a higher order function which given a pixelMap function and a size N, 
// returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
let createNeighboursFunction (pixelMap:Coordinate->Segment) (N:int) : (Segmentation -> Segment -> Set<Segment>) =
    (fun (segmentation: Segmentation) (segment: Segment) -> 
        // get all bottom level child segments
        let leafSegments = foldSegment(segment)
        // safely convert to pixels
        let pixels = List.choose getPixelCoordFromSegment leafSegments
        // get list of 4 adjacent pixels for each pixel and apply distinct operator via Set constructor
        let possibleAdjacentCoords = List.collect (getAdjacentCoords N) pixels |> Set.ofList
        let possibleAdjacentSegments: Segment Set = Set.map pixelMap possibleAdjacentCoords
        
        // Define a function which takes two Segment arguments, and determines 
        // whether the second argument has the first argument as its root Segment
        let hasRoot: (Segment -> Segment -> bool) = segmentIsRootOf segmentation
        
        let adjacentSegments = setFilterNot (hasRoot segment) possibleAdjacentSegments
        Set.map (findRoot segmentation) adjacentSegments
     )

 // Find the neighbour(s) of the given segment that has the (equal) best merge cost
 // (exclude neighbours if their merge cost is greater than the threshold)
let createBestNeighbourFunction (neighbours:Segmentation->Segment->Set<Segment>) (threshold:float) : (Segmentation->Segment->Set<Segment>) =
    (fun (segmentation: Segmentation) (segment: Segment) ->
        let segmentNeighbours: Segment Set = neighbours segmentation segment
        
        // create a tuple of neighbours and their merge costs
        let segmentNeighboursAndMergeCosts: (Segment * float) Set = 
            segmentNeighbours
            |> Set.map (fun (s: Segment) -> (s, (mergeCost s segment)) )
        
        let bestCost: float = setMinBy snd segmentNeighboursAndMergeCosts |> snd

        // filter the tuple to get Segments only when the mergeCost = bestCost
        segmentNeighboursAndMergeCosts
        |> Set.filter (snd >> ((=) bestCost))  
        |> Set.map fst
    )


// Try to find a neighbouring segmentB such that:
//     1) segmentB is one of the best neighbours of segment A, and 
//     2) segmentA is one of the best neighbours of segment B
// if such a mutally optimal neighbour exists then merge them,
// otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
let createTryGrowOneSegmentFunction (bestNeighbours:Segmentation->Segment->Set<Segment>) (pixelMap:Coordinate->Segment) : (Segmentation->Coordinate->Segmentation) =
    raise (System.NotImplementedException())
    // Fixme: add implementation here


// Try to grow the segments corresponding to every pixel on the image in turn 
// (considering pixel coordinates in special dither order)
let createTryGrowAllCoordinatesFunction (tryGrowPixel:Segmentation->Coordinate->Segmentation) (N:int) : (Segmentation->Segmentation) =
    raise (System.NotImplementedException())
    // Fixme: add implementation here


// Keep growing segments as above until no further merging is possible
let createGrowUntilNoChangeFunction (tryGrowAllCoordinates:Segmentation->Segmentation) : (Segmentation->Segmentation) =
    raise (System.NotImplementedException())
    // Fixme: add implementation here


// Segment the given image based on the given merge cost threshold, but only for the top left corner of the image of size (2^N x 2^N)
let segment (image:TiffModule.Image) (N: int) (threshold:float)  : (Coordinate -> Segment) =
    raise (System.NotImplementedException())
    // Fixme: use the functions above to help implement this function