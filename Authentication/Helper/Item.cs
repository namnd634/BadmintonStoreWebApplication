using System.ComponentModel.DataAnnotations;

namespace Authentication.Helper
{
    public class Item
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "Old Price")]
        [DataType(DataType.Currency)]
        //[DisplayFormat(DataFormatString = "{0:C2}")]
        public double Price { get; set; }
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public double Discount { get; set; }

        [Display(Name = "New Price")]
        [DataType(DataType.Currency)]
        public double NewPrice
        {
            get
            {
                return Price * (1 - Discount);
            }
        }

        [DataType(DataType.Currency)]
        public double Amount
        {
            get
            {
                return Quantity * NewPrice;
            }
        }
    }
}
