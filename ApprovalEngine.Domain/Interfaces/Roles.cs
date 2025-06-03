namespace ApprovalEngine.Domain.Interfaces;

public class Roles
{
     // --- Approvers typically used for reassignment ---
        public static readonly RoleId Approver1 = new("approver1");
        public static readonly RoleId Approver2 = new("approver2");
        public static readonly RoleId Approver3 = new("approver3");
        public static readonly RoleId Approver4 = new("approver4");
        public static readonly RoleId Approver5 = new("approver5");
        public static readonly RoleId SeniorAdminApprover = new("SeniorAdminApprover");

        // --- Other roles that might exist in the system ---
        // These might not be directly "ValidApprovers" for the CommonReassignmentRuleSet
        // but are valid roles within the system.
        public static readonly RoleId ProjectManager = new("ProjectManager");
        public static readonly RoleId HRCoordinator = new("HRCoordinator");
        public static readonly RoleId LegalCounsel = new("LegalCounsel");
        public static readonly RoleId Requester1 = new("requester1"); // Example requester
        public static readonly RoleId Requester2 = new("requester2"); // Example requester

        // Helper method to get all defined roles if needed elsewhere
        public static IEnumerable<RoleId> GetAllRoles()
        {
            return typeof(Roles)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.FieldType == typeof(RoleId))
                .Select(f => (RoleId)f.GetValue(null)!);
        }

        // Helper method specifically for getting roles considered as general approvers
        // This gives us finer control than reflecting *all* roles.
        // You can define which roles are considered "approvers" here.
        public static IEnumerable<RoleId> GetDefaultApproverRoles()
        {
            yield return Approver1;
            yield return Approver2;
            yield return Approver3;
            yield return Approver4;
            yield return Approver5;
            yield return SeniorAdminApprover;
        }
        public static IEnumerable<RoleId> GetDefaultApproverRolesSequencialOrder()
        {
            yield return Approver1;
            yield return Approver2;
            yield return Approver3;
            yield return Approver4;
            yield return Approver5;
        }
    
}