using System.Net.Mail;

namespace DatawareConfig.Helpers
{
    public static class Notificaciones
    {
        public static void Enviar(string tipomsg, string asunto, string mensaje)
        {
            string remitente = "dataware@datamovil.com";
            string usuariomail = "dataware@datamovil.com";
            string destinatario = "alex.saenz@consiss.com";
            string password = "cSe7m4N9sK";
            MailMessage mail = new MailMessage(remitente, destinatario);

            mail.IsBodyHtml = true;
            mail.Subject = asunto;

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

            mail.Body = contenido;

            SmtpClient smtpClient = new SmtpClient();

            smtpClient.Host = "mail.datamovil.com";
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(usuariomail, password);
            smtpClient.EnableSsl = false;
            smtpClient.Send(mail);
        }
    }
}
