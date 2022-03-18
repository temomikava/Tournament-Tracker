using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace TrackerLibrary
{
    public static class EmailLogic
    {
        public readonly static string Email = "temsonmikava@gmail.com";
        public readonly static string senderDisplayName = "Temo Mikava";
        public static void SendEmail(string to,string subject, string body)
        {
            MailAddress fromMailAddress=new MailAddress(Email,senderDisplayName);
            MailMessage message=new MailMessage();
            message.To.Add(to);
            message.From=fromMailAddress;
            message.Subject=subject;
            message.Body=body;
            message.IsBodyHtml=true;
            SmtpClient client=new SmtpClient();
            client.Send(message);
        }
    }
}
