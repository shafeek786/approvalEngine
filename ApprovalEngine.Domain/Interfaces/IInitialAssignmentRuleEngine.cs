using System.Collections.Generic;

namespace ApprovalEngine.Domain.Interfaces
{
    public interface IInitialAssignmentRuleEngine
    {
        IEnumerable<string> GetSupportedItemCategories();
        UserId? DetermineInitialAssignment(GenericApprovalPayload genericPayload);
    }
}