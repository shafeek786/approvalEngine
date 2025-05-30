using ApprovalEngine.Domain;
using ApprovalEngine.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq; // Required for .Any() if used

namespace ApprovalEngine.RuleSet
{
    public class SequentialApprovalPathRuleset : IApprovalSequenceRuleEngine
    {
        private static readonly Dictionary<string, List<RoleId>> _categorySequences = new()
        {
            {
                "UniversalApprovalRequest", new List<RoleId>
                {
                    new RoleId("approver1"),
                    new RoleId("approver2"),
                    new RoleId("approver3"),
                    new RoleId("approver4"),
                    new RoleId("approver5")
                }
            }
            // Add other categories and their sequences here if this rule manages them
            // e.g., { "SimpleTask", new List<RoleId> { new RoleId("approver1") } } // Single step sequence
        };

        public IEnumerable<string> GetSupportedItemCategories()
        {
            // This rule engine supports categories defined in its sequence map.
            return _categorySequences.Keys;
        }

        public List<RoleId>? GetApprovalSequence(GenericApprovalPayload genericPayload)
        {
            if (_categorySequences.TryGetValue(genericPayload.ItemCategory, out var sequence))
            {
                System.Console.WriteLine($"UniversalApprovalSequenceRule: Found sequence for category '{genericPayload.ItemCategory}': {string.Join(" -> ", sequence.Select(r => r.Value))}");
                return new List<RoleId>(sequence); // Return a copy to prevent external modification
            }
            System.Console.WriteLine($"UniversalApprovalSequenceRule: No sequence defined for category '{genericPayload.ItemCategory}'.");
            return null; // No sequence defined by this rule for this category
        }
    }

    // Optional: If you want your existing CommonInitialAssignmentRuleSet to NOT assign
    // if a sequence is already handling it, you might adjust it.
    // Or, keep it as a fallback if no sequence rule applies.
    // For now, ApprovalWorkflowService prioritizes sequence rules for initial assignment.
    // So, CommonInitialAssignmentRuleSet will only be called if no sequence provides an initial assignee.

    public class NoSequenceFallbackInitialAssignmentRule : IInitialAssignmentRuleEngine
    {
        public IEnumerable<string> GetSupportedItemCategories()
        {
            // This could be a generic fallback for categories not covered by sequence rules
            // or specific categories that explicitly do not use sequences.
            yield return "UniversalApprovalRequest"; // Example: still supports it as a fallback
            yield return "AnotherCategoryWithoutSequence";
        }

        public RoleId? DetermineInitialAssignment(GenericApprovalPayload genericPayload)
        {
            // This rule would only be hit by ApprovalWorkflowService if no IApprovalSequenceRuleEngine
            // provided a sequence (and thus an initial assignee) for genericPayload.ItemCategory.
            System.Console.WriteLine($"NoSequenceFallbackInitialAssignmentRule: Determining initial assignment for '{genericPayload.ItemCategory}' (as no sequence was primary).");

            if (genericPayload.ItemCategory == "UniversalApprovalRequest")
            {
                // This part might become redundant if UniversalApprovalSequenceRule always provides a sequence.
                // However, it acts as a safety net or for cases where that rule might be disabled/not apply.
                System.Console.WriteLine($"NoSequenceFallbackInitialAssignmentRule: Fallback for Universal - assigning to approver1_fallback.");
                return new RoleId("approver1_fallback"); // Example different assignee
            }
            if (genericPayload.ItemCategory == "AnotherCategoryWithoutSequence")
            {
                System.Console.WriteLine($"NoSequenceFallbackInitialAssignmentRule: Assigning 'AnotherCategoryWithoutSequence' to manager_level1.");
                return new RoleId("manager_level1");
            }
            return null;
        }
    }
}
