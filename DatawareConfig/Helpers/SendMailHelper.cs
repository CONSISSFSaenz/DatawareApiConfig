using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DatawareConfig.Helpers
{
    public static class SendMailHelper
    {
        public static void Send(string tipomsg, string asunto, string mensaje)
        {
            var email = new MimeMessage();

            string remitente = "dataware@datamovil.com";
            string usuariomail = "dataware@datamovil.com";
            string destinatario = "daniel.lopez@consiss.com";
            string password = "cSe7m4N9sK";
            string host = "mail.datamovil.com";
            int port = 25;
            bool useSSL = false;

            email.From.Add(MailboxAddress.Parse(remitente));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;

            string tipoNotificacion;
            if (tipomsg == "OK")
            {
                tipoNotificacion = "El proceso se ha completado";
            }
            else if (tipomsg == "ERROR")
            {
                tipoNotificacion = "Ha ocurrido un error";
            }
            else
            {
                tipoNotificacion = "Advertencia";
            }

            string contenido =
                "<strong style='font-size:16px;color:steelblue'>Notificación del sistema</strong><hr>" +
                "<strong>¡" + tipoNotificacion + "!</strong><br>" +
                "<strong>Detalles:</strong><br>" + mensaje + "<br>" +
                "-- <br><br>" +
                "<i style='font-size:11px;'>Este correo es generado automaticamente por el servidor. Favor de no responder.</i>";


            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = contenido };

            using var smtp = new SmtpClient();
            //smtp.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
            smtp.Connect(host, 45, SecureSocketOptions.None);
            smtp.Authenticate(usuariomail, password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}