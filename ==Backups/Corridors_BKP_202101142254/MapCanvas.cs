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
    public class MapCanvas : Panel
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MapCanvas()
        {
            this.DoubleBuffered = true;
        }

        #region Member Properties
        /// <summary>
        /// The map for this MapCanvas.
        /// </summary>
        public Map Map { get; set; } = new Map();

        /// <summary>
        /// The current index of the layer we are editing.
        /// </summary>
        public int MapLayerIndex { get; set; } = 0;

        /// <summary>
        /// Gets the current layer we are editing (see also MapLayerIndex).
        /// </summary>
        public MapLayer CurrentMapLayer
        {
            get
            {
                if (!this.Map.Layers.Any()) {
                    MapLayer lay = new MapLayer();
                    this.Map.Layers.Add(lay);
                }
                if (this.MapLayerIndex < 0)
                {
                    this.MapLayerIndex = 0;
                }
                if (this.MapLayerIndex >= this.Map.Layers.Count())
                {
                    this.MapLayerIndex = this.Map.Layers.Count() - 1;
                }
                return this.Map.Layers[MapLayerIndex];
            }
        }

        private int _zoom = 30;

        /// <summary>
        /// Gets or sets the current zoom factor for this map canvas (pixels per tile).
        /// </summary>
        public int Zoom
        {
            get
            {
                return this._zoom;
            }
            set
            {
                if (value <= 200 && value > 0)
                {
                    this._zoom = value;
                    this.InvalidateAll();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of modules in hand/modules to place.
        /// </summary>
        public List<MapModule> ModulesInHand { get; set; } = new List<MapModule>();

        /// <summary>
        /// Gets or sets the current selection rectangle (in tile coordinates).
        /// </summary>
        private Rectangle SelectionRectangle { get; set; } = Rectangle.Empty;

        private Mode _currentMode = Mode.Select;
        /// <summary>
        /// Gets or sets this MapCanvas' current mode.
        /// </summary>
        public Mode CurrentMode
        {
            get
            {
                return this._currentMode;
            }
            set
            {
                this._currentMode = value;
                switch (value)
                {
                    case Mode.Draw:
                    case Mode.Paint:
                        this.Cursor = Cursors.Cross;

                        // Clear selection
                        this.CurrentMapLayer.SelectedModules.ForEach(mod => mod.IsSelected = false);
                        ClearSelectionRectangle();

                        // Clear modules that were highlighted as to be deleted
                        this.CurrentMapLayer.Modules.Where(mod => mod.IsToBeDeleted).ToList().ForEach(mod => mod.IsToBeDeleted = false);
                        break;
                    case Mode.Erase:
                        this.Cursor = Cursors.No;
                        ClearModulesInHand();

                        //Clear selection
                        this.CurrentMapLayer.SelectedModules.ForEach(mod => mod.IsSelected = false);
                        ClearSelectionRectangle();

                        // Clear modules that were highlighted as to be deleted
                        this.CurrentMapLayer.Modules.Where(mod => mod.IsToBeDeleted).ToList().ForEach(mod => mod.IsToBeDeleted = false);
                        break;
                    case Mode.Move:
                        this.Cursor = Cursors.SizeAll;
                        ClearModulesInHand();
                        break;
                    case Mode.Select:
                        this.Cursor = Cursors.Default;
                        ClearModulesInHand();

                        // Clear modules that were highlighted as to be deleted
                        this.CurrentMapLayer.Modules.Where(mod => mod.IsToBeDeleted).ToList().ForEach(mod => mod.IsToBeDeleted = false);
                        break;
                    default:
                        this.Cursor = Cursors.Default;
                        ClearModulesInHand();
                        break;
                }

                this.Invalidate();
            }
        }

        /// <summary>
        /// Represents an edit mode for this MapCanvas.
        /// </summary>
        public enum Mode
        {
            Select,
            Draw,
            Paint,
            Erase,
            Move
        }

        /// <summary>
        /// The maximum width and height of the map, in tiles.
        /// </summary>
        private const int MaxMapSize = 100;
        #endregion

        #region Grid Settings
        /// <summary>
        /// Gets or sets this map's background color.
        /// </summary>
        public Color GridBackgroundColor { get; set; } = Color.LightGray;

        /// <summary>
        /// Gets or sets this map's grid minor line color.
        /// </summary>
        public Color GridMinorLineColor { get; set; } = Color.DarkGray;

        /// <summary>
        /// Gets or sets this map's grid major line color.
        /// </summary>
        public Color GridMajorLineColor { get; set; } = Color.DarkGray;

        /// <summary>
        /// Gets or sets this map's major grid line period (in number of tiles).
        /// For rooms that are 3 x 3, set this to 3.
        /// </summary>
        public int GridRoomSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the width (in number of tiles) for this map's grid.
        /// For gutters of 1 tile between rooms, set this to 1;
        /// </summary>
        public int GridGutterSize { get; set; } = 1;

        /// <summary>
        /// Gets or sets the thickness of minor grid lines.
        /// </summary>
        public int GridMinorLineWidth { get; set; } = 1;

        /// <summary>
        /// Gets or sets the thickness of major grid lines.
        /// </summary>
        public int GridMajorLineWidth { get; set; } = 3;
        #endregion

        #region Brushes and Pens
        /// <summary>
        /// Brush for painting the tint on selected modules.
        /// </summary>
        private static SolidBrush SelectionBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 0, 255));

        /// <summary>
        /// Brush for painting the tint on modules to be deleted (for hover in "Eraser" mode).
        /// </summary>
        private static SolidBrush EraserBrush { get; } = new SolidBrush(Color.FromArgb(95, 255, 0, 0));

        /// <summary>
        /// Brush for painting the tint on modules in hand.
        /// </summary>
        private static SolidBrush ModuleInHandBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 255, 0));

        /// <summary>
        /// Pen for drawing main walls around modules.
        /// </summary>
        private static Pen WallPen1 { get; } = new Pen(Color.Black, 2);

        /// <summary>
        /// Pen for drawing secondary walls (shadow) around modules.
        /// </summary>
        private static Pen WallPen2 { get; } = new Pen(Color.FromArgb(64, 0, 0, 0), 5);
        #endregion

        #region UI Helper Properties
        #region Mouse Buttons and Modifier Keys
        /// <summary>
        /// Gets or sets a boolean indicating whether the Ctrl key is currently pressed.
        /// </summary>
        private bool IsCtrlPressed { get; set; } = false;

        /// <summary>
        /// Gets or sets a boolean indicating whether the Shift key is currently pressed.
        /// </summary>
        private bool IsShiftPressed { get; set; } = false;

        /// <summary>
        /// Gets or sets a boolean indicating whether the Alt key is currently pressed.
        /// </summary>
        private bool IsAltPressed { get; set; } = false;

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

        private Point CanvasPanStartAutoScroll { get; set; } = Point.Empty;
        #endregion

        #endregion

        #region Mouse Input Events
        private Point ParentAutoScrollPosition = Point.Empty;

        /// <summary>
        /// Handles mouse enter events - Focuses this MapCanvas control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(EventArgs e)
        {
            this.Focus();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            // Reset the parent control's autoscroll position
            ((ScrollableControl)this.Parent).AutoScrollPosition = this.ParentAutoScrollPosition;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            // Store the parent control's autoscroll position
            ScrollableControl container = ((ScrollableControl)this.Parent);

            // Need to store the position as positive values, even though the container's AutoScrollPosition returns negative values
            this.ParentAutoScrollPosition = new Point(Math.Abs(container.AutoScrollPosition.X), Math.Abs(container.AutoScrollPosition.Y));

            // Reset the key pressed
            this.IsShiftPressed = false;
            this.IsAltPressed = false;
            this.IsCtrlPressed = false;
        }

        /// <summary>
        /// Handles mousewheel event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Mark the event as handled so it doesn't bubble up to the parent control
            ((HandledMouseEventArgs)e).Handled = true;

            ScrollableControl container = (ScrollableControl)this.Parent;

            if (this.Parent is ScrollableControl)
            {
                if (this.IsCtrlPressed)
                {
                    // Ctrl is pressed - Zoom in/out
                    if (e.Delta > 0)
                    {
                        // Positive scroll - Zoom in
                        this.ZoomIn();
                    }
                    else
                    {
                        // Negative scroll - Zoom out
                        this.ZoomOut();
                    }
                }
                else if (this.IsShiftPressed)
                {
                    // Shift is pressed - Perform a horizontal scroll
                    int newScroll = container.HorizontalScroll.Value - e.Delta;
                    if (newScroll < container.HorizontalScroll.Minimum)
                    {
                        newScroll = container.HorizontalScroll.Minimum;
                    }
                    if (newScroll > container.HorizontalScroll.Maximum)
                    {
                        newScroll = container.HorizontalScroll.Maximum;
                    }
                    container.HorizontalScroll.Value = newScroll;
                }
                else
                {
                    // No modifier keys pressed - Perform a vertical scroll
                    int newScroll = container.VerticalScroll.Value - e.Delta;
                    if (newScroll < container.VerticalScroll.Minimum)
                    {
                        newScroll = container.VerticalScroll.Minimum;
                    }
                    if (newScroll > container.VerticalScroll.Maximum)
                    {
                        newScroll = container.VerticalScroll.Maximum;
                    }
                    container.VerticalScroll.Value = newScroll;
                }
            }
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.IsMouseLeftPressed = true;
                this.MouseLeftDownPixelPos = e.Location;
                this.MouseLeftDownTilePos = GetTilePosFromPixelPos(e.Location);
                MapModule clickedMod = this.CurrentMapLayer.GetModuleAt(this.MouseLeftDownTilePos);

                switch (this.CurrentMode)
                {
                    case Mode.Select:
                        if (clickedMod != null && clickedMod.IsSelected)
                        {
                            // Started a click on a selected module - Start a move
                            this.CurrentMode = Mode.Move;
                        }
                        else
                        {
                            // Start the selection rectangle
                            this.SelectionRectangle = new Rectangle(this.MouseLeftDownTilePos, new Size(1, 1));
                        }
                        break;
                    case Mode.Draw:
                    case Mode.Paint:
                        if (this.IsAltPressed)
                        {
                            // Alt is pressed - Pipette tool
                            if (clickedMod != null)
                            {
                                MapModule mih = clickedMod.Clone();
                                mih.Position = this.MouseLeftDownTilePos;
                                mih.IsInHand = true;
                                this.ModulesInHand.Clear();
                                this.ModulesInHand.Add(mih);
                            }
                        }
                        else if (this.ModulesInHand.Any())
                        {
                            // Start drawing here
                            if (this.CurrentMapLayer.IsTileOccupied(this.MouseLeftDownTilePos))
                            {
                                // First tile clicked by user is occupied, treat this draw as an overwrite
                                this.CurrentMode = Mode.Paint;

                                // Delete the module that was here
                                this.RemoveModuleAt(this.MouseLeftDownTilePos);
                            }

                            // Add the modules in hand
                            this.ModulesInHand.ForEach(mod => this.CurrentMapLayer.Modules.Add(mod.Clone())
                            );
                        }

                        break;
                    case Mode.Erase:
                        // Erase this module
                        this.RemoveModuleAt(this.MouseLeftDownTilePos);
                        break;
                    default:
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.IsMouseRightPressed = true;
                this.MouseRightDownPixelPos = e.Location;
                this.MouseRightDownTilePos = GetTilePosFromPixelPos(e.Location);

                switch (this.CurrentMode)
                {
                    case Mode.Draw:
                    case Mode.Paint:
                        // Erase this module
                        this.RemoveModuleAt(this.MouseRightDownTilePos);
                        break;
                    default:
                        break;
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                this.IsMouseMiddlePressed = true;
                this.MouseMiddleDownPixelPos = Cursor.Position;
                this.MouseMiddleDownTilePos = GetTilePosFromPixelPos(e.Location);
                ScrollableControl container = ((ScrollableControl)this.Parent);

                // Need to store the position as positive values, even though the container's AutoScrollPosition returns negative values
                this.CanvasPanStartAutoScroll = new Point(Math.Abs(container.AutoScrollPosition.X), Math.Abs(container.AutoScrollPosition.Y));
            }
            this.Invalidate();
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.IsMouseLeftPressed = false;
                this.MouseLeftDownPixelPos = Point.Empty;

                switch (this.CurrentMode)
                {
                    case Mode.Paint:
                        // Fall back to "Draw" if we were painting
                        this.CurrentMode = Mode.Draw;
                        break;
                    case Mode.Select:
                        // Mark covered tiles as selected
                        this.CurrentMapLayer.Modules.ForEach(mod => mod.IsSelected = mod.Overlaps(this.SelectionRectangle));

                        // Clear the selection rectangle
                        ClearSelectionRectangle();
                        break;
                    case Mode.Move:
                        // Fall back to "Select"
                        this.CurrentMode = Mode.Select;
                        break;
                    default:
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.IsMouseRightPressed = false;
                this.MouseRightDownPixelPos = Point.Empty;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                this.IsMouseMiddlePressed = false;
                this.MouseMiddleDownPixelPos = Point.Empty;
            }
        }

        /// <summary>
        /// Handles mouse move event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseMiddlePressed)
            {
                // Middle mouse button is pressed - Treat this as a canvas pan
                Point delta = new Point(Cursor.Position.X - this.MouseMiddleDownPixelPos.X, Cursor.Position.Y - this.MouseMiddleDownPixelPos.Y);
                ScrollableControl container = ((ScrollableControl)this.Parent);
                container.AutoScrollPosition = new Point(this.CanvasPanStartAutoScroll.X - delta.X, this.CanvasPanStartAutoScroll.Y - delta.Y);
            }
            else
            {
                Point currMouseTilePos = GetTilePosFromPixelPos(e.Location);

                if (currMouseTilePos != this.MouseLastTilePos)
                {
                    switch (this.CurrentMode)
                    {
                        case Mode.Select:
                            if (this.IsMouseLeftPressed)
                            {
                                // Resize the selection rectangle
                                Point NW = new Point(Math.Min(currMouseTilePos.X, this.MouseLeftDownTilePos.X), Math.Min(currMouseTilePos.Y, this.MouseLeftDownTilePos.Y));
                                Point SE = new Point(Math.Max(currMouseTilePos.X, this.MouseLeftDownTilePos.X), Math.Max(currMouseTilePos.Y, this.MouseLeftDownTilePos.Y));
                                this.SelectionRectangle = new Rectangle(NW.X, NW.Y, (SE.X - NW.X) + 1, (SE.Y - NW.Y) + 1);
                            }
                            break;
                        case Mode.Draw:
                        case Mode.Paint:
                            if (this.ModulesInHand.Any())
                            {
                                // Get the top-left corner of the modules in hand
                                Point NWih = new Point(this.ModulesInHand.Min(mod => mod.X), this.ModulesInHand.Min(mod => mod.Y));
                                Point delta = new Point(currMouseTilePos.X - NWih.X, currMouseTilePos.Y - NWih.Y);

                                // Update the position of the modules in hand
                                this.ModulesInHand.ForEach(mod => mod.Position = new Point(mod.Position.X + delta.X, mod.Position.Y + delta.Y));

                                if (this.IsMouseLeftPressed)
                                {
                                    // Left mouse button is pressed - Drop the modules in hand here if there is room
                                    if (this.CurrentMode == Mode.Paint && this.CurrentMapLayer.IsTileOccupied(currMouseTilePos))
                                    {
                                        // There is a module here - Remove it
                                        this.RemoveModuleAt(currMouseTilePos);
                                    }

                                    // Check again if the tile is occupied before adding the module in hand
                                    if (!this.CurrentMapLayer.IsTileOccupied(currMouseTilePos))
                                    {
                                        // Drop the modules here
                                        this.AddModules(this.ModulesInHand);
                                    }
                                }
                            }

                            if (this.IsMouseRightPressed)
                            {
                                // Delete the module here
                                this.RemoveModuleAt(currMouseTilePos);
                            }
                            break;
                        case Mode.Erase:
                            // Remove "to be deleted" highlight from previous hover
                            this.CurrentMapLayer.Modules.ForEach(mod => mod.IsToBeDeleted = mod.ContainsTile(currMouseTilePos));

                            if (this.IsMouseLeftPressed)
                            {
                                // Left mouse button is pressed - Delete this module
                                this.RemoveModuleAt(currMouseTilePos);
                            }
                            break;
                        case Mode.Move:
                            // Move the selected modules
                            if (this.IsMouseLeftPressed)
                            {
                                if (this.CurrentMapLayer.SelectedModules.Any())
                                {
                                    // User is dragging selected modules around
                                    // Get the top-left corner of the selected modules
                                    //Point NWih = new Point(this.Map.SelectedModules.Min(mod => mod.X), this.Map.SelectedModules.Min(mod => mod.Y));
                                    Point delta = new Point(currMouseTilePos.X - this.MouseLastTilePos.X, currMouseTilePos.Y - this.MouseLastTilePos.Y);

                                    // Update the position of the selected modules
                                    this.CurrentMapLayer.SelectedModules.ForEach(mod => mod.Position = new Point(mod.Position.X + delta.X, mod.Position.Y + delta.Y));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                this.MouseLastTilePos = currMouseTilePos;
                this.Invalidate();
            }

        }
        #endregion

        #region Key Input Events
        /// <summary>
        /// Handles key down event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                // Ctrl key is pressed
                this.IsCtrlPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                // Shift key is pressed
                this.IsShiftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
            {
                // Alt key is pressed
                this.IsAltPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Delete selected modules
                this.RemoveSelectedModules();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Fall back to "Select" mode
                this.CurrentMode = Mode.Select;
            }
        }

        /// <summary>
        /// Handles key up event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                // Ctrl key was released
                this.IsCtrlPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                // Shift key was released
                this.IsShiftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
            {
                // Alt key was released
                this.IsAltPressed = false;
                e.Handled = true;
            }
        }
        #endregion

        #region Scroll Events
        protected override void OnScroll(ScrollEventArgs se)
        {
            this.InvalidateAll();
            base.OnScroll(se);
        }
        #endregion

        #region Draw Events
        /// <summary>
        /// Invalidates this MapCanvas in its entirety, triggering a full redraw on next paint.
        /// </summary>
        public void InvalidateAll()
        {
            this.Invalidate();
            this.CurrentMapLayer.Modules.ForEach(mod => mod.Invalidate());
            this.ModulesInHand.ForEach(mod => mod.Invalidate());
        }

        /// <summary>
        /// Handles painting this control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            ScrollableControl container = (ScrollableControl)this.Parent;
            if (container.AutoScrollMinSize != new Size(MaxMapSize * this.Zoom, MaxMapSize * this.Zoom))
            {
                container.AutoScrollMinSize = new Size(MaxMapSize * this.Zoom, MaxMapSize * this.Zoom);
            }

            // Draw the grid
            this.DrawGrid(e.Graphics, new Point(0, 0), MaxMapSize, MaxMapSize);

            // Draw each module in the map
            List<MapModule> modulesToPaint = new List<MapModule>();
            this.Map.Layers.ForEach(lay => modulesToPaint.AddRange(lay.Modules));

            // Draw modules in hand
            modulesToPaint.AddRange(this.ModulesInHand);

            // Paint the modules onto the canvas
            modulesToPaint.ForEach(mod => DrawModule(e.Graphics, mod));

            // Draw selection rectangle
            DrawSelectionRectangle(e.Graphics);
        }

        /// <summary>
        /// Draws this map's background grid.
        /// </summary>
        /// <param name="g">The graphics object on which to draw the grid.</param>
        private void DrawGrid(Graphics g, Point startPos, int w, int h)
        {
            // Draw the background first
            g.FillRectangle(new SolidBrush(this.GridBackgroundColor), Utils.TileRectangleToPixelRectangle(new Rectangle(startPos.X, startPos.Y, w, h), this.Zoom));

            // Draw the backing grid
            Pen gridPen = new Pen(this.GridMinorLineColor, this.GridMinorLineWidth);
            Pen gridMajorPen = new Pen(this.GridMajorLineColor, this.GridMajorLineWidth);

            w = Math.Min(w, MaxMapSize);
            h = Math.Min(h, MaxMapSize);

            int[] modulus = new int[] { this.GridGutterSize, 0 };
            int gridMajLineSum = this.GridGutterSize + this.GridRoomSize;
            if (gridMajLineSum == 0)
            {
                gridMajLineSum = 10000;
            }

            // Draw the vertical lines            
            for (int i = 0; i < w; i++)
            {
                // Use the appropriate pen based on minor/major grid lines
                Pen pen = modulus.Contains((startPos.X + i) % gridMajLineSum) ? gridMajorPen : gridPen;

                // Draw the vertical line
                g.DrawLine(pen, new Point((startPos.X + i) * Zoom, startPos.Y * Zoom), new Point((startPos.X + i) * Zoom, (startPos.Y + h) * Zoom));
            }

            // Draw the horizontal lines
            for (int j = 0; j < h; j++)
            {
                // Use the appropriate pen based on minor/major grid lines
                Pen pen = modulus.Contains((startPos.Y + j) % gridMajLineSum) ? gridMajorPen : gridPen;

                // Draw the horizontal line
                g.DrawLine(pen, new Point(startPos.X * Zoom, (startPos.Y + j) * Zoom), new Point((startPos.X + h) * Zoom, (startPos.Y + j) * Zoom));
            }
        }

        /// <summary>
        /// Draws the current selection rectangle.
        /// </summary>
        /// <param name="g">The graphics object on which to draw the selection rectangle.</param>
        private void DrawSelectionRectangle(Graphics g)
        {
            if (this.SelectionRectangle != Rectangle.Empty)
            {
                // There is a selection rectangle - Paint it onto the canvas
                g.FillRectangle(SelectionBrush, Utils.TileRectangleToPixelRectangle(this.SelectionRectangle, this.Zoom));
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
                mod.IsInvalidated = false;
                Point pixelPos = GetPixelPosFromTilePos(mod.Position);
                g.DrawImage(mod.Image, pixelPos.X, pixelPos.Y, mod.Width * this.Zoom, mod.Height * this.Zoom);

                // Draw the appropriate highlight for this module
                if (mod.IsSelected)
                {
                    g.FillRectangle(SelectionBrush, Utils.TileRectangleToPixelRectangle(mod.OccupiedRectangle, this.Zoom));
                }
                if (mod.IsToBeDeleted)
                {
                    g.FillRectangle(EraserBrush, Utils.TileRectangleToPixelRectangle(mod.OccupiedRectangle, this.Zoom));
                }
                if (mod.IsInHand)
                {
                    g.FillRectangle(ModuleInHandBrush, Utils.TileRectangleToPixelRectangle(mod.OccupiedRectangle, this.Zoom));
                }
            }
        }
        #endregion

        #region Zoom
        /// <summary>
        /// Zooms the map in.
        /// </summary>
        public void ZoomIn()
        {
            this.Zoom += 10;
        }

        /// <summary>
        /// Zooms the map out.
        /// </summary>
        public void ZoomOut()
        {
            this.Zoom -= 10;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns the tile position for the specified pixel position (useful for mouse/click events).
        /// </summary>
        /// <param name="pixelPos">The pixel position to convert to tile coordinates.</param>
        /// <returns></returns>
        private Point GetTilePosFromPixelPos(Point pixelPos)
        {
            return new Point(pixelPos.X / this.Zoom, pixelPos.Y / this.Zoom);
        }

        /// <summary>
        /// Returns the pixel position for the specified tile position.
        /// </summary>
        /// <param name="tilePos">The tile position to convert to pixel coordinates.</param>
        /// <returns></returns>
        private Point GetPixelPosFromTilePos(Point tilePos)
        {
            return new Point(tilePos.X * this.Zoom, tilePos.Y * this.Zoom);
        }
        #endregion

        #region MapModule Manipulation
        /// <summary>
        /// Clears the modules in hand.
        /// </summary>
        public void ClearModulesInHand()
        {
            if (this.ModulesInHand != null)
            {
                this.ModulesInHand.ForEach(mod => RemoveModule(mod));
            }
            this.ModulesInHand.Clear();
        }

        /// <summary>
        /// Clears the selection rectangle;
        /// </summary>
        public void ClearSelectionRectangle()
        {
            this.SelectionRectangle = Rectangle.Empty;
            this.Invalidate();
        }

        /// <summary>
        /// Rotates the selection or the module "in hand" clockwise and redraws.
        /// </summary>
        public void RotateCW()
        {
            List<MapModule> modulesToRotate = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.CurrentMapLayer.SelectedModules;

            if (modulesToRotate.Any())
            {
                Point selNW = Point.Empty;
                Point selSE = Point.Empty;

                selNW.X = modulesToRotate.Min(m => m.NW.X);
                selNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selSE.X = modulesToRotate.Max(m => m.SE.X);
                selSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selRect = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                // Get the pivot/rectangle center
                PointF pivot = new Point(selRect.X + selRect.Width / 2, selRect.Y + selRect.Height / 2);

                // Rotate the modules clockwise
                double cosTheta = Math.Cos(Math.PI / 2);
                double sinTheta = Math.Sin(Math.PI / 2);

                foreach (MapModule mod in modulesToRotate)
                {
                    // Rotating clockwise - Bottom-left/SW corner of rectangle will become new top-left/NW
                    Point newNW = mod.SW;
                    double x = (cosTheta * (newNW.X - pivot.X) - sinTheta * (newNW.Y - pivot.Y) + pivot.X);
                    double y = (sinTheta * (newNW.X - pivot.X) - cosTheta * (newNW.Y - pivot.Y) + pivot.X);
                    mod.Position = new Point((int)Math.Round(x), (int)Math.Round(y));
                    mod.RotateCW();
                }

                // Reset positions
                Point selNewNW = Point.Empty;
                Point selNewSE = Point.Empty;

                selNewNW.X = modulesToRotate.Min(m => m.NW.X);
                selNewNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selNewSE.X = modulesToRotate.Max(m => m.SE.X);
                selNewSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selNewRect = new Rectangle(selNewNW, new Size(selNewSE.X - selNewNW.X, selNewSE.Y - selNewNW.Y));
                Point delta = new Point(selNewRect.X - selRect.X, selNewRect.Y - selRect.Y);
                modulesToRotate.ForEach(mod => mod.Position = new Point(mod.X - delta.X, mod.Y - delta.Y));

                // Redraw the canvas
                this.Invalidate();
            }
        }

        /// <summary>
        /// Rotates the selection or the module "in hand" counter clockwise and redraws.
        /// </summary>
        public void RotateCCW()
        {
            List<MapModule> modulesToRotate = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.CurrentMapLayer.SelectedModules;

            if (modulesToRotate.Any())
            {
                Point selNW = Point.Empty;
                Point selSE = Point.Empty;

                selNW.X = modulesToRotate.Min(m => m.NW.X);
                selNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selSE.X = modulesToRotate.Max(m => m.SE.X);
                selSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selRect = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                // Get the pivot/rectangle center
                PointF pivot = new Point(selRect.X + selRect.Width / 2, selRect.Y + selRect.Height / 2);

                // Rotate the modules clockwise
                double cosTheta = Math.Cos(-Math.PI / 2);
                double sinTheta = Math.Sin(-Math.PI / 2);

                foreach (MapModule mod in modulesToRotate)
                {
                    // Rotating counter-clockwise - top-right/NE corner of rectangle will become new top-left/NW
                    Point newNW = mod.NE;
                    double x = (cosTheta * (newNW.X - pivot.X) - sinTheta * (newNW.Y - pivot.Y) + pivot.X);
                    double y = (sinTheta * (newNW.X - pivot.X) - cosTheta * (newNW.Y - pivot.Y) + pivot.X);
                    mod.Position = new Point((int)Math.Round(x), (int)Math.Round(y));
                    mod.RotateCCW();
                }

                // Reset positions
                Point selNewNW = Point.Empty;
                Point selNewSE = Point.Empty;

                selNewNW.X = modulesToRotate.Min(m => m.NW.X);
                selNewNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selNewSE.X = modulesToRotate.Max(m => m.SE.X);
                selNewSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selNewRect = new Rectangle(selNewNW, new Size(selNewSE.X - selNewNW.X, selNewSE.Y - selNewNW.Y));
                Point delta = new Point(selNewRect.X - selRect.X, selNewRect.Y - selRect.Y);
                modulesToRotate.ForEach(mod => mod.Position = new Point(mod.X - delta.X, mod.Y - delta.Y));

                // Redraw the canvas
                this.Invalidate();
            }
        }

        /// <summary>
        /// Flips the selected MapModules OR the MapModules in hand vertically.
        /// </summary>
        public void MirrorHorizontal()
        {
            List<MapModule> modulesToMirror = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.CurrentMapLayer.SelectedModules;

            // Flip the selected modules
            if (modulesToMirror.Any())
            {
                foreach (MapModule mod in modulesToMirror)
                {
                    // Flip the mod itself
                    mod.MirrorHorizontal();
                }

                this.Invalidate();
            }
        }

        /// <summary>
        /// Flips the selected MapModules OR the MapModules in hand horizontally.
        /// </summary>
        public void MirrorVertical()
        {
            List<MapModule> modulesToMirror = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.CurrentMapLayer.SelectedModules;

            // Flip the selected modules
            if (modulesToMirror.Any())
            {
                foreach (MapModule mod in modulesToMirror)
                {
                    // Flip the mod itself
                    mod.MirrorVertical();
                }

                this.Invalidate();
            }
        }
        #endregion

        #region Map Manipulation
        /// <summary>
        /// Adds the specified MapModules to the map.
        /// </summary>
        /// <param name="mods"></param>
        public void AddModules(List<MapModule> mods)
        {
            mods.ForEach(mod => AddModule(mod));
        }

        /// <summary>
        /// Adds the specified MapModule to the map.
        /// </summary>
        /// <param name="mod"></param>
        public void AddModule(MapModule mod)
        {
            if (mod != null)
            {
                this.CurrentMapLayer.Modules.Add(mod.Clone());
                mod.Invalidate();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Removes the specified MapModule from the map.
        /// </summary>
        /// <param name="mod"></param>
        public void RemoveModule(MapModule mod)
        {
            if (mod != null && this.CurrentMapLayer.Modules.Contains(mod))
            {
                this.CurrentMapLayer.Modules.Remove(mod);
                this.Invalidate();
            }
        }

        /// <summary>
        /// Removes the module at the specified position (in tile coordinates).
        /// </summary>
        /// <param name="pos"></param>
        public void RemoveModuleAt(Point pos)
        {
            MapModule mod = this.CurrentMapLayer.GetModuleAt(pos);
            if (mod != null)
            {
                RemoveModule(mod);
            }
        }

        /// <summary>
        /// Removes all selected MapModules from the map.
        /// </summary>
        public void RemoveSelectedModules()
        {
            this.CurrentMapLayer.SelectedModules.ForEach(mod => RemoveModule(mod));
            this.Invalidate();
        }

        /// <summary>
        /// Loads the specified Map file into this MapCanvas.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadMapFile(string fileName)
        {
            this.Map = JsonConvert.DeserializeObject<Map>(System.IO.File.ReadAllText(fileName));
        }

        /// <summary>
        /// Writes this MapCanvas' map to the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveMapFile(string fileName)
        {
            System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(this.Map));
        }

        /// <summary>
        /// Resets this MapCanvas and starts a new map from scratch.
        /// </summary>
        public void Clear()
        {
            this.Map.Layers.ForEach(lay => lay.Modules.Clear());
            this.Map.Layers.Clear();
            this.Map.Layers.Add(new MapLayer());
            this.ModulesInHand.Clear();
            this.Invalidate();
        }
        #endregion
    }
}
