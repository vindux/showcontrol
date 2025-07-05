# ShowControl

A WPF application for managing presentation slides and sending OSC (Open Sound Control) messages. ShowControl provides a dark-themed interface for controlling presentation slides with thumbnail previews and customizable button layouts.

## Features

- **JSON-based Configuration**: Load show data from JSON files with automatic file watching
- **Dynamic UI**: Responsive grid layout with configurable buttons per row
- **Thumbnail Support**: Display thumbnail images for slides and custom buttons
- **OSC Integration**: Send OSC messages via UDP for external system control
- **Dark Theme**: Modern dark UI with golden accents
- **Custom Buttons**: Footer area with up to 5 custom action buttons
- **Real-time Updates**: Automatic reload when JSON files are modified
- **Single File Deployment**: Self-contained executable with embedded resources

## Screenshots

The application features:
- Top bar with file selection and button configuration controls
- Main content area with chapter sections and slide thumbnails
- Footer with custom buttons and a prominent TAKE button
- Status display showing current event name and any error messages

## Requirements

- Windows 10/11 (x64)
- .NET 8.0 Runtime (only if using framework-dependent build)

## Installation

### Option 1: Self-Contained Build (Recommended)
1. Download the latest release from the releases page
2. Extract the ZIP file to your desired location
3. Run `ShowControl.exe` - no additional installation required

### Option 2: Build from Source
1. Clone the repository
2. Ensure you have .NET 8.0 SDK installed
3. Build the project:
   ```bash
   dotnet build -c Release
   ```
4. Or create a single-file deployment:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

## Usage

### Basic Operation
1. Launch ShowControl
2. Click "Select JSON File" to choose your show configuration
3. The interface will automatically populate with your slides and custom buttons
4. Click slide thumbnails to send OSC messages
5. Use custom buttons in the footer for special actions
6. Click "TAKE" to send a take command

### Button Layout
- Use the +/- buttons in the top bar to adjust buttons per row (6-20)
- The layout automatically adjusts to window size
- Manual adjustments are preserved when the JSON file updates

### File Watching
- The application monitors your JSON file for changes
- Interface updates automatically when the file is modified
- No need to manually reload after editing your show data

## JSON Configuration Format

```json
{
  "controlSettings": {
    "buttonsPerRow": 9
  },
  "event": "Your Event Name",
  "custom": [
    {
      "thumbnail": "/path/to/thumbnail.png",
      "title": "Custom Button",
      "templateData": {
        "@": "template/path",
        "property": "value"
      }
    }
  ],
  "content": [
    {
      "chapter": "Chapter Name",
      "slides": [
        {
          "thumbnail": "/path/to/slide/thumbnail.png",
          "title": "Slide Title",
          "templateData": {
            "@": "template/path",
            "property": "value"
          }
        }
      ]
    }
  ]
}
```

### Configuration Properties

- **controlSettings.buttonsPerRow**: Optional number of buttons per row (6-20)
- **event**: Display name shown in the top bar
- **custom**: Array of up to 5 custom buttons for the footer
- **content**: Array of chapters, each containing slides

### Thumbnail Paths
- Relative paths are resolved from the JSON file's directory
- Absolute paths are supported
- Missing images show a fallback placeholder

## OSC Messages

The application sends OSC messages via UDP to `127.0.0.1:18888` by default.

### Message Types

- **Slide Click**: `/slide` with title and templateData
- **Custom Button**: `/custom` with title and templateData  
- **Take Button**: `/take` (no parameters)

### OSC Message Format
Messages follow standard OSC format with:
- Address pattern (e.g., `/slide`)
- Type tags (`,ss` for two strings)
- String arguments (title, JSON templateData)

## Development

### Project Structure
```
ShowControl/
├── Constants/          # Application constants
├── Helpers/           # UI and Window helper classes
├── Models/            # Data models for JSON deserialization
├── Services/          # Core services (JSON, OSC, Thumbnails, FileWatcher)
├── Resources/         # Embedded images and assets
└── Properties/        # Publish profiles
```

### Key Components

- **MainWindow**: Primary UI and event handling
- **JsonService**: JSON file loading and validation
- **OscService**: OSC message creation and UDP transmission
- **ThumbnailService**: Image loading with fallback support
- **FileWatcherService**: Automatic file change detection

### Building

Standard .NET build process:
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Single file publish
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Configuration

### OSC Settings
Currently configured in `AppConstants.cs`:
- `DefaultOscHost`: Target IP address (default: 127.0.0.1)
- `DefaultOscPort`: Target port (default: 18888)

### UI Settings
Button layouts and sizes are defined in `AppConstants.cs`:
- Min/Max buttons per row: 6-20
- Button aspect ratio: 0.8 (height/width)
- Custom button size: 80x80 pixels

## Troubleshooting

### Common Issues

**JSON file not loading**: 
- Verify JSON syntax is valid
- Check file permissions
- Ensure all required properties are present

**Thumbnails not displaying**:
- Verify image paths are correct
- Check that image files exist
- Ensure images are accessible from JSON file location

**OSC messages not sending**:
- Verify target application is listening on correct port
- Check firewall settings
- Ensure UDP port 18888 is not blocked

### Debug Information
The application outputs debug information to the console, including:
- OSC message confirmations
- File loading status
- Error messages

## License

This project is provided as-is for educational and development purposes.

## Contributing

Feel free to submit issues and enhancement requests. When contributing:
1. Follow the existing code style
2. Add appropriate documentation
3. Test thoroughly before submitting
4. Include relevant screenshots for UI changes