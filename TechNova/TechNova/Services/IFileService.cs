
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TechNova.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            string webRoot = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );
            }

            string folderPath = Path.Combine(
                webRoot,
                folder.Replace('/', Path.DirectorySeparatorChar)
                      .Replace('\\', Path.DirectorySeparatorChar)
            );

            Directory.CreateDirectory(folderPath);

            string extension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;

            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + folder.Replace('\\', '/').Trim('/') + "/" + fileName;
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            string webRoot = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );
            }

            string relativePath = filePath.TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.GetFullPath(
                Path.Combine(webRoot, relativePath)
            );

            string rootPath = Path.GetFullPath(webRoot)
                .TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(
                rootPath,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
