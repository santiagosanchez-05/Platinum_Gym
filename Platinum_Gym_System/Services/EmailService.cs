using System.Net;
using System.Net.Mail;

namespace Platinum_Gym_System.Services
{
    public class EmailService
    {
        public static Task SendAsync(string to, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("santiagsanchez05@gmail.com", "TU_CLAVE_APP"),
                EnableSsl = true
            };

            return smtp.SendMailAsync("santiagsanchez05@gmail.com", to, subject, body);
        }

    }
}
