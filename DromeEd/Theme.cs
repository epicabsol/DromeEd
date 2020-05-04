using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DromeEd
{
    public static class Theme
    {
        public static Color BackgroundColor = Color.FromArgb(50, 50, 50);
        public static Color ForegroundColor = Color.FromArgb(90, 90, 90);
        public static Color WellColor = Color.FromArgb(35, 35, 35);
        public static Color BorderColor = Color.FromArgb(70, 70, 70);
        public static Color ApplicationColor = Color.FromArgb(255, 114, 33);
        public static Color ApplicationColorLight = Color.FromArgb(255, 146, 84);
        public static Color TextColor = Color.FromArgb(200, 200, 200);
        public static Font TitleFont = new Font("Segoe UI Light", 30);
        public static Font TitleFontBold = new Font("Segoe UI Semibold", 30);
        public static Font StatusFont = new Font("Segoe UI", 12);

    }
}
