using System.ComponentModel.DataAnnotations;

namespace Platinum_Gym_System.Models
{
    public class Product
    {
        [Key] 
        public int ProductId { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")] //Es obligatorio
        [Display(Name = "Nombre del Producto")] 
        // Cambiar el nombre en pantalla
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Solo se permiten letras y espacios.")]
        public string ProductName { get; set; } 
        [Required(ErrorMessage = "La categoría es obligatoria")] 
        //Es obligatorio [Display(Name = "Categoría")]
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Solo se permiten letras y espacios.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre {1} y {2}")]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        [Required(ErrorMessage = "La cantidad en stock es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo")]
        public int StockQuantity { get; set; } 
        [Display(Name = "Foto")] 
        [RegularExpression(@"^[\w\-/\\]+(\.(jpg|jpeg|png|gif|bmp))$", ErrorMessage = "Debe ser una ruta válida de imagen.")]
        [StringLength(255, ErrorMessage = "La ruta de la imagen no puede superar {1} caracteres")]

        [DataType(DataType.ImageUrl)]
        public string? ProductImage { get; set; }
    }
}
