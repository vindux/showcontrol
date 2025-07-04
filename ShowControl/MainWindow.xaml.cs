using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Documents;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace ShowControl
{
    public partial class MainWindow : Window
    {
        private const int MinButtonsPerRow = 3;
        private const int MaxButtonsPerRow = 10;
        private const int DefaultButtonsPerRow = 6;
        
        private string _currentJsonPath = "";
        private FileSystemWatcher _fileWatcher;
        private TextBlock _errorMessage;
        private TextBlock _eventNameLabel;
        private TextBlock _currentFileLabel;
        private TextBlock _buttonsPerRowLabel;
        private StackPanel _mainPanel;
        private StackPanel _customButtonsPanel;
        private int _buttonsPerRow = DefaultButtonsPerRow;
        private double _windowWidth = 1200; // Default window width
        
        // Dynamic sizes based on buttons per row
        private int ButtonWidth => (int)((_windowWidth - 60) / _buttonsPerRow) - 10; // 60px for margins, 10px for button margins
        private int ButtonHeight => (int)(ButtonWidth * 0.8); // 4:5 aspect ratio
        private int ThumbnailWidth => (int)(ButtonWidth * 0.85);
        private int ThumbnailHeight => (int)(ThumbnailWidth * 0.6);

        public MainWindow()
        {
            InitializeComponent();
            CreateUI();
            
            // Track window size changes
            this.SizeChanged += MainWindow_SizeChanged;
            
            // Cleanup when window closes
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowWidth = e.NewSize.Width;
            
            // Rebuild UI if we have data loaded
            if (!string.IsNullOrEmpty(_currentJsonPath))
            {
                LoadShowData();
            }
        }

        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle TAKE button click
            MessageBox.Show("TAKE button clicked!", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _fileWatcher?.Dispose();
        }

        private void CreateUI()
        {
            // Create the main grid
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Top bar
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Bottom footer

            // Create sticky top bar
            var topBarBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)), // Dark gray
                BorderBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64)), // Lighter gray border
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            Grid.SetRow(topBarBorder, 0);

            var topBarPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)) // Dark gray
            };

            // Create controls row
            var controlsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // File selection button
            var selectFileButton = new Button
            {
                Content = "Select JSON File",
                Width = 120,
                Height = 30,
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)), // Dark button
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1)
            };
            selectFileButton.Style = null;
            selectFileButton.Template = CreateFlatButtonTemplate();
            selectFileButton.Click += SelectFileButton_Click;

            // Current file path label
            _currentFileLabel = new TextBlock
            {
                Text = "No file selected",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)) // Light gray text
            };

            // Buttons per row label
            var buttonsPerRowLabel = new TextBlock
            {
                Text = "Buttons per row:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 5, 0),
                Foreground = new SolidColorBrush(Colors.White)
            };

            // Decrease buttons per row button
            var decreaseButton = new Button
            {
                Content = "-",
                Width = 25,
                Height = 25,
                Margin = new Thickness(0, 0, 2, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            decreaseButton.Style = null;
            decreaseButton.Template = CreateFlatButtonTemplate();
            decreaseButton.Click += DecreaseButtonsPerRow_Click;

            // Buttons per row value display
            _buttonsPerRowLabel = new TextBlock
            {
                Text = _buttonsPerRow.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                Foreground = new SolidColorBrush(Colors.White),
                MinWidth = 20,
                TextAlignment = TextAlignment.Center
            };

            // Increase buttons per row button
            var increaseButton = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Margin = new Thickness(2, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            increaseButton.Style = null;
            increaseButton.Template = CreateFlatButtonTemplate();
            increaseButton.Click += IncreaseButtonsPerRow_Click;

            controlsPanel.Children.Add(selectFileButton);
            controlsPanel.Children.Add(_currentFileLabel);
            controlsPanel.Children.Add(buttonsPerRowLabel);
            controlsPanel.Children.Add(decreaseButton);
            controlsPanel.Children.Add(_buttonsPerRowLabel);
            controlsPanel.Children.Add(increaseButton);

            // Create event name and error message row
            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Event name label
            _eventNameLabel = new TextBlock
            {
                Text = "",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 20, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)), // Light blue accent
                Visibility = Visibility.Collapsed
            };

            // Error message label
            _errorMessage = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100)), // Light red
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };

            statusPanel.Children.Add(_eventNameLabel);
            statusPanel.Children.Add(_errorMessage);

            // Add both panels to the top bar
            topBarPanel.Children.Add(controlsPanel);
            topBarPanel.Children.Add(statusPanel);

            topBarBorder.Child = topBarPanel;

            // Create scrollable content area
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)) // Dark background
            };
            Grid.SetRow(scrollViewer, 1);

            // Create main panel for content
            _mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)) // Dark background
            };

            scrollViewer.Content = _mainPanel;

            // Create sticky bottom footer
            var footerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)), // Dark gray
                BorderBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64)), // Lighter gray border
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 120 // Taller footer (1.5x of 80)
            };
            Grid.SetRow(footerBorder, 2);

            var footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Custom buttons area
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // TAKE button area

            // Left side - Custom buttons
            _customButtonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            };
            Grid.SetColumn(_customButtonsPanel, 0);

            // Right side - TAKE button
            var takeButton = new Button
            {
                Content = "TAKE",
                Width = 100,
                Height = 70,
                Margin = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            takeButton.Style = null;
            takeButton.Template = CreateFlatButtonTemplate();
            takeButton.Click += TakeButton_Click;
            Grid.SetColumn(takeButton, 1);

            footerGrid.Children.Add(_customButtonsPanel);
            footerGrid.Children.Add(takeButton);
            footerBorder.Child = footerGrid;

            // Add everything to the main grid
            mainGrid.Children.Add(topBarBorder);
            mainGrid.Children.Add(scrollViewer);
            mainGrid.Children.Add(footerBorder);

            // Set the main grid as the window content
            this.Content = mainGrid;
            
            // Set window background to dark
            this.Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select Show Data JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentJsonPath = openFileDialog.FileName;
                UpdateCurrentFileLabel();
                SetupFileWatcher();
                LoadShowData();
            }
        }

        private void UpdateCurrentFileLabel()
        {
            if (_currentFileLabel != null)
            {
                if (string.IsNullOrEmpty(_currentJsonPath))
                {
                    _currentFileLabel.Text = "No file selected";
                }
                else
                {
                    _currentFileLabel.Text = $"File: {Path.GetFileName(_currentJsonPath)}";
                }
            }
        }

        private void DecreaseButtonsPerRow_Click(object sender, RoutedEventArgs e)
        {
            if (_buttonsPerRow > MinButtonsPerRow)
            {
                _buttonsPerRow--;
                UpdateButtonsPerRowDisplay();
                
                // Rebuild UI if we have data loaded
                if (!string.IsNullOrEmpty(_currentJsonPath))
                {
                    // Reload the data but keep the user's manual setting
                    LoadShowDataWithManualButtonsPerRow();
                }
            }
        }

        private void IncreaseButtonsPerRow_Click(object sender, RoutedEventArgs e)
        {
            if (_buttonsPerRow < MaxButtonsPerRow)
            {
                _buttonsPerRow++;
                UpdateButtonsPerRowDisplay();
                
                // Rebuild UI if we have data loaded
                if (!string.IsNullOrEmpty(_currentJsonPath))
                {
                    // Reload the data but keep the user's manual setting
                    LoadShowDataWithManualButtonsPerRow();
                }
            }
        }

        private void LoadShowDataWithManualButtonsPerRow()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentJsonPath))
                {
                    ShowErrorMessage("No JSON file selected");
                    return;
                }

                if (!File.Exists(_currentJsonPath))
                {
                    ShowErrorMessage($"Selected file not found: {_currentJsonPath}");
                    return;
                }

                string json = File.ReadAllText(_currentJsonPath);
                
                // Validate JSON before attempting to build UI
                if (!IsValidJson(json))
                {
                    ShowErrorMessage("Invalid JSON file - please check the file format");
                    return;
                }

                var showData = JsonConvert.DeserializeObject<ShowData>(json);
                
                // If we got here, JSON is valid - hide any error messages
                HideErrorMessage();
                
                // Build UI without applying JSON control settings (keep manual setting)
                BuildUIWithManualSettings(showData);
            }
            catch (JsonException jsonEx)
            {
                ShowErrorMessage($"Invalid JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading data: {ex.Message}");
            }
        }

        private void BuildUIWithManualSettings(ShowData showData)
        {
            // Clear the scrollable content area only
            _mainPanel.Children.Clear();

            // Don't apply JSON control settings - keep the user's manual setting
            // The _buttonsPerRow value is already set by the +/- buttons

            // Update event name in top bar
            if (_eventNameLabel != null)
            {
                _eventNameLabel.Text = showData.Event;
                _eventNameLabel.Visibility = Visibility.Visible;
            }

            // Build custom buttons in footer
            BuildCustomButtons(showData.Custom);

            // Add chapters to the scrollable area
            foreach (var chapter in showData.Content)
            {
                AddChapterSection(chapter);
            }
        }

        private void UpdateButtonsPerRowDisplay()
        {
            if (_buttonsPerRowLabel != null)
            {
                _buttonsPerRowLabel.Text = _buttonsPerRow.ToString();
            }
        }

        private void SetupFileWatcher()
        {
            // Dispose existing watcher
            _fileWatcher?.Dispose();

            if (string.IsNullOrEmpty(_currentJsonPath) || !File.Exists(_currentJsonPath))
                return;

            try
            {
                _fileWatcher = new FileSystemWatcher();
                _fileWatcher.Path = Path.GetDirectoryName(_currentJsonPath);
                _fileWatcher.Filter = Path.GetFileName(_currentJsonPath);
                _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error setting up file watcher: {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Use dispatcher to update UI on main thread
            Dispatcher.Invoke(() =>
            {
                // Small delay to ensure file is fully written
                System.Threading.Thread.Sleep(100);
                LoadShowData();
            });
        }

        private void ShowErrorMessage(string message)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Text = message;
                _errorMessage.Visibility = Visibility.Visible;
            }
        }

        private void HideErrorMessage()
        {
            if (_errorMessage != null)
            {
                _errorMessage.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsValidJson(string jsonContent)
        {
            try
            {
                var testDeserialize = JsonConvert.DeserializeObject<ShowData>(jsonContent);
                return testDeserialize != null;
            }
            catch
            {
                return false;
            }
        }

        private void LoadShowData()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentJsonPath))
                {
                    ShowErrorMessage("No JSON file selected");
                    return;
                }

                if (!File.Exists(_currentJsonPath))
                {
                    ShowErrorMessage($"Selected file not found: {_currentJsonPath}");
                    return;
                }

                string json = File.ReadAllText(_currentJsonPath);
                
                // Validate JSON before attempting to build UI
                if (!IsValidJson(json))
                {
                    ShowErrorMessage("Invalid JSON file - please check the file format");
                    return;
                }

                var showData = JsonConvert.DeserializeObject<ShowData>(json);
                
                // If we got here, JSON is valid - hide any error messages
                HideErrorMessage();
                
                BuildUI(showData);
            }
            catch (JsonException jsonEx)
            {
                ShowErrorMessage($"Invalid JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading data: {ex.Message}");
            }
        }

        private void BuildUI(ShowData showData)
        {
            // Clear the scrollable content area only
            _mainPanel.Children.Clear();

            // Apply control settings if available
            if (showData.ControlSettings != null)
            {
                if (showData.ControlSettings.ButtonsPerRow.HasValue)
                {
                    _buttonsPerRow = Math.Max(MinButtonsPerRow, Math.Min(MaxButtonsPerRow, showData.ControlSettings.ButtonsPerRow.Value));
                    UpdateButtonsPerRowDisplay();
                }
            }

            // Update event name in top bar
            if (_eventNameLabel != null)
            {
                _eventNameLabel.Text = showData.Event;
                _eventNameLabel.Visibility = Visibility.Visible;
            }

            // Build custom buttons in footer
            BuildCustomButtons(showData.Custom);

            // Add chapters to the scrollable area
            foreach (var chapter in showData.Content)
            {
                AddChapterSection(chapter);
            }
        }

        private void BuildCustomButtons(List<CustomButton> customButtons)
        {
            if (_customButtonsPanel == null || customButtons == null) return;

            // Clear existing custom buttons
            _customButtonsPanel.Children.Clear();

            // Add up to 5 custom buttons
            for (int i = 0; i < Math.Min(5, customButtons.Count); i++)
            {
                var customButton = CreateCustomButton(customButtons[i]);
                _customButtonsPanel.Children.Add(customButton);
            }
        }

        private Button CreateCustomButton(CustomButton customButtonData)
        {
            var button = new Button
            {
                Width = 80,  // Slightly larger for taller footer
                Height = 80,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1)
            };

            button.Style = null;
            button.Template = CreateFlatButtonTemplate();

            // Create button content
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Add thumbnail
            var image = new Image
            {
                Width = 60,  // Larger for taller footer
                Height = 50,
                Stretch = Stretch.Uniform,
                Source = LoadCustomThumbnail(customButtonData.Thumbnail)
            };
            stackPanel.Children.Add(image);

            // Add title
            var titleText = new TextBlock
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

            // Add click handler
            button.Click += (sender, e) => OnCustomButtonClick(customButtonData);

            return button;
        }

        private BitmapImage LoadCustomThumbnail(string thumbnailPath)
        {
            try
            {
                if (string.IsNullOrEmpty(thumbnailPath))
                {
                    return LoadMissingCustomThumbnail();
                }

                // Try to load the thumbnail - use directory of the JSON file as base
                string baseDirectory = Path.GetDirectoryName(_currentJsonPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDirectory, thumbnailPath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = 60;
                    bitmap.DecodePixelHeight = 50;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                else
                {
                    return LoadMissingCustomThumbnail();
                }
            }
            catch
            {
                return LoadMissingCustomThumbnail();
            }
        }

        private BitmapImage LoadMissingCustomThumbnail()
        {
            try
            {
                // Try to load missing image from resources
                string missingImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missing-image.png");

                if (File.Exists(missingImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(missingImagePath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = 60;
                    bitmap.DecodePixelHeight = 50;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                // Ignore and return null
            }
            return null;
        }

        private void OnCustomButtonClick(CustomButton customButton)
        {
            try
            {
                string templateDataJson = JsonConvert.SerializeObject(customButton.TemplateData, Formatting.None);
                Console.WriteLine($"Custom button clicked: {templateDataJson}");

                // Also show in a message box for debugging
                MessageBox.Show($"Custom Button: {customButton.Title}\nTemplate Data:\n{templateDataJson}", 
                    "Custom Button Clicked", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error serializing custom button template data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddChapterSection(Chapter chapter)
        {
            // Chapter title
            var chapterTitle = new TextBlock
            {
                Text = chapter.ChapterName,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(10, 20, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)) // Light blue accent
            };
            _mainPanel.Children.Add(chapterTitle);

            // Create wrap panel for buttons with fixed width
            var wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 0, 10, 20),
                Width = _windowWidth - 40, // Account for margins
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Add slide buttons
            foreach (var slide in chapter.Slides)
            {
                var slideButton = CreateSlideButton(slide);
                wrapPanel.Children.Add(slideButton);
            }

            _mainPanel.Children.Add(wrapPanel);
        }

        private Button CreateSlideButton(Slide slide)
        {
            var button = new Button
            {
                Width = ButtonWidth,
                Height = ButtonHeight,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)), // Dark button background
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)), // Gray border
                BorderThickness = new Thickness(1)
            };

            // Simple approach: just remove the default button style
            button.Style = null;
            button.Template = CreateFlatButtonTemplate();

            // Create button content
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Add thumbnail
            var image = new Image
            {
                Width = ThumbnailWidth,
                Height = ThumbnailHeight,
                Stretch = Stretch.Uniform,
                Source = LoadThumbnail(slide.Thumbnail)
            };
            stackPanel.Children.Add(image);

            // Add slide title
            var titleText = new TextBlock
            {
                Text = slide.Title,
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2, 5, 2, 2),
                MaxWidth = ThumbnailWidth,
                Foreground = new SolidColorBrush(Colors.White) // White text
            };
            stackPanel.Children.Add(titleText);

            button.Content = stackPanel;

            // Add click handler
            button.Click += (sender, e) => OnSlideButtonClick(slide);

            return button;
        }

        private ControlTemplate CreateFlatButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            
            // Create simple border without hover effects
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(60, 60, 60)));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(80, 80, 80)));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            
            // Add content presenter
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.White));
            
            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            
            return template;
        }

        private BitmapImage LoadThumbnail(string thumbnailPath)
        {
            try
            {
                if (string.IsNullOrEmpty(thumbnailPath))
                {
                    return LoadMissingImageThumbnail();
                }

                // Try to load the thumbnail - use directory of the JSON file as base
                string baseDirectory = Path.GetDirectoryName(_currentJsonPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDirectory, thumbnailPath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = ThumbnailWidth;
                    bitmap.DecodePixelHeight = ThumbnailHeight;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                else
                {
                    return LoadMissingImageThumbnail();
                }
            }
            catch
            {
                return LoadMissingImageThumbnail();
            }
        }

        private BitmapImage LoadMissingImageThumbnail()
        {
            try
            {
                // Try to load missing image from resources
                string missingImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missing-image.png");

                if (File.Exists(missingImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(missingImagePath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = ThumbnailWidth;
                    bitmap.DecodePixelHeight = ThumbnailHeight;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                else
                {
                    // Create a simple placeholder if missing-image.png doesn't exist
                    return CreatePlaceholderImage();
                }
            }
            catch
            {
                return CreatePlaceholderImage();
            }
        }

        private BitmapImage CreatePlaceholderImage()
        {
            // This would create a simple colored rectangle as placeholder
            // For now, returning null will show empty space
            return null;
        }

        private void OnSlideButtonClick(Slide slide)
        {
            try
            {
                string templateDataJson = JsonConvert.SerializeObject(slide.TemplateData, Formatting.None);
                Console.WriteLine(templateDataJson);

                // Also show in a message box for debugging
                MessageBox.Show($"Template Data:\n{templateDataJson}", "Slide Clicked", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error serializing template data: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    // Data models
    public class ShowData
    {
        [JsonProperty("controlSettings")] public ControlSettings ControlSettings { get; set; }

        [JsonProperty("event")] public string Event { get; set; }

        [JsonProperty("custom")] public List<CustomButton> Custom { get; set; }

        [JsonProperty("content")] public List<Chapter> Content { get; set; }
    }

    public class ControlSettings
    {
        [JsonProperty("buttonsPerRow")] public int? ButtonsPerRow { get; set; }
    }

    public class CustomButton
    {
        [JsonProperty("thumbnail")] public string Thumbnail { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("templateData")] public object TemplateData { get; set; }
    }

    public class Chapter
    {
        [JsonProperty("chapter")] public string ChapterName { get; set; }

        [JsonProperty("slides")] public List<Slide> Slides { get; set; }
    }

    public class Slide
    {
        [JsonProperty("thumbnail")] public string Thumbnail { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("templateData")] public object TemplateData { get; set; }
    }
}