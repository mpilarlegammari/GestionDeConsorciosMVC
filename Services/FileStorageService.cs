using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GestionDeConsorciosMVC.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveAsync(IFormFile file, string folderName)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadsPath);

            var originalFileName = Path.GetFileName(file.FileName);
            var safeFileName = MakeSafeFileName(originalFileName);
            var storedFileName = $"{Guid.NewGuid():N}-{safeFileName}";
            var absolutePath = Path.Combine(uploadsPath, storedFileName);

            await using var stream = File.Create(absolutePath);
            await file.CopyToAsync(stream);

            return $"/uploads/{folderName}/{storedFileName}";
        }

        private static string MakeSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeChars = fileName
                .Select(character => invalidChars.Contains(character) ? '-' : character)
                .ToArray();

            return new string(safeChars).Replace(' ', '-');
        }
    }
}