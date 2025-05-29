namespace ApprovalEngine.Domain.Interfaces
{
    using ApprovalEngine.Domain; // Ensure UserId is accessible
    using System.Collections.Generic;

    public interface IReassignmentRuleEngineBase
    {
        IEnumerable<string> GetSupportedItemCategories();
        RoleId? DetermineReassignment(string itemCategory, object payload, RoleId currentApprover, RoleId proposedNewAssignedTo);
    }
}