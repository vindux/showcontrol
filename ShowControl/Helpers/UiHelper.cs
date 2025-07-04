using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ShowControl.Helpers
{
    public static class UiHelper
    {
        public static ControlTemplate CreateFlatButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            
            // border without hover effect
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(60, 60, 60)));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(80, 80, 80)));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            
            // content presenter
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(TextElement.ForegroundProperty, new SolidColorBrush(System.Windows.Media.Colors.White));
            
            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            
            return template;
        }

        public static class Colors
        {
            public static readonly Color DarkBackground = Color.FromRgb(32, 32, 32);
            public static readonly Color DarkGray = Color.FromRgb(45, 45, 45);
            public static readonly Color LightGray = Color.FromRgb(64, 64, 64);
            public static readonly Color ButtonBackground = Color.FromRgb(50, 50, 50);
            public static readonly Color ButtonBorder = Color.FromRgb(80, 80, 80);
            public static readonly Color LightBlueAccent = Color.FromRgb(100, 150, 255);
            public static readonly Color LightRed = Color.FromRgb(255, 100, 100);
            public static readonly Color LightGrayText = Color.FromRgb(200, 200, 200);
        }
    }
}