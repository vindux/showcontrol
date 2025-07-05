namespace ShowControl.Constants
{
    /// <summary>
    /// Static class containing application-wide constants for configuration and UI settings
    /// </summary>
    public static class AppConstants
    {
        #region Button Layout Constants

        /// <summary>
        /// Minimum number of buttons allowed per row in the main content area
        /// </summary>
        public const int MinButtonsPerRow = 6;

        /// <summary>
        /// Maximum number of buttons allowed per row in the main content area
        /// </summary>
        public const int MaxButtonsPerRow = 20;

        /// <summary>
        /// Default number of buttons per row when no setting is specified
        /// </summary>
        public const int DefaultButtonsPerRow = 10;

        #endregion

        #region UI Layout Constants

        /// <summary>
        /// Default width of the main application window in pixels
        /// </summary>
        public const int DefaultWindowWidth = 1200;

        /// <summary>
        /// Height of the footer area containing custom buttons and TAKE button
        /// </summary>
        public const int FooterHeight = 100;

        /// <summary>
        /// Height of the top bar containing controls and status information
        /// </summary>
        public const int TopBarHeight = 75;

        /// <summary>
        /// Maximum number of custom buttons that can be displayed in the footer
        /// </summary>
        public const int MaxCustomButtons = 5;

        #endregion

        #region File and Dialog Constants

        /// <summary>
        /// Filepath for the missing image placeholder
        /// </summary>
        public const string MissingImageFile = "pack://application:,,,/Resources/missing.png";

        /// <summary>
        /// File filter string for the JSON file selection dialog
        /// </summary>
        public const string JsonFileFilter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

        /// <summary>
        /// Title text for the JSON file selection dialog
        /// </summary>
        public const string JsonFileDialogTitle = "Select Show Data JSON File";

        #endregion

        #region UI Text Constants

        /// <summary>
        /// Text displayed when no JSON file is selected
        /// </summary>
        public const string NoFileSelectedText = "No file selected";

        /// <summary>
        /// Prefix text for displaying the selected file name
        /// </summary>
        public const string FilePrefix = "File: ";

        #endregion

        #region Dynamic Button Size Constants

        /// <summary>
        /// Aspect ratio for slide buttons (height/width ratio)
        /// </summary>
        public const double ButtonAspectRatio = 0.8;

        /// <summary>
        /// Size ratio of thumbnails relative to their containing buttons
        /// </summary>
        public const double ThumbnailSizeRatio = 0.85;

        /// <summary>
        /// Aspect ratio for thumbnail images (height/width ratio)
        /// </summary>
        public const double ThumbnailAspectRatio = 0.6;

        #endregion

        #region Fixed Custom Button Constants

        /// <summary>
        /// Fixed width for custom buttons in the footer area
        /// </summary>
        public const int CustomButtonWidth = 80;

        /// <summary>
        /// Fixed height for custom buttons in the footer area
        /// </summary>
        public const int CustomButtonHeight = 80;

        /// <summary>
        /// Fixed width for thumbnails in custom buttons
        /// </summary>
        public const int CustomButtonThumbnailWidth = 60;

        /// <summary>
        /// Fixed height for thumbnails in custom buttons
        /// </summary>
        public const int CustomButtonThumbnailHeight = 50;

        #endregion

        #region OSC Configuration Constants

        /// <summary>
        /// Default host IP address for OSC message transmission
        /// </summary>
        public const string DefaultOscHost = "127.0.0.1";

        /// <summary>
        /// Default port number for OSC message transmission
        /// </summary>
        public const int DefaultOscPort = 18888;

        #endregion
    }
}