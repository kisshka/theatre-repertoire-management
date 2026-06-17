namespace TheatreManagement.Client.Helpers
{
    public static class RoleHelper
    {
        private static readonly Dictionary<string, string> _roleDisplayNames = new()
        {
            ["MainAdmin"] = "Главный администратор",
            ["VisitAdmin"] = "Администратор по выездам",
            ["TourAdmin"] = "Администратор по гастролям",
            ["StationarAdmin"] = "Администратор по стационарам",
            ["SystemAdmin"] = "Системный администратор"
        };

        public static string GetDisplayName(string roleValue)
        {
            if (string.IsNullOrEmpty(roleValue))
                return "";

            return _roleDisplayNames.TryGetValue(roleValue, out var displayName)
                ? displayName
                : roleValue;
        }

        public static List<RoleItem> GetAllRoles()
        {
            return _roleDisplayNames.Select(kvp => new RoleItem
            {
                Value = kvp.Key,
                Text = kvp.Value
            }).ToList();
        }
    }

    public class RoleItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}