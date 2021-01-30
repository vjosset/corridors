using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        #region Properties and Fields
        /// <summary>
        /// The user's preferences/settings.
        /// </summary>
        private Preferences Preferences { get; set; } = new Preferences();

        private string mapFileName { get; set; } = "";
        private bool unsavedChanges { get; set; } = false;

        private StringBuilder _log = new StringBuilder();

        /// <summary>
        /// Represents the list of previously-occupied rectangles to be removed from the canvas on redraw.
        /// On each Paint event, these rectangles are replaced with grid and removed from the list.
        /// </summary>
        private List<Rectangle> RemovedModules { get; set; } = new List<Rectangle>();

        private string LogPath
        {
            get
            {
                return Utils.GetExecutableFolder() + "\\log.txt";
            }
        }

        /// <summary>
        /// The maximum width and height of the map, in tiles.
        /// </summary>
        private const int MaxMapSize = 100;

        public static SolidBrush SelectionBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 0, 255));
        public static SolidBrush EraserBrush { get; } = new SolidBrush(Color.FromArgb(95, 255, 0, 0));
        public static SolidBrush ModuleInHandBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 255, 0));
        public static Pen WallPen1 { get; } = new Pen(Color.Black, 2);
        public static Pen WallPen2 { get; } = new Pen(Color.FromArgb(64, 0, 0, 0), 5);

        /// <summary>
        /// Represents the current selection rectangle on the map, in Tile coordinates.
        /// </summary>
        private Rectangle SelectionRectangle { get; set; } = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// Stores whether the Ctrl key is currently pressed.
        /// </summary>
        private bool ctrlPressed = false;

        /// <summary>
        /// Stores whether the Shift key is currently pressed.
        /// </summary>
        private bool shiftPressed = false;

        /// <summary>
        /// Stores whether the Alt key is currently pressed.
        /// </summary>
        private bool altPressed = false;

        /// <summary>
        /// The current map.
        /// </summary>
        private Map Map { get; set; } = new Map();

        private Point MouseDownPixelPos { get; set; } = new Point(-1, -1);

        /// <summary>
        /// Gets or sets the tile position that received a MouseDown event.
        /// </summary>
        private Point MouseDownTilePos { get; set; } = new Point(-1, -1);

        /// <summary>
        /// Stores the last position (in tile coordinates) of the mouse cursor.
        /// </summary>
        private Point MouseLastTilePos = new Point(-1, -1);

        /// <summary>
        /// Stores the last position (in pixel coordinates) of the mouse cursor.
        /// </summary>
        private Point MouseLastPixelPos = new Point(-1, -1);

        /// <summary>
        /// Holds the module "in hand" for the user to place onto the map.
        /// </summary>
        private List<MapModule> ModulesInHand { get; set; } = null;

        private int _zoom = 60;
        /// <summary>
        /// Gets or sets the global drawing scale (pixels per tile).
        /// </summary>
        public int Zoom
        {
            get
            {
                return this._zoom;
            }
            set
            {
                // Update the scale of all map modules
                if (value != this._zoom)
                {
                    // Pause drawing updates on the canvas (and its controls/modules)
                    this.Canvas.SuspendLayout();

                    // Set the zoom factor on map modules
                    this.Map.Modules.ForEach(mod => mod.Zoom = value);

                    // Set the zoom factor on the module in hand
                    if (this.ModulesInHand != null)
                    {
                        this.ModulesInHand.ForEach(mod => mod.Zoom = value);
                    }

                    WallPen1.Width = Math.Max(value / 20, 2);
                    WallPen2.Width = Math.Max(value / 10, 2);

                    // Set the main zoom factor
                    this._zoom = value;

                    // Redraw the canvas
                    this.Canvas.Invalidate(false);

                    // Result drawing updates on the canvas and its controls/modules
                    this.Canvas.ResumeLayout();
                }
            }
        }

        private MouseMode _cursorMode = MouseMode.Select;
        /// <summary>
        /// Gets or sets the current mouse/cursor mode.
        /// </summary>
        private MouseMode CursorMode
        {
            get
            {
                return _cursorMode;
            }
            set
            {
                SetStatus("Setting mode " + value.ToString());
                _cursorMode = value;

                btnModeEraser.Checked = value == MouseMode.Erase;
                btnModeSelect.Checked = value == MouseMode.Select || value == MouseMode.Move;
                btnModeDraw.Checked = value == MouseMode.Draw;

                switch (value)
                {
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        this.Canvas.Cursor = Cursors.Cross;
                        break;
                    case MouseMode.Erase:
                        this.Canvas.Cursor = Cursors.No;
                        this.ModulesInHand?.ForEach(mod => DeleteModule(mod));
                        break;
                    case MouseMode.Move:
                        this.Canvas.Cursor = Cursors.SizeAll;
                        this.ModulesInHand?.ForEach(mod => DeleteModule(mod));
                        break;
                    case MouseMode.Select:
                        this.Canvas.Cursor = Cursors.Default;
                        this.ModulesInHand?.ForEach(mod => DeleteModule(mod));
                        break;
                    default:
                        this.Canvas.Cursor = Cursors.Default;
                        this.ModulesInHand?.ForEach(mod => DeleteModule(mod));
                        break;
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Rotates the selection or the module "in hand" clockwise and redraws.
        /// </summary>
        private void RotateCW()
        {
            List<MapModule> modulesToRotate = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.Map.SelectedModules;

            if (modulesToRotate.Any())
            {
                // Pause drawing updates on the canvas (and its controls/modules)
                this.Canvas.SuspendLayout();
                modulesToRotate.ForEach(mod => mod.PictureBox?.SuspendLayout());

                Point selNW = Point.Empty;
                Point selSE = Point.Empty;

                selNW.X = modulesToRotate.Min(m => m.NW.X);
                selNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selSE.X = modulesToRotate.Max(m => m.SE.X);
                selSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selRect = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                // Build a 2D array of the selected modules
                MapModule[,] matrix = new MapModule[selRect.Height, selRect.Width];
                modulesToRotate.ForEach(mod => matrix[mod.Y - selNW.Y, mod.X - selNW.X] = mod);

                // Now rotate the matrix
                matrix = Utils.RotateMatrixCW(matrix, selRect.Location);

                // Redraw rotated map
                this.unsavedChanges = true;

                // Resume drawing updates on the canvas (and its controls/modules)
                this.Canvas.ResumeLayout();
                modulesToRotate.ForEach(mod => mod.PictureBox?.ResumeLayout());
            }
        }

        /// <summary>
        /// Rotates the selection or the module "in hand" counter clockwise and redraws.
        /// </summary>
        private void RotateCCW()
        {
            List<MapModule> modulesToRotate = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.Map.SelectedModules;

            if (modulesToRotate.Any())
            {
                // Pause drawing updates on the canvas (and its controls/modules)
                this.Canvas.SuspendLayout();
                modulesToRotate.ForEach(mod => mod.PictureBox?.SuspendLayout());

                Point selNW = Point.Empty;
                Point selSE = Point.Empty;

                selNW.X = modulesToRotate.Min(m => m.NW.X);
                selNW.Y = modulesToRotate.Min(m => m.NW.Y);
                selSE.X = modulesToRotate.Max(m => m.SE.X);
                selSE.Y = modulesToRotate.Max(m => m.SE.Y);

                Rectangle selRect = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                // Build a 2D array of the selected modules
                MapModule[,] matrix = new MapModule[selRect.Height, selRect.Width];
                modulesToRotate.ForEach(mod => matrix[mod.Y - selNW.Y, mod.X - selNW.X] = mod);

                // Now rotate the matrix
                matrix = Utils.RotateMatrixCCW(matrix, selRect.Location);

                // Redraw rotated map
                this.unsavedChanges = true;

                // Resume drawing updates on the canvas (and its controls/modules)
                this.Canvas.ResumeLayout();
                this.Map.SelectedModules.ForEach(mod => mod.PictureBox?.ResumeLayout());
            }
        }

        private void MirrorHorizontal()
        {
            List<MapModule> modulesToMirror = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.Map.SelectedModules;

            // Flip the selected modules
            if (modulesToMirror.Any())
            {
                foreach (MapModule mod in modulesToMirror)
                {
                    // Flip the mod itself
                    mod.MirrorHorizontal();

                    // Update the module's position in the group
                    //[TBD]
                }
            }
        }

        private void MirrorVertical()
        {
            List<MapModule> modulesToMirror = this.ModulesInHand?.Count > 0 ? this.ModulesInHand : this.Map.SelectedModules;

            // Flip the selected modules
            if (modulesToMirror.Any())
            {
                foreach (MapModule mod in modulesToMirror)
                {
                    // Flip the mod itself
                    mod.MirrorVertical();

                    // Update the module's position in the group
                    //[TBD]
                }
            }
        }

        private void Log(string message)
        {
            //System.IO.File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "   " + message + "\r\n");
        }

        /// <summary>
        /// Writes the status message to the status label.
        /// </summary>
        /// <param name="message"></param>
        private void SetStatus(string message, bool writeToLog = true)
        {
            lblStatus.Text = message;
        }

        /// <summary>
        /// Represents the cursor edit mode (move, draw, select, etc.).
        /// </summary>
        private enum MouseMode
        {
            Select,
            Move,
            Erase,
            Draw,
            Paint,
        }

        private bool LeftMouseButtonIsPressed { get; set; } = false;
        private bool RightMouseButtonIsPressed { get; set; } = false;
        private bool MiddleMouseButtonIsPressed { get; set; } = false;

        /// <summary>
        /// Deletes all selected modules and redraws the map.
        /// </summary>
        private void DeleteSelectedModules()
        {
            for (int i = this.Map.Modules.Count - 1; i >= 0; i--)
            {
                if (this.Map.Modules[i].IsSelected)
                {
                    // This module was selected - Delete it
                    MapModule mod = this.Map.Modules[i];
                    DeleteModule(mod);
                }
            }

            // Redraw the map
            this.unsavedChanges = true;
        }

        private void DeleteModule(MapModule mod)
        {
            if (mod != null)
            {
                if (this.Map.Modules.Contains(mod))
                {
                    this.Map.Modules.Remove(mod);
                }

                // Remove this module's PictureBox from the canvas
                if (mod.PictureBox != null && this.Canvas.Controls.Contains(mod.PictureBox))
                {
                    this.Canvas.Controls.Remove(mod.PictureBox);
                }

                // Invalidate the adjancent modules so they can redraw their walls
                mod.InvalidateAdjacentModules();

                mod = null;

                this.unsavedChanges = true;
            }
        }
        #endregion

        #region Form Events
        /// <summary>
        /// Event handler for showing the form. Loads preferences and modules.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void frmMain_Shown(object sender, EventArgs e)
        {
            // Load the preferences/settings
            SetStatus("Loading preferences...");
            if (System.IO.File.Exists(Preferences.PreferenceFilePath))
            {
                // Preference file exists - Let's load it up
                this.Preferences = JsonConvert.DeserializeObject<Preferences>(System.IO.File.ReadAllText(Preferences.PreferenceFilePath));
            }
            else
            {
                // Preference file does not exist - Prepare a new set of preferences
                this.Preferences = new Preferences();

                // Save the default preferences
                this.Preferences.SavePreferences();
            }
            SetStatus("Done loading preferences");

            // Load the module packs
            SetStatus("Loading module packs...");
            this.LoadModulePacks();

            // Draw the initial grid
            this.Canvas.Invalidate();
            SetStatus("Ready");
        }

        public void ClearModulesInHand()
        {
            if (this.ModulesInHand != null)
            {
                this.ModulesInHand.ForEach(mod => DeleteModule(mod));
            }
            lsvModules.SelectedItems.Clear();
            this.ModulesInHand.Clear();
        }

        /// <summary>
        /// Event handler for key down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Deselect
                this.CursorMode = MouseMode.Select;
                lsvModules.SelectedItems.Clear();

                ClearModulesInHand();
                //Console.WriteLine("---------------ESC--------------");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Delete the selected modules
                DeleteSelectedModules();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                // Ctrl key is pressed
                this.ctrlPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                // Shift key is pressed
                this.shiftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
            {
                // Alt key is pressed
                this.altPressed = true;
                e.Handled = true;
            }
        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                // Ctrl key is no longer pressed
                this.ctrlPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                // Shift key is longer pressed
                this.shiftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
            {
                // Alt key is longer pressed
                this.altPressed = false;
                e.Handled = true;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Event handler for user selection from the list of modules.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lsvModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected module
            if (lsvModules.SelectedIndices.Count > 0)
            {
                // We have a selection - Get the module (in the selected item's tag)
                Module mod = (Module)lsvModules.SelectedItems[0].Tag;

                // Get the old position of the module in hand
                if (this.ModulesInHand != null)
                {
                    // We already had a module in hand, clear out the old one before building the new one
                    this.ModulesInHand.ForEach(m => DeleteModule(m));
                }

                ModulesInHand = new List<MapModule>();
                MapModule modIH = new MapModule(mod, this.Map, this.Zoom);
                modIH.Position = this.MouseLastTilePos;
                modIH.IsInHand = true;
                this.ModulesInHand.Add(modIH);
                this.ModulesInHand.ForEach(m => m.CreatePictureBox(this.Canvas));
                if (this.CursorMode != MouseMode.Draw && this.CursorMode != MouseMode.Paint)
                {
                    this.CursorMode = MouseMode.Draw;
                }
            }
            else
            {
                // No selection
                ClearModulesInHand();
                this.CursorMode = MouseMode.Select;
            }
        }
        #endregion

        #region Canvas Events

        /// <summary>
        /// Event handler for canvas paint event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>
        private void Canvas_Paint(object sender, System.Windows.Forms.PaintEventArgs pe)
        {
            this.SuspendLayout();
            // Make sure the canvas is large enough
            this.Canvas.AutoScrollMinSize = new Size(MaxMapSize * Zoom, MaxMapSize * Zoom);

            // Set the offset for drawing (to match scroll position)
            pe.Graphics.TranslateTransform(Canvas.AutoScrollPosition.X, Canvas.AutoScrollPosition.Y);

            // Set the smoothing mode for better performance
            pe.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            // Clear the canvas
            pe.Graphics.Clear(this.Preferences.GridBackgroundColor);

            // Draw the grid
            Rectangle visible = GetVisibleRectangle();
            DrawGrid(pe.Graphics, visible.Location, visible.Width, visible.Height);

            // Draw the selection rectangle
            DrawSelectionRectangle(pe.Graphics);
            this.ResumeLayout();
        }

        /// <summary>
        /// Event handler for mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.LeftMouseButtonIsPressed = true;

                // Get the click position
                Point mousePos = new Point(e.Location.X, e.Location.Y);

                this.MouseDownPixelPos = mousePos;

                // Offset the position based on the scroll offset
                mousePos.X -= Canvas.AutoScrollPosition.X;
                mousePos.Y -= Canvas.AutoScrollPosition.Y;

                // Store the tile coordinates for the tile that received the MouseDown event
                this.MouseDownTilePos = new Point(mousePos.X / Zoom, mousePos.Y / Zoom);

                if (altPressed)
                {
                    // If alt if pressed, we are in thermometer/pipette mode
                    MapModule mod = this.Map.GetModuleAt(this.MouseDownTilePos);
                    if (mod != null)
                    {
                        foreach (ListViewItem item in lsvModules.Items)
                        {
                            item.Selected = item.Tag == mod.Module;
                        }
                    }
                }
                else
                {
                    switch (this.CursorMode)
                    {
                        case MouseMode.Draw:
                        case MouseMode.Paint:
                            if (this.ModulesInHand != null)
                            {
                                // User has a tile selected from the module selection, wants to place it somewhere
                                List<MapModule> tilesOccupied = this.Map.Modules.Where(mod => this.ModulesInHand.Any(ih => ih.Overlaps(mod.OccupiedRectangle))).ToList();

                                // If there is a tile here, assume user wants to overwrite - Set to Paint mode
                                if (tilesOccupied.Any())
                                {
                                    this.CursorMode = MouseMode.Paint;

                                    // We're painting, and there is a module here - Remove it
                                    List<MapModule> modulesToRemove = tilesOccupied;
                                    modulesToRemove.ForEach(mod => DeleteModule(mod));
                                }

                                // Recheck for occupied tiles
                                tilesOccupied = this.Map.Modules.Where(mod => this.ModulesInHand.Any(ih => ih.Overlaps(mod.OccupiedRectangle))).ToList();

                                // Check if the clicked square is occupied and draw the module if there is room
                                if (!tilesOccupied.Any())
                                {
                                    foreach (MapModule mih in this.ModulesInHand)
                                    {
                                        MapModule mod = mih.Clone();
                                        this.Map.Modules.Add(mod);
                                        mod.CreatePictureBox(this.Canvas);
                                        mod.IsInHand = false;
                                        mod.InvalidateAdjacentModules();
                                    }

                                    this.unsavedChanges = true;
                                }
                            }
                            break;
                        case MouseMode.Select:// Check if the clicked tile was occupied, meaning this is a "Move" operation
                            foreach (MapModule mod in this.Map.SelectedModules)
                            {
                                if (mod.ContainsTile(this.MouseDownTilePos))
                                {
                                    // User clicked on an occupied tile - Start moving
                                    this.CursorMode = MouseMode.Move;
                                    this.MouseLastTilePos = this.MouseDownTilePos;
                                    break;
                                }
                            }
                            break;
                        case MouseMode.Erase:
                            // Button is pressed - Let's delete the module at this spot
                            DeleteModule(this.Map.GetModuleAt(this.MouseDownTilePos));
                            break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Middle mouse means "pan canvas"
                this.MiddleMouseButtonIsPressed = true;

                // Get the click position
                Point mousePos = new Point(e.Location.X, e.Location.Y);

                this.MouseDownPixelPos = mousePos;
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Event handler for mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            // Get the click position
            Point mousePos = new Point(e.Location.X, e.Location.Y);

            // Offset the position based on the scroll offset
            mousePos.X -= Canvas.AutoScrollPosition.X;
            mousePos.Y -= Canvas.AutoScrollPosition.Y;

            // Get the tile coordinates
            Point mouseUpTilePos = new Point(mousePos.X / Zoom, mousePos.Y / Zoom);

            if (e.Button == MouseButtons.Left)
            {
                switch (this.CursorMode)
                {
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        // Just switch back to Draw mode (instead of Paint)
                        this.CursorMode = MouseMode.Draw;
                        break;
                    case MouseMode.Select:
                        // Get the selected area
                        Point NW = new Point(Math.Min(mouseUpTilePos.X, this.MouseDownTilePos.X), Math.Min(mouseUpTilePos.Y, this.MouseDownTilePos.Y));
                        Point SE = new Point(Math.Max(mouseUpTilePos.X, this.MouseDownTilePos.X) + 1, Math.Max(mouseUpTilePos.Y, this.MouseDownTilePos.Y) + 1);
                        this.SelectionRectangle = new Rectangle(NW.X, NW.Y, SE.X - NW.X, SE.Y - NW.Y);

                        // Find all modules in the specified rectangle
                        foreach (MapModule mod in Map.Modules)
                        {
                            if (!this.ctrlPressed)
                            {
                                // Ctrl key is not pressed - this is a brand-new selection
                                // If Ctrl key is pressed, we're adding to existing selection
                                mod.IsSelected = false;
                            }
                            if (mod.Overlaps(this.SelectionRectangle))
                            {
                                mod.IsSelected = !mod.IsSelected;
                            }
                        }

                        // Clear the selection rectangle (mouse up)
                        this.SelectionRectangle = new Rectangle(0, 0, 0, 0);

                        // Redraw the map
                        this.Canvas.Invalidate(false);
                        break;
                    case MouseMode.Move:
                        // User lifted the mouse button after a move, go back to select mode
                        this.CursorMode = MouseMode.Select;
                        break;
                    default:
                        break;
                }
                this.MouseDownTilePos = new Point(-1, -1);
                this.LeftMouseButtonIsPressed = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right-click on a tile -> Delete that tile
                if (this.Map.IsTileOccupied(mouseUpTilePos))
                {
                    DeleteModule(this.Map.GetModuleAt(mouseUpTilePos));
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Middle mouse means "pan canvas"
                this.MiddleMouseButtonIsPressed = false;
            }
        }

        /// <summary>
        /// Event handler for canvas mouse over (show the selected module in its expected position).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the mouse position
            Point mousePos = e.Location;

            // Get the position in pixels on the map
            mousePos.X -= Canvas.AutoScrollPosition.X;
            mousePos.Y -= Canvas.AutoScrollPosition.Y;

            // Get the tile coordinates on the map (based on the pixel position on the map)
            Point mouseTilePos = new Point(mousePos.X / Zoom, mousePos.Y / Zoom);

            SetStatus("Cursor Position: " + mouseTilePos.X.ToString() + "/" + mouseTilePos.Y.ToString());

            // We're in "draw" mode - Show the module "in hand"
            if ((this.CursorMode == MouseMode.Paint || this.CursorMode == MouseMode.Draw) && this.ModulesInHand != null && this.ModulesInHand.Any())
            {
                // Transpose the tiles in hand
                Point NW = new Point(this.ModulesInHand.Min(mod => mod.X), this.ModulesInHand.Min(mod => mod.Y));
                Point delta = new Point(mouseTilePos.X - NW.X, mouseTilePos.Y - NW.Y);
                this.ModulesInHand.ForEach(mod => mod.Position = new Point(mod.Position.X + delta.X, mod.Position.Y + delta.Y));
            }

            if (this.LeftMouseButtonIsPressed)
            {
                if (mouseTilePos != this.MouseLastTilePos)
                {
                    switch (this.CursorMode)
                    {
                        case MouseMode.Select:
                            // We're in "select" mode - Draw the selection rectangle (if the current tile hasn't changed)
                            if (this.LeftMouseButtonIsPressed)
                            {
                                // Update the selection rectangle
                                Point NW = new Point(Math.Min(mouseTilePos.X, this.MouseDownTilePos.X), Math.Min(mouseTilePos.Y, this.MouseDownTilePos.Y));
                                Point SE = new Point(Math.Max(mouseTilePos.X, this.MouseDownTilePos.X) + 1, Math.Max(mouseTilePos.Y, this.MouseDownTilePos.Y) + 1);

                                this.SelectionRectangle = new Rectangle(NW.X, NW.Y, SE.X - NW.X, SE.Y - NW.Y);
                                this.Canvas.Invalidate(false);
                            }
                            break;
                        case MouseMode.Draw:
                        case MouseMode.Paint:
                            // We're in "draw" mode - Show the module "in hand"
                            if (this.ModulesInHand != null && this.ModulesInHand.Any())
                            {
                                // User has a tile selected from the module selection, wants to place it somewhere
                                List<MapModule> tilesOccupied = this.Map.Modules.Where(mod => this.ModulesInHand.Any(ih => ih.Overlaps(mod.OccupiedRectangle))).ToList();

                                // If there is a tile here and we're in paint mode, remove it before adding the modules in hand to the map
                                if (tilesOccupied.Any() && this.CursorMode == MouseMode.Paint)
                                {
                                    // We're painting, and there is a module here - Remove it
                                    List<MapModule> modulesToRemove = tilesOccupied;
                                    modulesToRemove.ForEach(mod => DeleteModule(mod));
                                }

                                // Recheck for occupied tiles
                                tilesOccupied = this.Map.Modules.Where(mod => this.ModulesInHand.Any(ih => ih.Overlaps(mod.OccupiedRectangle))).ToList();

                                if (!tilesOccupied.Any())
                                {
                                    // User is holding mouse button and moving - Paint this module right here
                                    foreach (MapModule mih in this.ModulesInHand)
                                    {
                                        MapModule mod = mih.Clone();
                                        this.Map.Modules.Add(mod);
                                        mod.CreatePictureBox(this.Canvas);
                                        mod.IsInHand = false;
                                        mod.InvalidateAdjacentModules();
                                    }
                                    this.unsavedChanges = true;
                                }
                            }
                            break;
                        case MouseMode.Move:
                            // Move the selection
                            if (this.LeftMouseButtonIsPressed)
                            {
                                int deltaX = mouseTilePos.X - this.MouseLastTilePos.X;
                                int deltaY = mouseTilePos.Y - this.MouseLastTilePos.Y;
                                foreach (MapModule mod in this.Map.SelectedModules)
                                {
                                    // Move this module
                                    Point modPos = mod.Position;
                                    modPos.X += deltaX;
                                    modPos.Y += deltaY;
                                    mod.Position = modPos;
                                }
                                this.unsavedChanges = true;
                            }
                            break;
                        case MouseMode.Erase:
                            // We're in "erase" mode
                            if (this.LeftMouseButtonIsPressed)
                            {
                                // Button is pressed - Let's delete the module at this spot
                                MapModule mod = this.Map.GetModuleAt(mouseTilePos);
                                if (mod != null)
                                {
                                    DeleteModule(mod);
                                }
                            }
                            else
                            {
                                // Mouse button is not pressed, highlight the module that will be erased
                                MapModule mod = this.Map.GetModuleAt(mouseTilePos);
                                if (mod != null)
                                {
                                    // There is a module under the cursor - Mark it as to be deleted
                                    mod.IsToBeDeleted = true;
                                }

                                // Check if there was a different module under the previous cursor position
                                MapModule mod2 = this.Map.GetModuleAt(this.MouseLastTilePos);
                                if (mod2 != null && mod2 != mod)
                                {
                                    // A different module was previously marked as "to be deleted"; clear that setting
                                    mod2.IsToBeDeleted = false;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (this.MiddleMouseButtonIsPressed)
            {
                // Middle mouse button means "pan canvas"
                this.Canvas.AutoScrollPosition = new Point(-this.Canvas.AutoScrollPosition.X - e.X + MouseDownPixelPos.X, -this.Canvas.AutoScrollPosition.Y - e.Y + MouseDownPixelPos.Y);
            }

            // Store the last tile and pixel positions to reduce redraws
            this.MouseLastTilePos = mouseTilePos;
        }

        #endregion

        #region Menu Items
        /// <summary>
        /// Event handler for "Preferences" menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuMainFilePreferences_Click(object sender, EventArgs e)
        {
            // Show the preferences dialog/form
            frmPreferences prefsDialog = new frmPreferences();
            if (prefsDialog.ShowDialog() == DialogResult.OK)
            {
                // User wants to save their preferences
                this.Preferences = prefsDialog.Preferences;

                // Save the preferences
                this.Preferences.SavePreferences();

                // Redraw
                this.Canvas.Invalidate();
            }
        }
        #endregion

        #region Toolbar Items
        /// <summary>
        /// Clears the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all modules and start over?", "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                this.Map.Modules.ForEach(mod => mod.IsSelected = true);
                DeleteSelectedModules();
                this.Canvas.Invalidate();
            }
        }

        /// <summary>
        /// Event handler for Zoom In button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (this.Zoom < 200)
            {
                this.Zoom += 10;
                if (this.Zoom > 200)
                {
                    this.Zoom = 200;
                }
            }
        }

        /// <summary>
        /// Event handler for Zoom Out button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (this.Zoom > 10)
            {
                this.Zoom -= 10;
                if (this.Zoom < 10)
                {
                    this.Zoom = 10;
                }
            }
        }

        /// <summary>
        /// Event handler for Rotate Clockwise toolbar button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRotateCW_Click(object sender, EventArgs e)
        {
            mnuMainEditRotateCW_Click(sender, e);
        }

        /// <summary>
        /// Event handler for Rotate Counter-Clockwise toolbar button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRotateCCW_Click(object sender, EventArgs e)
        {
            mnuMainEditRotateCCW_Click(sender, e);
        }
        #endregion

        #region Load Methods
        /// <summary>
        /// Loads the module packs (in Preferences.ModulePacks) and fills the listview of modules.
        /// </summary>
        private void LoadModulePacks()
        {
            this.Cursor = Cursors.WaitCursor;
            prg.ProgressBar.Style = ProgressBarStyle.Marquee;

            this.Preferences.ModulePacks.Clear();

            // Load the root folder for all modules, one subfolder per pack
            DirectoryInfo dir = new DirectoryInfo(this.Preferences.PathToModulePacks);
            foreach (DirectoryInfo sd in dir.EnumerateDirectories())
            {
                // Load this module pack
                SetStatus("Loading module pack " + sd.Name);
                ModulePack pack = ModulePack.LoadModuleDefinition(sd.FullName);
                if (pack != null)
                {
                    // Loaded successfully
                    this.Preferences.ModulePacks.Add(pack);
                }
            }

            RefillListView(this.Preferences.ModulePacks, "");
            this.Cursor = Cursors.Default;
            prg.ProgressBar.Style = ProgressBarStyle.Blocks;
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// Draws the grid in the specified position with the specified size.
        /// </summary>
        /// <param name="g">The graphics object on which to draw the grid.</param>
        /// <param name="pos">The position in tile coordinates of the top-left point of the grid to draw.</param>
        /// <param name="gridWidth">The grid width in tiles.</param>
        /// <param name="gridHeight">The grid height in tiles.</param>
        private void DrawGrid(Graphics g, Point pos, int gridWidth, int gridHeight)
        {
            // Draw the backing grid
            Pen gridPen = new Pen(this.Preferences.GridLineColor, 1);
            Pen gridMajorPen = new Pen(this.Preferences.GridMajorLineColor, 3);

            gridWidth = Math.Min(gridWidth, MaxMapSize);
            gridHeight = Math.Min(gridHeight, MaxMapSize);

            int[] thickModulus = { 0, this.Preferences.GridGutterWidth };

            // Draw the vertical lines            
            for (int i = 0; i < gridWidth; i++)
            {
                Pen pen = thickModulus.Contains((pos.X + i) % (this.Preferences.GridMajorCount - this.Preferences.GridGutterWidth)) ? gridMajorPen : gridPen;

                g.DrawLine(pen, new Point((pos.X + i) * Zoom, pos.Y * Zoom), new Point((pos.X + i) * Zoom, (pos.Y + gridHeight) * Zoom));
            }

            // Draw the horizontal lines
            for (int j = 0; j < gridHeight; j++)
            {
                Pen pen = thickModulus.Contains((pos.Y + j) % (this.Preferences.GridMajorCount - this.Preferences.GridGutterWidth)) ? gridMajorPen : gridPen;

                g.DrawLine(pen, new Point(pos.X * Zoom, (pos.Y + j) * Zoom), new Point((pos.X + gridHeight) * Zoom, (pos.Y + j) * Zoom));
            }
        }

        /// <summary>
        /// Returns a Rectangle (in tile coordinates) of the visible section of the map.
        /// </summary>
        /// <returns></returns>
        private Rectangle GetVisibleRectangle()
        {
            Rectangle visible = Rectangle.Empty;
            const int threshold = 20;

            visible.Location = new Point((-this.Canvas.AutoScrollPosition.X / Zoom) - (threshold / 2), (-this.Canvas.AutoScrollPosition.Y / Zoom) - (threshold / 2));
            visible.Width = (this.Canvas.Width / Zoom) + threshold;
            visible.Height = (this.Canvas.Height / Zoom) + threshold;

            // Done
            return visible;
        }

        private void DrawSelectionRectangle(Graphics g)
        {
            if (this.SelectionRectangle.Width > 0 && this.SelectionRectangle.Height > 0)
            {
                // There is a selection rectangle - Paint it onto the canvas
                g.FillRectangle(SelectionBrush, Utils.TileRectangleToPixelRectangle(this.SelectionRectangle, this.Zoom));
            }
        }
        #endregion

        private void mnuMainEditSelectAll_Click(object sender, EventArgs e)
        {
            this.Map.Modules.ForEach(m => m.IsSelected = true);
            this.Canvas.Invalidate(false);
        }

        private void mnuMainFileOpen_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Remove all the old modules
                this.Map.Modules.ForEach(mod => mod.IsSelected = true);
                DeleteSelectedModules();

                // Open the map file the user requested
                this.Map = JsonConvert.DeserializeObject<Map>(System.IO.File.ReadAllText(ofd.FileName));

                // Make sure we have references for each MapModule pointing to the correct ModulePack and Module
                foreach (MapModule mapmod in this.Map.Modules)
                {
                    if (mapmod.Module == null)
                    {
                        // Find the corresponding module in our loaded ModulePacks
                        foreach (ModulePack pack in this.Preferences.ModulePacks)
                        {
                            foreach (Module[] mods in pack.Categories.Values)
                            {
                                foreach (Module mod in mods)
                                {
                                    if (mod.Key == mapmod.ModuleKey)
                                    {
                                        // This is our module
                                        mapmod.Module = mod;
                                        mapmod.Map = this.Map;
                                        mapmod.Zoom = this.Zoom;
                                        mapmod.CreatePictureBox(this.Canvas);

                                        // Found it - we can stop this loop
                                        break;
                                    }
                                }
                                if (mapmod.Module != null)
                                {
                                    // Found it - we can stop this loop
                                    break;
                                }
                            }
                            if (mapmod.Module != null)
                            {
                                // Found it - we can stop this loop
                                break;
                            }
                        }
                    }
                }

                this.Canvas.Invalidate();

                this.mapFileName = ofd.FileName;
                this.unsavedChanges = false;
                SetStatus("Map loaded");
            }
        }

        private void mnuMainFileSaveAs_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Make sure nothing is marked as selected before we write the file
                this.Map.Modules.ForEach(mod => mod.IsSelected = false);

                // Write the map file
                System.IO.File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(this.Map));
                this.mapFileName = sfd.FileName;
                this.unsavedChanges = false;
                SetStatus("Map saved");
            }
        }

        private void mnuMainFileSaveMap_Click(object sender, EventArgs e)
        {
            if (this.mapFileName != string.Empty)
            {
                // This map was loaded from a file, just save it
                // Make sure nothing is marked as selected before we write the file
                this.Map.Modules.ForEach(mod => mod.IsSelected = false);
                System.IO.File.WriteAllText(this.mapFileName, JsonConvert.SerializeObject(this.Map));
                this.unsavedChanges = false;
                SetStatus("Map saved");
            }
            else
            {
                // This map was not loaded from a file (new map), use the "Save As" dialog
                this.mnuMainFileSaveAs_Click(sender, e);
                SetStatus("Map saved");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedModules();
        }

        private void mnuMainFileNew_Click(object sender, EventArgs e)
        {
            if (this.unsavedChanges)
            {
                // There are unsaved changes - Warn the user
                DialogResult conf = MessageBox.Show("You have unsaved changes on this map. \r\nDo you want to save your changes before creating a new map?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                switch (conf)
                {
                    case DialogResult.Cancel:
                        // Stop everything
                        return;
                    case DialogResult.No:
                        // Don't save changes
                        break;
                    case DialogResult.Yes:
                        // Save changes
                        mnuMainFileSaveMap_Click(sender, e);
                        break;
                }
            }

            // Start from scratch
            this.Map = new Map();
            this.Canvas.Controls.Clear();
            this.ClearModulesInHand();
            this.mapFileName = "";
            this.unsavedChanges = false;
            this.Canvas.Invalidate();
        }

        private void mnuMainEditRotateCW_Click(object sender, EventArgs e)
        {
            RotateCW();
        }

        private void mnuMainEditRotateCCW_Click(object sender, EventArgs e)
        {
            RotateCCW();
        }

        private void mnuMainEditMirrorVertical_Click(object sender, EventArgs e)
        {
            MirrorVertical();
        }

        private void mnuMainEditMirrorHorizontal_Click(object sender, EventArgs e)
        {
            MirrorHorizontal();
        }

        private void btnMirrorVertical_Click(object sender, EventArgs e)
        {
            mnuMainEditMirrorVertical_Click(sender, e);
        }

        private void btnMirrorHorizontal_Click(object sender, EventArgs e)
        {
            mnuMainEditMirrorHorizontal_Click(sender, e);
        }

        private void RefillListView(List<ModulePack> packs, string term)
        {
            // Now put the packs and their modules into the list
            SetStatus("Loading module previews...");
            lsvModules.BeginUpdate();
            lsvModules.Groups.Clear();
            lsvModules.Items.Clear();

            ImageList largeImages = new ImageList();
            largeImages.ImageSize = new Size(48, 48);
            largeImages.ColorDepth = ColorDepth.Depth32Bit;
            ImageList smallImages = new ImageList();
            smallImages.ImageSize = new Size(36, 36);
            smallImages.ColorDepth = ColorDepth.Depth32Bit;
            int imageIndex = 0;
            term = term.ToLower();

            foreach (ModulePack pack in packs)
            {
                SetStatus("Loading module previews for pack \"" + pack.Name + "\"...");
                foreach (string category in pack.Categories.Keys)
                {
                    ListViewGroup grp = new ListViewGroup(pack.Name + " - " + category);

                    // Now add each module into this group
                    foreach (Module mod in pack.Categories[category])
                    {
                        if (term == string.Empty || (pack.Name.ToLower() + " " + category.ToLower() + " " + mod.Name.ToLower()).Contains(term))
                        {
                            // This module's name matches the search term - OK to show
                            ListViewItem item = new ListViewItem(mod.Name + " (" + mod.Width + " x " + mod.Height + ")");
                            item.Tag = mod;
                            largeImages.Images.Add(pack.Name + "." + mod.Name, mod.Thumbnail);
                            smallImages.Images.Add(pack.Name + "." + mod.Name, mod.Thumbnail);
                            item.ImageKey = pack.Name + "." + mod.Name;
                            item.ImageIndex = imageIndex;

                            // Add this item to the group
                            item.Group = grp;
                            lsvModules.Items.Add(item);

                            imageIndex++;
                        }
                    }

                    // Add this group to the list
                    lsvModules.Groups.Add(grp);
                }
            }

            // Set the image list for the list of modules
            lsvModules.LargeImageList = largeImages;
            lsvModules.SmallImageList = smallImages;

            // Done
            lsvModules.EndUpdate();
            SetStatus("Ready");
        }

        System.Windows.Forms.Timer searchTimer;

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Search for this string in our list of modules
            if (searchTimer == null)
            {
                searchTimer = new Timer();
                searchTimer.Interval = 300;
                searchTimer.Tick += new EventHandler(this.searchTimer_Timeout);
            }
            searchTimer.Stop();
            searchTimer.Tag = (sender as TextBox).Text;
            searchTimer.Start();
        }

        private void searchTimer_Timeout(object sender, EventArgs e)
        {
            var timer = sender as Timer;

            if (timer == null)
            {
                return;
            }

            // The timer must be stopped! We want to act only once per keystroke.
            timer.Stop();

            RefillListView(this.Preferences.ModulePacks, txtSearch.Text);
        }

        private void btnModeDraw_Click(object sender, EventArgs e)
        {
            this.CursorMode = MouseMode.Draw;
        }

        private void btnModeEraser_Click(object sender, EventArgs e)
        {
            this.CursorMode = MouseMode.Erase;
        }

        private void btnModeSelect_Click(object sender, EventArgs e)
        {
            this.CursorMode = MouseMode.Select;
        }

        private void btnModePaint_Click(object sender, EventArgs e)
        {
            this.CursorMode = MouseMode.Paint;
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {

        }

        private void mnuMainEditCopy_Click(object sender, EventArgs e)
        {
            // Make a clone of each selected module and put it in hand
            ClearModulesInHand();
            foreach (MapModule mod in this.Map.SelectedModules)
            {
                MapModule mih = mod.Clone();
                mih.IsInHand = true;
                mih.IsSelected = false;
                mih.CreatePictureBox(this.Canvas);
                this.ModulesInHand.Add(mih);
            }
            this.CursorMode = MouseMode.Draw;
        }
    }
}
