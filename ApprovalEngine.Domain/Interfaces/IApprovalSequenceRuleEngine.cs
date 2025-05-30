using ApprovalEngine.Domain;
using System.Collections.Generic;

namespace ApprovalEngine.Domain.Interfaces
{
    /// <summary>
    /// Defines a rule engine that can provide an ordered sequence of approvers for a given request category.
    /// </summary>
    public interface IApprovalSequenceRuleEngine
    {
        /// <summary>
        /// Gets the item categories supported by this rule engine.
        /// </summary>
        IEnumerable<string> GetSupportedItemCategories();

        /// <summary>
        /// Determines the approval sequence for the given request.
        /// </summary>
        /// <param name="genericPayload">The payload of the approval request.</param>
        /// <returns>
        /// A list of RoleIds representing the ordered approval sequence.
        /// Returns null or an empty list if no specific sequence applies,
        /// or if this rule engine doesn't define a sequence for this payload.
        /// </returns>
        List<RoleId>? GetApprovalSequence(GenericApprovalPayload genericPayload);
    }
}
