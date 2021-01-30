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

        private SolidBrush SelectionBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 0, 255));
        private SolidBrush EraserBrush { get; } = new SolidBrush(Color.FromArgb(95, 255, 0, 0));
        private SolidBrush ModuleToPlaceBrush { get; } = new SolidBrush(Color.FromArgb(95, 0, 255, 0));

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
        /// The current map.
        /// </summary>
        private Map Map { get; set; } = new Map();

        /// <summary>
        /// Gets or sets the tile position that received a MouseDown event.
        /// </summary>
        private Point MouseDownTilePos { get; set; } = new Point(-1, -1);

        /// <summary>
        /// Stores the last position (in tile coordinates) of the mouse cursor.
        /// </summary>
        private Point MouseMoveLastTilePos = new Point(-1, -1);

        /// <summary>
        /// Holds the module "in hand" for the user to place onto the map.
        /// </summary>
        private MapModule ModuleToPlace { get; set; } = null;

        /// <summary>
        /// Snapshot of map render for faster redraws.
        /// </summary>
        private Bitmap MapSnapshot { get; set; } = null;

        /// <summary>
        /// Gets or sets the global drawing scale (pixels per tile).
        /// </summary>
        public new static int Scale { get; set; } = 60;

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
                btnModePaint.Checked = value == MouseMode.Paint;

                switch (value)
                {
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        this.canvas.Cursor = Cursors.Cross;
                        break;
                    case MouseMode.Erase:
                        this.canvas.Cursor = Cursors.No;
                        break;
                    case MouseMode.Move:
                        this.canvas.Cursor = Cursors.SizeAll;
                        break;
                    default:
                        this.canvas.Cursor = Cursors.Default;
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
            if (this.ModuleToPlace != null)
            {
                // Rotate the module "in hand"
                ModuleToPlace.RotateCW();
            }
            else
            {
                // Rotate the selected modules
                if (this.Map.SelectedModules.Any())
                {
                    Point selNW = Point.Empty;
                    Point selSE = Point.Empty;

                    selNW.X = this.Map.SelectedModules.Min(m => m.NW.X);
                    selNW.Y = this.Map.SelectedModules.Min(m => m.NW.Y);
                    selSE.X = this.Map.SelectedModules.Max(m => m.SE.X);
                    selSE.Y = this.Map.SelectedModules.Max(m => m.SE.Y);

                    Rectangle selection = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                    // Build a 2D array of the selected modules
                    MapModule[,] matrix = new MapModule[selection.Height, selection.Width];
                    for (int i = 0; i < selection.Height; i++)
                    {
                        for (int j = 0; j < selection.Width; j++)
                        {
                            // Find the module at this location
                            Point pos = new Point(j + selNW.X, i + selNW.Y);
                            MapModule mod = this.Map.GetModuleAt(pos);
                            if (mod != null && mod.IsSelected && mod.Position == pos)
                            {
                                // There is a seleced module here
                                matrix[i, j] = mod;
                            }
                        }
                    }

                    // Now rotate the matrix
                    matrix = Utils.RotateMatrix(matrix);

                    // Adjust the position (rotation was based on position 0, 0) and rotate the modules
                    foreach (MapModule mod in this.Map.SelectedModules)
                    {
                        mod.RotateCW();
                        mod.X += selection.X;
                        mod.Y += selection.Y;
                    }

                    // Redraw rotated map
                    this.canvas.Invalidate();
                    this.unsavedChanges = true;
                }
            }
        }

        /// <summary>
        /// Rotates the selection or the module "in hand" counter clockwise and redraws.
        /// </summary>
        private void RotateCCW()
        {
            if (this.ModuleToPlace != null)
            {
                // Rotate the module "in hand"
                ModuleToPlace.RotateCCW();
            }
            else
            {
                Point selNW = Point.Empty;
                Point selSE = Point.Empty;

                selNW.X = this.Map.SelectedModules.Min(m => m.NW.X);
                selNW.Y = this.Map.SelectedModules.Min(m => m.NW.Y);
                selSE.X = this.Map.SelectedModules.Max(m => m.SE.X);
                selSE.Y = this.Map.SelectedModules.Max(m => m.SE.Y);

                Rectangle selection = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                // Build a 2D array of the selected modules
                MapModule[,] matrix = new MapModule[selection.Height, selection.Width];
                for (int i = 0; i < selection.Height; i++)
                {
                    for (int j = 0; j < selection.Width; j++)
                    {
                        // Find the module at this location
                        Point pos = new Point(j + selNW.X, i + selNW.Y);
                        MapModule mod = this.Map.GetModuleAt(pos);
                        if (mod != null && mod.IsSelected && mod.Position == pos)
                        {
                            // There is a seleced module here
                            matrix[i, j] = mod;
                        }
                    }
                }

                // Now rotate the matrix (3 times for CCW)
                matrix = Utils.RotateMatrix(matrix);
                matrix = Utils.RotateMatrix(matrix);
                matrix = Utils.RotateMatrix(matrix);

                // Adjust the position (rotation was based on position 0, 0) and rotate the modules
                foreach (MapModule mod in this.Map.SelectedModules)
                {
                    mod.RotateCCW();
                    mod.X += selection.X;
                    mod.Y += selection.Y;
                }

                // Redraw rotated map
                this.canvas.Invalidate();
                this.unsavedChanges = true;
            }
        }

        private void MirrorHorizontal()
        {
            if (this.ModuleToPlace != null)
            {
                // Flip the module "in hand"
                ModuleToPlace.MirrorHorizontal();
            }
            else
            {
                // Flip the selected modules
                if (this.Map.SelectedModules.Any())
                {
                    foreach (MapModule mod in this.Map.SelectedModules)
                    {
                        // Flip the mod itself
                        mod.MirrorHorizontal();

                        // Update the module's position in the group
                        //[TBD]
                    }
                }
            }
        }

        private void MirrorVertical()
        {
            if (this.ModuleToPlace != null)
            {
                // Flip the module "in hand"
                ModuleToPlace.MirrorVertical();
            }
            else
            {
                // Flip the selected modules
                if (this.Map.SelectedModules.Any())
                {
                    foreach (MapModule mod in this.Map.SelectedModules)
                    {
                        // Flip the mod itself
                        mod.MirrorVertical();

                        // Update the module's position in the group
                        //[TBD]
                    }
                }
            }
        }

        private void Log(string message)
        {
            _log.AppendFormat("{0}   {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            //System.IO.File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "   " + message + "\r\n");
        }

        /// <summary>
        /// Writes the status message to the status label.
        /// </summary>
        /// <param name="message"></param>
        private void SetStatus(string message, bool writeToLog = true)
        {
            lblStatus.Text = message;
            Application.DoEvents();
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

        private bool LeftMouseButtonIsPressed
        {
            get
            {
                return this.MouseDownTilePos != new Point(-1, -1);
            }
        }

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
                    this.RemovedModules.Add(this.Map.Modules[i].OccupiedRectangle);
                    this.Map.Modules.RemoveAt(i);
                }
            }

            // Redraw the map
            this.canvas.Invalidate();
            this.unsavedChanges = true;
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
            SetStatus("Ready");
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

                if (ModuleToPlace != null)
                {
                    // Remove the "module in hand" (replace it with the grid)
                    this.RemovedModules.Add(this.ModuleToPlace.OccupiedRectangle);
                    ModuleToPlace = null;
                }
                this.canvas.Invalidate();
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
        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            this.ctrlPressed = false;
            this.shiftPressed = false;
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
                ModuleToPlace = new MapModule((Module)lsvModules.SelectedItems[0].Tag);
                this.CursorMode = MouseMode.Draw;
            }
            else
            {
                // No selection
                ModuleToPlace = null;
                this.CursorMode = MouseMode.Select;
            }
        }
        #endregion

        #region Canvas Events

        System.Windows.Forms.Timer paintTimer;

        private void paintTimer_Timeout(object sender, EventArgs e)
        {
            var timer = sender as Timer;

            if (timer == null)
            {
                return;
            }

            // The timer must be stopped!
            timer.Stop();

            // Make sure the canvas is large enough
            this.canvas.AutoScrollMinSize = new Size(MaxMapSize * Scale, MaxMapSize * Scale);
            Graphics pg = this.canvas.CreateGraphics();
            pg.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);
            DrawMap(pg);
        }

        /// <summary>
        /// Event handler for canvas paint event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pe"></param>
        private void canvas_Paint(object sender, System.Windows.Forms.PaintEventArgs pe)
        {
            // Make sure the canvas is large enough
            this.canvas.AutoScrollMinSize = new Size(MaxMapSize * Scale, MaxMapSize * Scale);
            pe.Graphics.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);
            DrawMap(pe.Graphics);
            return;

            if (paintTimer == null)
            {
                paintTimer = new Timer();
                paintTimer.Interval = 50;
                paintTimer.Tick += new EventHandler(this.paintTimer_Timeout);
            }
            paintTimer.Stop();
            paintTimer.Tag = pe.Graphics;
            paintTimer.Start();
        }

        /// <summary>
        /// Event handler for mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Get the click position
                Point clickPos = new Point(e.Location.X, e.Location.Y);

                // Offset the position based on the scroll offset
                clickPos.X -= canvas.AutoScrollPosition.X;
                clickPos.Y -= canvas.AutoScrollPosition.Y;

                // Store the tile coordinates for the tile that received the MouseDown event
                this.MouseDownTilePos = new Point(clickPos.X / Scale, clickPos.Y / Scale);

                switch (this.CursorMode)
                {
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        if (this.ModuleToPlace != null)
                        {
                            // User has a tile selected from the module selection, wants to place it somewhere

                            // If this is paint mode, remove the tile that was here
                            if (this.CursorMode == MouseMode.Paint && this.Map.IsTileOccupied(this.ModuleToPlace.Position))
                            {
                                // We're painting, and there is a module here - Remove it
                                MapModule mod = this.Map.GetModuleAt(this.ModuleToPlace.Position);
                                this.Map.Modules.Remove(mod);
                            }

                            // Check if the clicked square is occupied and draw the module if there is room
                            if (!this.Map.IsRectangleOccupied(this.ModuleToPlace.OccupiedRectangle))
                            {
                                MapModule mod = ModuleToPlace.Clone();
                                this.Map.Modules.Add(mod);

                                this.canvas.Invalidate();
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
                                this.MouseMoveLastTilePos = this.MouseDownTilePos;
                                break;
                            }
                        }
                        break;
                    case MouseMode.Erase:
                        // Button is pressed - Let's delete the module at this spot
                        MapModule modToErase = this.Map.GetModuleAt(this.MouseDownTilePos);
                        if (modToErase != null)
                        {
                            this.RemovedModules.Add(modToErase.OccupiedRectangle);
                            this.Map.Modules.Remove(modToErase);
                            this.canvas.Invalidate();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            // Get the click position
            Point clickPos = new Point(e.Location.X, e.Location.Y);

            // Offset the position based on the scroll offset
            clickPos.X -= canvas.AutoScrollPosition.X;
            clickPos.Y -= canvas.AutoScrollPosition.Y;

            // Get the tile coordinates
            Point mouseUpTilePos = new Point(clickPos.X / Scale, clickPos.Y / Scale);

            if (e.Button == MouseButtons.Left)
            {
                switch (this.CursorMode)
                {
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        // Draw the module at the selected position

                        // If this is paint mode, remove the tile that was here
                        if (this.CursorMode == MouseMode.Paint && this.Map.IsTileOccupied(this.ModuleToPlace.Position))
                        {
                            // We're painting, and there is a module here - Remove it
                            MapModule mod = this.Map.GetModuleAt(this.ModuleToPlace.Position);
                            this.Map.Modules.Remove(mod);
                        }

                        // Check if the clicked square is occupied
                        if (!this.Map.IsRectangleOccupied(this.ModuleToPlace.OccupiedRectangle) && this.ModuleToPlace != null)
                        {
                            MapModule mod = ModuleToPlace.Clone();
                            this.Map.Modules.Add(mod);

                            using (Graphics g = Graphics.FromImage(this.MapSnapshot))
                            {
                                mod.Draw(g, Scale, this.Map);
                            }

                            // Redraw the map
                            this.canvas.Invalidate();
                        }

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
                        this.canvas.Invalidate();
                        break;
                    case MouseMode.Move:
                        // User lifted the mouse button after a move, go back to select mode
                        this.CursorMode = MouseMode.Select;
                        break;
                    default:
                        break;
                }
                this.MouseDownTilePos = new Point(-1, -1);
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right-click on a tile -> Delete that tile
                if (this.Map.IsTileOccupied(mouseUpTilePos))
                {
                    MapModule mod = this.Map.GetModuleAt(mouseUpTilePos);
                    this.Map.Modules.Remove(mod);
                    this.RemovedModules.Add(mod.OccupiedRectangle);

                    this.canvas.Invalidate();
                }
            }
        }

        /// <summary>
        /// Event handler for canvas mouse over (show the selected module in its expected position).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            lock (this.canvas)
            {
                // Get the mouse position
                Point mapPixelPos = new Point(e.Location.X, e.Location.Y);

                // Get the position in pixels on the map
                mapPixelPos.X -= canvas.AutoScrollPosition.X;
                mapPixelPos.Y -= canvas.AutoScrollPosition.Y;

                // Get the tile coordinates on the map (based on the pixel position on the map)
                Point mouseMoveCurrTilePos = new Point(mapPixelPos.X / Scale, mapPixelPos.Y / Scale);

                SetStatus("Cursor Position: " + mouseMoveCurrTilePos.X.ToString() + "/" + mouseMoveCurrTilePos.Y.ToString());

                switch (this.CursorMode)
                {
                    case MouseMode.Select:
                        // We're in "select" mode - Draw the selection rectangle (if the current tile hasn't changed)
                        if (mouseMoveCurrTilePos != this.MouseMoveLastTilePos && this.LeftMouseButtonIsPressed)
                        {
                            // Update the selection rectangle
                            Point NW = new Point(Math.Min(mouseMoveCurrTilePos.X, this.MouseDownTilePos.X), Math.Min(mouseMoveCurrTilePos.Y, this.MouseDownTilePos.Y));
                            Point SE = new Point(Math.Max(mouseMoveCurrTilePos.X, this.MouseDownTilePos.X) + 1, Math.Max(mouseMoveCurrTilePos.Y, this.MouseDownTilePos.Y) + 1);

                            this.SelectionRectangle = new Rectangle(NW.X, NW.Y, SE.X - NW.X, SE.Y - NW.Y);
                            this.canvas.Invalidate();
                        }
                        break;
                    case MouseMode.Draw:
                    case MouseMode.Paint:
                        // We're in "draw" mode - Show the module "in hand"
                        if (this.ModuleToPlace != null)
                        {
                            // Save old position to avoid drawing too much
                            Point oldPos = this.ModuleToPlace.Position;

                            // Set new position
                            this.ModuleToPlace.Position = mouseMoveCurrTilePos;

                            if (oldPos != mouseMoveCurrTilePos)
                            {
                                Log("MouseMove OldTilePos: " + oldPos.ToString() + " - NewTilePos: " + mouseMoveCurrTilePos.ToString());

                                // Draw the selected module here
                                if (this.LeftMouseButtonIsPressed)
                                {
                                    // If this is paint mode, remove the tile that was here
                                    if (this.CursorMode == MouseMode.Paint && this.Map.IsTileOccupied(this.ModuleToPlace.Position))
                                    {
                                        // We're painting, and there is a module here - Remove it
                                        MapModule mod = this.Map.GetModuleAt(this.ModuleToPlace.Position);
                                        this.Map.Modules.Remove(mod);
                                    }

                                    if (!this.Map.IsRectangleOccupied(this.ModuleToPlace.OccupiedRectangle))
                                    {
                                        // User is holding mouse button and moving - Paint this module right here (if not already occupied))
                                        MapModule mod = ModuleToPlace.Clone();
                                        this.Map.Modules.Add(mod);

                                        // Capture a new snapshot
                                        this.canvas.Invalidate();
                                        this.unsavedChanges = true;
                                    }
                                }
                                this.canvas.Invalidate();
                            }
                        }
                        break;
                    case MouseMode.Move:
                        // Move the selection
                        if (this.MouseMoveLastTilePos != mouseMoveCurrTilePos && this.LeftMouseButtonIsPressed)
                        {
                            int deltaX = mouseMoveCurrTilePos.X - this.MouseMoveLastTilePos.X;
                            int deltaY = mouseMoveCurrTilePos.Y - this.MouseMoveLastTilePos.Y;
                            foreach (MapModule mod in this.Map.SelectedModules)
                            {
                                // Move this module
                                Point modPos = mod.Position;
                                modPos.X += deltaX;
                                modPos.Y += deltaY;
                                mod.Position = modPos;
                            }
                            this.canvas.Invalidate();
                            this.unsavedChanges = true;
                        }
                        break;
                    case MouseMode.Erase:
                        // We're in "erase" mode
                        if (this.LeftMouseButtonIsPressed)
                        {
                            // Button is pressed - Let's delete the module at this spot
                            MapModule mod = this.Map.GetModuleAt(mouseMoveCurrTilePos);
                            if (mod != null)
                            {
                                this.Map.Modules.Remove(mod);
                                this.RemovedModules.Add(mod.OccupiedRectangle);
                                this.canvas.Invalidate();
                            }
                        }
                        else
                        {
                            // Mouse button is not pressed, highlight the module that will be erased
                            if (this.MouseMoveLastTilePos != mouseMoveCurrTilePos)
                            {
                                MapModule mod = this.Map.GetModuleAt(mouseMoveCurrTilePos);
                                if (mod != null)
                                {
                                    // There is a module under the cursor - Mark it as to be deleted
                                    mod.IsToBeDeleted = true;
                                }

                                // Check if there was a different module under the previous cursor position
                                MapModule mod2 = this.Map.GetModuleAt(this.MouseMoveLastTilePos);
                                if (mod2 != null && mod2 != mod)
                                {
                                    // A different module was previously marked as "to be deleted"; clear that setting
                                    mod2.IsToBeDeleted = false;
                                }

                                this.canvas.Invalidate();
                            }
                        }
                        break;
                    default:
                        break;
                }

                // Store the last tile position to reduce redraws
                this.MouseMoveLastTilePos = mouseMoveCurrTilePos;
            }
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
                this.MapSnapshot = null;
                this.canvas.Invalidate();
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
                Map.Modules.Clear();
                this.MapSnapshot = null;
                this.canvas.Invalidate();
            }
        }

        /// <summary>
        /// Event handler for Zoom In button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            Scale += 10;
            if (Scale > 200)
            {
                Scale = 200;
            }
            this.MapSnapshot = null;
            this.canvas.Invalidate();
        }

        /// <summary>
        /// Event handler for Zoom Out button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            Scale -= 10;
            if (Scale < 10)
            {
                Scale = 10;
            }
            this.MapSnapshot = null;
            this.canvas.Invalidate();
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
            // Clear the canvas
            g.FillRectangle(new SolidBrush(this.Preferences.GridBackgroundColor), new Rectangle(new Point(pos.X * Scale, pos.Y * Scale), new Size(gridWidth * Scale, gridHeight * Scale)));

            // Draw the backing grid
            Pen gridPen = new Pen(this.Preferences.GridLineColor, 1);
            Pen gridMajorPen = new Pen(this.Preferences.GridMajorLineColor, 3);

            int[] thickModulus = { 0, this.Preferences.GridGutterWidth };

            // Draw the vertical lines            
            for (int i = 0; i < gridWidth; i++)
            {
                Pen pen = thickModulus.Contains((pos.X + i) % (this.Preferences.GridMajorCount - this.Preferences.GridGutterWidth)) ? gridMajorPen : gridPen;

                g.DrawLine(pen, new Point((pos.X + i) * Scale, pos.Y * Scale), new Point((pos.X + i) * Scale, (pos.Y + gridHeight) * Scale));
            }

            // Draw the horizontal lines
            for (int j = 0; j < gridHeight; j++)
            {
                Pen pen = thickModulus.Contains((pos.Y + j) % (this.Preferences.GridMajorCount - this.Preferences.GridGutterWidth)) ? gridMajorPen : gridPen;

                g.DrawLine(pen, new Point(pos.X * Scale, (pos.Y + j) * Scale), new Point((pos.X + gridHeight) * Scale, (pos.Y + j) * Scale));
            }
        }

        /// <summary>
        /// Returns a Rectangle (in tile coordinates) of the visible section of the map.
        /// </summary>
        /// <returns></returns>
        private Rectangle GetVisibleRectangle()
        {
            Rectangle visible = Rectangle.Empty;
            const int threshold = 10;

            visible.Location = new Point((this.canvas.AutoScrollOffset.X / Scale) - (threshold / 2), (this.canvas.AutoScrollOffset.Y / Scale) - (threshold / 2));
            visible.Width = (this.canvas.Width / Scale) + threshold;
            visible.Height = (this.canvas.Height / Scale) + threshold;

            // Done
            return visible;
        }

        /// <summary>
        /// Paints the map onto the canvas panel.
        /// </summary>
        /// <param name="g">The current Graphics object to use to paint the canvas.</param>
        /// <param name="redraw">Indicates whether to run a clean redraw from scratch, or paint the latest snapshot.</param>
        private void DrawMap(Graphics g, bool redraw = false)
        {
            Utils.PerfTracker perf = new Utils.PerfTracker("DrawMap");
            perf.StartStack("DrawMap()");

            this.Cursor = Cursors.AppStarting;

            //For best performance
            perf.StartStack("SetSmoothingMode");
            g.SmoothingMode = SmoothingMode.None;
            perf.StopStack("SetSmoothingMode");

            // Get the range of the map to draw
            perf.StartStack("GetVisibleRectangle");
            Rectangle visible = GetVisibleRectangle();
            perf.StopStack("GetVisibleRectangle");

            try
            {
                // Draw the visible portion of the grid
                perf.StartStack("DrawGrid");
                DrawGrid(g, visible.Location, visible.Width, visible.Height);
                perf.StopStack("DrawGrid");

                // Draw the modules
                perf.StartStack("GetVisibleModules");
                List<MapModule> modules = this.Map.GetVisibleModules(visible).ToList();
                perf.StopStack("GetVisibleModules");

                perf.StartStack("DrawVisibleModules");
                modules.ForEach(mod => mod.Draw(g, Scale, this.Map));
                perf.StopStack("DrawVisibleModules");

                // Draw the selection rectangle on top of the snapshot
                perf.StartStack("DrawSelectionRectangle");
                DrawSelectionRectangle(g);
                perf.StopStack("DrawSelectionRectangle");

                // Draw the module in hand on top of the snapshot
                perf.StartStack("DrawModuleToPlace");
                DrawModuleToPlace(g);
                perf.StopStack("DrawModuleToPlace");
            }
            catch (Exception ex)
            {
                Console.WriteLine("DrawMap() Failed: \r\n" + ex.ToString());
            }
            finally
            {
                // Done
                perf.StopStack("DrawMap()");
                Console.WriteLine(perf.SummaryTree(0, 0.001));
                this.Cursor = Cursors.Default;
            }

            /*
            try
            {
                if (redraw || this.MapSnapshot == null)
                {
                    perf.StartStack("Redraw");
                    // Run a clean redraw from scratch
                    // Prepare the bitmap for the snapshot
                    perf.StartStack("PrepSnapshotBitmap");
                    this.MapSnapshot = new Bitmap(canvas.AutoScrollMinSize.Width, canvas.AutoScrollMinSize.Height);
                    perf.StopStack("PrepSnapshotBitmap");

                    // Paint the modules and grid onto the snapshot
                    perf.StartStack("Bitmap.Graphics");
                    using (Graphics snapGraphics = Graphics.FromImage(this.MapSnapshot))
                    {
                        perf.StopStack("Bitmap.Graphics");

                        // Draw the grid first
                        perf.StartStack("DrawGrid");
                        DrawGrid(snapGraphics, new Point(0, 0), MaxMapSize, MaxMapSize);
                        perf.StopStack("DrawGrid");

                        // Draw each module
                        perf.StartStack("DrawModules");
                        Map.Modules.ForEach(mod => mod.Draw(snapGraphics, Scale, this.Map));
                        perf.StopStack("DrawModules");
                    }
                    perf.StopStack("Redraw");
                }
                else
                {
                    perf.StartStack("Snapshot");

                    // Draw the latest snapshot
                    perf.StartStack("PrepSnapshotBitmap");
                    Bitmap newSnapshot = new Bitmap(canvas.AutoScrollMinSize.Width, canvas.AutoScrollMinSize.Height);
                    perf.StopStack("PrepSnapshotBitmap");

                    // Paint the modules and grid onto the snapshot
                    perf.StartStack("CreateGraphics");
                    using (Graphics snapGraphics = Graphics.FromImage(newSnapshot))
                    {
                        perf.StopStack("CreateGraphics");

                        // Draw the old snapshot
                        perf.StartStack("DrawOldSnapshot");
                        snapGraphics.DrawImage(this.MapSnapshot, 0, 0);
                        perf.StopStack("DrawOldSnapshot");

                        // Replace removed modules with grid
                        perf.StartStack("DrawRectanglesForRemovedModules");
                        foreach (Rectangle rect in this.RemovedModules)
                        {
                            DrawGrid(snapGraphics, rect.Location, rect.Width, rect.Height);
                        }
                        perf.StopStack("DrawRectanglesForRemovedModules");

                        // Clear the removed modules
                        this.RemovedModules.Clear();

                        // Draw invalidated modules
                        perf.StartStack("DrawModules");
                        foreach (MapModule mod in this.Map.ModulesToBeDrawn)
                        {
                            mod.Draw(g, Scale, this.Map);
                        }
                        perf.StopStack("DrawModules");

                        // Save the new snapshot
                        perf.StartStack("SaveSnapshot");
                        this.MapSnapshot = newSnapshot;
                        perf.StopStack("SaveSnapshot");
                    }
                    perf.StopStack("Snapshot");
                }

                //For best performance
                perf.StartStack("SetSmoothingMode");
                g.SmoothingMode = SmoothingMode.None;
                perf.StopStack("SetSmoothingMode");

                // Draw the snapshot onto the canvas
                perf.StartStack("DrawSnapshot");
                g.DrawImage(this.MapSnapshot, 0, 0);
                perf.StopStack("DrawSnapshot");

                // Draw the selection rectangle on top of the snapshot
                perf.StartStack("DrawSelectionRectangle");
                DrawSelectionRectangle(g);
                perf.StopStack("DrawSelectionRectangle");

                // Draw the module in hand on top of the snapshot
                perf.StartStack("DrawModuleToPlace");
                DrawModuleToPlace(g);
                perf.StopStack("DrawModuleToPlace");
            }
            catch (Exception ex)
            {
                Console.WriteLine("DrawMap() Failed: \r\n" + ex.ToString());
            }
            finally
            {
                // Done
                perf.StopStack("DrawMap()");
                Console.WriteLine(perf.SummaryTree(0, 0.001));
                this.Cursor = Cursors.Default;
            }
            */
        }

        private void DrawSelectionRectangle(Graphics g)
        {
            if (this.SelectionRectangle.Width > 0 && this.SelectionRectangle.Height > 0)
            {
                // There is a selection rectangle - Paint it onto the canvas
                g.FillRectangle(this.SelectionBrush, TileRectangleToPixelRectangle(this.SelectionRectangle));
            }
        }

        private Rectangle TileRectangleToPixelRectangle(Rectangle tileRect)
        {
            Rectangle pixelRect = new Rectangle(tileRect.X * Scale, tileRect.Y * Scale, tileRect.Width * Scale, tileRect.Height * Scale);
            return pixelRect;
        }

        /// <summary>
        /// Draw the module "in hand".
        /// </summary>
        private void DrawModuleToPlace(Graphics g)
        {
            if (ModuleToPlace != null)
            {
                this.ModuleToPlace.IsInHand = true;
                this.ModuleToPlace.Draw(g, Scale, this.Map);
            }
        }
        #endregion

        private void mnuMainEditSelectAll_Click(object sender, EventArgs e)
        {
            this.Map.Modules.ForEach(m => m.IsSelected = true);
            this.canvas.Invalidate();
        }

        private void mnuMainFileOpen_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
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

                this.MapSnapshot = null;
                this.canvas.Invalidate();

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
            this.mapFileName = "";
            this.unsavedChanges = false;
            this.MapSnapshot = null;
            this.canvas.Invalidate();
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
            largeImages.ImageSize = new Size(100, 100);
            largeImages.ColorDepth = ColorDepth.Depth32Bit;
            ImageList smallImages = new ImageList();
            smallImages.ImageSize = new Size(48, 48);
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
    }
}
