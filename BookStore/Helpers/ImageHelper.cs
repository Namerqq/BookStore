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
    }
}