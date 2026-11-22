
using Microsoft.AspNetCore.Identity;

namespace ITHelpdesk.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? EmployeeId { get; set; }
}
