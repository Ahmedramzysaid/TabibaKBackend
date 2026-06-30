using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string? subfolder = null);

    Task<List<string>> SaveFilesAsync(List<IFormFile> files, string? subfolder = null);

    Task<bool> DeleteFileAsync(string filePath);

    string GetPhysicalPath(string filePath);

    bool FileExists(string filePath);

    bool IsValidImage(IFormFile file);

    bool IsValidFileSize(IFormFile file, long maxSizeInBytes = 5242880);

    bool IsValidVideo(IFormFile file);

    void GetAllowedVideoTypes(out string[] extensions, out string acceptAttribute, out string acceptMimeTypes);
}

