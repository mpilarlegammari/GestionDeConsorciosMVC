using Microsoft.AspNetCore.Http;

namespace GestionDeConsorciosMVC.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string folderName);
    }
}