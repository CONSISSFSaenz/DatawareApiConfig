using Dapper;
using DatawareConfig.Helpers;
using DatawareConfig.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace DatawareConfig.Servicios
{
    public class ProcesosExtendidos
    {
        #region Gestoria
        public class ReqModel
        {
            public string? sol_id { get; set; }
            public string? contrato { get; set; }
            public string? vin { get; set; }
            public int? id_datamovil { get; set; }
            public string? status_datamovil { get; set; }
            public string? motivo_gestoria { get; set; }
            public string? desc_gestoria { get; set; }
            public string? NombreMarca { get; set; }
            public string? NombreModelo { get; set; }
            public string? NombreYear { get; set; }
            public string? NombreVersion { get; set; }
        }

        public class RecordatorioModel
        {
            public Guid GeneralId { get; set; }
            public string? VIN { get; set; }
            public string? Marca { get; set; }
            public string? Modelo { get; set; }
            public string? Year { get; set; }
            public string? Version { get; set; }
            public string? Contrato { get; set; }
            public string? FechaCancelacion { get; set; }

        }

        public static async Task<ReqModel> GetDatosRequest(string generalId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Acendes].[SP_GetAll_ListAutos_CANCELACIONGESTORIA] @GeneralId='" + generalId + "'";
                var rows = await cnx.QueryFirstOrDefaultAsync<ReqModel>(sql);
                await cnx.CloseAsync();
                return rows;
            }
        }

        public static async Task<string> CancelarGestoriaAcendes(long PTAId,string NombreTarea,string CadenaIds)
        {
            if (CadenaIds.Contains(","))
            {
                var rows = CadenaIds.Split(",");
                int totalPorProcesar = rows.Count();
                int totalProcesados = 0;
                string generalId = "";

                foreach(var row in rows)
                {
                    if(row != "0")
                    {
                        generalId = row;
                        var Request = GetDatosRequest(generalId).Result;
                        AcendesHelper.ReqCancelacionGestoria req = new AcendesHelper.ReqCancelacionGestoria
                        {
                            sol_id = Request.sol_id,
                            contrato = Request.contrato,
                            vin = Request.vin,
                            id_datamovil = Request.id_datamovil,
                            status_datamovil = Request.status_datamovil,
                            motivo_gestoria = Request.motivo_gestoria,
                            desc_gestoria = Request.desc_gestoria
                        };
                        var (statusAcendes, mensajeAcendes) = await AcendesHelper.CancelacionGestoria(req);
                        if(statusAcendes == "proceso_correcto")
                        {
                            await ReglasAutomaticas.UpdProcesoExtendido(PTAId, 3);
                            if(NombreTarea == "GestoriaCancelada")
                            {
                                string asuntoMail = "Cancelación de Contrato por Gestoría";
                                string mensajeMail = "<p>La Gestoría para la unidad <b>"
                                + Request.NombreMarca + " " + Request.NombreModelo + " " + Request.NombreYear
                                + " " + Request.NombreVersion + "</b> con el VIN <b>" + Request.vin + "</b> del contrato <b>"
                                + Request.contrato + "</b> ha sido cancelada." + "</p>";
                                ///+ "<p>Favor de subir la Carta de Terminación de Contrato.</p>";
                                SendMailHelper.CorreoGenerico(asuntoMail, mensajeMail);
                            }
                        }
                        else
                        {
                            await ReglasAutomaticas.UpdProcesoExtendido(PTAId, 4);
                        }
                        var logPTA = await ReglasAutomaticas.LogPTACancelacionGestoria(Request.vin, Request.sol_id, Request.contrato, statusAcendes, generalId);
                        totalProcesados++;
                    }
                }

                return "Procesados: " + totalProcesados + "/" + totalPorProcesar;

            }
            else
            {
                return "Procesados: 0";
            }
        }

        public static async Task<IEnumerable<RecordatorioModel>> GetDatosRecordatorio()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Logs].[SP_Recordatorio_Gestoria] @Accion='All'";
                var rows = await cnx.QueryAsync<RecordatorioModel>(sql);
                await cnx.CloseAsync();
                return rows;
            }
        }

        public static async Task<long> UpdRecordatorioEnviado(Guid generalId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            long ret = 0;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("[Logs].[SP_Recordatorio_Gestoria]", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GeneralId", System.Data.SqlDbType.UniqueIdentifier).Value = generalId;
                    ret = await cmd.ExecuteNonQueryAsync();
                }
                await cnx.CloseAsync();
            }
            return ret;
        }

        public static async Task<string> EnviarCorreoRecordatorio()
        {
            var model = await GetDatosRecordatorio();
            if(model != null)
            {
                string asuntoMail = "Recordatorio: Contratos cancelados sin evidencias";
                string tHead = "<table><thead><tr><th>VIN</th><th>Unidad</th><th>Contrato</th><th>Fecha Cancelación</th></tr></thead>";
                string tBody = "<tbody>";
                string tRow = "";
                string Unidad = "";
                foreach (var row in model)
                {
                    Unidad = row.Marca + " " + row.Modelo + " " + row.Year + " " + row.Version;
                    tRow += "<tr><td>" + row.VIN + "</td><td>" + Unidad + "</td><td>" + row.Contrato + "</td><td>" + row.FechaCancelacion + "</td></tr>";
                    await UpdRecordatorioEnviado(row.GeneralId);
                }

                string tableMail = tHead + tBody + tRow + "</tbody></table>";
                SendMailHelper.CorreoGenerico(asuntoMail, tableMail);
                return "OKEMAIL";
            }
            else
            {
                return "NOEMAIL";
            }
        }

        public static async Task<string> NotificaCambioEstatus(long PTAId, string NombreTarea,string CadenaIds)
        {
            if (CadenaIds.Contains(","))
            {
                var rows = CadenaIds.Split(",");
                int totalPorProcesar = rows.Count();
                int totalProcesados = 0;
                string generalId = "";
                string CadenaVINs = "";
                string msjReturn = "";

                foreach (var row in rows)
                {
                    if(row != "0")
                    {
                        generalId = row;
                        var Request = GetDatosRequest(generalId).Result;
                        var (ID_CE, MOTIVO_CE) = await AcendesHelper.CambioEstatusByEndPoint("LiberarPrueba");
                        AcendesHelper.ReqCambioEstatus reqCE = new AcendesHelper.ReqCambioEstatus
                        {
                            sol_id = Request.sol_id,
                            vin = Request.vin,
                            id_datamovil = Convert.ToString(Request.id_datamovil),
                            status_datamovil = Request.status_datamovil,
                            motivo_cambio_estatus = MOTIVO_CE,
                            desc_cambio_estatus = MOTIVO_CE,
                            sol_estatus = ID_CE
                        };
                        var (statusAcendesCE, mensajeAcendesCE) = await AcendesHelper.CambioEstatus(reqCE);
                        if (statusAcendesCE == "proceso_correcto")
                        {
                            await ReglasAutomaticas.UpdProcesoExtendido(PTAId, 3);
                            CadenaVINs = CadenaVINs + "<b style=\"color:#006400 !important;\">" + Request.vin + "</b><br>";
                        }
                        else
                        {
                            await ReglasAutomaticas.UpdProcesoExtendido(PTAId, 4);
                            CadenaVINs = CadenaVINs + "<b style=\"color:#8B0000 !important;\">" + Request.vin + "</b><br>";
                        }
                        var logPTA = await ReglasAutomaticas.LogPTAPruebaManejoSeguro(Request.vin, Request.sol_id, Request.contrato, statusAcendesCE, generalId);
                        totalProcesados++;
                    }
                }

                msjReturn = "Procesados: " + totalProcesados + "/" + totalPorProcesar;

                if (NombreTarea == "Pruebademanejo")
                {
                    string asuntoMail = "Cambio de estatus por Regla Automática";
                    string mensajeMail = "<p>Se ha ejecutado la regla automática de Prueba de Manejo y afectó a los siguientes VIN:</p>"
                    + CadenaVINs
                    + "<p>" + msjReturn + ".<br>Revisar Logs para mas detalle.</p>";
                    SendMailHelper.CorreoGenerico(asuntoMail, mensajeMail);
                }

                return msjReturn;
            }
            else
            {
                return "Procesados: 0";
            }
        }
        #endregion

        #region RenovacionSeguros
        public class ListRenovacionSeguro
        {
            public Guid? GeneralId { get; set; }
            public string? vin { get; set; }
            public string? NombreMarca { get; set; }
            public string? NombreModelo { get; set; }
            public string? NombreYear { get; set; }
            public string? NombreVersion { get; set; }
            public string? Tipo { get; set; }
            public DateTime? FechaVencimiento { get; set; }
            public int? DiasVigencia { get; set; }
        }

        public static async Task<ListRenovacionSeguro> GetDatosRequestRenovacionSeguro(string generalId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Acendes].[SP_GetAll_ListAutos_RenovacionSeguro] @GeneralId='" + generalId + "'";
                var rows = await cnx.QueryFirstOrDefaultAsync<ListRenovacionSeguro>(sql);
                await cnx.CloseAsync();
                return rows;
            }
        }

        public static async Task<string> NotificacionRenovarSeguro(long PTAId, string NombreTarea, string CadenaIds)
        {
            if (CadenaIds.Contains(","))
            {
                var rows = CadenaIds.Split(",");
                int totalPorProcesar = rows.Count();
                int totalProcesados = 0;
                string generalId = "";
                string tblrow = "";
                foreach (var row in rows)
                {
                    if (row != "0")
                    {
                        generalId = row;
                        var Request = GetDatosRequestRenovacionSeguro(generalId).Result;
                        await ReglasAutomaticas.UpdProcesoExtendido(PTAId, 3);
                        if (NombreTarea == "RenovacionVehiculo")
                        {
                            tblrow += "<tr>"
                                + "<td>" + Request.vin + "</td>"
                                + "<td>" + Request.NombreMarca + "</td><td>" + Request.NombreModelo + "</td><td>" + Request.NombreYear + "</td><td>" + Request.NombreVersion + "</td>"
                                + "<td>" + Request.Tipo + "</td><td>" + Request.FechaVencimiento + "</td><td>" + Request.DiasVigencia + "</td>"
                                + "</tr>";
                            
                        }
                        var logPTA = await ReglasAutomaticas.LogPTARenovacionSeguro(Request.vin, Request.NombreMarca, Request.NombreModelo, Request.NombreYear, Request.Tipo, Request.FechaVencimiento, Request.DiasVigencia, generalId, null);
                        totalProcesados++;
                    }
                }

                string asuntoMail = "Renovacion de seguro";
                string mensajeMail = "<p>Los siguientes vehículos requieren una renovación de seguro</p>"
                    + "<table><thead>" 
                    + "<tr><th>VIN</th><th>MARCAR</th><th>MODELO</th><th>AÑO</th><th>VERSION</th><th>TIPO</th><th>FECHA VENCIMIENTO</th><th>DIAS VIGENCIA</th></tr>"
                    + "</thead><tbody>" + tblrow + "</tbody></table>";

                SendMailHelper.CorreoGenerico(asuntoMail, mensajeMail);
                return "Procesados: " + totalProcesados + "/" + totalPorProcesar;

            }
            else
            {
                return "Procesados: 0";
            }
        }

        #endregion

    }
}
