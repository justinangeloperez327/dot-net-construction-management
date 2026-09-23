using System.ComponentModel.DataAnnotations;

namespace Web.Components.Pages.Clients;

public sealed class ClientFormModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [StringLength(320)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(1000)]
    public string? Address { get; set; }
}
