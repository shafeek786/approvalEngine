using System.Collections.Generic;

namespace ApprovalEngine.Domain.Interfaces
{
    public interface IInitialAssignmentRuleEngine
    {
        IEnumerable<string> GetSupportedItemCategories();
        RoleId? DetermineInitialAssignment(GenericApprovalPayload genericPayload);
    }
}