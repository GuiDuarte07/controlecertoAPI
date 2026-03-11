namespace ControleCerto.Services.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string key);
        Task DeleteFileAsync(string key);
    }
}
