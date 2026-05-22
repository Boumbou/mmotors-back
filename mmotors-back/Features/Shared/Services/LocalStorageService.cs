/*
    * this file implement the local storage service for the application
    * it will be used to handle the storage of images for the vehicles and applications
    * it implements the IStorageService interface to ensure that it has the necessary methods for handling storage
*/

using System.IO;
using System.Threading.Tasks;
using mmotors_back.Features.Shared.Interfaces;  

namespace mmotors_back.Features.Shared.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _storagePath;

        public LocalStorageService(string storagePath)
        {
            _storagePath = storagePath;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<(string Url, string Key)> UploadFileAsync(IFormFile file,string subfolder = "")
        {
            string key = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_storagePath, subfolder, key);

            if (!string.IsNullOrEmpty(subfolder))
            {
                string subfolderPath = Path.Combine(_storagePath, subfolder);
                if (!Directory.Exists(subfolderPath))
                {
                    Directory.CreateDirectory(subfolderPath);
                }
            }
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStreamOutput);
            }

            return (Url: filePath, Key: key);
        }

        public Task DeleteFileAsync(string key, string subfolder = "")
        {
             string filePath = Path.Combine(_storagePath, subfolder, key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

        public Task<Stream> GetFileAsync(string key, string subfolder = "")
        {
            string filePath = Path.Combine(_storagePath, subfolder, key);
            if (File.Exists(filePath))
            {
                Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return Task.FromResult(fileStream);
            }
            throw new FileNotFoundException($"File with key {key} not found.");
        }

        public string GetFileUrl(string key, string subfolder = "")
        {
            string filePath = Path.Combine(_storagePath, subfolder, key);
            if (File.Exists(filePath))
            {
                return filePath;
            }
            throw new FileNotFoundException($"File with key {key} not found.");
        }
    }   
}
       