using System;

namespace CSharpSegmenter
{
    public class Pixel
    {
        public int x { get; protected set; }
        public int y { get; protected set; }
        public byte[] subPixels { get; protected set; }
        
        public Pixel(int x, int y, byte[] subPixels)
        {
            this.x = x;
            this.y = y;
            this.subPixels = subPixels;
        }
        
        public byte this[int i]
        {
            get { return subPixels[i]; }
            protected set { subPixels[i] = value; }
        }

        // WARNING: -> only checks whether the x and y coords are the same.
        // Does not examine the content of the pixel.
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || this.GetType() != obj.GetType()) 
                return false;
                       
            // check x and y coords match
            Pixel other = (Pixel) obj;

            if (this.x == 0 && this.y == 4)
            {
                if (other.x == 0 && other.y == 4)
                {
                    bool bugIncoming = true;
                    Console.WriteLine(bugIncoming);
                }
            }
            
            if (this.x == other.x && this.x == other.y) 
                return true;
            else 
                return false;
        }
    }
}
