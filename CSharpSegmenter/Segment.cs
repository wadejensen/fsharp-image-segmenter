
using System;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace CSharpSegmenter
{
    // OO representation of a Segment as an array of child pixels.
    // Contains a cached value
    public class Segment
    {
        public Pixel[] pixels { get; private set; }
        // cache for standard deviation calculation
        public double[] stdDevs { get; private set; }
        
        // Segment constructor for the bottom level of the Segmentation
        // A segment with a single pixel only
        public Segment(Pixel pixel)
        {
            this.pixels = new Pixel[1] {pixel};
        }

        // Constructor for creating a parent Segment from two children
        public Segment(Segment segment1, Segment segment2)
        {
            int length1 = segment1.pixels.Length;
            int length2 = segment2.pixels.Length;

            this.pixels = new Pixel[length1 + length2];
            segment1.pixels.CopyTo(this.pixels, 0);
            segment2.pixels.CopyTo(this.pixels, length1);
        }
        
        // determine the cost of merging the given segments in our Segmentation algorithm: 
        // equal to the standard deviation of the combined the segments minus the sum
        // of the standard deviations of the individual segments, weighted by their
        // respective sizes and summed over all colour bands
        public static double MergeCost(Segment segment1, Segment segment2)
        {
            double combinedWeight = CalculateSegmentWeight(new Segment(segment1, segment2));
            double segment1Weight = CalculateSegmentWeight(segment1);
            double segment2Weight = CalculateSegmentWeight(segment2);

            return combinedWeight - segment1Weight - segment2Weight;
        }
        
        // Calculate the contribution of a specific segment to the merge cost of two segments
        private static double CalculateSegmentWeight(Segment segment)
        {
            segment.CalculateStdDevs();   
            
            double sum = 0;
            foreach (var deviation in segment.stdDevs) sum += deviation;
            return sum * segment.pixels.Length;
        }
        
        // Return a list of the standard deviations of the pixel colours in the given segment.
        // The list contains one entry for each colour band, typically: [red, green and blue].
        private double[] CalculateStdDevs()
        {
            // return a cached result if possible
            if (stdDevs != null) return stdDevs;
            
            // Get dimensions
            int numBands = GetNumSubPixelsPerPixel();
            int numPixels = pixels.Length;
            
            // An array of all subpixels in a band for a Segment
            // [ [r0; r1; r2; r3; ... rN]; [g0; g1; g2; g3; ... gN]; [b0; b1; b2; b3; ... bN] ]
            byte[][] subPixels = Transpose(pixels, numPixels, numBands);
            
            // Std dev for each subpixel band
            stdDevs = new double[numBands];
            for (int i=0; i<numBands; i++) this.stdDevs[i] = stdDev(subPixels[i]);
            return stdDevs;
        }

        private byte[][] Transpose(Pixel[] arr, int numPixels, int numBands)
        {
            byte[][] subPixels = new byte[numBands][];
            for (int j=0; j<numBands; j++)
            {
                subPixels[j] = new byte[numPixels];
                for (int i=0; i<numPixels; i++) subPixels[j][i] = pixels[i][j];
            }
            return subPixels;
        }

        // Returns the number of subpixels per pixel for an image.
        // There should be no cases in this program where a pixel object is not allocated,
        // but we access the length cautiously as it is an infrequest operation anyway.
        private int GetNumSubPixelsPerPixel()
        {
            if (pixels == null) 
                throw new NullReferenceException("Cannot get num subpixels per pixel. Segment does not contain pixels.");
            if (pixels[0] == null) throw new 
                IndexOutOfRangeException("Cannot get num subpixels per pixel. First pixel missing from Segment");
            return pixels[0].subPixels.Length;
        }

        private double stdDev(byte[] arr)
        {
            double sum = 0;
            foreach (var x in arr) sum += x;
            double average = sum / arr.Length;

            double sumOfSquareMeanDistances = 0;
            foreach (var x in arr) sumOfSquareMeanDistances += (x - average) * (x - average);

            double variance = sumOfSquareMeanDistances / arr.Length;
            return (double) Math.Sqrt(variance);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || this.GetType() != obj.GetType()) 
                return false;

            Segment other = (Segment) obj;
            
            
            if (this.pixels.Length != other.pixels.Length)
                return false;
            
            // expects pixels to be in the same array order
            for (int i = 0; i < this.pixels.Length; i++)
            {
                if ( !(this.pixels[i] == (other.pixels[i])) )
                    return false;
            }

            return true;
        }
    }
}
