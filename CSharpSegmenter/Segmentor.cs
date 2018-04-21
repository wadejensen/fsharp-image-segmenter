using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSegmenter
{
    public class Segmentor
    {
        // a 2D array of pointers to Segment objects, our OO representation of a Segmentation.
        // Used to lookup the Segment which owns a pixel based on its x,y coordinates.
        private Segment[,] segmentation;
        private readonly int N;
        private readonly int size;
        private double threshold;

        public Segmentor(int N, double threshold)
        {
            this.N = N;
            this.threshold = threshold;
            size = (int) Math.Pow(2.0, (double) N);           
        }

        // Loop over every segment in dither order and attempt to grow it until no change in the segmentation
            // attempt to grow a segment
                // get the neighbour segments
                    // if their are no neighbours we are done and we can bail out
                // get the best neighbour segments
                // if there are no best neighbours then bail out and indicate that nothing has changed
                // get the mutually optimal neighbour segments
                // if none do "gradient descent" merging
                // else merge the optimal neighbours and exit, indicate that something has changed        
        public Segment[,] SegmentImage(TiffImage image)
        {   
            // Build an initial segmentation
            CreateSegmentationFromImage(image);
            
            bool unfinished = true;
            while (unfinished)
            {
                unfinished = TryGrowAllSegments();
                
            }
            return segmentation;
        }
        
        // Initialise the segmentation with a Segment for every pixel in the image
        private void CreateSegmentationFromImage(TiffImage image)
        {   
            // allocate and initialise segmentation
            segmentation = new Segment[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Pixel pixel = new Pixel(x, y, image.GetColourBands(x, y));
                    segmentation[x, y] = new Segment(pixel);
                }
            }
        }

        // Attempt to grow all segments in the segmentation.
        // Returns true if at least one segment was grown, ie. the segmentation should continue
        private bool TryGrowAllSegments()
        {
            // returns an array of coordinate pairs in dither order
            IEnumerable<Tuple<int, int>> coordsInDitherOrder = Dither.Coordinates(N);
            
            bool hasGrown = false;
            foreach (var coord in coordsInDitherOrder)
            {
                Segment segment = segmentation[coord.Item1, coord.Item2];
                hasGrown = TryGrowSegment(segment) || hasGrown;
            }
            return hasGrown;
        }

        // Try to find a neighbouring segmentB such that:
        //     1) segmentB is one of the best neighbours of segment A, and 
        //     2) segmentA is one of the best neighbours of segment B
        // if such a mutally optimal neighbour exists then merge them,
        // otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
        // Returns true if a change was made to the segmentation, ie. the
        private bool TryGrowSegment(Segment segmentA)
        {
            HashSet<Segment> bestNeighboursofSegmentA = FindBestNeighbours(segmentA);

            if (bestNeighboursofSegmentA.Count == 0) return false;

            // Find neighbours of segmentA whose best neighbour is also segmentA
            HashSet<Segment> mutallyOptimalNeighbours = new HashSet<Segment>();
            foreach (var segmentB in bestNeighboursofSegmentA)
            {
                HashSet<Segment> bestNeighbours = FindBestNeighbours(segmentB);
                if (bestNeighbours.Contains(segmentA) && (!segmentA.Equals(segmentB))) 
                    mutallyOptimalNeighbours.Add(segmentB);
            }
            
            // if there are no mutually optimal neighbours, perform gradient descent search on non-optimal ones
            if (mutallyOptimalNeighbours.Count == 0) 
                return TryGrowSegment(bestNeighboursofSegmentA.ToList()[0]);
            
            // if there is more than one mutually optimal neighbour, perform gradient descent search on optimal ones
            if (mutallyOptimalNeighbours.Count > 1)
                return TryGrowSegment(mutallyOptimalNeighbours.ToList()[0]);
            
            // if there is exactly one mutually optimal neighbour, merge segmentA and segmentB
            if (mutallyOptimalNeighbours.Count == 1)
            {
                // create new Segment
                Segment mergedSegment = new Segment(segmentA, mutallyOptimalNeighbours.ToList()[0]);
                
                // update the segmentation with pointers to the new Segment
                foreach (var pixel in mergedSegment.pixels)
                    segmentation[pixel.x, pixel.y] = mergedSegment;
                
                // Draw a pretty illustration
                //SegmentationIllustrator.DrawSegmentingProcess(segmentation, size);
                
                // we need to continue segmenting
                return true;
            }
            
            throw new Exception("Unexpected number of mutually optimal neighbours.");
        }

        
        // Find the neighbour(s) of the given segment that has the (equal) best merge cost
        // (exclude neighbours if their merge cost is greater than the threshold)
        private HashSet<Segment> FindBestNeighbours(Segment segment)
        {
            HashSet<Segment> neighbours = FindNeighbours(segment);
            
            Dictionary<Segment, double> bestNeighboursWithMergeCost = new Dictionary<Segment, double>();
            double bestMergeCost = Double.MaxValue;
            
            // make an initial pass over neighbours to filter out neighbours above the merge cost threshold
            foreach (var neighbour in neighbours)
            {                
                double mergeCost = Segment.MergeCost(segment, neighbour);
                if ((mergeCost <= bestMergeCost) && (mergeCost <= threshold))
                {
                    if (!segment.Equals(neighbour))
                    {
                        bestNeighboursWithMergeCost.Add(neighbour, mergeCost);
                        bestMergeCost = mergeCost;    
                    }
                }
            }
            
            // make a second pass to extract only the neighbours with the equal lowest merge cost
            HashSet<Segment> bestNeighbours = new HashSet<Segment>();
            foreach (var neighbour in bestNeighboursWithMergeCost)
            {
                if (neighbour.Value == bestMergeCost) 
                    bestNeighbours.Add(neighbour.Key);
            }
            
            return bestNeighbours;
        }

        // Find the neighbouring segments of the given segment
        // (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
        private HashSet<Segment> FindNeighbours(Segment segment)
        {
            HashSet<Segment> neighbours = new HashSet<Segment>();
            foreach (var pixel in segment.pixels)
            {
                int x = pixel.x;
                int y = pixel.y;
                
                // Create list of adjacent coords
                Coord[] potentialNeighbourCoords = new Coord[4]
                {
                    new Coord(x+1, y), 
                    new Coord(x-1, y), 
                    new Coord(x, y+1), 
                    new Coord(x, y-1)
                };
                
                // filter out those coords which are outside the segmentation bounds
                foreach (var coord in potentialNeighbourCoords)
                {
                    if (IsWithinSize(coord.x, coord.y))
                        neighbours.Add(segmentation[coord.x, coord.y]);
                }
            }
            return neighbours;
        }
        
        // Ensure a coordinate lies within the bounds of the segmentation
        private bool IsWithinSize(int x, int y)
        {
            if ((0 <= x) && (x < size) && (0 <= y) && (y < size)) 
                return true;
            else 
                return false;    
        }
    }
}
