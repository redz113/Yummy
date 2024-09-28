namespace AppFoods.Models
{
    public class Build
    {
        public Table? Table {get; set;}
        public List<ComboOrder>? ComboOrders {get; set;}
        public List<MenuOrder>? MenuOrders {get; set;}
    }
}