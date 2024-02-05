namespace SFA.DAS.AAN.Hub.Data.Entities;

public class Member
{
    public Guid Id { get; set; }
    public UserType UserType { get; set; }
    public string Email { get; set; } = null!;
    public MemberStatus Status { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? OrganisationName { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool? IsRegionalChair { get; set; }
    public DateTime? EndDate { get; set; }
    public virtual List<MemberProfile> MemberProfiles { get; set; } = new List<MemberProfile>();
    public virtual List<MemberPreference> MemberPreferences { get; set; } = new List<MemberPreference>();
    public virtual List<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual List<Audit> Audits { get; set; } = new List<Audit>();
    public virtual Apprentice? Apprentice { get; set; }
    public virtual Employer? Employer { get; set; }
}
