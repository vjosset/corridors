using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    [Serializable]
    public class MiniMapCanvas : Panel
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MiniMapCanvas(MapCanvas canvas)
        {
            this.MapCanvas = canvas;
            this.DoubleBuffered = true;
        }
        public MiniMapCanvas()
        {
            this.DoubleBuffered = true;
        }

        #region Member Properties
        public MapCanvas MapCanvas { get; set; } = null;
        /// <summary>
        /// The map for this MiniMap.
        /// </summary>
        public Map Map => this.MapCanvas?.Map;
        #endregion

        #region Grid Settings
        /// <summary>
        /// Gets or sets this map's background color.
        /// </summary>
        public Color GridBackgroundColor => this.MapCanvas.GridBackgroundColor;
        #endregion

        #region UI Helper Properties
        #region Mouse Buttons and Modifier Keys
        /// <summary>
        /// Gets or sets a boolean indicating whether the left mouse button is currently pressed.
        /// </summary>
        private bool IsMouseLeftPressed { get; set; } = false;

        /// <summary>
        /// Gets or sets a boolean indicating whether the right mouse button is currently pressed.
        /// </summary>
        private bool IsMouseRightPressed { get; set; } = false;

        /// <summary>
        /// Gets or sets a boolean indicating whether the middle mouse button is currently pressed.
        /// </summary>
        private bool IsMouseMiddlePressed { get; set; } = false;
        #endregion

        #region Mouse Positions
        /// <summary>
        /// Gets or sets the start position (in pixel coordinates) of the scroll (middle mouse button canvas pan).
        /// </summary>
        private Point ScrollStartPos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (tile) position when left mouse button was last pressed.
        /// Point.Empty if left mouse button is not currently pressed.
        /// </summary>
        private Point MouseLeftDownTilePos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (tile) position when right mouse button was last pressed.
        /// Point.Empty if right mouse button is not currently pressed.
        /// </summary>
        private Point MouseRightDownTilePos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (tile) position when middle mouse button was last pressed.
        /// Point.Empty if middle mouse button is not currently pressed.
        /// </summary>
        private Point MouseMiddleDownTilePos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (pixel) position when left mouse button was last pressed.
        /// Point.Empty if left mouse button is not currently pressed.
        /// </summary>
        private Point MouseLeftDownPixelPos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (pixel) position when right mouse button was last pressed.
        /// Point.Empty if right mouse button is not currently pressed.
        /// </summary>
        private Point MouseRightDownPixelPos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the (pixel) position when middle mouse button was last pressed.
        /// Point.Empty if middle mouse button is not currently pressed.
        /// Note that this property holds the Cursor position, not the position relative to the map.
        /// </summary>
        private Point MouseMiddleDownPixelPos { get; set; } = Point.Empty;

        /// <summary>
        /// Gets or sets the last tile position of the cursor/mouse.
        /// </summary>
        private Point MouseLastTilePos { get; set; } = Point.Empty;
        #endregion
        #endregion

        #region Mouse Input Events
        private Point ParentAutoScrollPosition = Point.Empty;

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // [TBD]
            }
            this.Invalidate();
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // [TBD]
        }

        /// <summary>
        /// Handles mouse move event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // [TBD]
        }
        #endregion

        #region Draw Events
        /// <summary>
        /// Handles painting this control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.MapCanvas != null && this.Map != null && this.Map.Layers != null)
            {
                // Fill the background
                e.Graphics.Clear(this.GridBackgroundColor);

                // Draw the modules
                foreach (MapLayer lay in this.Map.Layers)
                {
                    foreach (MapModule mod in lay.Modules)
                    {
                        DrawModule(e.Graphics, mod);
                    }
                }

                // Draw the visible rectangle
                Rectangle visible = this.MapCanvas.GetVisiblePixelRectangle();
                float ratio = (MapCanvas.MaxMapSize * this.MapCanvas.Zoom) / this.Width;
                Rectangle miniVisible = new Rectangle((int)(Math.Floor(visible.X / ratio)), (int)(Math.Floor(visible.Y / ratio)), (int)(Math.Floor(visible.Width / ratio)), (int)(Math.Floor(visible.Height / ratio)));
                e.Graphics.DrawRectangle(new Pen(Color.Red, 2), miniVisible);
            }
        }

        /// <summary>
        /// Draws the specified module.
        /// </summary>
        /// <param name="g">The current Graphics object on which to draw the module.</param>
        /// <param name="mod">The module to draw.</param>
        private void DrawModule(Graphics g, MapModule mod)
        {
            if (mod != null)
            {
                float pixelsPerTile = (float)this.Width / (float)MapCanvas.MaxMapSize;
                PointF pixelPos = new PointF(mod.Position.X * pixelsPerTile, mod.Position.Y * pixelsPerTile);

                if (mod.Module != null)
                {
                    using (SolidBrush brush = new SolidBrush(mod.Module == null ? Color.Black : mod.Module.Color))
                    {
                        g.FillRectangle(brush, mod.Position.X * pixelsPerTile, mod.Position.Y * pixelsPerTile, mod.Width * pixelsPerTile, mod.Height * pixelsPerTile);
                    }
                }
            }
        }
        #endregion
    }
}
