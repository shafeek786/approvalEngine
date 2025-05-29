using System;
using System.Collections.Generic;
using ApprovalEngine.Domain;
using ApprovalEngine.Domain.Interfaces;

namespace ApprovalEngine.Rules
{
    public class CommonInitialAssignmentRuleEngine : IInitialAssignmentRuleEngine
    {
        public IEnumerable<string> GetSupportedItemCategories()
        {
            yield return "UniversalApprovalRequest";
        }

        public UserId? DetermineInitialAssignment(GenericApprovalPayload genericPayload)
        {
            Console.WriteLine($"Applying general initial assignment rule for category: {genericPayload.ItemCategory}");
            return new UserId("approver1");
        }
    }
}