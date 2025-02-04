namespace SFA.DAS.AAN.Hub.Data.Dto;

public record EmployerUserAccounts(bool IsSuspended, string FirstName, string LastName, string EmployerUserId, List<EmployerIdentifier> UserAccounts);

public record EmployerIdentifier(string AccountId, string EmployerName, string Role);
