using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corridors
{
    public class DBPanel : Panel
    {
        public DBPanel()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //this.AutoScroll = false;
            base.OnMouseWheel(e);
            //this.AutoScroll = true;
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
