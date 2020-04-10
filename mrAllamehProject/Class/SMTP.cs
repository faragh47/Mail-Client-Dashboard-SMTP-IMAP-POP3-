using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;

namespace mrAllamehProject.Class
{
    class SMTP
    {


        public void connectToSMTP(string from,string to,string sub,string body,string pass)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress(from);
                message.To.Add(new MailAddress(to));
                message.Subject = sub;
                message.Body = body;
                /*message.From = new MailAddress("socket1socket@gmail.com");
                message.To.Add(new MailAddress("socket02socket@gmail.com"));
                message.Subject = "Test";
                message.Body = "Content";*/

                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(from, pass);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("err: " + ex.Message);
            }


        }

    }
}
