using System;

namespace ApprovalEngine.Domain
{
    public class RoleId
    {
        public string Value { get; }
        public RoleId(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is RoleId id && Value == id.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}