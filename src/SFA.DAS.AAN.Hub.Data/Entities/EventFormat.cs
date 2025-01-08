using System.ComponentModel;

namespace SFA.DAS.AAN.Hub.Data.Entities;

public enum EventFormat
{
    [Description("In person")]
    InPerson,
    [Description("Online")]
    Online,
    [Description("Hybrid")]
    Hybrid
}