/*
    * this files defines the interface for the storage service
    * it will be used to handle the storage of images for the vehicles and applications
    * it will be implemented by a class that will handle the actual storage (e.g. local / cloud storage)
    * it will be injected into the repositories to handle the storage of images for the vehicles and applications
    * it will define methods for uploading, retrieving and deleting images
    * it will be used by the controllers to handle the storage of images for the vehicles and applications
*/
using System.IO;
using System.Threading.Tasks;

namespace mmotors_back.Features.Shared.Interfaces
{
    public interface IStorageService
    {
        Task<(string Url, string Key)> UploadFileAsync(IFormFile file, string subfolder = "");
        Task DeleteFileAsync(string key, string subfolder = "");
        Task<Stream> GetFileAsync(string key, string subfolder = "");
        string GetFileUrl(string key, string subfolder = "");
    }
}