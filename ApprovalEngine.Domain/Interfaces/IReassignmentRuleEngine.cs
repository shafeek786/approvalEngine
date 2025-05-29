using System.Collections.Generic;

namespace ApprovalEngine.Domain.Interfaces
{
    public interface IReassignmentRuleEngine
    {
        IEnumerable<string> GetSupportedItemCategories();

        // THIS IS THE MODIFIED SIGNATURE
        RoleId? DetermineReassignment(GenericApprovalPayload genericPayload, RoleId currentApprover, RoleId proposedNewAssignedTo);
    }
}