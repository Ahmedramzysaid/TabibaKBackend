using System.ComponentModel.DataAnnotations;

namespace DomainLayer.DTOs;

public class CreateAdviceCommentDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
