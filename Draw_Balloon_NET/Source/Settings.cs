using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw_Balloon_net
{
    public class Settings
    {
        #region Default Value
        const string DefaultStr = "A";
        const double DefaultValue = 10;

        readonly List<string> lstColorMap = new List<string>(new string[] { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta", "White", "Gray" });
        #endregion


        #region Constructor
        private Settings()
        {
            // set the default value.
            IsSelectedAny = false;
            IsSelectedAll = true;

            Text = DefaultStr;
            Diameter = DefaultValue;

            ColorText = "Red";
            ColorLine = "Red";
            ColorCircle = "Red";

            IndexColorText = convertStringColorToIndex(ColorText);
            IndexColorLine = convertStringColorToIndex(ColorLine);
            IndexColorCircle = convertStringColorToIndex(ColorCircle);
        }
        #endregion


        #region Single instance
        private static Settings setting = null;

        public static Settings getInstance()
        {
            if (setting == null)
            {
                setting = new Settings();
            }

            return setting;
        }
        #endregion


        #region Properties
        public bool IsSelectedAny { get; set; }

        public bool IsSelectedAll { get; set; }

        public string Text { get; set; }

        public double Diameter { get; set; }

        public string ColorText { get; set; }

        public string ColorCircle { get; set; }

        public string ColorLine { get; set; }

        public int IndexColorText { get; set; }

        public int IndexColorLine { get; set; }

        public int IndexColorCircle { get; set; }
        #endregion


        #region Methods
        public int convertStringColorToIndex(string strColor)
        {
            int indexColor = 0;

            for (int i = 0; i < lstColorMap.Count; ++i)
            {
                if (lstColorMap[i] == strColor)
                {
                    indexColor = i + 1;
                }
            }           

            return indexColor;
        }

        #endregion
    }
}
