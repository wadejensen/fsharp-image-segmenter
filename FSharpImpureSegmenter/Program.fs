﻿module Program

open System.Diagnostics
open System

[<EntryPoint>]
let main argv =    

    // load a Tiff image
    let image = Tiff.loadImage argv.[0]

    // testing using sub-image of size 32x32 pixels
    let N = 4

    // increasing this threshold will result in more segment merging and therefore fewer final segments
    let threshold = 800.0

    // determine the segmentation for the (top left corner of the) image (2^N x 2^N) pixels
    let segmentation = Segmentation.segmentImage image N threshold

    // draw the (top left corner of the) original image but with the segment boundaries overlayed in blue
    Tiff.overlaySegmentation image "impure-segmented.tif" N segmentation
    
    0 // return an integer exit code
    