namespace ApprovalEngine.Domain
{
    /// <summary>
    /// Represents a generic "tray" for any approval request payload.
    /// This is what rule engines will receive, allowing them to independently
    /// decide how to parse the raw JSON into their specific domain models,
    /// or apply general rules.
    /// </summary>
    public record GenericApprovalPayload(string ItemCategory, string RawJsonPayload);
}