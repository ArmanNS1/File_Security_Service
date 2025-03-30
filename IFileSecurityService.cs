using Microsoft.AspNetCore.Http;

namespace File_Security_Service
{
    public interface IFileSecurityService
    {
        Task<byte[]> DecryptFileAsync(byte[] encryptedData);
        Task<byte[]> EncryptFileAsync(IFormFile file);
        bool IsValidFile(IFormFile file);
    }
}
