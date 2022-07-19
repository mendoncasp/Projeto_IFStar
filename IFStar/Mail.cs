using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace IFStar
{
    public class Mail
    {
        public static void Send(string assunto, string conteudo, string email)
        {

            try
            {
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.office365.com", 587);

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("testeifstar2.0@outlook.com", "DEVifstarFINAL2022");
                smtp.EnableSsl = true;

                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress("testeifstar2.0@outlook.com", "Grande Final - IFStar");

                mail.To.Add(new System.Net.Mail.MailAddress(email));

                mail.IsBodyHtml = false;
                mail.Body = conteudo;
                mail.Subject = assunto;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.BodyEncoding = Encoding.UTF8;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao enviar e-mail de Recuperação de Senha: " + ex.Message);
            }
        }
    }
}