using System.IO;

namespace ShowControl.Services
{
    /// <summary>
    /// Service for monitoring file changes and triggering callbacks when files are modified
    /// </summary>
    public class FileWatcherService : IDisposable
    {
        private FileSystemWatcher _fileWatcher;
        private readonly Action _onFileChanged;

        /// <summary>
        /// Initializes a new instance of the FileWatcherService class
        /// </summary>
        /// <param name="onFileChanged">Callback action to execute when a file change is detected</param>
        public FileWatcherService(Action onFileChanged)
        {
            _onFileChanged = onFileChanged;
        }

        /// <summary>
        /// Starts watching the specified file for changes
        /// </summary>
        /// <param name="filePath">The full path to the file to watch</param>
        /// <exception cref="Exception">Thrown when the file watcher cannot be set up</exception>
        public void WatchFile(string filePath)
        {
            // Dispose existing watcher
            Dispose();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            try
            {
                _fileWatcher = new FileSystemWatcher();
                _fileWatcher.Path = Path.GetDirectoryName(filePath);
                _fileWatcher.Filter = Path.GetFileName(filePath);
                _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting up file watcher: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Handles file change events from the FileSystemWatcher
        /// </summary>
        /// <param name="sender">The FileSystemWatcher that raised the event</param>
        /// <param name="e">Event arguments containing file change information</param>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Small delay to ensure file is fully written
            Thread.Sleep(100);
            _onFileChanged?.Invoke();
        }

        /// <summary>
        /// Disposes of the file watcher and releases resources
        /// </summary>
        public void Dispose()
        {
            _fileWatcher?.Dispose();
            _fileWatcher = null;
        }
    }
}