using System;

namespace CSharpSegmenter
{
    public class Segmentor
    {
        // a 2D array of pointers to Segment objects, our OO representation of a Segmentation.
        // Used to lookup the Segment which owns a pixel based on its x,y coordinates.
        private Segment[,] segmentation;
        private int size;
        private double threshold;

        public Segmentor(TiffModule.Image image, int N, double threshold)
        {
            this.size = (int) Math.Pow(2.0, (double) N);
            
            // turn the Tiff Image into a segmentation
            // TODO
            this.segmentation = new Segment[size, size];            
        }
        
        public Segment[,] SegmentImage()
        {
            
            //TODO
            // Loop over every segment in dither order and attempt to grow it until no change in the segmentation
                // attempt to grow a segment
                    // get the neighbour segments
                    // if their are no neighbours we are done and we can bail out
                    // get the best neighbour segments
                    // if there are no best neighbours then bail out and indicate that nothing has changed
                    // get the mutually optimal neighbour segments
                    // if none do "gradient descent" merging
                    // else merge the optimal neighbours and exit, indicate that something has changed

            return this.segmentation;
        }       
    }
}

/*
 
    -- COPIED FROM FSharpPureSegmenter.SegmentationModule --
    
    -- GOALS OF OO RE-WRITE -> perf, readbility, maintainability

    module SegmentationModule
    
    open UtilModule
    open SegmentModule
    
    // Maps segments to their immediate parent segment that they are contained within (if any) 
    type Segmentation = Map<Segment, Segment>
    
    -- NOT NECESSARY -- JUST KEEP THE SEGMENTATION 2D ARRAY UP TO DATE AT ALL TIMES
    
    // Find the largest/top level segment that the given segment is a part of (based on the current segmentation)
    let rec findRoot (segmentation: Segmentation) (segment : Segment) =  
    
    -- NOT NECESSARY -- THE SEGMENTATION 2D ARRAY CAN BE ADDRESSED WITH COORDINATES AND RETURN THE HIGHEST SEGMENT
    
    // Initially, every pixel/coordinate in the image is a separate Segment
    // Note: this is a higher order function which given an image, 
    // returns a function which maps each coordinate to its corresponding (initial) Segment (of kind Pixel)
    let createPixelMap (image:TiffModule.Image) : (Coordinate -> Segment) =
    
    -- NOT NECESSARY -- WE WILL ALWAYS BE DEALING WITH ROOT SEGMENTS
    
    // Determine whether a given segment is the highest level parent of another segment using a Segmentation map
    let segmentIsRootOf (segmentation: Segmentation) (expectedRoot: Segment) (child: Segment) : bool = 
     
    -- DEFINITELY NEEDED -- 
     
    // Ensure a coordinate lies within the bounds of an image given a max coordinate value    
    let isCoordWithinImage (max : int) (coord : Coordinate) : bool =
    
    -- DEFINITELY NEEDED --
    
    // Return the vertically and horizontally adjacent pixel coordinates (no diagonals)
    let getAdjacentCoords (maxPixelCoord : int) (coord : Coordinate) : Coordinate list =
    
    
    
    -- HOF IS TOTALLY UNNECESSARY HERE, LETS TRY TOP DOWN OO DECOMPOSITION, RATHER THAN BOTTOM UP FUNCTIONAL --
    
    // Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
    // Note: this is a higher order function which given a pixelMap function and a size N, 
    // returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
    let createNeighboursFunction (pixelMap:Coordinate->Segment) (N:int) : (Segmentation -> Segment -> Set<Segment>) =
    
    -- HOF IS TOTALLY UNNECESSARY HERE, LETS TRY TOP DOWN OO DECOMPOSITION, RATHER THAN BOTTOM UP FUNCTIONAL --
    
     // Find the neighbour(s) of the given segment that has the (equal) best merge cost
     // (exclude neighbours if their merge cost is greater than the threshold)
    let createBestNeighbourFunction (neighbours:Segmentation->Segment->Set<Segment>) (threshold:float) : (Segmentation->Segment->Set<Segment>) =
                               
    // Determine whether segmentA is one of the best neighbours of segmentB
    let bestNeighbourChecker (bestNeighbours : Segmentation->Segment->Set<Segment>) (segmentation : Segmentation) : Segment->Segment->bool =
        
        
    // Merge two segments by creating their parent and adding them to the segmentation
    let mergeSegments (segmentation : Segmentation) (segmentA: Segment) (segmentB: Segment) : Segmentation =
    
    
    // Try to find a neighbouring segmentB such that:
    //     1) segmentB is one of the best neighbours of segment A, and 
    //     2) segmentA is one of the best neighbours of segment B
    // if such a mutally optimal neighbour exists then merge them,
    // otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
    let createTryGrowOneSegmentFunction (bestNeighbours:Segmentation->Segment->Set<Segment>) (pixelMap:Coordinate->Segment) : (Segmentation->Coordinate->Segmentation) =
    
             
    // Try to grow the segments corresponding to every pixel on the image in turn 
    // (considering pixel coordinates in special dither order)
    let createTryGrowAllCoordinatesFunction (tryGrowPixel:Segmentation->Coordinate->Segmentation) (N:int) : (Segmentation->Segmentation) =
    
    
    
    // Keep growing segments as above until no further merging is possible
    let createGrowUntilNoChangeFunction (tryGrowAllCoordinates:Segmentation->Segmentation) : (Segmentation->Segmentation) =
    
    
    
    // Segment the given image based on the given merge cost threshold, but only for the top left corner of the image of size (2^N x 2^N)
    let segment (image:TiffModule.Image) (N: int) (threshold:float)  : (Coordinate -> Segment) =
*/
        

        
    
