using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Product Name")]
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters long.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        //[Display(Name = "Category")]
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maximum {1} characters long.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters and spaces are allowed.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between {1} and {2}.")]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Photo")]
        [StringLength(255, ErrorMessage = "The image path cannot exceed {1} characters.")]
        [DataType(DataType.ImageUrl)]
        public string? ProductImage { get; set; }
    }
}
