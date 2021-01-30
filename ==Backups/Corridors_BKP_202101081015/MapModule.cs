using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corridors
{
    /// <summary>
    /// Represents a Module placed on a map.
    /// </summary>
    public class MapModule
    {
        #region Backing Fields
        private bool _isToBeDeleted = false;
        private bool _isSelected = false;
        private bool _isInHand = false;
        private Point _position = new Point(0, 0);

        /// <summary>
        /// Private backing field for property "Rotation".
        /// </summary>
        private int _rotation = 0;

        /// <summary>
        /// Private backing field for property "Mirror".
        /// </summary>
        private int _mirror = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the position (in squares) of the top-left corner of this module.
        /// </summary>
        public Point Position
        {
            get
            {
                return this._position;
            }
            set {
                this.IsDrawn = false;
                this._position = value;
            }

        }

        /// <summary>
        /// The Module archetype for this MapModule.
        /// </summary>
        [JsonIgnore]
        public Module Module { get; set; } = null;

        /// <summary>
        /// Gets the key for this MapModule's Module pointed. Used to load maps.
        /// </summary>
        public string ModuleKey { get; set; } = "";

        /// <summary>
        /// Gets the width (in tiles) of this MapModule (reflects rotation).
        /// </summary>
        public int Width
        {
            get
            {
                switch (this.Rotation)
                {
                    case 1:
                    case 3:
                        return this.Module.Height;
                    default:
                        return this.Module.Width;
                }
            }
        }

        /// <summary>
        /// Gets the height (in tiles) of this MapModule (reflects rotation).
        /// </summary>
        public int Height
        {
            get
            {
                switch (this.Rotation)
                {
                    case 1:
                    case 3:
                        return this.Module.Width;
                    default:
                        return this.Module.Height;
                }
            }
        }

        /// <summary>
        /// Gets or sets this MapModule's rotation.
        /// </summary>
        public int Rotation
        {
            get
            {
                return this._rotation;
            }
            set
            {
                while (value < 0)
                {
                    value += 4;
                }
                value = value % 4;
                this._rotation = value;

                this.IsDrawn = false;
            }
        }

        /// <summary>
        /// Gets or sets this MapModule's mirror.
        /// </summary>
        public int Mirror
        {
            get
            {
                return this._mirror;
            }
            set
            {
                if (value < 0)
                {
                    value += 4;
                }
                value = value % 4;
                this._mirror = value;
            }
        }

        /// <summary>
        /// Gets the X position of this module. Same as Module.Position.X.
        /// </summary>
        public int X
        {
            get
            {
                return this.Position.X;
            }
            set
            {
                this.Position = new Point(value, this.Y);
            }
        }

        /// <summary>
        /// Gets the Y position of this module. Same as Module.Position.Y.
        /// </summary>
        public int Y
        {
            get
            {
                return this.Position.Y;
            }
            set
            {
                this.Position = new Point(this.X, value);
            }
        }

        /// <summary>
        /// Gets the image (reflecting rotation and mirror) for this MapModule.
        /// </summary>
        [JsonIgnore]
        public Image Image
        {
            get
            {
                return this.Module.Images[this.Mirror, this.Rotation];
            }
        }

        /// <summary>
        /// Indicates whether this module has been painted onto the canvas.
        /// </summary>
        [JsonIgnore]
        public bool IsDrawn { get; private set; } = false;

        /// <summary>
        /// Indicates whether this module should be highlighted as "to be deleted".
        /// </summary>
        [JsonIgnore]
        public bool IsToBeDeleted
        {
            get
            {
                return this._isToBeDeleted;
            }
            set
            {
                this._isToBeDeleted = value;

                // Mark this MapModule as invalidated so it can be redrawn
                this.IsDrawn = false;
            }
        }

        /// <summary>
        /// Gets or sets whether this module has been selected/clicked
        /// </summary>
        [JsonIgnore]
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                this.IsDrawn = false;
            }
        }

        /// <summary>
        /// Gets or sets whether this module is "in hand", not on the map
        /// </summary>
        [JsonIgnore]
        public bool IsInHand
        {
            get
            {
                return this._isInHand;
            }
            set
            {
                this._isInHand = value;
                this.IsDrawn = false;
            }
        }

        /// <summary>
        /// Gets the position of this module's top-left (North-West) corner (in tile coordinates).
        /// </summary>
        public Point NW
        {
            get
            {
                return this.Position;
            }
        }

        /// <summary>
        /// Gets the position of this module's top-right (North-East) corner (in tile coordinates).
        /// </summary>
        public Point NE
        {
            get
            {
                return new Point(this.X + this.Width, this.Y);
            }
        }

        /// <summary>
        /// Gets the position of this module's bottom-left (South-West) corner (in tile coordinates).
        /// </summary>
        public Point SW
        {
            get
            {
                return new Point(this.X, this.Y + this.Height);
            }
        }

        /// <summary>
        /// Gets the position of this module's bottom-right (South-East) corner (in tile coordinates).
        /// </summary>
        public Point SE
        {
            get
            {
                return new Point(this.X + this.Width, this.Y + this.Height);
            }
        }
        #endregion

        #region Methods
        public MapModule(Module mod)
        {
            this.Module = mod;
            if (mod != null)
            {
                this.ModuleKey = mod.Key;
            }
        }

        /// <summary>
        /// Helper method to get Source Rectangle when drawing this tile.
        /// </summary>
        /// <returns></returns>
        private Rectangle GetSourceRectangle()
        {
            return new Rectangle(0, 0, this.Image.Width, this.Image.Height);
        }

        /// <summary>
        /// Helper method to get Destination Rectangle when drawing this tile.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Rectangle GetDestinationRectangle(int scale)
        {
            Rectangle rect = new Rectangle(this.X * scale, this.Y * scale, this.Width * scale, this.Height * scale);

            // Done
            return rect;
        }

        public static SolidBrush SelectedBrush = new SolidBrush(Color.FromArgb(95, 0, 0, 255));
        public static SolidBrush InHandBrush = new SolidBrush(Color.FromArgb(95, 0, 255, 0));
        public static Pen wallPen = new Pen(Color.Black, 3);

        /// <summary>
        /// Draws this module onto the specified graphics container at the specified scale.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="scale"></param>
        public void Draw(Graphics g, int scale, Map map)
        {
            // Calculate the corner points (in pixel coordinates) to make drawing easier
            Point modNW = new Point(this.X * scale, this.Y * scale);
            Point modNE = new Point((this.X + this.Width) * scale, this.Y * scale);
            Point modSW = new Point(this.X * scale, (this.Y + this.Height) * scale);
            Point modSE = new Point((this.X + this.Width) * scale, (this.Y + this.Height) * scale);

            // Draw the module
            g.DrawImage(this.Image, this.GetDestinationRectangle(scale), this.GetSourceRectangle(), GraphicsUnit.Pixel);

            // Draw the tint rectangle on this module
            Color tint = Color.Transparent;

            SolidBrush overlay = null;

            if (this.IsSelected)
            {
                overlay = SelectedBrush;
            }
            else if (this.IsInHand)
            {
                overlay = InHandBrush;
            }

            if (overlay != null)
            {
                // Draw the overlay rectangle for tint
                g.FillRectangle(overlay, modNW.X, modNW.Y, this.Width * scale, this.Height * scale);
            }
            
            // Draw the edges/walls of this module
            // North and South edges
            for (int col = 0; col < this.Width; col++)
            {
                // Check to the north of the northernmost tiles
                if (!map.IsTileOccupied(new Point(col + this.Position.X, this.Position.Y - 1)))
                {
                    // The tile above this one is not occupied - Draw a wall North
                    g.DrawLine(wallPen, new Point((col + this.Position.X) * scale, this.Position.Y * scale), new Point((col + this.Position.X + 1) * scale, this.Position.Y * scale));
                }

                // Check to the south of the southernmost tiles
                if (!map.IsTileOccupied(new Point(col + this.Position.X, this.Position.Y + this.Height)))
                {
                    // The tile below this one is not occupied - Draw a wall South
                    g.DrawLine(wallPen, new Point((col + this.Position.X) * scale, (this.Position.Y + this.Height) * scale), new Point((col + this.Position.X + 1) * scale, (this.Position.Y + this.Height) * scale));
                }
            }

            // East and West edges
            for (int row = 0; row < this.Height; row++)
            {
                // Check to the west of the westernmost tiles
                if (!map.IsTileOccupied(new Point(this.Position.X - 1, row + this.Position.Y)))
                {
                    // The tile to the left of this one is not occupied - Draw a wall West
                    g.DrawLine(wallPen, new Point(this.Position.X * scale, (row + this.Position.Y) * scale), new Point(this.Position.X * scale, (row + this.Position.Y + 1) * scale));
                }

                // Check to the east of the easternmost tiles
                if (!map.IsTileOccupied(new Point(this.Position.X + this.Width, row + this.Position.Y)))
                {
                    // The tile to the right of this one is not occupied - Draw a wall East
                    g.DrawLine(wallPen, new Point((this.Position.X + this.Width) * scale, (row + this.Position.Y) * scale), new Point((this.Position.X + this.Width) * scale, (row + this.Position.Y + 1) * scale));
                }
            }

            // Mark this module as drawn
            this.IsDrawn = true;
        }

        /// <summary>
        /// Draws the specified edge on a tile.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void DrawEdge(Graphics g, Pen pen, Point start, Point end)
        {
            // Draw the line first
            g.DrawLine(pen, start, end);
        }

        /// <summary>
        /// Returns a boolean indicating whether the specified tile position is contained within this module.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public bool ContainsTile(Point tilePos)
        {
            return
                tilePos.X >= this.Position.X && tilePos.X < this.Position.X + this.Width &&
                tilePos.Y >= this.Position.Y && tilePos.Y < this.Position.Y + this.Height;
        }

        /// <summary>
        /// Returns the rectangle that this module occupies on the map (in tile coordinates).
        /// </summary>
        public Rectangle OccupiedRectangle
        {
            get
            {
                return new Rectangle(this.Position.X, this.Position.Y, this.Width, this.Height);
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether the specified rectangle and this Module's OccupiedRectangle overlap.
        /// </summary>
        /// <param name="comp">The Rectangle to check for overlap (in tile coordinates).</param>
        /// <returns></returns>
        public bool Overlaps(Rectangle comp)
        {
            return this.OccupiedRectangle.IntersectsWith(comp);
        }

        /// <summary>
        /// Rotates this MapModule clockwise.
        /// </summary>
        public void RotateCW()
        {
            this.Rotation++;
        }

        /// <summary>
        /// Rotates this MapModule counter-clockwise.
        /// </summary>
        public void RotateCCW()
        {
            this.Rotation--;
        }

        /// <summary>
        /// Mirrors this MapModule on its X axis.
        /// </summary>
        public void MirrorHorizontal()
        {
            switch (this.Mirror)
            {
                case 0:
                    // Not currently mirrored - Becomes horizontal
                    this.Mirror = 1;
                    break;
                case 1:
                    // Already mirrored horizontal - Becomes not mirrored
                    this.Mirror = 0;
                    break;
                case 2:
                    // Already mirrored vertical - Becomes horizontal and vertical
                    this.Mirror = 3;
                    break;
                case 3:
                    // Already mirrored horizontal and vertical - Becomes vertical
                    this.Mirror = 2;
                    break;
            }

            this.IsDrawn = false;
        }

        /// <summary>
        /// Rotates this MapModule on its Y axis.
        /// </summary>
        public void MirrorVertical()
        {
            switch (this.Mirror)
            {
                case 0:
                    // Not currently mirrored - Becomes vertical
                    this.Mirror = 2;
                    break;
                case 1:
                    // Already mirrored horizontal - Becomes horizontal and vertical
                    this.Mirror = 3;
                    break;
                case 2:
                    // Already mirrored vertical - Becomes not mirrored
                    this.Mirror = 0;
                    break;
                case 3:
                    // Already mirrored horizontal and vertical - Becomes horizontal
                    this.Mirror = 1;
                    break;
            }

            this.IsDrawn = false;
        }

        /// <summary>
        /// Returns a deep-copy clone of this MapModule.
        /// </summary>
        /// <returns></returns>
        public MapModule Clone()
        {
            MapModule mod = new MapModule(this.Module);

            // Copy fields
            mod.Position = this.Position;
            mod.Rotation = this.Rotation;
            mod.Mirror = this.Mirror;

            // Done
            return mod;
        }
        #endregion
    }
}
