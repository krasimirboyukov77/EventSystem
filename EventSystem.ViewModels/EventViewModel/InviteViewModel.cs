

namespace EventSystem.ViewModels.EventViewModel
{
    public class InviteViewModel
    {
        public virtual ICollection<InviteInfo> PendingInvites { get; set; } = new List<InviteInfo>();
        public virtual ICollection<InviteInfo> AnsweredInvites { get; set; } = new List<InviteInfo>();
        public virtual ICollection<InviteInfo> SentInvites { get; set; } = new List<InviteInfo>();
    }

    public class InviteInfo
    {
        public required string Id { get; set; } = null!;

        public required string CreatorName { get; set; } = null!;

        public required string EventName { get; set; } = null!;

        public required DateTime InviteDate { get; set; }

        public required int InvitationStatus { get; set; }

        public bool IsSentByMe { get; set; }
    }
}
