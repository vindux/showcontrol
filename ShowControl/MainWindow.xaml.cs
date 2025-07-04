using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ShowControl.Constants;
using ShowControl.Helpers;
using ShowControl.Models;
using ShowControl.Services;

namespace ShowControl
{
    public partial class MainWindow : Window
    {
        #region Private Fields
        
        private string _currentJsonPath = "";
        private FileWatcherService _fileWatcherService;
        private JsonService _jsonService;
        private ThumbnailService _thumbnailService;
        private OscService _oscService;
        
        private TextBlock _errorMessage;
        private TextBlock _eventNameLabel;
        private TextBlock _currentFileLabel;
        private TextBlock _buttonsPerRowLabel;
        private StackPanel _mainPanel;
        private StackPanel _customButtonsPanel;
        private int _buttonsPerRow = AppConstants.DefaultButtonsPerRow;
        private double _windowWidth = AppConstants.DefaultWindowWidth;
        private bool _userHasManuallyChangedButtonsPerRow = false;
        
        #endregion

        #region Dynamic Size Properties
        
        private int ButtonWidth => (int)((_windowWidth - 60) / _buttonsPerRow) - 10;
        private int ButtonHeight => (int)(ButtonWidth * AppConstants.ButtonAspectRatio);
        private int ThumbnailWidth => (int)(ButtonWidth * AppConstants.ThumbnailSizeRatio);
        private int ThumbnailHeight => (int)(ThumbnailWidth * AppConstants.ThumbnailAspectRatio);
        
        #endregion

        #region Constructor and Initialization
        
        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            CreateUi();
            
            SizeChanged += MainWindow_SizeChanged;
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Initializes all required services for the application
        /// </summary>
        private void InitializeServices()
        {
            _jsonService = new JsonService();
            _fileWatcherService = new FileWatcherService(OnFileChanged);
            _oscService = new OscService(AppConstants.DefaultOscHost, AppConstants.DefaultOscPort);
        }
        
        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles window size change events to update button layout
        /// </summary>
        /// <param name="sender">The window that triggered the event</param>
        /// <param name="e">Event arguments containing size information</param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowWidth = e.NewSize.Width;

            if (string.IsNullOrEmpty(_currentJsonPath)) return;
            
            if (_userHasManuallyChangedButtonsPerRow)
            {
                LoadShowDataWithManualButtonsPerRow();
            }
            else
            {
                LoadShowData();
            }
        }

