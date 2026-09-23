using System.ComponentModel.DataAnnotations;

namespace Web.Components.Pages.Projects;

public sealed class ProjectFormModel
{
    [Required]
    [StringLength(50)]
    public string ProjectNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Location { get; set; }

    [Required]
    public DateOnly StartDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    public DateOnly? TargetCompletionDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }
}
