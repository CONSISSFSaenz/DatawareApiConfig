using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Microsoft.Data.SqlClient;

namespace DatawareConfig.Helpers
{
    public class AcendesHelper
    {
        #region Conexion y Token
        public const string urlLoginAcendes = "session/auth/login";
        public class ReqAuthLoginAcendes
        {
            public string? login { get; set; }
            public string? password { get; set; }
        }
        public class sidTokenAcendes
        {
            public string? sid { get; set; }
            public string? expires_at { get; set; }
        }

        public class RespAuthLoginAcendes
        {
            public sidTokenAcendes? session { get; set; }
        }

        public async static Task<string> GetDtsAcendes(string tipoAccion, string? token = null, string? fechaExpira = null)
        {
            string cnxStr = LogsDataware.CnxStrDb();

            string? dts = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Sistema.SP_ApiAcendes", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = tipoAccion;
                    if (tipoAccion == "UpdToken")
                    {
                        cmd.Parameters.Add("@Token", System.Data.SqlDbType.NVarChar).Value = token;
                        cmd.Parameters.Add("@Fecha", System.Data.SqlDbType.NVarChar).Value = fechaExpira;
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

        public async static Task<string> GetTokenAcendes(string tipoAccion)
        {
            string? dts = null;
            if (tipoAccion == "Get")
            {
                dts = await GetDtsAcendes(tipoAccion);
                var split = dts.Split('|');
                return split[2];
            }
            else
            {
                dts = await GetDtsAcendes("Get");
                var accesos = dts.Split('|');
                ReqAuthLoginAcendes dtsAccesos = new ReqAuthLoginAcendes
                {
                    login = accesos[0],
                    password = accesos[1]
                };

                try
                {
                    var urlAcendes = accesos[3];
                    var (result, _httpResponseMessage) =
                    await HttpClientUtility.PostAsync<RespAuthLoginAcendes>(urlAcendes + urlLoginAcendes, dtsAccesos, null);
                    if (_httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        dts = await GetDtsAcendes("UpdToken", result.session.sid, result.session.expires_at);
                        var split = dts.Split('|');
                        return split[2];
                    }
                    else if (_httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return await GetTokenAcendes("UpdToken");
                    }
                    else
                    {
                        string FechaHora = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")).ToString("yyyy-MM-dd HH:mm:ss");
                        string Html = "<p>Ha ocurrido un error al obtener el token Acendes.</p><p>Fecha de Ejecución: " + FechaHora + " </p>"
                            + "<p>Favor obtener el token de forma manual e ingresarlo en la tabla de parametros para continuar con la operación.</p>";
                        SendMailHelper.Notificaciones(6, Html);
                        dts = await GetDtsAcendes(tipoAccion);
                        var split = dts.Split('|');
                        return split[2];
                    }
                }
                catch (Exception ex)
                {
                    LogsDataware.LogSistema(3, LogsDataware.ERRORActualizar, "Error al obtener token", ex.Message);
                    return await GetTokenAcendes("UpdToken");
                }

            }
        }

        public async static Task<(string, string)> UsarTokenAcendes()
        {
            var dts = await GetDtsAcendes("Get");
            var split = dts.Split('|');
            var token = split[2];
            var url = split[3];
            var expired = Convert.ToInt32(split[4]);
            if (expired < 5)
            {
                var ndts = await GetTokenAcendes("UpdToken");
                return (ndts, url);

            }
            else
            {
                return (token, url);
            }
        }
        #endregion

        #region Procesos

        #region URLS
        public const string urlMantenerDriveTest = "interfaces/autos/mantenerdrivetest";
        public const string urlDeleteDriveTest = "interfaces/autos/deletedrivetest";
        public const string urlSepVeh = "interfaces/autos/sepveh";
        public const string urlUploadDocument = "interfaces/documentos/uploaddocument";
        public const string urlSegAuto = "interfaces/procesos/segauto";
        public const string urlLiberaEnganche = "interfaces/procesos/liberaenganche";
        public const string urlSerDMS = "interfaces/autos/serdms";
        public const string urlCambioEstatus = "interfaces/procesos/cambioestatus";
        public const string urlGestoria = "interfaces/procesos/gestoria";
        public const string urlCancelacionGestoria = "interfaces/procesos/cancelaciongestoria";
        public const string urlEntrega = "interfaces/procesos/entrega";
        #endregion

        #region Entidades
        public class ReqMantenerPrueba
        {
            public string? sol_id { get; set; }
            public string? motivo_mantener { get; set; }
            public string? desc_mantener { get; set; }
            public string? vin { get; set; }
            public string? fecha_apartado { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
        }
        public class RespMantenerPrueba
        {
            public string? sol_id { get; set; }
            public string? motivo_mantener { get; set; }
            public string? desc_mantener { get; set; }
            public string? vin { get; set; }
            public string? fecha_apartado { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? prueba_manejo { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultMantenerPrueba
        {
            public RespMantenerPrueba result { get; set; }
        }

        public class ReqCargarDocumento
        {
            public string name { get; set; }
            public string type { get; set; }
            public string vin { get; set; }
            public string sol_id { get; set; }
            public string tipo_docto { get; set; }
            public string documento { get; set; }
        }
        public class RespCargaDocumento
        {
            public string status { get; set; }
            public string message { get; set; }
        }
        public class ResultCargaDocumento
        {
            public RespCargaDocumento result { get; set; }
        }

        public class ReqLiberarPrueba
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
        }
        public class RespLiberarPrueba
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? prueba_manejo { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultLiberarPrueba
        {
            public RespLiberarPrueba result { get; set; }
        }

        public class ReqSepararVehiculo
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? uso_unidad { get; set; }
        }
        public class RespSepararVehiculo
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? separado_vehiculo { get; set; }
            public bool? separado_vehiculo_ware { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
            public string? uso_unidad { get; set; }
        }
        public class ResultSepararVehiculo
        {
            public RespSepararVehiculo result { get; set; }
        }

        public class ReqSeguroAutorizado
        {
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? status_seguro { get; set; }
            public string? aseguradora { get; set; }
            public string? tel_aseguradora { get; set; }
            public string? uso_unidad { get; set; }
            public string? sol_id { get; set; }
            public string? poliza { get; set; }
            public string? fecha_ini_seguro { get; set; }
            public string? fecha_fin_seguro { get; set; }
            public int dias_vencidos_seguro { get; set; }
            public string? vin { get; set; }
            public bool? seguro_autorizado { get; set; }
            public bool? seguro_final { get; set; }
            public decimal costo { get; set; }
            public bool? seguro_proporcional { get; set; }
            public string? agente_seguro { get; set; }
            public string? seguro { get; set; }
        }
        public class RespSeguroAutorizado
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? seguro { get; set; }
            public string? aseguradora { get; set; }
            public string? poliza { get; set; }
            public string? fecha_ini_seguro { get; set; }
            public string? fecha_fin_seguro { get; set; }
            public int dias_vencidos_seguro { get; set; }
            public string? tel_aseguradora { get; set; }
            public bool? seguro_autorizado { get; set; }
            public bool? seguro_final { get; set; }
            public string? status_seguro { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
            public decimal costo { get; set; }
            public string? uso_unidad { get; set; }
            public bool? seguro_proporcional { get; set; }
            public string? agente_seguro { get; set; }
        }
        public class ResultSeguroAutorizado
        {
            public RespSeguroAutorizado result { get; set; }
        }

        public class ReqLiberaEnganche
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? motivo_enganche { get; set; }
            public string? desc_enganche { get; set; }
        }
        public class RespLiberaEnganche
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? enganche { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultLiberaEnganche
        {
            public RespLiberaEnganche result { get; set; }
        }

        public class ReqSerDMS
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? datamovil_contrato { get; set; }
        }
        public class RespSerDMS
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? datamovil_contrato { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultSerDMS
        {
            public RespSerDMS result { get; set; }
        }

        public class ReqCambioEstatus
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? motivo_cambio_estatus { get; set; }
            public string? desc_cambio_estatus { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? sol_estatus { get; set; }
        }
        public class RespCambioEstatus
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? motivo_cambio_estatus { get; set; }
            public string? desc_cambio_estatus { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? sol_estatus { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultCambioEstatus
        {
            public RespCambioEstatus result { get; set; }
        }

        public class ReqGestoria
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? numero_placas { get; set; }
        }
        public class RespGestoria
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? gestoria { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultGestoria
        {
            public RespGestoria result { get; set; }
        }

        public class ReqCancelacionGestoria
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? motivo_gestoria { get; set; }
            public string? desc_gestoria { get; set; }
        }
        public class RespCancelacionGestoria
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? motivo_gestoria { get; set; }
            public string? desc_gestoria { get; set; }
            public bool? gestoria { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultCancelacionGestoria
        {
            public RespCancelacionGestoria result { get; set; }
        }

        public class ReqEntrega
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? kilometraje { get; set; }
        }
        public class RespEntrega
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? entrega { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultEntrega
        {
            public RespEntrega result { get; set; }
        }

        #endregion

        #region EndPoints
        public async static Task<(string, string)> MantenerPrueba(string solId, string motivoMantener, string descMantener, string Vin, string fechaApartado, string idDatamovil, string statusDatamovil)
        {
            var (token, url) = await UsarTokenAcendes();
            ReqMantenerPrueba request = new ReqMantenerPrueba();
            try
            {
                request = new ReqMantenerPrueba
                {
                    sol_id = solId,
                    motivo_mantener = motivoMantener,
                    desc_mantener = descMantener,
                    vin = Vin,
                    fecha_apartado = fechaApartado,
                    id_datamovil = idDatamovil,
                    status_datamovil = statusDatamovil,
                };
                var (resultPost, _httpResponse) =
                    await HttpClientUtility.PostAsyncAcendes<ResultMantenerPrueba>(url + urlMantenerDriveTest, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return (resultPost.result.status, resultPost.result.message);
                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> CargaDocumento(string nombreDoc, string tipoArchivo, string Vin, string solId, string tipoDoc, string documento)
        {
            var (token, url) = await UsarTokenAcendes();
            ReqCargarDocumento request = new ReqCargarDocumento();
            try
            {
                request = new ReqCargarDocumento
                {
                    name = nombreDoc,
                    type = tipoArchivo,
                    vin = Vin,
                    sol_id = solId,
                    tipo_docto = tipoDoc,
                    documento = documento
                };

                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCargaDocumento>(url + urlUploadDocument, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return (resultPost.result.status, resultPost.result.message);
                    //return ("ok", "OK");
                }
                else
                {
                    return ("Error", "Error al cargar documento en Acendes");
                }


            }
            catch (Exception ex)
            {
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> LiberarPrueba(string solId, string Vin, string idDatamovil, string statusDatamovil)
        {
            var (token, url) = await UsarTokenAcendes();
            ReqLiberarPrueba request = new ReqLiberarPrueba();
            try
            {
                request = new ReqLiberarPrueba
                {
                    sol_id = solId,
                    vin = Vin,
                    id_datamovil = idDatamovil,
                    status_datamovil = statusDatamovil
                };

                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultLiberarPrueba>(url + urlDeleteDriveTest, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result.status == null)
                    {
                        return ("Error", "Error al procesar información en Acendes");
                    }
                    else
                    {
                        return (resultPost.result.status, resultPost.result.message);
                        //return ("proceso_correcto", "OK");
                    }

                }
                else
                {
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> SepararVehiculo(string solId, string Vin, string idDatamovil, string statusDatamovil, string usoUnidad)
        {
            var (token, url) = await UsarTokenAcendes();
            ReqSepararVehiculo request = new ReqSepararVehiculo();
            request = new ReqSepararVehiculo
            {
                sol_id = solId,
                vin = Vin,
                id_datamovil = idDatamovil,
                status_datamovil = statusDatamovil,
                uso_unidad = usoUnidad
            };
            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSepararVehiculo>(url + urlSepVeh, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> SeguroAutorizado(ReqSeguroAutorizado request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSeguroAutorizado>(url + urlSegAuto, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> LiberaEnganche(ReqLiberaEnganche request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultLiberaEnganche>(url + urlLiberaEnganche, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> SerDMS(ReqSerDMS request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSerDMS>(url + urlSerDMS, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> CambioEstatus(ReqCambioEstatus request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCambioEstatus>(url + urlCambioEstatus, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> Gestoria(ReqGestoria request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultGestoria>(url + urlGestoria, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> CancelacionGestoria(ReqCancelacionGestoria request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCancelacionGestoria>(url + urlCancelacionGestoria, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        public async static Task<(string, string)> Entrega(ReqEntrega request)
        {
            var (token, url) = await UsarTokenAcendes();

            var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultEntrega>(url + urlEntrega, request, token);
            if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (resultPost.result.status, resultPost.result.message);
                //return ("proceso_correcto", "OK");
            }
            else
            {
                return ("Error", "El proceso falló al notificar a Acendes");
            }
        }
        #endregion

        #endregion

    }
}