        /// <summary>
        /// Handles window closing event to clean up resources
        /// </summary>
        /// <param name="sender">The window that triggered the event</param>
        /// <param name="e">Event arguments for window closing</param>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _fileWatcherService.Dispose();
            _oscService.Dispose();
        }
        
        #endregion

        #region UI Creation Methods
        
        /// <summary>
        /// Creates the main user interface layout programmatically
        /// </summary>
        private void CreateUi()
        {
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border topBar = CreateTopBar();
            Grid.SetRow(topBar, 0);

            ScrollViewer scrollViewer = CreateScrollViewer();
            Grid.SetRow(scrollViewer, 1);
            
            Border footer = CreateFooter();
            Grid.SetRow(footer, 2);

            mainGrid.Children.Add(topBar);
            mainGrid.Children.Add(scrollViewer);
            mainGrid.Children.Add(footer);

            Content = mainGrid;
            Background = new SolidColorBrush(UiHelper.Colors.DarkBackground);
        }

        /// <summary>
        /// Creates the top bar containing file controls and status information
        /// </summary>
        /// <returns>Border element containing the top bar layout</returns>
        private Border CreateTopBar()
        {
            Border topBarBorder = new Border
            {
                Background = new SolidColorBrush(UiHelper.Colors.DarkGray),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.LightGray),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Height = AppConstants.TopBarHeight
            };

            StackPanel topBarPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(UiHelper.Colors.DarkGray)
            };

            StackPanel controlsPanel = CreateControlsPanel();
            StackPanel statusPanel = CreateStatusPanel();

            topBarPanel.Children.Add(controlsPanel);
            topBarPanel.Children.Add(statusPanel);

            topBarBorder.Child = topBarPanel;
            return topBarBorder;
        }

        /// <summary>
        /// Creates the controls panel containing file selection and button configuration
        /// </summary>
        /// <returns>StackPanel containing the controls layout</returns>
        private StackPanel CreateControlsPanel()
        {
            StackPanel controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(0, 5, 0, 5)
            };

            Button selectFileButton = CreateSelectFileButton();
            _currentFileLabel = CreateCurrentFileLabel();
            var buttonsPerRowControls = CreateButtonsPerRowControls();

            controlsPanel.Children.Add(selectFileButton);
            controlsPanel.Children.Add(_currentFileLabel);
            foreach (UIElement control in buttonsPerRowControls)
            {
                controlsPanel.Children.Add(control);
            }

            return controlsPanel;
        }

        /// <summary>
        /// Creates the file selection button for choosing JSON files
        /// </summary>
        /// <returns>Button configured for file selection</returns>
        private Button CreateSelectFileButton()
        {
            Button button = new Button
            {
                Content = "Select JSON File",
                Width = 120,
                Height = 30,
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };
            button.Click += SelectFileButton_Click;
            return button;
        }

        /// <summary>
        /// Creates the label displaying the currently selected file name
        /// </summary>
        /// <returns>TextBlock configured for file name display</returns>
        private static TextBlock CreateCurrentFileLabel()
        {
            return new TextBlock
            {
                Text = AppConstants.NoFileSelectedText,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(UiHelper.Colors.LightGrayText)
            };
        }

        /// <summary>
        /// Creates the buttons per row control group with increase/decrease buttons
        /// </summary>
        /// <returns>Array of UI elements for buttons per row controls</returns>
        private UIElement[] CreateButtonsPerRowControls()
        {
            TextBlock buttonsPerRowLabel = new TextBlock
            {
                Text = "Buttons per row:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 5, 0),
                Foreground = new SolidColorBrush(Colors.White)
            };

            Button decreaseButton = new Button
            {
                Content = "-",
                Width = 25,
                Height = 25,
                Margin = new Thickness(0, 0, 2, 0),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };
            decreaseButton.Click += DecreaseButtonsPerRow_Click;

            _buttonsPerRowLabel = new TextBlock
            {
                Text = _buttonsPerRow.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                Foreground = new SolidColorBrush(Colors.White),
                MinWidth = 20,
                TextAlignment = TextAlignment.Center
            };

            Button increaseButton = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Margin = new Thickness(2, 0, 10, 0),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };
            increaseButton.Click += IncreaseButtonsPerRow_Click;

            return [buttonsPerRowLabel, decreaseButton, _buttonsPerRowLabel, increaseButton];
        }

        /// <summary>
        /// Creates the status panel containing event name and error message displays
        /// </summary>
        /// <returns>StackPanel containing the status layout</returns>
        private StackPanel CreateStatusPanel()
        {
            StackPanel statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 5)
            };

            _eventNameLabel = new TextBlock
            {
                Text = "",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 20, 0),
                Foreground = new SolidColorBrush(UiHelper.Colors.LightBlueAccent),
                Visibility = Visibility.Collapsed
            };

            _errorMessage = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(UiHelper.Colors.LightRed),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };

            statusPanel.Children.Add(_eventNameLabel);
            statusPanel.Children.Add(_errorMessage);

            return statusPanel;
        }

        /// <summary>
        /// Creates the scrollable content area for displaying slides
        /// </summary>
        /// <returns>ScrollViewer containing the main content panel</returns>
        private ScrollViewer CreateScrollViewer()
        {
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = new SolidColorBrush(UiHelper.Colors.DarkBackground)
            };

            _mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(UiHelper.Colors.DarkBackground)
            };

            scrollViewer.Content = _mainPanel;
            return scrollViewer;
        }

        /// <summary>
        /// Creates the footer containing custom buttons and the TAKE button
        /// </summary>
        /// <returns>Border element containing the footer layout</returns>
        private Border CreateFooter()
        {
            Border footerBorder = new Border
            {
                Background = new SolidColorBrush(UiHelper.Colors.DarkGray),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.LightGray),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = AppConstants.FooterHeight
            };

            Grid footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _customButtonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            };
            Grid.SetColumn(_customButtonsPanel, 0);

            Button takeButton = new Button
            {
                Content = "TAKE",
                Width = 100,
                Height = 70,
                Margin = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };
            takeButton.Click += TakeButton_Click;
            Grid.SetColumn(takeButton, 1);

            footerGrid.Children.Add(_customButtonsPanel);
            footerGrid.Children.Add(takeButton);
            footerBorder.Child = footerGrid;

            return footerBorder;
        }
        
        #endregion

        #region File Management Methods
        
        /// <summary>
        /// Handles the file selection button click event
        /// </summary>
        /// <param name="sender">The button that triggered the event</param>
        /// <param name="e">Event arguments for the click</param>
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = AppConstants.JsonFileFilter,
                Title = AppConstants.JsonFileDialogTitle
            };

            if (openFileDialog.ShowDialog() != true) return;

            _currentJsonPath = openFileDialog.FileName;
            _userHasManuallyChangedButtonsPerRow = false; // Reset when loading new file
            UpdateCurrentFileLabel();
            SetupFileWatcher();
            LoadShowData();
        }

        /// <summary>
        /// Updates the current file label with the selected file name
        /// </summary>
        private void UpdateCurrentFileLabel()
        {
            _currentFileLabel.Text = string.IsNullOrEmpty(_currentJsonPath) 
                ? AppConstants.NoFileSelectedText 
                : $"{AppConstants.FilePrefix}{Path.GetFileName(_currentJsonPath)}";
        }

        /// <summary>
        /// Sets up file system monitoring for the currently selected JSON file
        /// </summary>
        private void SetupFileWatcher()
        {
            try
            {
                _fileWatcherService.WatchFile(_currentJsonPath);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error setting up file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles file change notifications from the file watcher
        /// </summary>
        private void OnFileChanged()
        {
            Dispatcher.Invoke(() =>
            {
                // If user has manually changed buttons per row, preserve that setting
                if (_userHasManuallyChangedButtonsPerRow)
                {
                    LoadShowDataWithManualButtonsPerRow();
                }
                else
                {
                    LoadShowData();
                }
            });
        }
        
        #endregion

        #region Button Control Methods
        
        /// <summary>
        /// Handles the decrease buttons per row button click
        /// </summary>
        /// <param name="sender">The button that triggered the event</param>
        /// <param name="e">Event arguments for the click</param>
        private void DecreaseButtonsPerRow_Click(object sender, RoutedEventArgs e)
        {
            if (_buttonsPerRow <= AppConstants.MinButtonsPerRow) return;

            _buttonsPerRow--;
            _userHasManuallyChangedButtonsPerRow = true; // Mark as manually changed
            UpdateButtonsPerRowDisplay();
                
            if (!string.IsNullOrEmpty(_currentJsonPath))
            {
                LoadShowDataWithManualButtonsPerRow();
            }
        }

        /// <summary>
        /// Handles the increase buttons per row button click
        /// </summary>
        /// <param name="sender">The button that triggered the event</param>
        /// <param name="e">Event arguments for the click</param>
        private void IncreaseButtonsPerRow_Click(object sender, RoutedEventArgs e)
        {
            if (_buttonsPerRow >= AppConstants.MaxButtonsPerRow) return;
            
            _buttonsPerRow++;
            _userHasManuallyChangedButtonsPerRow = true; // Mark as manually changed
            UpdateButtonsPerRowDisplay();
                
            if (!string.IsNullOrEmpty(_currentJsonPath))
            {
                LoadShowDataWithManualButtonsPerRow();
            }
        }

        /// <summary>
        /// Updates the display of the current buttons per row value
        /// </summary>
        private void UpdateButtonsPerRowDisplay()
        {
            _buttonsPerRowLabel.Text = _buttonsPerRow.ToString();
        }
        
        #endregion

        #region Data Loading Methods
        
        /// <summary>
        /// Loads show data from the current JSON file and applies all settings
        /// </summary>
        private void LoadShowData()
        {
            try
            {
                ShowData showData = _jsonService.LoadShowData(_currentJsonPath);
                HideErrorMessage();
                BuildUi(showData);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads show data while preserving manual button per row settings
        /// </summary>
        private void LoadShowDataWithManualButtonsPerRow()
        {
            try
            {
                ShowData showData = _jsonService.LoadShowData(_currentJsonPath);
                HideErrorMessage();
                BuildUiWithManualSettings(showData);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading data: {ex.Message}");
            }
        }
        
        #endregion

        #region UI Building Methods
        
        /// <summary>
        /// Builds the complete UI from loaded show data, applying JSON settings
        /// </summary>
        /// <param name="showData">The show data to build the UI from</param>
        private void BuildUi(ShowData showData)
        {
            _mainPanel.Children.Clear();

            // Only apply JSON settings if user hasn't manually changed them
            if (!_userHasManuallyChangedButtonsPerRow && showData.ControlSettings.ButtonsPerRow.HasValue)
            {
                _buttonsPerRow = Math.Max(AppConstants.MinButtonsPerRow, 
                    Math.Min(AppConstants.MaxButtonsPerRow, showData.ControlSettings.ButtonsPerRow.Value));
                UpdateButtonsPerRowDisplay();
            }

            UpdateEventName(showData.Event);
            BuildCustomButtons(showData.Custom);
            AddChaptersToUi(showData.Content);
        }

        /// <summary>
        /// Builds the UI while preserving manual button per row settings
        /// </summary>
        /// <param name="showData">The show data to build the UI from</param>
        private void BuildUiWithManualSettings(ShowData showData)
        {
            _mainPanel.Children.Clear();
            UpdateEventName(showData.Event);
            BuildCustomButtons(showData.Custom);
            AddChaptersToUi(showData.Content);
        }

        /// <summary>
        /// Updates the event name display in the status bar
        /// </summary>
        /// <param name="eventName">The event name to display</param>
        private void UpdateEventName(string eventName)
        {
            _eventNameLabel.Text = eventName;
            _eventNameLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Builds custom buttons in the footer from the provided data
        /// </summary>
        /// <param name="customButtons">List of custom button data</param>
        private void BuildCustomButtons(List<CustomButton> customButtons)
        {
            _customButtonsPanel.Children.Clear();
            _thumbnailService = new ThumbnailService(Path.GetDirectoryName(_currentJsonPath) ?? AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i < Math.Min(AppConstants.MaxCustomButtons, customButtons.Count); i++)
            {
                Button customButton = CreateCustomButton(customButtons[i]);
                _customButtonsPanel.Children.Add(customButton);
            }
        }

        /// <summary>
        /// Creates a single custom button UI element
        /// </summary>
        /// <param name="customButtonData">The data for the custom button</param>
        /// <returns>Button element configured as a custom button</returns>
        private Button CreateCustomButton(CustomButton customButtonData)
        {
            Button button = new Button
            {
                Width = AppConstants.CustomButtonWidth,
                Height = AppConstants.CustomButtonHeight,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Image image = new Image
            {
                Width = AppConstants.CustomButtonThumbnailWidth,
                Height = AppConstants.CustomButtonThumbnailHeight,
                Stretch = Stretch.Uniform,
                Source = _thumbnailService.LoadThumbnail(customButtonData.Thumbnail, 
                    AppConstants.CustomButtonThumbnailWidth, AppConstants.CustomButtonThumbnailHeight)
            };
            stackPanel.Children.Add(image);

            TextBlock titleText = new TextBlock
            {
                Text = customButtonData.Title,
                FontSize = 10,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(1, 3, 1, 1),
                MaxWidth = 75,
                Foreground = new SolidColorBrush(Colors.White)
            };
            stackPanel.Children.Add(titleText);

            button.Content = stackPanel;
            button.Click += (_, _) => OnCustomButtonClick(customButtonData);

            return button;
        }

        /// <summary>
        /// Adds all chapters and their slides to the main UI
        /// </summary>
        /// <param name="chapters">List of chapters to add</param>
        private void AddChaptersToUi(List<Chapter> chapters)
        {
            _thumbnailService = new ThumbnailService(Path.GetDirectoryName(_currentJsonPath) ?? AppDomain.CurrentDomain.BaseDirectory);

            foreach (Chapter chapter in chapters)
            {
                AddChapterSection(chapter);
            }
        }

        /// <summary>
        /// Adds a single chapter section with its slides to the UI
        /// </summary>
        /// <param name="chapter">The chapter to add</param>
        private void AddChapterSection(Chapter chapter)
        {
            TextBlock chapterTitle = new TextBlock
            {
                Text = chapter.ChapterName,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(10, 20, 10, 10),
                Foreground = new SolidColorBrush(UiHelper.Colors.LightBlueAccent)
            };
            _mainPanel.Children.Add(chapterTitle);

            WrapPanel wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 0, 10, 20),
                Width = _windowWidth - 40,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            foreach (Button slideButton in chapter.Slides.Select(CreateSlideButton))
            {
                wrapPanel.Children.Add(slideButton);
            }

            _mainPanel.Children.Add(wrapPanel);
        }

        /// <summary>
        /// Creates a single slide button UI element
        /// </summary>
        /// <param name="slide">The slide data to create a button for</param>
        /// <returns>Button element configured as a slide button</returns>
        private Button CreateSlideButton(Slide slide)
        {
            Button button = new Button
            {
                Width = ButtonWidth,
                Height = ButtonHeight,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(UiHelper.Colors.ButtonBackground),
                BorderBrush = new SolidColorBrush(UiHelper.Colors.ButtonBorder),
                BorderThickness = new Thickness(1),
                Style = null,
                Template = UiHelper.CreateFlatButtonTemplate()
            };

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Image image = new Image
            {
                Width = ThumbnailWidth,
                Height = ThumbnailHeight,
                Stretch = Stretch.Uniform,
                Source = _thumbnailService.LoadThumbnail(slide.Thumbnail, ThumbnailWidth, ThumbnailHeight)
            };
            stackPanel.Children.Add(image);

            TextBlock titleText = new TextBlock
            {
                Text = slide.Title,
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2, 5, 2, 2),
                MaxWidth = ThumbnailWidth,
                Foreground = new SolidColorBrush(Colors.White)
            };
            stackPanel.Children.Add(titleText);

            button.Content = stackPanel;
            button.Click += (_, _) => OnSlideButtonClick(slide);

            return button;
        }
        
        #endregion

        #region Button Click Handlers
        
        /// <summary>
        /// Handles custom button click events and sends OSC messages
        /// </summary>
        /// <param name="customButton">The custom button that was clicked</param>
        private void OnCustomButtonClick(CustomButton customButton)
        {
            try
            {
                _oscService.SendCustomButtonMessage(customButton);
                
                // string templateDataJson = _jsonService.SerializeTemplateData(customButton.TemplateData);
                // MessageBox.Show($"Custom Button: {customButton.Title}\nOSC Message sent!\nTemplate Data:\n{templateDataJson}", 
                //     "Custom Button Clicked", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with custom button: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles slide button click events and sends OSC messages
        /// </summary>
        /// <param name="slide">The slide that was clicked</param>
        private void OnSlideButtonClick(Slide slide)
        {
            try
            {
                _oscService.SendSlideMessage(slide);
                
                // string templateDataJson = _jsonService.SerializeTemplateData(slide.TemplateData);
                // MessageBox.Show($"Slide: {slide.Title}\nOSC Message sent!\nTemplate Data:\n{templateDataJson}", 
                //     "Slide Clicked", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with slide: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the TAKE button click event and sends OSC message
        /// </summary>
        /// <param name="sender">The button that triggered the event</param>
        /// <param name="e">Event arguments for the click</param>
        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _oscService.SendTakeMessage();
                
                // MessageBox.Show("TAKE button clicked!\nOSC Message sent!", "Action", 
                //     MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with TAKE button: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion

        #region Error Handling Methods
        
        /// <summary>
        /// Displays an error message in the status bar
        /// </summary>
        /// <param name="message">The error message to display</param>
        private void ShowErrorMessage(string message)
        {
            _errorMessage.Text = message;
            _errorMessage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the error message in the status bar
        /// </summary>
        private void HideErrorMessage()
        {
            _errorMessage.Visibility = Visibility.Collapsed;
        }
        
        #endregion
    }
}