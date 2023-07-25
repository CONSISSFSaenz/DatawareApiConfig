using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Helpers;
using DatawareConfig.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Office.Interop.Excel;
using System.Net.NetworkInformation;

namespace DatawareConfig.Servicios
{
    public class EliminarRegistros
    {
        public static async Task<(int,string)> DelIntelimotorIds(string apiUrl,string apiKey, string apiSecret,object userId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            int resultados = 0;
            int updexito = 0;
            int adderror = 0;
            string respuesta = "";
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            //https://app.intelimotor.com/api/units/unitId?apiKey={YOUR_API_KEY}}&apiSecret={YOUR_API_SECRET}
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if(cnx.State == System.Data.ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Sistema].[SP_EliminarIntelimotor] @Accion='Get'";
                var rows = await cnx.QueryAsync<string>(sql);
                var total = rows.Count();
                if(total > 0)
                {
                    foreach (var row in rows)
                    {
                        if(!string.IsNullOrEmpty(row))
                        {
                            var UrlIntelimotor = $"{apiUrl}units/{row}?apiKey={apiKey}&apiSecret={apiSecret}";
                            var (result, _httpResponseMessage) =
                            await HttpClientUtility.DeleteAsyncObject<ResponseDataDTOModel>(UrlIntelimotor, null);
                            if(_httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                updexito = await UpdExito(row);
                                if(updexito == 0)
                                {
                                    LogsDataware.LogSistema(LogsDataware.Dataware,LogsDataware.ERRORActualizar,"Proceso automático","Error al actualizar campos Intelimotor en General. IntelimotorId="+row);
                                }
                                resultados++;
                            }
                            else if(_httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                adderror = await AddError(row,result.error,userId);
                                if(adderror == 0)
                                {
                                    LogsDataware.LogSistema(LogsDataware.Dataware, LogsDataware.ERRORInsertar, "Proceso automático", "Error al insertar registro de falla al eliminar de intelimotor. IntelimotorId=" + row);
                                }
                                resultados = 0;
                            }
                            else
                            {
                                adderror = await AddError(row,_httpResponseMessage.ReasonPhrase,userId);
                                if(adderror == 0)
                                {
                                    LogsDataware.LogSistema(LogsDataware.Dataware, LogsDataware.ERRORInsertar, "Proceso automático", "Error al insertar registro de falla al eliminar de intelimotor. IntelimotorId=" + row);
                                }
                                resultados = 0;
                            }
                            respuesta += row + ",";
                        }
                    }
                    EnviarNotificacion();
                }
                else
                {
                    resultados = 0;
                }
                await cnx.CloseAsync();
                
            }

            var resIds = respuesta.Split(',');
            string totalIds = "";
            if(resIds.Length == 2)
            {
                totalIds = resIds[0].Replace(",","");
            }
            else if (resIds.Length == 0)
            {
                totalIds = "Sin resultados";
            }
            else
            {
                totalIds = respuesta;
            }

            return (resultados,totalIds);
        }

        public static async Task<int> UpdExito(string intelimotorId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            int resultados = 0;

            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == System.Data.ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Sistema].[SP_EliminarIntelimotor] @Accion='UpdByIntelimotorId',@IntelimotorId='" + intelimotorId + "'";
                var rows = await cnx.QueryAsync<int>(sql);
                var total = rows.Count();
                if (total > 0)
                {
                    foreach (var row in rows)
                    {
                        resultados = row;
                    }
                }
                else
                {
                    resultados = 0;
                }
                await cnx.CloseAsync();

            }

            return resultados;
        }

        public static async Task<int> AddError(string intelimotorId, string error, object userId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            int resultados = 0;

            using(SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if(cnx.State == System.Data.ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Sistema].[SP_EliminarIntelimotor] @Accion='Add',@IntelimotorId='"+intelimotorId+"',@Error='"+error+"',@UserId='"+userId+"'";
                var rows = await cnx.QueryAsync<int>(sql);
                var total = rows.Count();
                if(total > 0)
                {
                    foreach( var row in rows)
                    {
                        resultados = row;
                    }
                }
                else
                {
                    resultados = 0;
                }
                await cnx.CloseAsync();

            }

            return resultados;
        }

        public static async void EnviarNotificacion()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
            var filasTablaHtml = "";
            using(SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if(cnx.State == System.Data.ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC Sistema.SP_EliminarIntelimotor @Accion='GetDataNoti'";
                var rows = await cnx.QueryAsync<SyncInventarioAltaFueraDataware>(sql);
                var total = rows.Count();
                if(total > 0)
                {
                    foreach( var row in rows)
                    {
                        string FechaAdquisicion = row.Fecha + " " + row.Hora;
                        filasTablaHtml += "<tr><td>" + row.Vin + "</td><td>" + row.NombreMarca + "</td><td>" + row.NombreModelo + "</td><td>" + row.NombreYear + "</td><td>" + row.NombreVersion
                            + "</td><td>" + row.CVColorValue + "</td><td>" + FechaAdquisicion.Replace("00:00:00", "") + "</td></tr>";
                    }

                    var txtHeaderHtml = "<p>Se ha ejecutado el proceso de Eliminación automática de Intelimotor, se encontró que hay " + total + " vehículos que no se pudieron eliminar.</p><p>Fecha de Ejecución: " + FechaHora + " </p>";

                    var tablaHtml = "<table role='presentation' style='width:100%;border:1 solid #000;background:#FFF'>"
                        + "<tr><td>VIN</td><td>MARCA</td><td>MODELO</td><td>AÑO</td><td>VERSION</td><td>COLOR</td><td>FECHA ADQUISICION</td><tr>";

                    var txtFooterHtml = "</table><br>";

                    var Html = txtHeaderHtml + tablaHtml + filasTablaHtml + txtFooterHtml;

                    SendMailHelper.Notificaciones(2, Html);

                }
                cnx.CloseAsync();
            }



        }
    }
}
