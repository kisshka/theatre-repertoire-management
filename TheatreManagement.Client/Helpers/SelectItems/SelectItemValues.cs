namespace TheatreManagement.Client.Helpers.SelectItems
{
    public class SelectItemValues
    {
        //Для мероприятий
        static public List<SelectItem<string>> HallTypes = new()
        {
            new SelectItem<string> { Name = "Большой зал", Value = "Большой зал" },
            new SelectItem<string> { Name = "Малый зал", Value = "Малый зал" }
        };

        static public List<SelectItem<string>> PerformanceTypes = new()
        {
            new SelectItem<string> { Name = "Репетиция", Value = "Репетиция" },
            new SelectItem<string> { Name = "Выступление", Value = "Выступление" }
        };

        static public List<SelectItem<string>> EventTypes = new()
        {
            new SelectItem<string> { Name = "Стационар", Value = "stationar" },
            new SelectItem<string> { Name = "Гастроли", Value = "tour" },
            new SelectItem<string> { Name = "Выезд", Value = "visit" }
        };
    }
}
