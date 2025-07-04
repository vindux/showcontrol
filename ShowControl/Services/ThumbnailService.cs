using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShowControl.Constants;

namespace ShowControl.Services
{
    /// <summary>
    /// Service for loading and generating thumbnail images with fallback support
    /// </summary>
    public class ThumbnailService
    {
        private readonly string _baseDirectory;

        /// <summary>
        /// Initializes a new instance of the ThumbnailService class
        /// </summary>
        /// <param name="baseDirectory">The base directory for resolving relative thumbnail paths</param>
        public ThumbnailService(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Loads a thumbnail image from the specified path with automatic fallback handling
        /// </summary>
        /// <param name="thumbnailPath">The relative or absolute path to the thumbnail image</param>
        /// <param name="width">The desired width of the loaded thumbnail</param>
        /// <param name="height">The desired height of the loaded thumbnail</param>
        /// <returns>A BitmapImage object containing the thumbnail, or a fallback image if loading fails</returns>
        public BitmapImage LoadThumbnail(string thumbnailPath, int width, int height)
        {
            try
            {
                if (string.IsNullOrEmpty(thumbnailPath))
                {
                    return LoadMissingImageThumbnail(width, height);
                }

                string fullPath = Path.Combine(_baseDirectory, thumbnailPath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = width;
                    bitmap.DecodePixelHeight = height;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                else
                {
                    return LoadMissingImageThumbnail(width, height);
                }
            }
            catch
            {
                return LoadMissingImageThumbnail(width, height);
            }
        }

        /// <summary>
        /// Attempts to load a missing image placeholder, falling back to a programmatically generated image
        /// </summary>
        /// <param name="width">The desired width of the placeholder image</param>
        /// <param name="height">The desired height of the placeholder image</param>
        /// <returns>A BitmapImage containing either the missing image file or a generated fallback</returns>
        private BitmapImage LoadMissingImageThumbnail(int width, int height)
        {
            try
            {
                string missingImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.MissingImageFileName);

                if (File.Exists(missingImagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(missingImagePath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = width;
                    bitmap.DecodePixelHeight = height;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                return CreateFallbackImage(width, height);
            }
            return CreateFallbackImage(width, height);
        }

        /// <summary>
        /// Creates a programmatically generated fallback image when no thumbnail is available
        /// </summary>
        /// <param name="width">The width of the fallback image</param>
        /// <param name="height">The height of the fallback image</param>
        /// <returns>A BitmapImage containing a grey background with an X symbol</returns>
        /// <exception cref="SystemException">Thrown when the fallback image cannot be created</exception>
        private BitmapImage CreateFallbackImage(int width, int height)
        {
            try
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    drawingContext.DrawRectangle(backgroundBrush, null, new Rect(0, 0, width, height));
            
                    Pen xPen = new Pen(new SolidColorBrush(Color.FromRgb(64, 64, 64)), 2);
                    double centerX = width / 2.0;
                    double centerY = height / 2.0;
                    double size = Math.Min(width, height) * 0.3;
            
                    drawingContext.DrawLine(xPen, 
                        new Point(centerX - size, centerY - size), 
                        new Point(centerX + size, centerY + size));
                    drawingContext.DrawLine(xPen, 
                        new Point(centerX + size, centerY - size), 
                        new Point(centerX - size, centerY + size));
                }
        
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(drawingVisual);
        
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    encoder.Save(memoryStream);
                    memoryStream.Position = 0;
            
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                }
        
                return bitmapImage;
            }
            catch
            {
                throw new SystemException("Failed to create fallback image");
            }
        }
    }
}