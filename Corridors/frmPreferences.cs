using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    public partial class frmPreferences : Form
    {
        public Preferences Preferences { get; set; }  = new Preferences();

        public frmPreferences()
        {
            InitializeComponent();
            Preferences = Preferences.LoadPreferences();
            cdpGridBackground.Color = Preferences.GridBackgroundColor;
            cdpGridMinorLine.Color = Preferences.GridMinorLineColor;
            cdpGridMajorLine.Color = Preferences.GridMajorLineColor;
            btnPickGridBGColor.BackColor = Preferences.GridBackgroundColor;
            btnPickGridMinorLineColor.BackColor = Preferences.GridMinorLineColor;
            btnPickGridMajorLineColor.BackColor = Preferences.GridMajorLineColor;
            txtGridRoomSize.Text = Preferences.GridRoomSize.ToString();
            txtGridGutterWidth.Text = Preferences.GridGutterWidth.ToString();
        }

        public frmPreferences(Preferences prefs)
        {
            InitializeComponent();
            Preferences = prefs;
            cdpGridBackground.Color = Preferences.GridBackgroundColor;
            cdpGridMinorLine.Color = Preferences.GridMinorLineColor;
            cdpGridMajorLine.Color = Preferences.GridMajorLineColor;
            btnPickGridBGColor.BackColor = Preferences.GridBackgroundColor;
            btnPickGridMinorLineColor.BackColor = Preferences.GridMinorLineColor;
            btnPickGridMajorLineColor.BackColor = Preferences.GridMajorLineColor;
            txtGridRoomSize.Text = Preferences.GridRoomSize.ToString();
            txtGridGutterWidth.Text = Preferences.GridGutterWidth.ToString();
        }

        private void btnPickGridBGColor_Click(object sender, EventArgs e)
        {
            if (cdpGridBackground.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridBackgroundColor = cdpGridBackground.Color;
                btnPickGridBGColor.BackColor = Preferences.GridBackgroundColor;
            }
        }

        private void btnPickGridLineColor_Click(object sender, EventArgs e)
        {
            if (cdpGridMinorLine.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridMinorLineColor = cdpGridMinorLine.Color;
                btnPickGridMinorLineColor.BackColor = Preferences.GridMinorLineColor;
            }
        }

        private void btnPickMajorGridLineColor_Click(object sender, EventArgs e)
        {
            if (cdpGridMajorLine.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridMajorLineColor = cdpGridMajorLine.Color;
                btnPickGridMajorLineColor.BackColor = Preferences.GridMajorLineColor;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Preferences.GridRoomSize = int.Parse(txtGridRoomSize.Text);
            this.Preferences.GridGutterWidth = int.Parse(txtGridGutterWidth.Text);
        }
    }
}
