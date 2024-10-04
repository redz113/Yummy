using System.ComponentModel.DataAnnotations;

namespace AppFoods.Models
{
    public class DataThongKe{
        public int Key {get; set;}

        [DataType(DataType.Currency)]
        public int Total {get; set;}
    }

    public class DataThongKeNgay{
        public string Name {get; set;}

        public List<DataThongKe> Data {get; set;}
    }
}