using System;
using System.Collections.Generic;
using System.Linq;

namespace ApprovalEngine.Domain
{
    public class ApprovalRequest
    {
        public Guid Id { get; private set; }
        public string ItemCategory { get; private set; }
        public string RequestPayloadJson { get; private set; }
        public ApprovalStatus CurrentStatus { get; private set; }
        public RoleId? AssignedToUser { get; private set; }
        public RoleId RequestedBy { get; private set; }

        private readonly List<ApprovalEvent> _history = new List<ApprovalEvent>();
        public IReadOnlyList<ApprovalEvent> History => _history.AsReadOnly();

        public ApprovalRequest(string itemCategory, string requestPayloadJson, RoleId requestedBy)
        {
            Id = Guid.NewGuid();
            ItemCategory = itemCategory ?? throw new ArgumentNullException(nameof(itemCategory));
            RequestPayloadJson = requestPayloadJson ?? throw new ArgumentNullException(nameof(requestPayloadJson));
            RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
            CurrentStatus = ApprovalStatus.Pending;
        }

        public void SetInitialAssignment(RoleId assignedTo, RoleId actor)
        {
            if (CurrentStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException("Initial assignment can only be set for pending requests.");

            AssignedToUser = assignedTo ?? throw new ArgumentNullException(nameof(assignedTo));
            _history.Add(new ApprovalEvent(ApprovalActionType.Submit, actor, null, assignedTo, comments: "Initial submission and assignment"));
            CurrentStatus = ApprovalStatus.Pending;
        }

        public void Approve(RoleId approver, string? comments = null)
        {
            if (CurrentStatus != ApprovalStatus.Pending && CurrentStatus != ApprovalStatus.Reassigned)
                throw new InvalidOperationException($"Cannot approve request in '{CurrentStatus}' status.");

            if (!AssignedToUser?.Equals(approver) ?? true)
                throw new InvalidOperationException($"User {approver.Value} is not assigned to approve this request.");

            _history.Add(new ApprovalEvent(ApprovalActionType.Approve, approver, AssignedToUser, null, comments: comments));
            CurrentStatus = ApprovalStatus.Approved;
            AssignedToUser = null;
        }

        public void Reject(RoleId rejecter, string reason, string? comments = null)
        {
            if (CurrentStatus != ApprovalStatus.Pending && CurrentStatus != ApprovalStatus.Reassigned)
                throw new InvalidOperationException($"Cannot reject request in '{CurrentStatus}' status.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason cannot be empty.", nameof(reason));

            if (!AssignedToUser?.Equals(rejecter) ?? true)
                throw new InvalidOperationException($"User {rejecter.Value} is not assigned to reject this request.");

            _history.Add(new ApprovalEvent(ApprovalActionType.Reject, rejecter, AssignedToUser, null, reason: reason, comments: comments));
            CurrentStatus = ApprovalStatus.Rejected;
            AssignedToUser = null;
        }

        public void Reassign(RoleId reassigner, RoleId newAssignedTo, string reason, string? comments = null)
        {
            if (CurrentStatus != ApprovalStatus.Pending && CurrentStatus != ApprovalStatus.Reassigned)
                throw new InvalidOperationException($"Cannot reassign request in '{CurrentStatus}' status.");
            if (newAssignedTo == null)
                throw new ArgumentNullException(nameof(newAssignedTo));
            if (AssignedToUser != null && newAssignedTo.Equals(AssignedToUser))
                throw new InvalidOperationException("Cannot reassign to the same approver.");

            _history.Add(new ApprovalEvent(ApprovalActionType.Reassign, reassigner, AssignedToUser, newAssignedTo, reason: reason, comments: comments));
            AssignedToUser = newAssignedTo;
            CurrentStatus = ApprovalStatus.Reassigned;
        }

        public bool IsAssignedTo(RoleId role)
        {
            return AssignedToUser != null && AssignedToUser.Equals(role);
        }
    }
}