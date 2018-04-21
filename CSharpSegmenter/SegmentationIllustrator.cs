using CSharpSegmenter;
using  System;

namespace CSharpSegmenter
{
    public class SegmentationIllustrator
    {
        public static void DrawSegmentingProcess(Segment[,] segmentation, int size)
        {
            DrawTable(size);
            DrawSegmentSizes(segmentation, size);
        }

        private static void DrawTable(int size)
        {
            int unit = 3;

            // Draw rows
            {
                int y = 1;
                for (int x = 1; x < 2*size+2; x++)
                {
                    Console.SetCursorPosition(x*unit, y);
                    Console.Write("-");
                }
                
                y = size + 2;
                for (int x = 1; x < 2*size+2; x++)
                {
                    Console.SetCursorPosition(x*unit, y);
                    Console.Write("-");
                }
            }

            // Draw columns
            {
                for (int x = 1; x < 2*size+3; x += 2)
                {
                    for (int y = 1; y < size+3; y++)
                    {
                        Console.SetCursorPosition(x*unit, y);
                        Console.Write("|");
                    }
                }                
            }
            
            // Draw corners
            {
                int y = 1;
                for (int x = 1; x < 2*size+3; x += 2)
                {
                    Console.SetCursorPosition(x*unit, y);
                    Console.Write("+");
                }

                y = size + 2;
                for (int x = 1; x < 2*size+3; x += 2)
                {
                    Console.SetCursorPosition(x*unit, y);
                    Console.Write("+");
                }
            }
            
            // Draw x-y scale
            {
                // x axis
                {
                    int y = 0;
                    for (int x = 2; x < 2*size+2; x+=2)
                    {
                        Console.SetCursorPosition(x*unit, y);
                        Console.Write((x-2)/2);
                    }    
                }

                // y axis
                {
                    int x = 0;
                    for (int y = 2; y < 2+size; y++)
                    {
                        Console.SetCursorPosition(x*unit, y);
                        Console.Write(y-2);
                    }                    
                }
            }
            Console.Out.Flush();
        }

        private static void DrawSegmentSizes(Segment[,] segmentation, int size)
        {
            int unit = 3;
            
            for (int i = 0; i < size; i++)
            {
                int x = 2*i+2;
                for (int j = 0; j < size; j++)
                {
                    int y = j+2;
                    Console.SetCursorPosition(x*unit, y);
                    Console.Write(segmentation[i,j].pixels.Length);
                }
            }
            Console.Out.Flush();
        }
    }
}
