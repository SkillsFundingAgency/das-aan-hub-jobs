using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IMemberDataCleanupRepository
    {
        Task<List<Member>> GetWithdrawnOrDeletedMembers();
        Task<List<Member>> GetRemovedMembers();
        void UpdateMemberDetails(Member member, CancellationToken cancellationToken);
        void DeleteMemberProfile(List<MemberProfile> memberProfiles, CancellationToken cancellationToken);
        void DeleteMemberPreference(List<MemberPreference> memberPreferences, CancellationToken cancellationToken);
        void DeleteMemberNotifications(List<Notification> memberNotifications, CancellationToken cancellationToken);
        void DeleteMemberAudit(List<Audit> memberAudits, CancellationToken cancellationToken);
        void DeleteMemberApprentice(Apprentice memberApprentice, CancellationToken cancellationToken);
        void DeleteMemberEmployer(Employer memberEmployer, CancellationToken cancellationToken);
        Task<List<Attendance>> GetMemberAttendances(Guid memberId, CancellationToken cancellationToken);
        Task<List<CalendarEvent>> GetFutureCalendarEvents(List<Guid> attendanceEventIds, CancellationToken cancellationToken);
        void UpdateMemberFutureAttendance(Attendance memberAttendance, CancellationToken cancellationToken);
    }
}
