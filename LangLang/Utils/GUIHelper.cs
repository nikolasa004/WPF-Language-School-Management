using System.Collections.Generic;
using System.Windows.Controls;

namespace LangLang.Utils
{
    public class GUIHelper
    {
        public static void ChangePanel(List<Grid> panels, int activateIndex)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (i == activateIndex)
                    panels[i].Visibility = System.Windows.Visibility.Visible;
                else
                    panels[i].Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
