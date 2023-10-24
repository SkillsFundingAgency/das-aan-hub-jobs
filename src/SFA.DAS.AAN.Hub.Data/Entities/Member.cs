namespace SFA.DAS.AAN.Hub.Data.Entities;

public class Member
{
    public Guid Id { get; set; }
    public string UserType { get; set; } = null!;
    public string Email { get; set; } = null!;
}
