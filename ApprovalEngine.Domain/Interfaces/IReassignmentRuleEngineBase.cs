namespace ApprovalEngine.Domain.Interfaces
{
    using ApprovalEngine.Domain; // Ensure UserId is accessible
    using System.Collections.Generic;

    public interface IReassignmentRuleEngineBase
    {
        IEnumerable<string> GetSupportedItemCategories();
        UserId? DetermineReassignment(string itemCategory, object payload, UserId currentApprover, UserId proposedNewAssignedTo);
    }
}