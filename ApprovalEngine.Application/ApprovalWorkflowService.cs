using ApprovalEngine.Domain;
using ApprovalEngine.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApprovalEngine.Application
{
    public class ApprovalWorkflowService
    {
        private readonly IEnumerable<IInitialAssignmentRuleEngine> _initialAssignmentRuleEngines;
        private readonly IEnumerable<IReassignmentRuleEngine> _reassignmentRuleEngines;
        private readonly IEnumerable<IApprovalSequenceRuleEngine> _approvalSequenceRuleEngines; // New

        public ApprovalWorkflowService(
            IEnumerable<IInitialAssignmentRuleEngine> initialAssignmentRuleEngines,
            IEnumerable<IReassignmentRuleEngine> reassignmentRuleEngines,
            IEnumerable<IApprovalSequenceRuleEngine> approvalSequenceRuleEngines) // New
        {
            _initialAssignmentRuleEngines = initialAssignmentRuleEngines ?? throw new ArgumentNullException(nameof(initialAssignmentRuleEngines));
            _reassignmentRuleEngines = reassignmentRuleEngines ?? throw new ArgumentNullException(nameof(reassignmentRuleEngines));
            _approvalSequenceRuleEngines = approvalSequenceRuleEngines ?? throw new ArgumentNullException(nameof(approvalSequenceRuleEngines)); // New
        }

        private List<RoleId>? GetSequenceForRequest(string itemCategory, GenericApprovalPayload payload)
        {
            foreach (var engine in _approvalSequenceRuleEngines.Where(e => e.GetSupportedItemCategories().Contains(itemCategory)))
            {
                var sequence = engine.GetApprovalSequence(payload);
                if (sequence != null && sequence.Any())
                {
                    return sequence;
                }
            }
            return null;
        }

        public ApprovalRequest SubmitRequest(string itemCategory, string requestPayloadJson, RoleId requestedBy)
        {
            var request = new ApprovalRequest(itemCategory, requestPayloadJson, requestedBy);
            var genericPayload = new GenericApprovalPayload(itemCategory, requestPayloadJson);
            RoleId? assignedTo = null;

            // Try to get an approval sequence first
            var sequence = GetSequenceForRequest(itemCategory, genericPayload);

            if (sequence != null && sequence.Any())
            {
                assignedTo = sequence.First(); // Assign to the first approver in the sequence
                Console.WriteLine($"Approval sequence found for '{itemCategory}'. Initial assignment to: {assignedTo.Value} (start of sequence).");
            }
            else
            {
                // If no sequence, use initial assignment rule engines
                Console.WriteLine($"No approval sequence found for '{itemCategory}'. Using IInitialAssignmentRuleEngine.");
                foreach (var engine in _initialAssignmentRuleEngines.Where(e => e.GetSupportedItemCategories().Contains(itemCategory)))
                {
                    assignedTo = engine.DetermineInitialAssignment(genericPayload);
                    if (assignedTo != null)
                    {
                        Console.WriteLine($"IInitialAssignmentRuleEngine assigned to: {assignedTo.Value}.");
                        break;
                    }
                }
            }

            if (assignedTo == null)
            {
                throw new InvalidOperationException($"No rule engine (sequence or initial assignment) could determine an assignment for item category '{itemCategory}'.");
            }

            request.SetInitialAssignment(assignedTo, requestedBy);
            return request;
        }

        public void ApproveRequest(ApprovalRequest request, RoleId approver, string? comments = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (approver == null) throw new ArgumentNullException(nameof(approver));

            if (!request.IsAssignedTo(approver))
            {
                throw new InvalidOperationException($"User {approver.Value} is not the current assignee for request {request.Id}. " +
                                                  $"Currently assigned to: {request.AssignedToUser?.Value ?? "none"}.");
            }

            var genericPayload = new GenericApprovalPayload(request.ItemCategory, request.RequestPayloadJson);
            var sequence = GetSequenceForRequest(request.ItemCategory, genericPayload);

            if (sequence != null && sequence.Any())
            {
                int currentIndex = sequence.FindIndex(r => r.Equals(approver));

                if (currentIndex == -1)
                {
                    Console.WriteLine($"Approver {approver.Value} is not in the defined sequence for {request.ItemCategory}. Treating as final approval for this step.");
                    request.Approve(approver, comments ?? "Approved (acting outside of defined sequence path).");
                }
                else if (currentIndex < sequence.Count - 1)
                {
                    RoleId nextApprover = sequence[currentIndex + 1];
                    request.ProcessIntermediateApproval(approver, nextApprover, comments);
                    Console.WriteLine($"Request {request.Id} approved by {approver.Value}, forwarded to {nextApprover.Value} (sequence).");
                }
                else
                {
                    request.Approve(approver, comments); // Final approval in sequence
                    Console.WriteLine($"Request {request.Id} approved by {approver.Value} (final approver in sequence).");
                }
            }
            else
            {
                // No sequence defined, direct approval
                request.Approve(approver, comments);
                Console.WriteLine($"Request {request.Id} approved by {approver.Value} (no sequence defined).");
            }
        }

        public void RejectRequest(ApprovalRequest request, RoleId rejecter, string reason, string? comments = null)
        {
            // Ensure the user rejecting is the one assigned
            if (!request.IsAssignedTo(rejecter))
            {
                throw new InvalidOperationException($"User {rejecter.Value} is not the current assignee for request {request.Id}. " +
                                                  $"Currently assigned to: {request.AssignedToUser?.Value ?? "none"}.");
            }
            request.Reject(rejecter, reason, comments);
            Console.WriteLine($"Request {request.Id} rejected by {rejecter.Value}.");
        }

        public void ReassignRequest(ApprovalRequest request, RoleId reassigner, RoleId newAssignedTo, string reason, string? comments = null)
        {
            // Basic check: is the request in a state that can be reassigned?
            if (request.CurrentStatus != ApprovalStatus.Pending && request.CurrentStatus != ApprovalStatus.Reassigned)
            {
                throw new InvalidOperationException($"Cannot reassign request {request.Id} as it is in '{request.CurrentStatus}' status.");
            }

            // Ensure the reassigner has the authority (could be the current assignee or an admin)
            // For this example, we assume the reassigner is valid if they are the current assignee OR if a rule allows it.
            // A more complex system might have separate permissions for reassigning.
            bool isCurrentUserReassigning = request.IsAssignedTo(reassigner);

            var genericPayload = new GenericApprovalPayload(request.ItemCategory, request.RequestPayloadJson);
            RoleId? validatedNewAssignedTo = null;

            foreach (var engine in _reassignmentRuleEngines.Where(e => e.GetSupportedItemCategories().Contains(request.ItemCategory)))
            {
                // The rule engine should validate if 'reassigner' can reassign and if 'newAssignedTo' is a valid target.
                // The 'currentApprover' passed to the rule engine would be request.AssignedToUser.
                validatedNewAssignedTo = engine.DetermineReassignment(genericPayload, request.AssignedToUser ?? reassigner, newAssignedTo);
                if (validatedNewAssignedTo != null)
                {
                    break;
                }
            }

            if (validatedNewAssignedTo == null)
            {
                // If no rule engine explicitly allows or modifies the reassignment,
                // we might fall back to a default behavior or throw an error.
                // For now, let's throw if no rule validates it.
                throw new InvalidOperationException($"Reassignment to '{newAssignedTo.Value}' by '{reassigner.Value}' is not permitted by any rule engine for item category '{request.ItemCategory}'.");
            }

            request.Reassign(reassigner, validatedNewAssignedTo, reason, comments);
            Console.WriteLine($"Request {request.Id} reassigned by {reassigner.Value} to {validatedNewAssignedTo.Value}.");
        }
    }
}
