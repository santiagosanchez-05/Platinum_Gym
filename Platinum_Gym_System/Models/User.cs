using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Platinum_Gym_System.Models
{
    public class User
    {
        [Key]
        public int UserId {  get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")] //Es obligatorio
        [Display(Name = "Razon social")] // Cambiar el nombre en pantalla
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        public string? BillingName { get; set; }
        [Required(ErrorMessage = "El CI es obligatorio.")]
        [RegularExpression(@"^\d{5,10}(-?[A-Za-z]{1,2})?$",
         ErrorMessage = "Formato inválido. Ejemplos válidos: 1234567, 1234567LP, 1234567-LP, 1234567A")]
        public string CI { get; set; }


    }
}
