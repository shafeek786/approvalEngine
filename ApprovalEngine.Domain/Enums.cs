namespace ApprovalEngine.Domain
{
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected,
        Reassigned,
        Cancelled
    }

    public enum ApprovalActionType
    {
        Submit,
        Approve,
        Reject,
        Reassign,
        Cancel
    }
}