using System;

namespace ApprovalEngine.Domain
{
    public class UserId
    {
        public string Value { get; }
        public UserId(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is UserId id && Value == id.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}