using _BaseUtilities.Constants;
using _BusinessUtilities.Services.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using static _BaseUtilities.Constants.RegisterMode;

namespace File_Security_Service
{
    public class FileSecuritySettings : ISingletonDependency
    {
        public string[] AllowedExtensions { get; set; } //{ ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        public int MaxFileSize { get; set; } //5 * 1024 * 1024; 

    }

    public class FileSecurityService(FileSecuritySettings _fileSettings) : ISingletonDependency, IFileSecurityService
    {
        public bool IsValidFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_fileSettings.AllowedExtensions.Contains(extension))
            {
                throw new BadImageFormatException($"Unsupported file type: {extension}");
            }

            if (file.Length > _fileSettings.MaxFileSize)
            {
                throw new BadImageFormatException($"File size exceeded: {file.Length} bytes");
            }

            return true;
        }

        public async Task<byte[]> EncryptFileAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] fileData = ms.ToArray();

            using Aes aes = Aes.Create();
            aes.Key = GenerateKey();
            aes.IV = GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(fileData, encryptor);
        }

        public async Task<byte[]> DecryptFileAsync(byte[] encryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = GenerateKey();
            aes.IV = GenerateIV();

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            return await Task.Run(() => PerformCryptography(encryptedData, decryptor));
        }

        #region Private Methods

        private static byte[] GenerateKey()
        {
            return Convert.FromBase64String("YOUR_SECRET_KEY_HERE"); // Store securely (e.g., Azure Key Vault)
        }

        private static byte[] GenerateIV()
        {
            return Convert.FromBase64String("YOUR_SECRET_IV_HERE");
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform transform)
        {
            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, transform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return ms.ToArray();
        }

        #endregion
    }
}