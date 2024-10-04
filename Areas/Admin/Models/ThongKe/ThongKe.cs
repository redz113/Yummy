using System.ComponentModel.DataAnnotations;

namespace AppFoods.Models
{
    public class ThongKe
    {
        public Restaurant Restaurant {get; set;}
        public string TenCoSo {get; set;}
        [DataType(DataType.Currency)]
        public int TotalDay {get; set;}
        [DataType(DataType.Currency)]
        public int TotalMonth {get; set;}
        [DataType(DataType.Currency)]
        public int TotalYear {get; set;}

        public int scaleDay {get; set;}         // $
        public int scaleMonth {get; set;}       // %
        public int scaleYear {get; set;}        // %
    }
}