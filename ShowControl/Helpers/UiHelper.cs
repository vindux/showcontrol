using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ShowControl.Helpers
{
    /// <summary>
    /// Static helper class providing UI-related utilities and styling
    /// </summary>
    public static class UiHelper
    {
        /// <summary>
        /// Creates a flat button template without hover effects for consistent styling
        /// </summary>
        /// <returns>A ControlTemplate configured for flat button appearance</returns>
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

        /// <summary>
        /// Static class containing predefined color constants for consistent theming
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// Dark background color for main application areas
            /// </summary>
            public static readonly Color DarkBackground = Color.FromRgb(32, 32, 32);
            
            /// <summary>
            /// Dark gray color for header and footer areas
            /// </summary>
            public static readonly Color DarkGray = Color.FromRgb(45, 45, 45);
            
            /// <summary>
            /// Light gray color for borders and dividers
            /// </summary>
            public static readonly Color LightGray = Color.FromRgb(64, 64, 64);
            
            /// <summary>
            /// Background color for buttons and interactive elements
            /// </summary>
            public static readonly Color ButtonBackground = Color.FromRgb(50, 50, 50);
            
            /// <summary>
            /// Border color for buttons and interactive elements
            /// </summary>
            public static readonly Color ButtonBorder = Color.FromRgb(80, 80, 80);
            
            /// <summary>
            /// Golden yellow accent color for event title
            /// </summary>
            public static readonly Color GoldenYellow = Color.FromRgb(255, 225, 0);
            
            /// <summary>
            /// Darker golden color for highlights and emphasis
            /// </summary>
            public static readonly Color DarkerGolden = Color.FromRgb(204, 179, 0);
            
            /// <summary>
            /// Light red color for error messages and warnings
            /// </summary>
            public static readonly Color LightRed = Color.FromRgb(255, 100, 100);
            
            /// <summary>
            /// Light gray color for secondary text and labels
            /// </summary>
            public static readonly Color LightGrayText = Color.FromRgb(200, 200, 200);
        }
    }
}