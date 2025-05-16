namespace Reception.Middleware.Authentication
{
    /// <summary>
    /// Static collection of hardcoded parameter key names (<see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties"/>)
    /// </summary>
    public static class Constants
    {
        public const string TOKEN_REQUIRED_POLICY = "IncludesToken";
        public const string AUTHENTICATED_POLICY = "Authenticated";
        public const string SESSION_TOKEN_HEADER = "x-mage-token";
        public const string SCHEME = "mage-authentication";
        public const string TOKEN_CONTEXT_KEY = "token";
        public const string SESSION_CONTEXT_KEY = "session";
        public const string ACCOUNT_CONTEXT_KEY = "account";
        public const string CLIENT_CONTEXT_KEY = "client";
    }


    /// <summary>
    /// Static collection of hardcoded privilege values.
    /// </summary>
    public static class Privilege
    {
        public const byte NONE = 0b000000;
        public const byte VIEW = 0b000001;
        public const byte VIEW_ALL = 0b000010;
        public const byte CREATE = 0b000100;
        public const byte UPDATE = 0b001000;
        public const byte DELETE = 0b010000;
        public const byte ADMIN = 0b100000;
    }

    /// <summary>
    /// Static collection of roles as hardcoded privileges.
    /// </summary>
    public static class Role
    {
        public const byte GUEST =
            Privilege.NONE;

        public const byte MEMBER =
            Privilege.VIEW;

        public const byte FAMILY =
            Privilege.VIEW | Privilege.CREATE | Privilege.UPDATE;

        public const byte TRUSTED =
            Privilege.VIEW | Privilege.VIEW_ALL | Privilege.CREATE | Privilege.UPDATE | Privilege.DELETE;

        public const byte ADMIN =
            Privilege.VIEW | Privilege.VIEW_ALL | Privilege.CREATE | Privilege.UPDATE | Privilege.DELETE | Privilege.ADMIN;

        public const string CUSTOM = "Custom";
    }
}
