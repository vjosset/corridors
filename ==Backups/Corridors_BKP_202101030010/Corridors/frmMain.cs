using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        // [TBD]
        private Color EraseColor = Color.Red;
        private Color DrawColor = Color.Green;
        private Color SelectColor = Color.Blue;

        private const int MaxMapSize = 100;

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
        private Module ModuleToPlace { get; set; } = null;

        /// <summary>
        /// Snapshot of map render for faster redraws.
        /// </summary>
        private Bitmap mapSnapshot = null;

        /// <summary>
        /// Gets or sets the global drawing scale (pixels per square)
        /// </summary>
        public new static int Scale { get; set; } = 30;

        private MouseMode _cursorMode = MouseMode.Select;
        /// <summary>
        /// Gets or sets the current mouse/cursor mode.
        /// </summary>
        private MouseMode CursorMode {
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
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Rotates the selection or the module "in hand" clockwise and redraws.
        /// </summary>
        private void RotateClockwise()
        {
            if (this.ModuleToPlace != null)
            {
                // Rotate the module "in hand"
                ModuleToPlace.SetRotation(1);
                DrawSnapshot();
                DrawModuleToPlace();
            }
            else
            {
                // Rotate the selected modules

                if (this.Map.SelectedModules.Any())
                {
                    // Calculate the rectangle representing the full selection
                    Point selNW = Point.Empty;
                    Point selSE = Point.Empty;

                    selNW.X = this.Map.SelectedModules.Min(m => m.NW.X);
                    selNW.Y = this.Map.SelectedModules.Min(m => m.NW.Y);
                    selSE.X = this.Map.SelectedModules.Max(m => m.SE.X);
                    selSE.Y = this.Map.SelectedModules.Max(m => m.SE.Y);

                    Rectangle selection = new Rectangle(selNW, new Size(selSE.X - selNW.X, selSE.Y - selNW.Y));

                    // Get the center of rotation
                    PointF pivot = new PointF(selection.X + ((float)selection.Width / 2), selection.Y + ((float)selection.Height / 2));

                    SetStatus("Rectangle: " + selection.X.ToString() + "/" + selection.Y.ToString() + " - " + selection.Width.ToString() + "/" + selection.Height.ToString());
                    double cos = Math.Cos(Math.PI / 2);
                    double sin = Math.Sin(Math.PI / 2);
                    foreach (Module mod in this.Map.SelectedModules)
                    {
                        // Calculate the new position of this module
                        float dx = mod.SW.X - pivot.X;
                        float dy = mod.SW.Y - pivot.Y;
                        double x = cos * dx - sin * dy + pivot.X;
                        double y = sin * dx + cos * dy + pivot.Y;

                        mod.Position = new Point((int)Math.Floor(x), (int)Math.Floor(y));

                        // Rotate the module itself
                        mod.SetRotation(1);
                    }

                    // Reset the position
                    Point selNewNW = Point.Empty;
                    Point selNewSE = Point.Empty;
                    selNewNW.X = this.Map.SelectedModules.Min(m => m.NW.X);
                    selNewNW.Y = this.Map.SelectedModules.Min(m => m.NW.Y);
                    selNewSE.X = this.Map.SelectedModules.Max(m => m.SE.X);
                    selNewSE.Y = this.Map.SelectedModules.Max(m => m.SE.Y);

                    Rectangle newSelection = new Rectangle(selNewNW, new Size(selNewSE.X - selNewNW.X, selNewSE.Y - selNewNW.Y));
                    if (newSelection.X != selection.X)
                    {
                        // The whole selection has shifted, let's put it back in place
                        foreach (Module mod in this.Map.SelectedModules)
                        {
                            Point modPos = mod.Position;
                            modPos.X += selection.X - newSelection.X;
                            mod.Position = modPos;
                        }
                    }
                    if (newSelection.Y != selection.Y)
                    {
                        // The whole selection has shifted, let's put it back in place
                        foreach (Module mod in this.Map.SelectedModules)
                        {
                            Point modPos = mod.Position;
                            modPos.Y += selection.Y - newSelection.Y;
                            mod.Position = modPos;
                        }
                    }

                    // Redraw rotated map
                    DrawMap();
                    this.unsavedChanges = true;
                }
            }
        }

        /// <summary>
        /// Rotates the selection or the module "in hand" counter clockwise and redraws.
        /// </summary>
        private void RotateCounterClockwise()
        {
            if (this.ModuleToPlace != null)
            {
                // Rotate the module "in hand"
                ModuleToPlace.SetRotation(3);
                DrawSnapshot();
                DrawModuleToPlace();
            }
            else
            {
                // Rotate the selected modules
                foreach (Module mod in this.Map.SelectedModules)
                {
                    mod.SetRotation(3);
                }
                DrawMap();
                this.unsavedChanges = true;
            }
        }

        private void MirrorHorizontal()
        {
            if (this.ModuleToPlace != null)
            {
                // Flip the module "in hand"
                ModuleToPlace.MirrorHorizontal();
                DrawSnapshot();
                DrawModuleToPlace();
            }
            else
            {
                // Flip the selected modules
                if (this.Map.SelectedModules.Any())
                {
                    // [TBD]
                }
            }
        }

        private void MirrorVertical()
        {
            if (this.ModuleToPlace != null)
            {
                // Flip the module "in hand"
                ModuleToPlace.MirrorVertical();
                DrawSnapshot();
                DrawModuleToPlace();
            }
            else
            {
                // Flip the selected modules
                if (this.Map.SelectedModules.Any())
                {
                    // [TBD]
                }
            }
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
                    this.Map.Modules.RemoveAt(i);
                }
            }

            // Redraw the map
            DrawMap();
            DrawModuleToPlace();
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

            // Draw the starting grid
            DrawMap();
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
                lsvModules.SelectedItems.Clear();
                ModuleToPlace = null;

                DrawMap();
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
                ModuleToPlace = ((Module)lsvModules.SelectedItems[0].Tag).Clone();
                this.CursorMode = MouseMode.Draw;
            }
            else
            {
                // No selection
                ModuleToPlace = null;
                DrawSnapshot();
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
        private void canvas_Paint(object sender, System.Windows.Forms.PaintEventArgs pe)
        {
            DrawMap();
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
                        if (this.ModuleToPlace != null)
                        {
                            // User has a tile selected from the module selection, wants to place it somewhere

                            // Check if the clicked square is occupied and draw the module if there is room
                            if (!this.Map.IsTileOccupied(this.MouseDownTilePos))
                            {
                                Module mod = ModuleToPlace.Clone();
                                this.Map.Modules.Add(mod);

                                Graphics g = this.canvas.CreateGraphics();
                                g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);

                                mod.Draw(g, Scale);
                                this.GetSnapshot();
                                this.unsavedChanges = true;
                            }
                        }
                        break;
                    case MouseMode.Select:// Check if the clicked tile was occupied, meaning this is a "Move" operation
                        foreach (Module mod in this.Map.SelectedModules)
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
                        // [TBD]
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
                        // Draw the module at the selected position
                        // Check if the clicked square is occupied
                        if (!this.Map.IsTileOccupied(mouseUpTilePos) && this.ModuleToPlace != null)
                        {
                            Module mod = ModuleToPlace.Clone();
                            this.Map.Modules.Add(mod);

                            Graphics g = this.canvas.CreateGraphics();
                            g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);

                            mod.Draw(g, Scale);
                            this.GetSnapshot();
                            this.unsavedChanges = true;
                        }

                        break;
                    case MouseMode.Select:
                        // Get the selected area
                        Point NW = new Point(Math.Min(mouseUpTilePos.X, this.MouseDownTilePos.X), Math.Min(mouseUpTilePos.Y, this.MouseDownTilePos.Y));
                        Point SE = new Point(Math.Max(mouseUpTilePos.X, this.MouseDownTilePos.X) + 1, Math.Max(mouseUpTilePos.Y, this.MouseDownTilePos.Y) + 1);
                        Rectangle selectedRect = new Rectangle(NW.X, NW.Y, SE.X - NW.X, SE.Y - NW.Y);

                        // Find all modules in the specified rectangle
                        foreach (Module mod in Map.Modules)
                        {
                            if (!this.ctrlPressed)
                            {
                                // Ctrl key is not pressed - this is a brand-new selection
                                // If Ctrl key is pressed, we're adding to existing selection
                                mod.IsSelected = false;
                            }
                            if (mod.Overlaps(selectedRect))
                            {
                                mod.IsSelected = !mod.IsSelected;
                            }
                        }

                        // Redraw the map
                        DrawMap();
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
                    this.Map.Modules.Remove(this.Map.GetModuleAt(mouseUpTilePos));
                    DrawMap();
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
                        Graphics g = this.canvas.CreateGraphics();
                        g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);

                        // Draw the snapshot
                        DrawSnapshot();

                        // Draw the selection rectangle
                        SolidBrush selectedBrush = new SolidBrush(Color.FromArgb(127, 255, 0, 0));
                        Point NW = new Point(Math.Min(mouseMoveCurrTilePos.X, this.MouseDownTilePos.X), Math.Min(mouseMoveCurrTilePos.Y, this.MouseDownTilePos.Y));
                        Point SE = new Point(Math.Max(mouseMoveCurrTilePos.X, this.MouseDownTilePos.X) + 1, Math.Max(mouseMoveCurrTilePos.Y, this.MouseDownTilePos.Y) + 1);

                        g.FillRectangle(selectedBrush, NW.X * Scale, NW.Y * Scale, (SE.X - NW.X) * Scale, (SE.Y - NW.Y) * Scale);
                    }
                    break;
                case MouseMode.Draw:
                    // We're in "draw" mode - Show the module "in hand"
                    if (this.ModuleToPlace != null)
                    {
                        // Old pos to avoid drawing too much
                        Point oldPos = this.ModuleToPlace.Position;
                        this.ModuleToPlace.Position = mouseMoveCurrTilePos;

                        if (oldPos != mouseMoveCurrTilePos)
                        {
                            // Draw the selected module here
                            DrawSnapshot();
                            if (this.MouseDownTilePos.X >= 0 && this.MouseDownTilePos.Y >= 0 && !this.Map.IsTileOccupied(mouseMoveCurrTilePos))
                            {
                                // User is holding mouse button and moving - Paint this module right here (if not already occupied))
                                Module mod = ModuleToPlace.Clone();
                                this.Map.Modules.Add(mod);

                                Graphics g = this.canvas.CreateGraphics();
                                g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);

                                mod.Draw(g, Scale);
                                this.GetSnapshot();
                                this.unsavedChanges = true;
                            }
                            DrawModuleToPlace();
                        }
                    }
                    break;
                case MouseMode.Move:
                    // Move the selection
                    if (this.MouseMoveLastTilePos != mouseMoveCurrTilePos && this.LeftMouseButtonIsPressed)
                    {
                        int deltaX = mouseMoveCurrTilePos.X - this.MouseMoveLastTilePos.X;
                        int deltaY = mouseMoveCurrTilePos.Y - this.MouseMoveLastTilePos.Y;
                        foreach (Module mod in this.Map.SelectedModules)
                        {
                            // Move this module
                            Point modPos = mod.Position;
                            modPos.X += deltaX;
                            modPos.Y += deltaY;
                            mod.Position = modPos;
                        }
                        DrawMap();
                        this.unsavedChanges = true;
                    }
                    break;
                case MouseMode.Erase:
                    // We're in "erase" mode
                    if (this.LeftMouseButtonIsPressed)
                    {
                        // Button is pressed - Let's delete the module at this spot
                        Module mod = this.Map.GetModuleAt(mouseMoveCurrTilePos);
                        if (mod != null)
                        {
                            this.Map.Modules.Remove(mod);

                            // Draw this module's previously-occupied space as empty
                            //DrawGrid(mod.Position, mod.Width, mod.Height);
                            //GetSnapshot();
                            DrawMap();
                        }
                    }
                    break;
                default:
                    break;
            }

            // Store the last tile position to reduce redraws
            this.MouseMoveLastTilePos = mouseMoveCurrTilePos;
        }

        private void canvas_MouseEnter(object sender, EventArgs e)
        {
            switch (this.CursorMode)
            {
                case MouseMode.Draw:
                    this.Cursor = Cursors.Cross;
                    break;
                case MouseMode.Erase:
                    this.Cursor = Cursors.No;
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }
        private void canvas_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
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
                DrawMap();
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
                DrawMap();
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
            DrawMap();
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
            DrawMap();
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
            this.Preferences.LoadModulePacks();

            RefillListView(this.Preferences.ModulePacks, "");
        }
        #endregion


        #region Draw Methods

        private void DrawGrid(Point pos, int width, int height, Graphics g = null)
        {
            if (g == null)
            {
                g = this.canvas.CreateGraphics();
                g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);
            }

            // Clear the canvas
            g.FillRectangle(new SolidBrush(this.Preferences.GridBackgroundColor), new Rectangle(pos, new Size(Width * Scale, Height * Scale)));

            // Draw the backing grid
            Pen gridPen = new Pen(this.Preferences.GridLineColor, 1);
            Pen gridMajorPen = new Pen(this.Preferences.GridMajorLineColor, 3);

            // Horizontal and vertical lines drawn in the same loop
            for (int i = 0; i < width; i++)
            {
                Pen pen = i % this.Preferences.GridMajorCount == 0 ? gridMajorPen : gridPen;

                // Draw the horizontal line
                g.DrawLine(pen, new Point(pos.X, i * Scale), new Point(height * Scale, i * Scale));

                // Draw the vertical line
                g.DrawLine(pen, new Point(i * Scale, pos.Y), new Point(i * Scale, height * Scale));
            }
        }

        /// <summary>
        /// Draw a fresh render of the map (also saves a snapshot).
        /// </summary>
        private void DrawMap()
        {
            // Make sure the canvas is large enough
            this.canvas.AutoScrollMinSize = new Size(MaxMapSize * Scale, MaxMapSize * Scale);

            Graphics g = this.canvas.CreateGraphics();
            g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);

            DrawGrid(new Point(0, 0), MaxMapSize, MaxMapSize, g);

            // Draw each module
            Map.Modules.ForEach(mod => mod.Draw(g, Scale));

            Pen wallPen = new Pen(Color.Black, Scale / 15);

            foreach (Module mod in this.Map.Modules)
            {
                // Loop through the edge tiles of this module
                // North and South edges
                for (int col = 0; col < mod.Width; col++)
                {
                    // Check to the north of the northernmost tiles
                    if (!this.Map.IsTileOccupied(new Point(col + mod.Position.X, mod.Position.Y - 1)))
                    {
                        // The tile above this one is not occupied - Draw a wall North
                        g.DrawLine(wallPen, new Point((col + mod.Position.X) * Scale, mod.Position.Y * Scale), new Point((col + mod.Position.X + 1) * Scale, mod.Position.Y * Scale));
                    }

                    // Check to the south of the southernmost tiles
                    if (!this.Map.IsTileOccupied(new Point(col + mod.Position.X, mod.Position.Y + mod.Height)))
                    {
                        // The tile below this one is not occupied - Draw a wall South
                        g.DrawLine(wallPen, new Point((col + mod.Position.X) * Scale, (mod.Position.Y + mod.Height) * Scale), new Point((col + mod.Position.X + 1) * Scale, (mod.Position.Y + mod.Height) * Scale));
                    }
                }

                // East and West edges
                for (int row = 0; row < mod.Height; row++)
                {
                    // Check to the west of the westernmost tiles
                    if (!this.Map.IsTileOccupied(new Point(mod.Position.X - 1, row + mod.Position.Y)))
                    {
                        // The tile to the left of this one is not occupied - Draw a wall West
                        g.DrawLine(wallPen, new Point(mod.Position.X * Scale, (row + mod.Position.Y) * Scale), new Point(mod.Position.X * Scale, (row + mod.Position.Y + 1) * Scale));
                    }

                    // Check to the east of the easternmost tiles
                    if (!this.Map.IsTileOccupied(new Point(mod.Position.X + mod.Width, row + mod.Position.Y)))
                    {
                        // The tile to the right of this one is not occupied - Draw a wall East
                        g.DrawLine(wallPen, new Point((mod.Position.X + mod.Width) * Scale, (row + mod.Position.Y) * Scale), new Point((mod.Position.X + mod.Width) * Scale, (row + mod.Position.Y + 1) * Scale));
                    }
                }
            }

            // Get a snapshot of the rendered map
            this.GetSnapshot();
        }

        /// <summary>
        /// Saves a snapshot fo the rendered map (see also DrawSnapshot()).
        /// </summary>
        private void GetSnapshot()
        {
            Bitmap bitmap = new Bitmap(canvas.Width, canvas.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Point pScreen = canvas.PointToScreen(Point.Empty);
                g.CopyFromScreen(pScreen, Point.Empty, canvas.Size);
            }

            this.mapSnapshot = bitmap;
        }

        /// <summary>
        /// Draws the cached version of the map (much faster than running a new render each time).
        /// </summary>
        private void DrawSnapshot()
        {
            Graphics g = this.canvas.CreateGraphics();
            g.DrawImage(this.mapSnapshot, 0, 0);
        }

        /// <summary>
        /// Draw the module "in hand".
        /// </summary>
        private void DrawModuleToPlace()
        {
            if (ModuleToPlace != null)
            {
                Graphics g = this.canvas.CreateGraphics();
                g.TranslateTransform(canvas.AutoScrollPosition.X, canvas.AutoScrollPosition.Y);
                this.ModuleToPlace.Draw(g, Scale, 0.6f);
            }
        }
        #endregion

        private void mnuMainEditSelectAll_Click(object sender, EventArgs e)
        {
            this.Map.Modules.ForEach(m => m.IsSelected = true);
            DrawMap();
        }

        private void mnuMainFileOpen_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Open the map file the user requested
                this.Map = JsonConvert.DeserializeObject<Map>(System.IO.File.ReadAllText(ofd.FileName));
                DrawMap();
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
            DrawMap();
        }

        private void mnuMainEditRotateCW_Click(object sender, EventArgs e)
        {
            RotateClockwise();
        }

        private void mnuMainEditRotateCCW_Click(object sender, EventArgs e)
        {
            RotateCounterClockwise();
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
                            largeImages.Images.Add(pack.Name + "." + mod.Name, mod.GetThumbnail());
                            smallImages.Images.Add(pack.Name + "." + mod.Name, mod.GetThumbnail());
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
        
    }
}
