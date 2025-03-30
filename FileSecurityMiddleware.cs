using _BaseUtilities.Exceptions.Common;
using _BusinessUtilities.Services.Contracts;
using CommonTools.Utilities;
using Microsoft.AspNetCore.Http;

namespace _ControllerUtilities.Middlewares
{
    public class FileSecurityMiddleware(RequestDelegate _next, IFileSecurityService _fileSecurityService)
    {
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
            {
                foreach (var file in context.Request.Form.Files)
                {
                    if (!_fileSecurityService.IsValidFile(file))
                    {
                        throw new BadRequestException($"Invalid file upload attempt: {file.FileName}");
                    }

                    var encryptedFile = await _fileSecurityService.EncryptFileAsync(file)
                        ?? throw new BaseException("Internal Server Error.");
                    
                    context.Items["File"] = encryptedFile;

                    //context.Items["EncryptedFile"] = encryptedFile;
                    //context.Items.Remove("File");  // Optionally remove original file reference

                }
            }

            await _next(context);
        }
    }
}