namespace AppFoods.Models
{
    public class Paging
    {
        public int TotalRecord { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int Limit { get; set; } = 10;
        //public Func<int?, string> GenerateUrl { get; set; }
        public string Action { get; set; } = "Index";
        public string? Param { get; set; } = null;
    }
}
