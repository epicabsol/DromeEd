using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Controls
{
    public class FocusZone
    {
        public static FocusZone Current { get; private set; } = null;
        //private static List<FocusZone> Zones = new List<FocusZone>();
        public static void Focus(FocusZone zone)
        {
            Current?.Unfocused?.Invoke(zone, new EventArgs());
            Current = zone;
            zone?.Focused?.Invoke(null, new EventArgs());
        }

        public FocusZone()
        {
            //Zones.Add(this);
            if (Current == null)
                Focus(this);
        }

        public void Focus()
        {
            Focus(this);
        }

        public void Unfocus()
        {
            Focus(null);
        }

        public bool IsFocused { get { return this.Equals(Current); } }

        public event EventHandler Focused = null;
        public event EventHandler Unfocused = null;

        public void Bind(System.Windows.Forms.Control control)
        {
            control.MouseDown += BoundControlClicked;
            control.ControlAdded += BoundControlChildAdded;
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                Bind(child);
            }
        }

        private void BoundControlClicked(object sender, System.Windows.Forms.MouseEventArgs args)
        {
            Focus();
        }

        private void BoundControlChildAdded(object sender, System.Windows.Forms.ControlEventArgs e)
        {
            Bind(e.Control);
        }
    }
}
