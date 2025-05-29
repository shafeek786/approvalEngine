using System;
using System.Collections.Generic;
using ApprovalEngine.Domain;
using ApprovalEngine.Domain.Interfaces;

namespace ApprovalEngine.RuleSet
{
    public class CommonInitialAssignmentRuleSet : IInitialAssignmentRuleEngine
    {
        public IEnumerable<string> GetSupportedItemCategories()
        {
            yield return "UniversalApprovalRequest";
        }

        public RoleId? DetermineInitialAssignment(GenericApprovalPayload genericPayload)
        {
            Console.WriteLine($"Applying general initial assignment rule for category: {genericPayload.ItemCategory}");
            return new RoleId("approver1");
        }
    }
}