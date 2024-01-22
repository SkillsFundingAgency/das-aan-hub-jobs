namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Apprentice
{
    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;
}
