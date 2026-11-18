using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.Employees;

public class CreateEmployeeRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }
}