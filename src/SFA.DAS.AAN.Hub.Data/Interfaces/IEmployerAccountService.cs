using SFA.DAS.AAN.Hub.Data.Dto;

namespace SFA.DAS.AAN.Hub.Data.Interfaces;

public interface IEmployerAccountsService
{
    Task<UserAccountsApiResponse> GetEmployerUserAccounts(string userId, string email);
}