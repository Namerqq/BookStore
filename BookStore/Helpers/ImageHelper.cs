using System;
using System.IO;

namespace BookStore.Helpers
{
    public static class ImageHelper
    {
        public static string SaveProductImage(string sourceFilePath, int productId)
        {
            try
            {
                string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string extension = Path.GetExtension(sourceFilePath);
                string fileName = $"prod_{productId}_{DateTime.Now.Ticks}{extension}";
                string destFilePath = Path.Combine(targetFolder, fileName);

                File.Copy(sourceFilePath, destFilePath, true);
                return $"/Images/Products/{fileName}";
            }
            catch
            {
                return sourceFilePath;
            }
        }

        /// <summary>Chuyển đường dẫn ảnh lưu trong DB thành đường dẫn file hợp lệ cho WPF Image.Source.</summary>
        public static string? ResolveImagePath(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return null;

            string trimmed = imageUrl.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (Path.IsPathRooted(trimmed) && File.Exists(trimmed))
            {
                return trimmed;
            }

            string relative = trimmed.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            return trimmed;
        }
    }
}
