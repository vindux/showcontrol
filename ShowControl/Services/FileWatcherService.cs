using System.IO;

namespace ShowControl.Services
{
    public class FileWatcherService : IDisposable
    {
        private FileSystemWatcher _fileWatcher;
        private readonly Action _onFileChanged;

        public FileWatcherService(Action onFileChanged)
        {
            _onFileChanged = onFileChanged;
        }

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

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Small delay to ensure file is fully written
            Thread.Sleep(100);
            _onFileChanged?.Invoke();
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
            _fileWatcher = null;
        }
    }
}