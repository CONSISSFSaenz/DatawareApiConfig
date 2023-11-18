using Dapper;
using DatawareConfig.Models;
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

        public static async Task<int> AltaInventarioIntelimotor(long identifier)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            var filasTablaHtml = "";
            //string FechaAdquisicion = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "SELECT * FROM Sistema.VWSyncInventarioIntelimotorAltaFueraDataware WHERE ValidacionVIN = 1 AND ValidacionColor = 1 AND ValidacionMMYV = 4 AND Identifier=" + identifier;
                var rows = await cnx.QueryAsync<SyncInventarioAltaFueraDataware>(sql);
                var total = rows.Count();
                foreach(var row in rows)
                {
                    string FechaAdquisicion = row.Fecha + " " + row.Hora;
                    filasTablaHtml += "<tr><td>" + row.Vin + "</td><td>" + row.NombreMarca + "</td><td>" + row.NombreModelo + "</td><td>" + row.NombreYear + "</td><td>" + row.NombreVersion 
                        + "</td><td>" + row.CVColorValue + "</td><td>" + FechaAdquisicion.Replace("00:00:00","") + "</td></tr>";
                }

                var txtHeaderHtml = "<p>Se ha ejecutado el proceso de Sync del Inventario de Intelimotor, se encontró que hay "+total+" vehículos dados de alta.</p><p>Fecha de Ejecución: "+FechaHora+" </p>";

                var tablaHtml = "<table role='presentation' style='width:100%;border:1 solid #000;background:#FFF'>"
                    + "<tr><td>VIN</td><td>MARCA</td><td>MODELO</td><td>AÑO</td><td>VERSION</td><td>COLOR</td><td>FECHA ADQUISICION</td><tr>";

                var txtFooterHtml = "</table><br>";

                await cnx.CloseAsync();
                if(total > 0)
                {
                    var Html = txtHeaderHtml + tablaHtml + filasTablaHtml + txtFooterHtml;

                    Notificaciones(3, Html);
                }
                
                return total;
            }
        }

        /*public static async void CrearFolderSyncInventario(long identifier)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            var filasTablaHtml = "";
            //string FechaAdquisicion = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "SELECT * FROM Sistema.VWVehiculosPrealtaInventarioIntelimotorCrearFolder WHERE Identifier=" + identifier;
                var rows = await cnx.QueryAsync<SyncInventarioVehiculosCrearFolder>(sql);
                var total = rows.Count();
                if(total > 0)
                {
                    foreach (var row in rows)
                    {
                        var Vin = row.Vin;
                        var GId = row.GeneralId;
                        await ApiHelper.UpdFolderDatadocs(Vin, GId);
                    }
                }
                
                await cnx.CloseAsync();

            }
        }*/

        /*public static async Task<int> CrearFolderSyncInventario(long identifier)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "SELECT * FROM Sistema.VWVehiculosPrealtaInventarioIntelimotorCrearFolder WHERE Identifier=" + identifier;
                var rows = await cnx.QueryAsync<SyncInventarioVehiculosCrearFolder>(sql);
                var total = rows.Count();
                if (total > 0)
                {
                    foreach (var row in rows)
                    {
                        var Vin = row.Vin;
                        var GId = row.GeneralId;
                        await ApiHelper.UpdFolderDatadocs(Vin, GId);
                    }
                }
                else
                {
                    total = 0;
                }

                await cnx.CloseAsync();
                return total;
            }
        }*/

        public static async Task<int> AltaInventarioIntelimotorErrores(long identifier)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            var filasTablaHtml = "";
            //string FechaAdquisicion = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "SELECT * FROM Sistema.VWSyncInventarioIntelimotorAltaFueraDataware WHERE (ValidacionVIN = 0 OR ValidacionColor = 0 OR ValidacionMMYV < 4) AND Identifier=" + identifier;
                var rows = await cnx.QueryAsync<SyncInventarioAltaFueraDataware>(sql);
                var total = rows.Count();
                foreach (var row in rows)
                {
                    string FechaAdquisicion = row.Fecha + " " + row.Hora;
                    filasTablaHtml += "<tr><td>" + row.Vin + "</td><td>" + row.NombreMarca + "</td><td>" + row.NombreModelo + "</td><td>" + row.NombreYear + "</td><td>" + row.NombreVersion
                        + "</td><td>" + row.CVColorValue + "</td><td>" + FechaAdquisicion.Replace("00:00:00", "") + "</td></tr>";
                }

                var txtHeaderHtml = "<p>Se ha ejecutado el proceso de Sync del Inventario de Intelimotor, se encontró que hay " + total + " vehículos dados de alta con errores.</p><p>Fecha de Ejecución: " + FechaHora + " </p>";

                var tablaHtml = "<table role='presentation' style='width:100%;border:1 solid #000;background:#FFF'>"
                    + "<tr><td>VIN</td><td>MARCA</td><td>MODELO</td><td>AÑO</td><td>VERSION</td><td>COLOR</td><td>FECHA ADQUISICION</td><tr>";

                var txtFooterHtml = "</table><br>";

                await cnx.CloseAsync();
                if (total > 0)
                {
                    var Html = txtHeaderHtml + tablaHtml + filasTablaHtml + txtFooterHtml;

                    Notificaciones(2, Html);
                }

                return total;
            }
        }



        public static async void ErrorSyncInventarioIntelimotor(string contenidoMsj)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            string Html = "<p>Se ha ejecutado el proceso de Sync del Inventario de Intelimotor.</p><p>Fecha de Ejecución: " + FechaHora + " </p>"
                + "<p>Detalles:</p><p>" + contenidoMsj + "</p>";
            Notificaciones(2, Html);
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
                                        var containerTipoCorreo = obj.Contenido.Replace("{ContainerTipoCorreo}", obj.TipoCorreo);
                                        //var containerHref = containerTipoCorreo.Replace("{ContainerHref}", "#");
                                        var containerDescripcion = containerTipoCorreo.Replace("{ContainerDescripcion}", contenidoMsj);
                                        Enviar(obj.TipoCorreo, toEmail, obj.Prioridad, containerDescripcion);
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
            string destinatario = "alex.saenz@consiss.com";
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

        public static async void CorreoGenerico(string asunto, string mensaje)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            NotificacionModel obj = new NotificacionModel();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Notificaciones.SP_GetAll_TipoCorreos", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "TipoCorreosById";
                    cmd.Parameters.Add("@TablaId", System.Data.SqlDbType.BigInt).Value = 9;
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
                                    var containerTipoCorreo = obj.Contenido.Replace("{ContainerTipoCorreo}", asunto);
                                    var containerDescripcion = containerTipoCorreo.Replace("{ContainerDescripcion}", mensaje);
                                    Enviar(asunto, toEmail, obj.Prioridad, containerDescripcion);
                                }

                            }
                        }

                    }

                }
                await cnx.CloseAsync();
            }
        }
    }
}