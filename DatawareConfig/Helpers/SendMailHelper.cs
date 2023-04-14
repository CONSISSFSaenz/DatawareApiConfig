using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using MimeKit;
using System.Data;

namespace DatawareConfig.Helpers
{
    public static class SendMailHelper
    {
        internal class NotificacionModel
        {
            public string? TipoCorreo { get; set; }
            public string? Correos { get; set; }
            public string? Prioridad { get; set; }
            public string? Contenido { get; set; }
            public bool? Status { get; set; }
        }
        public static async void Notificaciones(long tipoCorreoId, string contenidoMsj)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            NotificacionModel obj = new NotificacionModel();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Notificaciones.SP_GetAll_TipoCorreos", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "TipoCorreosById";
                        cmd.Parameters.Add("@TablaId", System.Data.SqlDbType.BigInt).Value = tipoCorreoId;
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            if (reader.HasRows)
                            {
                                obj = new NotificacionModel
                                {
                                    TipoCorreo = reader.GetString(0),
                                    Correos = reader.GetString(1),
                                    Prioridad = reader.GetString(2),
                                    Contenido = reader.GetString(3),
                                    Status = reader.GetBoolean(4)
                                };

                                if (obj.Status == true)
                                {
                                    var toEmails = obj.Correos.Split(',');
                                    foreach (var toEmail in toEmails)
                                    {
                                        Enviar(obj.TipoCorreo, toEmail, obj.Prioridad, obj.Contenido.Replace("{ContainerMail}", contenidoMsj));
                                    }

                                }
                            }

                        }

                    }
                await cnx.CloseAsync();
            }

        }

        public static void Enviar(string subject, string toEmail, string priority, string container)
        {
            string Fecha = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            var email = new MimeMessage();

            string remitente = "dataware@datamovil.com";
            string usuariomail = "dataware@datamovil.com";
            string password = "cSe7m4N9sK";
            string host = "mail.datamovil.com";
            int port = 25;
            bool useSSL = false;

            email.From.Add(MailboxAddress.Parse(remitente));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject + " | " + priority + " - " + Fecha;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = container };

            using var smtp = new SmtpClient();
            smtp.Connect(host, 45, SecureSocketOptions.None);
            smtp.Authenticate(usuariomail, password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

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