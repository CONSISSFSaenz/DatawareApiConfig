using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Dapper;
using DatawareConfig.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;

namespace DatawareConfig.Helpers
{
    public static class ApiHelper
    {
        public const string urlSubirDocumentoDatadocs = "Documento";
        public const string urlLoginDatadocs = "usuario/login";
        public const string urlCreateFolderDatadocs = "Fichero/Create";
        public const string urlScopes = "Documento/scopes";

        public async static Task<string> GetUrlDataDocs()
        {
            string cnxStr = LogsDataware.CnxStrDb();

            string? dts = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Sistema.SP_ApiDataDocs", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "Url";
                    dts = Convert.ToString(await cmd.ExecuteScalarAsync());

                }
                await cnx.CloseAsync();
            }
            if (dts == null)
            {
                return "NODATA";
            }
            else
            {
                return dts;
            }
        }

        public static async Task<string> GetDtsDatadocs(string tipoAccion, string? valor = null)
        {
            string cnxStr = LogsDataware.CnxStrDb();

            string? dts = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Sistema.SP_ApiDataDocs", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = tipoAccion;
                    if (tipoAccion == "UpdToken")
                    {
                        cmd.Parameters.Add("@Token", System.Data.SqlDbType.NVarChar).Value = valor;
                    }

                    dts = Convert.ToString(await cmd.ExecuteScalarAsync());

                }
                await cnx.CloseAsync();
            }
            if (dts == null)
            {
                return "NODATA";
            }
            else
            {
                return dts;
            }

        }

        public static async Task<string> GetTokenDatadocs(string tipoAccion)
        {
            string? dts = null;
            if (tipoAccion == "Get")
            {
                dts = await GetDtsDatadocs(tipoAccion);
                var split = dts.Split('|');
                return split[2];
            }
            else
            {
                dts = await GetDtsDatadocs(tipoAccion);
                var accesos = dts.Split('|');
                object usrpass = new
                {
                    usuario = accesos[0],
                    contraseña = accesos[1]
                };
                try
                {
                    var urlDD = await GetUrlDataDocs();
                    var (result, _httpResponseMessage) =
                    await HttpClientUtility.PostAsync<respDatadocsModel>(urlDD + urlLoginDatadocs, usrpass, null);
                    if (_httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        dts = await GetDtsDatadocs("UpdToken", result.token);
                        var split = dts.Split('|');
                        return split[2];
                    }
                    else if (_httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return await GetTokenDatadocs("UpdToken");
                    }
                    else
                    {
                        string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
                        string Html = "<p>Ha ocurrido un error al obtener el token DataDocs.</p><p>Fecha de Ejecución: " + FechaHora + " </p>"
                            + "<p>Favor obtener el token de forma manual e ingresarlo en la tabla de parametros para continuar con la operación.</p>";
                        SendMailHelper.Notificaciones(6, Html);
                        dts = await GetDtsDatadocs(tipoAccion);
                        var split = dts.Split('|');
                        return split[2];
                    }
                }
                catch (Exception ex)
                {
                    LogsDataware.LogSistema(3, LogsDataware.ERRORActualizar, "Error al obtener token", ex.Message);
                    return await GetTokenDatadocs("UpdToken");
                }

            }
        }

        public async static Task<string> UsarTokenDatadocs()
        {
            string token = "";
            var urlDD = await GetUrlDataDocs();
            try
            {

                var dts = await GetDtsDatadocs("Get");
                var split = dts.Split('|');
                token = split[2];
                var (result, _httpResponseMessage) =
                await HttpClientUtility.PostAsync<respDatadocsModel>(urlDD + urlScopes, null, token);
                if (_httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return token;
                }
                else if (_httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return await GetTokenDatadocs("UpdToken");
                }
                else
                {
                    return await GetTokenDatadocs("UpdToken");
                }

            }
            catch (Exception ex)
            {
                LogsDataware.LogSistema(3, LogsDataware.ERRORActualizar, "Error al obtener token", ex.Message);
                return await GetTokenDatadocs("UpdToken");
            }

        }

        public static async Task<string> UpdFolderDatadocs(string nombreFolder, object generalId, string? numIdFolder = null, string? tipoAccion = null)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            if (numIdFolder == null)
            {
                string token = "";
                if (tipoAccion == null)
                {
                    tipoAccion = "Get";
                    token = await GetTokenDatadocs(tipoAccion);
                }
                else
                {
                    token = await GetTokenDatadocs(tipoAccion);
                }

                object paramDatadocs = new
                {
                    ficheroConfiguracionId = 1,
                    vin = nombreFolder,
                    idOrigen = 1,
                    activo = true
                };
                try
                {
                    var urlDD = await GetUrlDataDocs();
                    var (result, _httpResponseMessage) =
                    await HttpClientUtility.PostAsyncString<object>(urlDD + urlCreateFolderDatadocs, paramDatadocs, token);
                    //await HttpClientUtility.PostAsyncString<object>("http://68.178.207.49:8099/api/usuario/lo", paramDatadocs, token);
                    if (_httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        using (SqlConnection cnx = new SqlConnection(cnxStr))
                        {
                            await cnx.OpenAsync();
                            using (SqlCommand cmd = new SqlCommand("Sistema.SP_CRUD_PreAlta", cnx))
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "UpdateFicheroGeneral";
                                cmd.Parameters.Add("@NumIdFolder", System.Data.SqlDbType.NVarChar).Value = result;
                                cmd.Parameters.Add("@Folder", System.Data.SqlDbType.NVarChar).Value = nombreFolder;
                                cmd.Parameters.Add("@GeneralId", System.Data.SqlDbType.UniqueIdentifier).Value = generalId;

                                await cmd.ExecuteScalarAsync();
                            }
                            await cnx.CloseAsync();
                        }
                    }
                    else if (_httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await UpdFolderDatadocs(nombreFolder, generalId, null, "UpdToken");
                    }
                    else
                    {
                        string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
                        string Html = "<p>Ha ocurrido un error al momento de crear el folder de la unidad con el VIN: <strong>" + nombreFolder + "</strong>.</p><p>Fecha de Ejecución: " + FechaHora + " </p>"
                            + "<p>Favor de generar el folder en DataDocs manualmente.</p>";
                        SendMailHelper.Notificaciones(5, Html);
                    }

                }
                catch (Exception ex)
                {
                    LogsDataware.LogSistema(3, LogsDataware.ERRORInsertar, "Crear Folder VIN: " + nombreFolder, ex.Message);
                    await UpdFolderDatadocs(nombreFolder, generalId, null, tipoAccion);
                }
                return "FolderDataDocs";
            }
            else
            {
                string? resultUpdFolder = "";
                using (SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Sistema.SP_CRUD_PreAlta", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "UpdateFicheroGeneral";
                        cmd.Parameters.Add("@NumIdFolder", System.Data.SqlDbType.NVarChar).Value = Convert.ToString(numIdFolder);
                        cmd.Parameters.Add("@Folder", System.Data.SqlDbType.NVarChar).Value = nombreFolder;
                        cmd.Parameters.Add("@GeneralId", System.Data.SqlDbType.UniqueIdentifier).Value = generalId;

                        //resultUpdFolder = Convert.ToString(await cmd.ExecuteScalarAsync());
                        var resultExecute = await cmd.ExecuteScalarAsync();
                        if (resultExecute != null)
                        {
                            resultUpdFolder = "Folder guardado correctamente";
                        }
                        else
                        {
                            resultUpdFolder = "Error al guardar folder";
                        }
                        
                    }
                    await cnx.CloseAsync();

                    return resultUpdFolder;
                }

            }


        }

        public static async Task<int> CrearFolderSyncInventario(long identifier)
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
                        await UpdFolderDatadocs(Vin, GId);
                    }
                }
                else
                {
                    total = 0;
                }

                await cnx.CloseAsync();
                return total;
            }
        }
    }
}
