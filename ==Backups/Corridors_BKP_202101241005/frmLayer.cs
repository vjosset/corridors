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
    public partial class frmLayer : Form
    {
        public frmLayer()
        {
            InitializeComponent();
        }

        private MapLayer _origLayer = null;

        public MapLayer OrigLayer
        {
            get
            {
                return this._origLayer;
            }
            set
            {
                this._origLayer = value;
                this.Text = this.OrigLayer.Name + " - Settings";
                this.chkIsVisible.Checked = OrigLayer.IsVisible;
                this.chkShadows.Checked = OrigLayer.ShowShadows;
                this.txtNewLayerName.Text = this.OrigLayer.Name;
            }
        }

        public string NewLayerName { get; set; } = "";
        public bool NewVisible { get; set; } = true;
        public bool NewShowShadows { get; set; } = true;

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.NewLayerName = this.txtNewLayerName.Text;
            this.NewVisible = this.chkIsVisible.Checked;
            this.NewShowShadows = this.chkShadows.Checked;
            this.DialogResult = DialogResult.OK;
        }
    }
}
