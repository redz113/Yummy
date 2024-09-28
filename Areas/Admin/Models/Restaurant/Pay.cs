using System.ComponentModel.DataAnnotations;

namespace AppFoods.Models
{
    public class Pay
    {
        public int Id {get; set;}
        [Display(Name = "Tên món")]
        public string Name {get; set;}
        [Display(Name = "Giá")]
        [DataType(DataType.Currency)]
        public int Price {get; set;}
        [Display(Name = "Số lượng")]
        public int Quantity {get; set;}

        [Display(Name = "G x SL")]
        [DataType(DataType.Currency)]
        public int PxQ {get; set;}

        public bool Status {get; set;}

        public DateTime Time {get; set;}

        public bool IsMenu {get; set;}
    }
}