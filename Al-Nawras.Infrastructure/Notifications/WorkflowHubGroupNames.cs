namespace Al_Nawras.Infrastructure.Notifications
{
    public static class WorkflowHubGroupNames
    {
        public static string User(string userId) => $"user:{userId}";
        public static string User(int userId) => User(userId.ToString());
        public static string Role(string roleName) => $"role:{roleName.Trim().ToLowerInvariant()}";
        public static string Client(string clientId) => $"client:{clientId}";
        public static string Client(Guid clientId) => Client(clientId.ToString());
    }
}
