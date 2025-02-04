namespace SFA.DAS.AAN.Hub.Data.Dto;

public class GetMemberByIdQueryResult
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserType { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public long EmployerAccountId { get; set; }
    public Guid UserRef { get; set; }
}
