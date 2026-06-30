using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileService fileService, ILogger<FileController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> UploadFile(IFormFile file, [FromQuery] string? subfolder = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        if (!_fileService.IsValidFileSize(file))
            return BadRequest("File size exceeds the maximum allowed size (5MB)");

        try
        {
            var filePath = await _fileService.SaveFileAsync(file, subfolder);
            return Ok(new { filePath, message = "File uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "Error uploading file");
        }
    }

    [HttpPost("upload-multiple")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<string>>> UploadFiles(List<IFormFile> files, [FromQuery] string? subfolder = null)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files provided");

        foreach (var file in files)
        {
            if (!_fileService.IsValidFileSize(file))
                return BadRequest($"File {file.FileName} exceeds the maximum allowed size (5MB)");
        }

        try
        {
            var filePaths = await _fileService.SaveFilesAsync(files, subfolder);
            return Ok(new { filePaths, message = "Files uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files");
            return StatusCode(500, "Error uploading files");
        }
    }

    [HttpPost("upload-image")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> UploadImage(IFormFile file, [FromQuery] string? subfolder = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        if (!_fileService.IsValidImage(file))
            return BadRequest("File must be a valid image (jpg, jpeg, png, gif, bmp, webp)");

        if (!_fileService.IsValidFileSize(file))
            return BadRequest("File size exceeds the maximum allowed size (5MB)");

        try
        {
            var filePath = await _fileService.SaveFileAsync(file, subfolder);
            return Ok(new { filePath, message = "Image uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, "Error uploading image");
        }
    }

    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteFile([FromQuery] string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return BadRequest("File path is required");

        var deleted = await _fileService.DeleteFileAsync(filePath);
        
        if (deleted)
            return Ok(new { message = "File deleted successfully" });
        
        return NotFound("File not found");
    }

    [HttpGet("exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public ActionResult<bool> FileExists([FromQuery] string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return BadRequest("File path is required");

        var exists = _fileService.FileExists(filePath);
        return Ok(exists);
    }
}

