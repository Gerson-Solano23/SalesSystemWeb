namespace SalesSystem.API.Utility
{
    /// <summary>
    /// Role ids carried in the JWT role claim and the authorization policies built on them.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "1";
        public const string Employee = "2";
        public const string Supervisor = "3";

        public static string ToRoleId(string? rolDescription) => rolDescription switch
        {
            "Admin" => Admin,
            "Employee" => Employee,
            "Supervisor" => Supervisor,
            _ => ""
        };
    }

    public static class RateLimitPolicies
    {
        public const string Fixed = "fixed";
        public const string Login = "login";
    }
}
