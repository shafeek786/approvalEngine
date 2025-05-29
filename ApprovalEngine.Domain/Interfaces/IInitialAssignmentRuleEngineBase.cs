namespace ApprovalEngine.Domain.Interfaces
{
    using ApprovalEngine.Domain; // Ensure UserId is accessible
    using System.Collections.Generic;

    public interface IInitialAssignmentRuleEngineBase
    {
        IEnumerable<string> GetSupportedItemCategories();
        RoleId? DetermineInitialAssignment(string itemCategory, object payload);
    }
}