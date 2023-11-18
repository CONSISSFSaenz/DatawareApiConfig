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
    }
}
