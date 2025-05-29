using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalEngine.Domain;
using ApprovalEngine.Domain.Interfaces;

namespace ApprovalEngine.RuleSet
{
    public class CommonReassignmentRuleSet : IReassignmentRuleEngine
    {
        private static readonly List<RoleId> ValidApprovers = new List<RoleId>
        {
            new RoleId("approver1"),
            new RoleId("approver2"),
            new RoleId("approver3"),
            new RoleId("approver4"),
            new RoleId("approver5"),
            new RoleId("SeniorAdminApprover")
        };

        public IEnumerable<string> GetSupportedItemCategories()
        {
            yield return "UniversalApprovalRequest";
        }

        // --- IMPORTANT: Signature must match the interface exactly ---
        public RoleId? DetermineReassignment(GenericApprovalPayload genericPayload, RoleId currentApprover, RoleId proposedNewAssignedTo)
        {
            Console.WriteLine($"Applying general reassignment validation rule for category: {genericPayload.ItemCategory}");
            Console.WriteLine($"Current Approver: {currentApprover.Value}, Proposed New Approver: {proposedNewAssignedTo.Value}");

            if (ValidApprovers.Any(u => u.Value == proposedNewAssignedTo.Value))
            {
                Console.WriteLine($"Reassignment to {proposedNewAssignedTo.Value} is VALID.");
                return proposedNewAssignedTo;
            }
            else
            {
                Console.WriteLine($"Reassignment to {proposedNewAssignedTo.Value} is INVALID. Not in valid approvers list.");
                return null;
            }
        }
    }
}