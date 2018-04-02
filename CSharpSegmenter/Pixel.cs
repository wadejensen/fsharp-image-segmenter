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
    }
}