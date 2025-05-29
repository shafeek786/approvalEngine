using System;

namespace ApprovalEngine.Domain
{
    public class ApprovalEvent
    {
        public DateTime Timestamp { get; }
        public ApprovalActionType Action { get; }
        public RoleId Actor { get; }
        public RoleId? FromApprover { get; }
        public RoleId? ToApprover { get; }
        public string? Reason { get; }
        public string? Comments { get; }

        public ApprovalEvent(ApprovalActionType action, RoleId actor, RoleId? fromApprover = null, RoleId? toApprover = null, string? reason = null, string? comments = null)
        {
            Timestamp = DateTime.UtcNow;
            Action = action;
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            FromApprover = fromApprover;
            ToApprover = toApprover;
            Reason = reason;
            Comments = comments;
        }

        public override string ToString()
        {
            var approverInfo = "";
            if (FromApprover != null && ToApprover != null)
                approverInfo = $" from {FromApprover} to {ToApprover}";
            else if (FromApprover != null)
                approverInfo = $" (current approver: {FromApprover})";

            var reasonInfo = string.IsNullOrWhiteSpace(Reason) ? "" : $" (Reason: {Reason})";
            var commentInfo = string.IsNullOrWhiteSpace(Comments) ? "" : $" - \"{Comments}\"";

            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} - {Action} by {Actor}{approverInfo}{reasonInfo}{commentInfo}";
        }
    }
}