namespace ShowControl.Constants
{
    public static class AppConstants
    {
        public const int MinButtonsPerRow = 6;
        public const int MaxButtonsPerRow = 20;
        public const int DefaultButtonsPerRow = 10;
        public const int DefaultWindowWidth = 1200;
        public const int FooterHeight = 100;
        public const int TopBarHeight = 75;
        public const int MaxCustomButtons = 5;
        
        public const string MissingImageFileName = "missing-image.png";
        public const string JsonFileFilter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
        public const string JsonFileDialogTitle = "Select Show Data JSON File";
        
        public const string NoFileSelectedText = "No file selected";
        public const string FilePrefix = "File: ";
        
        // Button sizes (calculated dynamically based on buttons per row)
        public const double ButtonAspectRatio = 0.8; // height/width ratio
        public const double ThumbnailSizeRatio = 0.85; // relative to button size
        public const double ThumbnailAspectRatio = 0.6; // height/width ratio
        
        // Custom button sizes (fixed for footer)
        public const int CustomButtonWidth = 80;
        public const int CustomButtonHeight = 80;
        public const int CustomButtonThumbnailWidth = 60;
        public const int CustomButtonThumbnailHeight = 50;
        
        // OSC Configuration
        public const string DefaultOscHost = "127.0.0.1";
        public const int DefaultOscPort = 18888;
    }
}