using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class Order
    {
        public List<MenuOrder>? MenuOrders {get; set;} = new List<MenuOrder>();
        public List<ComboOrder>? ComboOrders {get; set;} = new List<ComboOrder>();
    }

    public class MenuOrder
    {
        [Key]
        public int Id {get; set;}
        public int TableId {get; set;}
        [ForeignKey("TableId")]
        public Table? Table {get; set;}
        public int MenuId {get; set;}
        [ForeignKey("MenuId")]
        public Menu? Menu {get; set;}
        public int Quantity {get; set;}


        public DateTime Time {get; set;} = DateTime.Now;
        public bool Status {get; set;} = false;
    }

    public class ComboOrder
    {
        [Key]
        public int Id {get; set;}
        public int TableId {get; set;}
        [ForeignKey("TableId")]
        public Table? Table {get; set;}
        public int ComboId {get; set;}
        [ForeignKey("ComboId")]
        public Combo? Combo {get; set;}
        public int Quantity {get; set;}


        public DateTime Time {get; set;} = DateTime.Now;
        public bool Status {get; set;} = false;
    }
}