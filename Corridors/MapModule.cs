﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    /// <summary>
    /// Represents a Module placed on a map.
    /// </summary>
    [Serializable]
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
            set
            {
                if (value != this.Position)
                {
                    this._position = value;
                    this.IsInvalidated = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether this MapModule has changed since its last draw/paint.
        /// </summary>
        public bool IsInvalidated { get; set; } = true;

        /// <summary>
        /// The Module archetype for this MapModule.
        /// </summary>
        [JsonIgnore]
        public Module Module { get; set; } = null;

        /// <summary>
        /// Gets the key for this MapModule's Module. Used to load maps.
        /// </summary>
        public string ModuleKey { get; set; } = "";

        /// <summary>
        /// Gets the width (in tiles) of this MapModule (reflects rotation).
        /// </summary>
        public int Width
        {
            get
            {
                if (this.Module == null)
                {
                    return 1;
                }
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
                if (this.Module == null)
                {
                    return 1;
                }
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
                if (value != this.Rotation)
                {
                    while (value < 0)
                    {
                        value += 4;
                    }
                    value = value % 4;
                    this._rotation = value;
                    this.IsInvalidated = true;
                }
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
                if (value != this.Mirror)
                {
                    if (value < 0)
                    {
                        value += 4;
                    }
                    value = value % 4;
                    this._mirror = value;
                    this.IsInvalidated = true;
                }
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
                // Build a properly-sized image to fit in the PictureBox (based on zoom)
                if (this.Module == null)
                {
                    return null;
                }
                return this.Module.Images[this.Mirror, this.Rotation];
            }
        }

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
            }
        }

        /// <summary>
        /// Gets the position of this module's top-left (North-West) corner (in tile coordinates).
        /// </summary>
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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

        /*
        public void PictureBox_Paint(object sender, PaintEventArgs pe)
        {
            // Draw the edges
            Graphics g = pe.Graphics;

            float lineOffset1 = frmMain.WallPen1.Width / 2;
            float lineOffset2 = frmMain.WallPen2.Width / 2;

            // North and South edges
            for (int col = 0; col < this.Width; col++)
            {
                // Check to the north of the northernmost tiles
                if (!this.Map.IsTileOccupied(new Point(col + this.Position.X, this.Position.Y - 1)))
                {
                    // The tile above this one is not occupied - Draw a wall North
                    g.DrawLine(frmMain.WallPen1, new PointF(col * this.Zoom, lineOffset1), new PointF((col + 1) * this.Zoom, lineOffset1));
                    g.DrawLine(frmMain.WallPen2, new PointF(col * this.Zoom, lineOffset2), new PointF((col + 1) * this.Zoom, lineOffset2));
                }

                // Check to the south of the southernmost tiles
                if (!this.Map.IsTileOccupied(new Point(col + this.Position.X, this.Position.Y + this.Height)))
                {
                    // The tile below this one is not occupied - Draw a wall South
                    g.DrawLine(frmMain.WallPen1, new PointF(col * this.Zoom, (this.Height * this.Zoom) - lineOffset1), new PointF((col + 1) * this.Zoom, (this.Height * this.Zoom) - lineOffset1));
                    g.DrawLine(frmMain.WallPen2, new PointF(col * this.Zoom, (this.Height * this.Zoom) - lineOffset2), new PointF((col + 1) * this.Zoom, (this.Height * this.Zoom) - lineOffset2));
                }
            }

            // East and West edges
            for (int row = 0; row < this.Height; row++)
            {
                // Check to the west of the westernmost tiles
                if (!this.Map.IsTileOccupied(new Point(this.Position.X - 1, row + this.Position.Y)))
                {
                    // The tile to the left of this one is not occupied - Draw a wall West
                    g.DrawLine(frmMain.WallPen1, new PointF(lineOffset1, (row * this.Zoom)), new PointF(lineOffset1, (row + 1) * this.Zoom));
                    g.DrawLine(frmMain.WallPen2, new PointF(lineOffset2, (row * this.Zoom)), new PointF(lineOffset2, (row + 1) * this.Zoom));
                }

                // Check to the east of the easternmost tiles
                if (!this.Map.IsTileOccupied(new Point(this.Position.X + this.Width, row + this.Position.Y)))
                {
                    // The tile to the right of this one is not occupied - Draw a wall East
                    g.DrawLine(frmMain.WallPen1, new PointF((this.Width * this.Zoom) - lineOffset1, row * this.Zoom), new PointF((this.Width * this.Zoom) - lineOffset1, (row + 1) * this.Zoom));
                    g.DrawLine(frmMain.WallPen2, new PointF((this.Width * this.Zoom) - lineOffset2, row * this.Zoom), new PointF((this.Width * this.Zoom) - lineOffset2, (row + 1) * this.Zoom));
                }
            }

            // Draw the highlight/tint
            if (this.IsSelected)
            {
                g.FillRectangle(frmMain.SelectionBrush, new Rectangle(0, 0, this.Width * this.Zoom, this.Height * this.Zoom));
            }
            else if (this.IsInHand)
            {
                g.FillRectangle(frmMain.ModuleInHandBrush, new Rectangle(0, 0, this.Width * this.Zoom, this.Height * this.Zoom));
            }
            if (this.IsToBeDeleted)
            {
                g.FillRectangle(frmMain.EraserBrush, new Rectangle(0, 0, this.Width * this.Zoom, this.Height * this.Zoom));
            }
        }
        */

        public void InvalidateAdjacentModules(MapLayer lay)
        {
            //if (lay != null)
            //{
            //    Rectangle comp = new Rectangle(this.X - 2, this.Y - 2, this.Width + 3, this.Height + 3);
            //
            //    lay.Modules.Where(mod =>  mod != this && mod.Overlaps(comp)).ToList().ForEach(mod => mod.Invalidate());
            //}
            // [TBD]
        }
        #endregion
    }
}
