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

        private string LogPath
        {
            get
            {
                return Utils.GetExecutableFolder() + "\\log.txt";
            }
        }

        /// <summary>
        /// The current map.
        /// </summary>
        private Map Map => this.Canvas.Map;

        /// <summary>
        /// Gets or sets the current mouse/cursor mode.
        /// </summary>
        private MapCanvas.Mode CurrentMode
        {
            get
            {
                return this.Canvas.CurrentMode;
            }
            set
            {
                SetStatus("Setting mode " + value.ToString());
                this.Canvas.CurrentMode = value;

                btnModeEraser.Checked = value == MapCanvas.Mode.Erase;
                btnModeSelect.Checked = value == MapCanvas.Mode.Select || value == MapCanvas.Mode.Move;
                btnModeDraw.Checked = value == MapCanvas.Mode.Draw;
            }
        }
        #endregion

        #region Helper Methods

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
                if (this.Canvas.ModulesInHand != null)
                {
                    // We already had a module in hand, clear out the old one before building the new one
                    this.Canvas.ClearModulesInHand();
                }

                this.Canvas.ClearModulesInHand();
                MapModule modIH = new MapModule(mod);
                modIH.IsInHand = true;
                this.Canvas.ModulesInHand.Add(modIH);
                this.Canvas.ModulesInHand.ForEach(m => m.Invalidate());
                this.CurrentMode = MapCanvas.Mode.Draw;
            }
            else
            {
                // No selection
                this.Canvas.ClearModulesInHand();
                this.CurrentMode = MapCanvas.Mode.Select;
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
            frmPreferences prefsDialog = new frmPreferences(this.Preferences);
            if (prefsDialog.ShowDialog() == DialogResult.OK)
            {
                // User wants to save their preferences
                this.Preferences = prefsDialog.Preferences;

                // Save the preferences
                this.Preferences.SavePreferences();

                // Reset the canvas properties/colors
                this.Canvas.GridBackgroundColor = this.Preferences.GridBackgroundColor;
                this.Canvas.GridMajorLineColor = this.Preferences.GridMajorLineColor;
                this.Canvas.GridMinorLineColor = this.Preferences.GridMinorLineColor;
                this.Canvas.GridRoomSize = this.Preferences.GridRoomSize;
                this.Canvas.GridGutterSize = this.Preferences.GridGutterWidth;

                // Redraw the canvas
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
                this.Canvas.Clear();
            }
        }

        /// <summary>
        /// Event handler for Zoom In button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            this.Canvas.ZoomIn();
        }

        /// <summary>
        /// Event handler for Zoom Out button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            this.Canvas.ZoomOut();
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
        
        private void mnuMainEditSelectAll_Click(object sender, EventArgs e)
        {
            this.Canvas.CurrentMapLayer.Modules.ForEach(mod => mod.IsSelected = true);
            this.Canvas.Invalidate(false);
        }

        private void mnuMainFileOpen_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // Remove all the old modules
                this.Canvas.CurrentMapLayer.Modules.ForEach(mod => mod.IsSelected = true);
                this.Canvas.RemoveSelectedModules();

                // Open the map file the user requested
                this.Canvas.LoadMapFile(ofd.FileName);

                // Make sure we have references for each MapModule pointing to the correct ModulePack and Module
                foreach (MapLayer lay in this.Map.Layers)
                {
                    foreach (MapModule mapMod in lay.Modules)
                    {
                        if (mapMod.Module == null)
                        {
                            // Find the corresponding module in our loaded ModulePacks
                            foreach (ModulePack pack in this.Preferences.ModulePacks)
                            {
                                foreach (Module[] mods in pack.Categories.Values)
                                {
                                    foreach (Module mod in mods)
                                    {
                                        if (mod.Key == mapMod.ModuleKey)
                                        {
                                            // This is our module
                                            mapMod.Module = mod;
                                            mapMod.Invalidate();

                                            // Found it - we can stop this loop
                                            break;
                                        }
                                    }
                                    if (mapMod.Module != null)
                                    {
                                        // Found it - we can stop this loop
                                        break;
                                    }
                                }
                                if (mapMod.Module != null)
                                {
                                    // Found it - we can stop this loop
                                    break;
                                }
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
                this.Canvas.SaveMapFile(sfd.FileName);
                SetStatus("Map saved");
            }
        }

        private void mnuMainFileSaveMap_Click(object sender, EventArgs e)
        {
            if (this.mapFileName != string.Empty)
            {
                // This map was loaded from a file, just save it
                // Make sure nothing is marked as selected before we write the file
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
            this.Canvas.RemoveSelectedModules();
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
            this.Canvas.Clear();
            this.Canvas.Controls.Clear();
            this.Canvas.ClearModulesInHand();
            this.mapFileName = "";
            this.unsavedChanges = false;
            this.Canvas.Invalidate();
        }

        private void mnuMainEditRotateCW_Click(object sender, EventArgs e)
        {
            this.Canvas.RotateCW();
        }

        private void mnuMainEditRotateCCW_Click(object sender, EventArgs e)
        {
            this.Canvas.RotateCCW();
        }

        private void mnuMainEditMirrorVertical_Click(object sender, EventArgs e)
        {
            this.Canvas.MirrorVertical();
        }

        private void mnuMainEditMirrorHorizontal_Click(object sender, EventArgs e)
        {
            this.Canvas.MirrorHorizontal();
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
            this.CurrentMode = MapCanvas.Mode.Draw;
        }

        private void btnModeEraser_Click(object sender, EventArgs e)
        {
            this.CurrentMode = MapCanvas.Mode.Erase;
        }

        private void btnModeSelect_Click(object sender, EventArgs e)
        {
            this.CurrentMode = MapCanvas.Mode.Select;
        }

        private void mnuMainEditCopy_Click(object sender, EventArgs e)
        {
            // Make a clone of each selected module and put it in hand
            this.Canvas.ClearModulesInHand();
            foreach (MapModule mod in this.Canvas.CurrentMapLayer.SelectedModules)
            {
                MapModule mih = mod.Clone();
                mih.IsInHand = true;
                mih.IsSelected = false;
                mih.Invalidate();
                this.Canvas.ModulesInHand.Add(mih);
            }
            this.Canvas.CurrentMode = MapCanvas.Mode.Draw;
        }
    }
}
