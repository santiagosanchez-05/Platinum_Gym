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
                Credentials = new NetworkCredential("mvaleriano1105@gmail.com", "topu yzup buav ywsw"),
                EnableSsl = true
            };

            return smtp.SendMailAsync("mvaleriano1105@gmail.com", to, subject, body);
        }

    }
}
