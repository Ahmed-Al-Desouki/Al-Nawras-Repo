using Al_Nawras.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            // Reads from appsettings.json — "FileStorage:BasePath"
            _basePath = configuration["FileStorage:BasePath"]
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(
            Stream fileStream,
            string fileName,
            string folder,
            CancellationToken cancellationToken = default)
        {
            // Build a unique filename to prevent overwrites
            var extension = Path.GetExtension(fileName);
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var relativePath = Path.Combine(folder, uniqueName).Replace("\\", "/");
            var absolutePath = Path.Combine(_basePath, relativePath);

            // Ensure the folder exists
            var directory = Path.GetDirectoryName(absolutePath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await using var fileOutput = new FileStream(absolutePath, FileMode.Create);
            await fileStream.CopyToAsync(fileOutput, cancellationToken);

            return relativePath;   // store relative — portable across environments
        }

        public Task<Stream?> OpenReadAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            var absolutePath = Path.Combine(_basePath, storagePath);
            if (!File.Exists(absolutePath))
                return Task.FromResult<Stream?>(null);

            Stream stream = new FileStream(
                absolutePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return Task.FromResult<Stream?>(stream);
        }

        public void Delete(string storagePath)
        {
            var absolutePath = Path.Combine(_basePath, storagePath);
            if (File.Exists(absolutePath))
                File.Delete(absolutePath);
        }
    }
}
