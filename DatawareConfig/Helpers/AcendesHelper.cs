using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

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
        public const string urlExpediente = "interfaces/documentos/expediente";
        public const string urlLiberaVehiculo = "interfaces/autos/liberavehiculo";
        public const string urlCancelacionContrato = "interfaces/procesos/cancelacioncontrato";
        public const string urlRenovacionSeguro = "interfaces/procesos/renovacionseguro";
        #endregion

        #region PARAMETROS_EndPoint_CAMBIOESTATUS 
        public async static Task<(string, string)> CambioEstatusByEndPoint(string endpoint)
        {
            string cnxStr = LogsDataware.CnxStrDb();

            string? ID = "";
            string? MOTIVO = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Sistema.SP_Get_Acendes_CAMBIOESTATUS", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = "EP";
                    cmd.Parameters.Add("@EndPoint", System.Data.SqlDbType.NVarChar).Value = endpoint.ToUpper();
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        if (reader.HasRows)
                        {
                            ID = reader.GetString(0);
                            MOTIVO = reader.GetString(1);
                        }
                    }

                }
                await cnx.CloseAsync();
            }
            if (!string.IsNullOrEmpty(ID))
            {
                return (ID, MOTIVO);
            }
            else
            {
                return ("ERROR ID", "ESTATUS NO ENCONTRADO");
            }
        }
        public static (string, string) CambioEstatusByEndPointOBSOLETO(string endpoint)
        {
            string ID = "";
            string MOTIVO = "";
            switch (endpoint)
            {
                case "LiberarPrueba":
                    ID = "estatus_04a_validacion";
                    MOTIVO = "Cita sin concretar";
                    break;
                case "SepararVehiculo":
                    ID = "estatus_06b_separado_vehiculo";
                    MOTIVO = "06b Separado de Vehículo";
                    break;
                case "SeguroAutorizado":
                    ID = "status_06c_seguro_autorizado";
                    MOTIVO = "6c Seguro autorizado";
                    break;
                case "LiberaEnganche":
                    ID = "estatus_04a_validacion";
                    MOTIVO = "4a Validación";
                    break;
                case "CancelacionGestoria":
                    ID = "estatus_04a_validacion";
                    MOTIVO = "4a Validación";
                    break;
                case "Gestoria":
                    ID = "status_13a_agendar_entrega";
                    MOTIVO = "3a Agendar Entrega";
                    break;
                case "Entrega":
                    ID = "estatus_14a_expediente";
                    MOTIVO = "4a Expediente";
                    break;
                case "LiberaVehiculo":
                    ID = "estatus_04a_validacion";
                    MOTIVO = "6c Seguro autorizado";
                    break;
                case "CancelacionContrato":
                    ID = "estatus_04a_validacion";
                    MOTIVO = "4a Validación";
                    break;
                case "CancelacionContratoJOB":
                    ID = "estatus_06b_separado_vehiculo";
                    MOTIVO = "98a Cancelado por tiempo";
                    break;
                case "Expendiente":
                    ID = "status_99_autorizado";
                    MOTIVO = "Autorizado";
                    break;
                case "CotizacionFinal":
                    ID = "estatus_07a_cotizacion_final";
                    MOTIVO = "07a Cotización Final";
                    break;
                default:
                    ID = "";
                    MOTIVO = "";
                    break;
            }
            return (ID, MOTIVO);
        }
        #endregion

        #region EntidadesOriginacion
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

        public class ReqExpediente
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public bool? endosado { get; set; }
            public bool? facturado { get; set; }
            public string? a_nombre_de { get; set; }
            public string? tipo_expediente { get; set; }
        }
        public class RespExpediente
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public bool? endosado { get; set; }
            public bool? facturado { get; set; }
            public string? a_nombre_de { get; set; }
            public string? tipo_expediente { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultExpediente
        {
            public RespExpediente result { get; set; }
        }

        public class ReqLiberaVehiculo
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
        }
        public class RespLiberaVehiculo
        {
            public string? sol_id { get; set; }
            public string? vin { get; set; }
            public string? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? separado_vehiculo { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultLiberaVehiculo
        {
            public RespLiberaVehiculo result { get; set; }
        }

        public class ReqCancelacionContrato
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? activo { get; set; }
            public string? motivo_cancelacion { get; set; }
            public string? desc_cancelacion { get; set; }
        }
        public class RespCancelacionContrato
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public bool? activo { get; set; }
            public string? motivo_cancelacion { get; set; }
            public string? desc_cancelacion { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultCancelacionContrato
        {
            public RespCancelacionContrato result { get; set; }
        }

        public class ReqRenovacionSeguro
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public string? seguro { get; set; }
            public string? aseguradora { get; set; }
            public string? poliza { get; set; }
            public string? fecha_ini_seguro { get; set; }
            public string? fecha_fin_seguro { get; set; }
            public string? dias_vencidos_seguro { get; set; }
            public string? tel_aseguradora { get; set; }
            public bool? seguro_autorizado { get; set; }
            public string? status_seguro { get; set; }
            public decimal? costo { get; set; }
            public string? uso_unidad { get; set; }
            public string? agente_seguro { get; set; }
            public string? fecha_compromiso { get; set; }
        }
        public class RespRenovacionSeguro
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public string? seguro { get; set; }
            public string? aseguradora { get; set; }
            public string? poliza { get; set; }
            public string? fecha_ini_seguro { get; set; }
            public string? fecha_fin_seguro { get; set; }
            public string? dias_vencidos_seguro { get; set; }
            public string? tel_aseguradora { get; set; }
            public bool? seguro_autorizado { get; set; }
            public string? status_seguro { get; set; }
            public decimal? costo { get; set; }
            public string? uso_unidad { get; set; }
            public string? agente_seguro { get; set; }
            public string? fecha_compromiso { get; set; }
            public string? status { get; set; }
            public string? message { get; set; }
        }
        public class ResultRenovacionSeguro
        {
            public RespRenovacionSeguro result { get; set; }
        }
        #endregion

        #region EndPointsOriginacion
        public async static Task<(string, string)> MantenerPrueba(string solId, string motivoMantener, string descMantener, string Vin, string fechaApartado, string idDatamovil, string statusDatamovil)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "MANTENERDRIVETEST");
            var (token, url) = await UsarTokenAcendes();
            ReqMantenerPrueba request = new ReqMantenerPrueba();
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
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                    await HttpClientUtility.PostAsyncAcendes<ResultMantenerPrueba>(url + urlMantenerDriveTest, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "MANTENERDRIVETEST", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {

                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "MANTENERDRIVETEST", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "MANTENERDRIVETEST", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "MANTENERDRIVETEST", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> CargaDocumento(string nombreDoc, string tipoArchivo, string Vin, string solId, string tipoDoc, string documento)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "UPLOADDOCUMENT");
            var (token, url) = await UsarTokenAcendes();
            ReqCargarDocumento request = new ReqCargarDocumento();
            request = new ReqCargarDocumento
            {
                name = nombreDoc,
                type = tipoArchivo,
                vin = Vin,
                sol_id = solId,
                tipo_docto = tipoDoc,
                documento = documento
            };
            object requestSinDoc = new
            {
                name = nombreDoc,
                type = tipoArchivo,
                vin = Vin,
                sol_id = solId,
                tipo_docto = tipoDoc,
                documento = "No puede mostrar el código completo: " + documento.Substring(0, 30)
            };
            var jsonreq = JsonConvert.SerializeObject(requestSinDoc);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCargaDocumento>(url + urlUploadDocument, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "UPLOADDOCUMENT", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {

                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "UPLOADDOCUMENT", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }
                    //return ("ok", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "UPLOADDOCUMENT", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "Error al cargar documento en Acendes");
                }


            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "UPLOADDOCUMENT", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> LiberarPrueba(string solId, string Vin, string idDatamovil, string statusDatamovil)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "DELETEDRIVETEST");
            var (token, url) = await UsarTokenAcendes();
            ReqLiberarPrueba request = new ReqLiberarPrueba();
            request = new ReqLiberarPrueba
            {
                sol_id = solId,
                vin = Vin,
                id_datamovil = idDatamovil,
                status_datamovil = statusDatamovil
            };
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultLiberarPrueba>(url + urlDeleteDriveTest, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result.status == null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "DELETEDRIVETEST", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return ("proceso_correcto", "Error al procesar resultado de Acendes");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "DELETEDRIVETEST", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return (resultPost.result.status, resultPost.result.message);
                        //return ("proceso_correcto", "OK");
                    }

                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "DELETEDRIVETEST", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "DELETEDRIVETEST", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> SepararVehiculo(string solId, string Vin, string idDatamovil, string statusDatamovil, string usoUnidad)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "SEPVEH");
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
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSepararVehiculo>(url + urlSepVeh, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEPVEH", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEPVEH", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEPVEH", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEPVEH", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> SeguroAutorizado(ReqSeguroAutorizado request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "SEGAUTO");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSeguroAutorizado>(url + urlSegAuto, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEGAUTO", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEGAUTO", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }
                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEGAUTO", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SEGAUTO", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> LiberaEnganche(ReqLiberaEnganche request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "LIBERAENGANCHE");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultLiberaEnganche>(url + urlLiberaEnganche, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAENGANCHE", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAENGANCHE", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAENGANCHE", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAENGANCHE", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> SerDMS(ReqSerDMS request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "SERDMS");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultSerDMS>(url + urlSerDMS, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SERDMS", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SERDMS", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SERDMS", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "SERDMS", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> CambioEstatus(ReqCambioEstatus request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "CAMBIOESTATUS");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCambioEstatus>(url + urlCambioEstatus, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CAMBIOESTATUS", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CAMBIOESTATUS", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CAMBIOESTATUS", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CAMBIOESTATUS", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> Gestoria(ReqGestoria request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "GESTORIA");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultGestoria>(url + urlGestoria, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "GESTORIA", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "GESTORIA", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "GESTORIA", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "GESTORIA", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> CancelacionGestoria(ReqCancelacionGestoria request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "CANCELACIONGESTORIA");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCancelacionGestoria>(url + urlCancelacionGestoria, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONGESTORIA", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONGESTORIA", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONGESTORIA", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONGESTORIA", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> Entrega(ReqEntrega request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "ENTREGA");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultEntrega>(url + urlEntrega, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "ENTREGA", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "ENTREGA", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "ENTREGA", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "ENTREGA", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> Expediente(ReqExpediente request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "EXPEDIENTE");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultExpediente>(url + urlExpediente, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "EXPEDIENTE", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "EXPEDIENTE", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "EXPEDIENTE", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "EXPEDIENTE", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> LiberarVehiculo(string solId, string Vin, string idDatamovil, string statusDatamovil)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "LIBERAVEHICULO");
            var (token, url) = await UsarTokenAcendes();
            ReqLiberaVehiculo request = new ReqLiberaVehiculo();
            request = new ReqLiberaVehiculo
            {
                sol_id = solId,
                vin = Vin,
                id_datamovil = idDatamovil,
                status_datamovil = statusDatamovil
            };
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultLiberaVehiculo>(url + urlLiberaVehiculo, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result.status == null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAVEHICULO", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return ("proceso_correcto", "Error al procesar resultado de Acendes");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAVEHICULO", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return (resultPost.result.status, resultPost.result.message);
                        //return ("proceso_correcto", "OK");
                    }

                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAVEHICULO", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "LIBERAVEHICULO", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> CancelacionContrato(ReqCancelacionContrato request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "CANCELACIONCONTRATO");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultCancelacionContrato>(url + urlCancelacionContrato, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONCONTRATO", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONCONTRATO", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONCONTRATO", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "CANCELACIONCONTRATO", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        public async static Task<(string, string)> RenovacionSeguro(ReqRenovacionSeguro request)
        {
            var IdLogInterfaz = await LogsDataware.LogInterfaz(LogsDataware.Acendes, LogsDataware.OKActualizar, "[JOB] Notificar a Acendes", "RENOVACIONSEGURO");
            var (token, url) = await UsarTokenAcendes();
            var jsonreq = JsonConvert.SerializeObject(request);
            try
            {
                var (resultPost, _httpResponse) =
                await HttpClientUtility.PostAsyncAcendes<ResultRenovacionSeguro>(url + urlRenovacionSeguro, request, token);
                if (_httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (resultPost.result != null)
                    {
                        var jsonres = JsonConvert.SerializeObject(resultPost);
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "RENOVACIONSEGURO", "[JOB] Notificación realizada", "Request: " + jsonreq + ", Response: " + jsonres);
                        return (resultPost.result.status, resultPost.result.message);
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(IdLogInterfaz, "RENOVACIONSEGURO", "[JOB] Notificación realizada", "Error al retornar respuesta. Request: " + jsonreq);
                        return ("proceso_correcto", "El proceso falló al retornar respuesta");
                    }

                    //return ("proceso_correcto", "OK");
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(IdLogInterfaz, "RENOVACIONSEGURO", "[JOB] Error al notificar", "Response: " + _httpResponse.ReasonPhrase + ", Request: " + jsonreq);
                    return ("Error", "El proceso falló al notificar a Acendes");
                }
            }
            catch (Exception ex)
            {
                LogsDataware.LogInterfazDetalle(IdLogInterfaz, "RENOVACIONSEGURO", "[JOB] Error al notificar", "Exepción: " + ex.Message + ", Request: " + jsonreq);
                return ("Error", ex.Message);
                throw ex;
            }

        }
        #endregion

        #endregion

    }
}
