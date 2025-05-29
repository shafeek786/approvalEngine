using System.Collections.Generic;

namespace ApprovalEngine.Domain.Interfaces
{
    public interface IReassignmentRuleEngine
    {
        IEnumerable<string> GetSupportedItemCategories();

        // THIS IS THE MODIFIED SIGNATURE
        UserId? DetermineReassignment(GenericApprovalPayload genericPayload, UserId currentApprover, UserId proposedNewAssignedTo);
    }
}