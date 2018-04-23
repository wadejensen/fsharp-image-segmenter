module Program

open System

open System.Diagnostics
open System

[<EntryPoint>]
let main argv =    

    // load a Tiff image
    let image = TiffModule.loadImage "C:\Users\WadeJensen\Dropbox\01_EN40\YEAR_5_SEM_1\SegmentationSkeleton\TestImages\L15-3662E-1902N-Q4.tif" //argv.[0]

    // testing using sub-image of size 32x32 pixels
    let N = 4

    // increasing this threshold will result in more segment merging and therefore fewer final segments
    let threshold = 800.0

    let sw = new Stopwatch();
    sw.Start()  

    // determine the segmentation for the (top left corner of the) image (2^N x 2^N) pixels
    let segmentation = SegmentationModule.segment image N threshold

    // draw the (top left corner of the) original image but with the segment boundaries overlayed in blue
    TiffModule.overlaySegmentation image "segmented.tif" N segmentation

    sw.Stop();
    Console.WriteLine("Elapsed={0}",sw.Elapsed);
    
    0 // return an integer exit code
    