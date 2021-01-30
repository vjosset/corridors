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
    public class Module
    {
        /// <summary>
        /// Gets or sets the name of this module.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the path to the bitmap for this module (at rotation 0).
        /// </summary>
        public string[] Images { get; set; }

        /// <summary>
        /// Gets the width (in squares) of this module.
        /// </summary>
        public int Width { get; set; } = 1;

        /// <summary>
        /// Gets or sets the height (in squares) of this module.
        /// </summary>
        public int Height { get; set; } = 1;

        /// <summary>
        /// Gets or sets the position (in squares) of the top-left corner of this module.
        /// </summary>
        public Point Position { get; set; } = new Point(0, 0);

        /// <summary>
        /// Gets or sets the squares and their properties (walls/edges) for this module.
        /// </summary>
        public Tile[,] Tiles { get; set; }

        /// <summary>
        /// Gets or sets whether this module has been selected/clicked
        /// </summary>
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// Backing field for the "Bitmaps" property.
        /// </summary>
        [JsonIgnore]
        private Bitmap[] _bitmaps = null;

        /// <summary>
        /// Gets or sets the bitmaps for this module.
        /// </summary>
        [JsonIgnore]
        public Bitmap[] Bitmaps
        {
            get
            {
                if (this._bitmaps == null || this._bitmaps.Length == 0)
                {
                    // Not loaded yet, build them
                    this._bitmaps = new Bitmap[this.Images.Length];
                    for (int i = 0; i < this.Images.Length; i++)
                    {
                        if (System.IO.File.Exists(this.Images[i]))
                        {
                            this._bitmaps[i] = new Bitmap(this.Images[i]);
                        }
                        else
                        {
                            // This image doesn't actually exist
                            this.Name += " (image file not found)";
                            this._bitmaps[i] = new Bitmap(10, 10);
                        }
                    }
                }

                return this._bitmaps;
            }
            set
            {
                this._bitmaps = value;
            }
        }

        /// <summary>
        /// Returns a deep-copy clone of this module.
        /// </summary>
        /// <returns></returns>
        public Module Clone()
        {
            Module mod = new Module();
            mod.Name = this.Name;

            // Copy the image paths
            mod.Images = new string[this.Images.Length];
            for (int i = 0; i < this.Images.Length; i++)
            {
                mod.Images[i] = this.Images[i];
            }
            mod.Width = this.Width;
            mod.Height = this.Height;
            mod.Position = new Point(this.Position.X, this.Position.Y);

            // Copy the bitmaps
            mod.Bitmaps = new Bitmap[this.Bitmaps.Length];
            for (int i = 0; i < this.Bitmaps.Length; i++)
            {
                mod.Bitmaps[i] = this.Bitmaps[i].Clone(new Rectangle(0, 0, this.Bitmaps[i].Width, this.Bitmaps[i].Height), this.Bitmaps[i].PixelFormat);
            }

            // Copy the tiles
            if (this.Tiles != null)
            {
                mod.Tiles = new Tile[this.Tiles.GetLength(0), this.Tiles.GetLength(1)];
                for (int row = 0; row < this.Tiles.GetLength(0); row++)
                {
                    for (int col = 0; col < this.Tiles.GetLength(1); col++)
                    {
                        mod.Tiles[row, col] = this.Tiles[row, col].Clone();
                    }
                }
            }

            // Done
            return mod;
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
                return new Point(this.Position.X + this.Width, this.Position.Y);
            }
        }

        /// <summary>
        /// Gets the position of this module's bottom-left (South-West) corner (in tile coordinates).
        /// </summary>
        public Point SW
        {
            get
            {
                return new Point(this.Position.X, this.Position.Y + this.Height);
            }
        }

        /// <summary>
        /// Gets the position of this module's bottom-right (South-East) corner (in tile coordinates).
        /// </summary>
        public Point SE
        {
            get
            {
                return new Point(this.Position.X + this.Width, this.Position.Y + this.Height);
            }
        }

        /// <summary>
        /// Rotates this module.
        /// </summary>
        /// <param name="rotation">
        /// How many quarter-turns to rotate this module:
        /// 0: No rotation
        /// 1: 90 degrees clockwise
        /// 2: 180 degrees clockwise
        /// 3: 270 degrees clockwise
        /// </param>
        public void SetRotation(int rotation)
        {
            // Make sure the rotation value is a valid one
            rotation = rotation % 4;

            if (rotation == 0)
            {
                // Nothing to rotate
                return;
            }

            // Set the new width and height (in squares) of this module
            if (rotation == 1 || rotation == 3)
            {
                int oldWidth = this.Width;
                int oldHeight = this.Height;
                this.Width = oldHeight;
                this.Height = oldWidth;
            }

            // Rotate the tile edges
            if (this.Tiles != null)
            {
                foreach (Tile tile in this.Tiles)
                {
                    tile.SetRotation(rotation);
                }
            }

            // Rotate the tiles (matrix transform)
            for (int i = 1; i <= rotation; i++)
            {
                // Each rotation is 90 degrees
                if (this.Tiles != null)
                {
                    this.Tiles = (Tile[,])Utils.RotateMatrix(this.Tiles);
                }
            }

            // Rotate the bitmaps
            foreach (Bitmap bmp in this.Bitmaps)
            {
                switch (rotation)
                {
                    case 0:
                        break;
                    case 1:
                        // Rotate 90 degrees clockwise
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 2:
                        // Rotate 180 degrees clockwise
                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 3:
                        // Rotate 270 degrees clockwise
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    default:
                        break;
                }
            }
        }

        public void MirrorHorizontal()
        {
            // Flip the bitmaps
            foreach (Bitmap bmp in this.Bitmaps)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
        }

        public void MirrorVertical()
        {
            // Flip the bitmaps
            foreach (Bitmap bmp in this.Bitmaps)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
        }

        /// <summary>
        /// Helper method to get Source Rectangle when drawing this tile.
        /// </summary>
        /// <returns></returns>
        private Rectangle GetSourceRectangle()
        {
            Rectangle rect = new Rectangle(0, 0, this.Bitmaps[0].Width, this.Bitmaps[0].Height);

            // Done
            return rect;
        }

        /// <summary>
        /// Helper method to get Destination Rectangle when drawing this tile.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Rectangle GetDestinationRectangle(int scale)
        {
            Rectangle rect = new Rectangle(this.Position.X * scale, this.Position.Y * scale, this.Width * scale, this.Height * scale);

            // Done
            return rect;
        }

        /// <summary>
        /// Draws this module onto the specified graphics container at the specified scale.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="scale"></param>
        public void Draw(Graphics g, int scale, float opacity = 1.0f)
        {
            // Pick a random bitmap from the loaded bitmaps
            Bitmap bmp = Utils.GetRandomArrayMember(this.Bitmaps);

            // Draw the texture/bitmap
            if (opacity != 1.0f)
            {   
                // Create a color matrix object  
                ColorMatrix matrix = new ColorMatrix();

                // Set the opacity  
                matrix.Matrix33 = opacity;

                // Create image attributes  
                ImageAttributes attributes = new ImageAttributes();

                // Set the color(opacity) of the image  
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Now draw the image  
                g.DrawImage(bmp, this.GetDestinationRectangle(scale), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
            }
            else
            {
                // Draw the module at full opacity
                //g.DrawImage(bmp, this.GetDestinationRectangle(scale), this.GetSourceRectangle(), GraphicsUnit.Pixel);
            }

            // Draw the edges
            int penWidth = scale / 10;
            Pen wallpen = new Pen(Color.Black, penWidth);
            wallpen.Alignment = PenAlignment.Center;
            Pen doorpen = new Pen(Color.Orange, penWidth);
            doorpen.Alignment = PenAlignment.Center;
            SolidBrush selectedBrush = new SolidBrush(Color.FromArgb(127, 255, 0, 0));

            // Build the corner points to make drawing easier
            Point modNW = new Point(this.Position.X * scale, this.Position.Y * scale);
            Point modNE = new Point((this.Position.X + this.Width) * scale, this.Position.Y * scale);
            Point modSW = new Point(this.Position.X * scale, (this.Position.Y + this.Height) * scale);
            Point modSE = new Point((this.Position.X + this.Width) * scale, (this.Position.Y + this.Height) * scale);

            if (this.IsSelected)
            {
                // Draw the selection rectangle on this module
                g.FillRectangle(selectedBrush, modNW.X, modNW.Y, this.Width * scale, this.Height * scale);
            }
            else
            {
                // Not selected - Draw borders as defined in the tiles
                if (this.Tiles != null)
                {
                    for (int row = 0; row < this.Tiles.GetLength(0); row++)
                    {
                        for (int col = 0; col < this.Tiles.GetLength(1); col++)
                        {
                            // Draw these edges
                            Tile tile = this.Tiles[row, col];
                            string N = tile.Edges.Substring(0, 1);
                            string E = tile.Edges.Substring(1, 1);
                            string S = tile.Edges.Substring(2, 1);
                            string W = tile.Edges.Substring(3, 1);

                            // Build the corner points to make drawing easier
                            Point tileNW = new Point((this.Position.X + col) * scale, (this.Position.Y + row) * scale);
                            Point tileNE = new Point((this.Position.X + col + 1) * scale, (this.Position.Y + row) * scale);
                            Point tileSW = new Point((this.Position.X + col) * scale, (this.Position.Y + row + 1) * scale);
                            Point tileSE = new Point((this.Position.X + col + 1) * scale, (this.Position.Y + row + 1) * scale);

                            // Draw the walls where necessary
                            if (N == "W")
                            {
                                // Draw a wall to the north of this tile
                                this.DrawEdge(g, wallpen, tileNW, tileNE);
                            }
                            if (N == "D")
                            {
                                // Draw a door to the north of this tile
                                this.DrawEdge(g, doorpen, tileNW, tileNE);
                            }

                            if (E == "W")
                            {
                                // Draw a wall to the east of this tile
                                this.DrawEdge(g, wallpen, tileSE, tileNE);
                            }
                            if (E == "D")
                            {
                                // Draw a door to the east of this tile
                                this.DrawEdge(g, doorpen, tileSE, tileNE);
                            }

                            if (S == "W")
                            {
                                // Draw a wall to the south of this tile
                                this.DrawEdge(g, wallpen, tileSW, tileSE);
                            }
                            if (S == "D")
                            {
                                // Draw a door to the south of this tile
                                this.DrawEdge(g, doorpen, tileSW, tileSE);
                            }

                            if (W == "W")
                            {
                                // Draw a wall to the west of this tile
                                this.DrawEdge(g, wallpen, tileSW, tileNW);
                            }
                            if (W == "D")
                            {
                                // Draw a door to the west of this tile
                                this.DrawEdge(g, doorpen, tileSW, tileNW);
                            }
                        }
                    }
                }
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

        private Bitmap _thumbnail = null;

        /// <summary>
        /// Returns a scaled Image object to be displayed in the TreeView
        /// </summary>
        /// <param name="size">The size (in pixels) of the generated thumbnail to return</param>
        /// <returns></returns>
        public Image GetThumbnail(int size = 100)
        {
            if (this._thumbnail is null)
            {
                // Generate a scaled thumbnail for this module's image (for display in TreeView)
                //these are target width, target height, the target x and y
                int tw, th, tx, ty;

                int w = this.Bitmaps[0].Width;
                int h = this.Bitmaps[0].Height;

                //get the ratio of width / height
                double whRatio = (double)w / h;

                //if width >= height
                if (this.Bitmaps[0].Width >= this.Bitmaps[0].Height)
                {
                    //set the width to [size] and compute the height
                    tw = size;
                    th = (int)(tw / whRatio);
                }
                else
                {
                    //otherwise set the height to [size] and compute the width
                    th = size;
                    tw = (int)(th * whRatio);
                }

                //now we compute where in our final image we're going to draw it
                tx = (size - tw) / 2;
                ty = (size - th) / 2;

                //create our final image - you can set the PixelFormat to anything that suits
                this._thumbnail = new Bitmap(size, size, PixelFormat.Format24bppRgb);

                //get a Graphics from this final image
                Graphics g = Graphics.FromImage(this._thumbnail);

                //clear the final image to white (or anything else) for the areas that the input image doesn't cover
                g.Clear(Color.White);

                //housework -
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                //and here is where everything happens
                //the first Rectangle is where (tx, ty) and what size (tw, th) we want to draw the image
                //the second Rectangle is the portion of the image we want to draw, in this case all of it
                //the drawing routine does the resizing

                g.DrawImage(this.Bitmaps[0], new Rectangle(tx, ty, tw, th), new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);

                // If this module has multiple bitmaps, indicate it on its thumbnail
                if (this.Images.Length > 1)
                {
                    // Draw background
                    g.FillRectangle(new SolidBrush(Color.White), 0, 0, tw / 5, th / 5);

                    // Draw number of alternate tiles
                    g.DrawString(this.Images.Length.ToString(), new Font(FontFamily.GenericMonospace, tw / 10), new SolidBrush(Color.Black), 0, 0);
                }
            }

            return this._thumbnail;
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
    }
}
