using System;
using System.Diagnostics;

namespace CSharpSegmenter
{
    class Program
    {
        static int Main(string[] args)
        {

            // filepath to the image
            string imagePath = args[0];

            TiffImage image = new TiffImage(imagePath);

            // testing using sub-image of size 32x32 pixels
            int N = 5;

            // increasing this threshold will result in more segment merging and therefore fewer final segments
            double threshold = 800.0;

            Segmentor segmentor = new Segmentor(N, threshold);

            // determine the segmentation for the (top left corner of the) image (2^N x 2^N) pixels
            Segment[,] segmentation = segmentor.SegmentImage(image);

                    
            // draw the (top left corner of the) original image but with the segment boundaries overlayed in blue
            image.OverlaySegmentation("csharp-segmented.tif", N, segmentation);
    
            // return an integer exit code
            return 0; 
        }
    }
}
