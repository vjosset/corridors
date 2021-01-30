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
            cdpGridBG.Color = Preferences.GridBackgroundColor;
            cdpGridLine.Color = Preferences.GridLineColor;
            btnPickGridBGColor.BackColor = Preferences.GridBackgroundColor;
            btnPickGridLineColor.BackColor = Preferences.GridLineColor;
            btnPickMajorGridLineColor.BackColor = Preferences.GridMajorLineColor;
            txtGridMajorCount.Text = Preferences.GridMajorCount.ToString();
            txtGridGutterWidth.Text = Preferences.GridGutterWidth.ToString();
        }

        private void btnPickGridBGColor_Click(object sender, EventArgs e)
        {
            if (cdpGridBG.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridBackgroundColor = cdpGridBG.Color;
                btnPickGridBGColor.BackColor = Preferences.GridBackgroundColor;
            }
        }

        private void btnPickGridLineColor_Click(object sender, EventArgs e)
        {
            if (cdpGridLine.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridLineColor = cdpGridLine.Color;
                btnPickGridLineColor.BackColor = Preferences.GridLineColor;
            }
        }

        private void btnPickMajorGridLineColor_Click(object sender, EventArgs e)
        {
            if (cdpMajorGridLine.ShowDialog() == DialogResult.OK)
            {
                this.Preferences.GridMajorLineColor = cdpMajorGridLine.Color;
                btnPickMajorGridLineColor.BackColor = Preferences.GridMajorLineColor;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Preferences.GridMajorCount = int.Parse(txtGridMajorCount.Text);
            this.Preferences.GridGutterWidth = int.Parse(txtGridGutterWidth.Text);
        }
    }
}
