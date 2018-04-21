using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter
{
    public class Coord
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return String.Format("Coord({0},{1})", this.x, this.y);
        }
    }
}
