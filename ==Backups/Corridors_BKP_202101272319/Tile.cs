using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    public class Tile
    {
        /// <summary>
        /// The string representing the edges of this tile.
        /// [North][East][South][West]
        /// Valid characters:
        /// N: Nothing
        /// W: Wall
        /// D: Door
        /// </summary>
        public string Edges = "NNNN";

        public Tile Clone()
        {
            Tile s = new Tile();
            s.Edges = this.Edges;

            // Done
            return s;
        }

        public void SetRotation(int rotation)
        {
            // Make sure the rotation value is a valid one
            rotation = rotation % 4;

            // Rotate the edges
            this.Edges = Utils.ShiftString(this.Edges, rotation);
        }
    }
}
