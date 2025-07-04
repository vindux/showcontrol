using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShowControl.Constants;

namespace ShowControl.Services
{
    public class ThumbnailService
    {
        private readonly string _baseDirectory;

        public ThumbnailService(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

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

        private BitmapImage CreateFallbackImage(int width, int height)
        {
            try
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    // Grey background
                    SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    drawingContext.DrawRectangle(backgroundBrush, null, new Rect(0, 0, width, height));
            
                    // Draw an "X" in the middle
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
        
                // Render to bitmap
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(drawingVisual);
        
                // Convert to BitmapImage
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