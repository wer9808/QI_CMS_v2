using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace QI_CMS_v2.Utils
{
    public static class UIHelper
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null)
            {
                if (parent is T parentT)
                    return parentT;

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
