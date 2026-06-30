using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class UpdateAdviceCommentDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
