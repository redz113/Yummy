using AppFoods.Models;

namespace AppFoods.Models
{
    public class IndexModel
    {
        public bool? SortByName { get; set; } = null;
        public bool? SortByPrice { get; set; } = null;

        public int? GroupId { get; set; } = null;

        public string? SearchKey { get; set; } = string.Empty;

        public Paging Paging = new Paging();
    }
}
