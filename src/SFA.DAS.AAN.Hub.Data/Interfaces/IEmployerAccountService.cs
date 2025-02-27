﻿using SFA.DAS.AAN.Hub.Data.Dto;

namespace SFA.DAS.AAN.Hub.Data.Interfaces;

public interface IEmployerAccountsService
{
    Task<string> GetEmployerUserAccounts(Guid memberId);
}