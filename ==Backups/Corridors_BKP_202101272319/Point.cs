using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public class CPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CPoint(int xPos = 0, int yPos = 0)
        {
            this.X = xPos;
            this.Y = yPos;
        }
    }
}
