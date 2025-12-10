using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Platinum_Gym_System.Models
{
    public class User
    {
        [Key]
        public int UserId {  get; set; }
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Billing Name")] // Cambiar el nombre en pantalla
        [StringLength(50, ErrorMessage = "{0} must be: minimum {2} and maxmum {1} character long ", MinimumLength = 3)]
        public string? BillingName { get; set; }
        [Required(ErrorMessage = "CI is required.")]
        [RegularExpression(@"^\d{5,10}(-?[A-Za-z]{1,2})?$",
          ErrorMessage = "Invalid format. Valid examples: 1234567, 1234567LP, 1234567-LP, 1234567A")]
        public string? CI { get; set; }
        [StringLength(25, ErrorMessage = "Password must be minimun 6 and maximun 25 character long", MinimumLength = 6)]
        [Display(Name = "Password")]
        [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "Must contain uppercase, lowercase, and a number.")]
        public string? Password { get; set; }
        // 1 admin, 2 recepcionista, 3 cliente
        [Required(ErrorMessage = "Role is required")]
        [Display(Name ="Role")]
        public byte Role { get; set; }
        //1 activo 2 inactivo
        [Required(ErrorMessage = "State is required.")]
        [Display(Name = "Status")]
        public byte State { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Must be a valid email format.")]
        public string? Email { get; set; }

    }
}
