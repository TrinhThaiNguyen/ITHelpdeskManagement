
using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.Dashboard;

public class DashboardExportRequest : IValidatableObject
{
    [Required]
    public DateOnly? FromDate { get; set; }

    [Required]
    public DateOnly? ToDate { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (FromDate.HasValue &&
            ToDate.HasValue &&
            FromDate.Value > ToDate.Value)
        {
            yield return new ValidationResult(
                "FromDate cannot be later than ToDate.",
                new[] { nameof(FromDate), nameof(ToDate) });
        }

        if (FromDate.HasValue &&
            ToDate.HasValue &&
            ToDate.Value.DayNumber - FromDate.Value.DayNumber > 365)
        {
            yield return new ValidationResult(
                "Date range cannot exceed 366 days.",
                new[] { nameof(FromDate), nameof(ToDate) });
        }
    }
}
