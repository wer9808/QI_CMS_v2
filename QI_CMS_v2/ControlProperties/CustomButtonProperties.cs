using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace QI_CMS_v2.ControlProperties
{
    public static class CustomButtonProperties
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(CustomButtonProperties), new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(CustomButtonProperties), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.RegisterAttached("MouseOverForeground", typeof(Brush), typeof(CustomButtonProperties), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.RegisterAttached("CheckedBackground", typeof(Brush), typeof(CustomButtonProperties), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty CheckedForegroundProperty =
            DependencyProperty.RegisterAttached("CheckedForeground", typeof(Brush), typeof(CustomButtonProperties), new PropertyMetadata(Brushes.Black));

        public static void SetCornerRadius(UIElement element, CornerRadius value) => element.SetValue(CornerRadiusProperty, value);
        public static CornerRadius GetCornerRadius(UIElement element) => (CornerRadius)element.GetValue(CornerRadiusProperty);

        public static void SetMouseOverBackground(UIElement element, Brush value) => element.SetValue(MouseOverBackgroundProperty, value);
        public static Brush GetMouseOverBackground(UIElement element) => (Brush)element.GetValue(MouseOverBackgroundProperty);

        public static void SetMouseOverForeground(UIElement element, Brush value) => element.SetValue(MouseOverForegroundProperty, value);
        public static Brush GetMouseOverForeground(UIElement element) => (Brush)element.GetValue(MouseOverForegroundProperty);

        public static void SetCheckedBackground(UIElement element, Brush value) => element.SetValue(CheckedBackgroundProperty, value);
        public static Brush GetCheckedBackground(UIElement element) => (Brush)element.GetValue(CheckedBackgroundProperty);

        public static void SetCheckedForeground(UIElement element, Brush value) => element.SetValue(CheckedBackgroundProperty, value);
        public static Brush GetCheckedForeground(UIElement element) => (Brush)element.GetValue(CheckedBackgroundProperty);
    }

}
