using ApprovalEngine.Domain;

namespace ClientApplication;


public static class Roles
{
    public static readonly RoleId Approver1 = new("approver1");
    public static readonly RoleId Approver2 = new("approver2");
    public static readonly RoleId Approver3 = new("approver3");
    public static readonly RoleId Approver4 = new("approver4");
    public static readonly RoleId Approver5 = new("approver5");

    public static readonly RoleId SeniorAdminApprover = new("SeniorAdminApprover");
    public static readonly RoleId ProjectManager = new("ProjectManager");
    public static readonly RoleId HRCoordinator = new("HRCoordinator");
    public static readonly RoleId LegalCounsel = new("LegalCounsel");
}

