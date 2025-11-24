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
        public string? CI { get; set; }
        [StringLength(25, ErrorMessage = "Password must be minimun 6 and maximun 25 character long", MinimumLength = 6)]
        [Display(Name = "Contraseña")]
        [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Debe contener mayúscula, minúscula y número.")]
        public string? Password { get; set; }
        // 1 admin, 2 recepcionista, 3 cliente
        [Required(ErrorMessage ="El rol es obligatorio")]
        [Display(Name ="Rol")]
        public byte Role { get; set; }
        //1 activo 2 inactivo
        [Required(ErrorMessage ="El estado es obligatorio")]
        [Display(Name ="Estado")]
        public byte State { get; set; }
        [Display(Name ="Foto")]
        [RegularExpression(@"^[\w\-/\\]+(\.(jpg|jpeg|png|gif|bmp))$",
    ErrorMessage = "Debe ser una ruta válida de imagen.")]
        public string? Photo { get; set; }

    }
}
