using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
namespace BusinessLayer.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly string _wwwrootPath;
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv" };
    private const long DefaultMaxFileSize = 5242880; // 5MB
    public const long DefaultMaxVideoSizeBytes = 100 * 1024 * 1024;

    public FileService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        _wwwrootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        var imagesPath = Path.Combine(_wwwrootPath, "images");
        var uploadsPath = Path.Combine(_wwwrootPath, "uploads");
        var videosPath = Path.Combine(_wwwrootPath, "uploads", "videos");

        if (!Directory.Exists(imagesPath))
            Directory.CreateDirectory(imagesPath);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);
        if (!Directory.Exists(videosPath))
            Directory.CreateDirectory(videosPath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string? subfolder = null)
    {
        if (file == null)
            throw new ArgumentException("File cannot be null. Please provide a valid file.", nameof(file));

        if (file.Length == 0)
            throw new ArgumentException("File size cannot be zero. Please provide a file with content.", nameof(file));

        if (string.IsNullOrWhiteSpace(file.FileName))
            throw new ArgumentException("File name cannot be null, empty, or whitespace. Please provide a file with a valid name.", nameof(file.FileName));

        try
        {
            var isImage = IsValidImage(file);
            var baseFolder = isImage ? "images" : "uploads";

            var folderPath = string.IsNullOrEmpty(subfolder)
                ? Path.Combine(_wwwrootPath, baseFolder)
                : Path.Combine(_wwwrootPath, baseFolder, subfolder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = string.IsNullOrEmpty(subfolder)
                ? $"/{baseFolder}/{fileName}"
                : $"/{baseFolder}/{subfolder}/{fileName}";

            return GetAbsoluteUrl(relativePath);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error saving file '{file.FileName}': {ex.Message}", ex);
        }
    }

    public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string? subfolder = null)
    {
        if (files == null || files.Count == 0)
            return new List<string>();

        var savedPaths = new List<string>();
        foreach (var file in files)
        {
            if (file != null && file.Length > 0 && !string.IsNullOrWhiteSpace(file.FileName))
            {
                try
                {
                    var path = await SaveFileAsync(file, subfolder);
                    savedPaths.Add(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to save file '{file.FileName}': {ex.Message}");
                }
            }
        }
        return savedPaths;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        try
        {
            var physicalPath = GetPhysicalPath(filePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to delete file '{filePath}': {ex.Message}");
            return false;
        }
    }

    public string GetPhysicalPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri))
        {
            filePath = uri.AbsolutePath;
        }

        if (string.IsNullOrWhiteSpace(_wwwrootPath))
        {
            throw new InvalidOperationException("Web root path is not configured correctly.");
        }

        var cleanPath = filePath.TrimStart('/');

        return Path.Combine(_wwwrootPath, cleanPath);
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        try
        {
            var physicalPath = GetPhysicalPath(filePath);
            return File.Exists(physicalPath);
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName))
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedImageExtensions.Contains(extension);
    }

    public bool IsValidFileSize(IFormFile file, long maxSizeInBytes = DefaultMaxFileSize)
    {
        if (file == null)
            return false;

        return file.Length <= maxSizeInBytes && file.Length > 0;
    }

    public bool IsValidVideo(IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName))
            return false;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedVideoExtensions.Contains(extension);
    }

    public void GetAllowedVideoTypes(out string[] extensions, out string acceptAttribute, out string acceptMimeTypes)
    {
        extensions = (string[])_allowedVideoExtensions.Clone();
        acceptAttribute = string.Join(",", _allowedVideoExtensions);
        var mimes = new[] { "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo", "video/x-matroska" };
        acceptMimeTypes = string.Join(",", mimes);
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        if (originalFileName == null)
            throw new ArgumentNullException(nameof(originalFileName), "Original file name cannot be null.");
        
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException(
                $"Original file name cannot be empty or whitespace. Provided value: '{originalFileName}'",
                nameof(originalFileName));

        try
        {
            if (originalFileName == null)
                throw new ArgumentNullException(nameof(originalFileName), "File name became null unexpectedly.");

            var extension = Path.GetExtension(originalFileName);

            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    $"File name '{originalFileName}' must have a valid extension (e.g., .jpg, .png)",
                    nameof(originalFileName));

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            
            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                fileNameWithoutExtension = "file";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileNameWithoutExtension.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "file";
            }

            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);

            var uniqueName = $"{sanitized}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";

            return uniqueName;
        }
        catch (ArgumentNullException)
        {
            throw; // Re-throw null argument exceptions
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation errors
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"Error generating unique filename for '{originalFileName ?? "null"}': {ex.Message}",
                nameof(originalFileName),
                ex);
        }
    }

    private string GetAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return relativePath;

        if (!relativePath.StartsWith("/"))
            relativePath = "/" + relativePath;

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            try
            {
                var request = httpContext.Request;
                
                var scheme = request.Scheme;
                var host = request.Host;
                
                if (!string.IsNullOrWhiteSpace(scheme) && host.HasValue)
                {
                    var baseUrl = $"{scheme}://{host}";
                    return $"{baseUrl}{relativePath}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to get base URL from HttpContext: {ex.Message}");
            }
        }

        if (_configuration != null)
        {
            try
            {
                var configuredBaseUrl = _configuration["BaseUrl"] ?? _configuration["AppSettings:BaseUrl"];
                if (!string.IsNullOrEmpty(configuredBaseUrl))
                {
                    configuredBaseUrl = configuredBaseUrl.TrimEnd('/');
                    return $"{configuredBaseUrl}{relativePath}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to get base URL from configuration: {ex.Message}");
            }
        }

        return relativePath;
    }
}
