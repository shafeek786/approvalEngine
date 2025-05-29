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

        public ApprovalWorkflowService(
            IEnumerable<IInitialAssignmentRuleEngine> initialAssignmentRuleEngines,
            IEnumerable<IReassignmentRuleEngine> reassignmentRuleEngines)
        {
            _initialAssignmentRuleEngines = initialAssignmentRuleEngines ?? throw new ArgumentNullException(nameof(initialAssignmentRuleEngines));
            _reassignmentRuleEngines = reassignmentRuleEngines ?? throw new ArgumentNullException(nameof(reassignmentRuleEngines));
        }

        public ApprovalRequest SubmitRequest(string itemCategory, string requestPayloadJson, UserId requestedBy)
        {
            var request = new ApprovalRequest(itemCategory, requestPayloadJson, requestedBy);

            var genericPayload = new GenericApprovalPayload(itemCategory, requestPayloadJson);

            UserId? assignedTo = null;
            foreach (var engine in _initialAssignmentRuleEngines.Where(e => e.GetSupportedItemCategories().Contains(itemCategory)))
            {
                assignedTo = engine.DetermineInitialAssignment(genericPayload);
                if (assignedTo != null)
                {
                    break;
                }
            }

            if (assignedTo == null)
            {
                throw new InvalidOperationException($"No rule engine could determine an initial assignment for item category '{itemCategory}'.");
            }

            request.SetInitialAssignment(assignedTo, requestedBy);
            return request;
        }

        public void ApproveRequest(ApprovalRequest request, UserId approver, string? comments = null)
        {
            request.Approve(approver, comments);
        }

        public void RejectRequest(ApprovalRequest request, UserId rejecter, string reason, string? comments = null)
        {
            request.Reject(rejecter, reason, comments);
        }

        // --- IMPORTANT: This method's signature includes newAssignedTo ---
        public void ReassignRequest(ApprovalRequest request, UserId reassigner, UserId newAssignedTo, string reason, string? comments = null)
        {
            if (request.AssignedToUser == null)
            {
                throw new InvalidOperationException("Cannot reassign a request that is not currently assigned to an approver.");
            }

            var genericPayload = new GenericApprovalPayload(request.ItemCategory, request.RequestPayloadJson);

            UserId? validatedNewAssignedTo = null;
            foreach (var engine in _reassignmentRuleEngines.Where(e => e.GetSupportedItemCategories().Contains(request.ItemCategory)))
            {
                // --- IMPORTANT: Pass the newAssignedTo to the rule engine's method ---
                validatedNewAssignedTo = engine.DetermineReassignment(genericPayload, request.AssignedToUser, newAssignedTo);
                if (validatedNewAssignedTo != null)
                {
                    break;
                }
            }

            if (validatedNewAssignedTo == null)
            {
                throw new InvalidOperationException($"Reassignment to '{newAssignedTo.Value}' is not permitted by any rule engine for item category '{request.ItemCategory}'.");
            }

            request.Reassign(reassigner, validatedNewAssignedTo, reason, comments);
        }
    }
}