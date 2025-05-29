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
    
    public enum RoleNames
    {
        approver1,
        approver2,
        approver3,
        approver4,
        approver5,
        SeniorAdminApprover,
        ProjectManager,
        HRCoordinator,
        LegalCounsel
    }
}