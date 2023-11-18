using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Net;

namespace DatawareConfig.Helpers
{
    public class ReglasAutomaticas
    {
        public static async Task<EjecucionReglaAutomaticaModel> ObtenerDetonante()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "[Sistema].[SP_GetAll_EjecucionAutomatica]";
                var rows = await cnx.QueryFirstOrDefaultAsync<EjecucionReglaAutomaticaModel>(sql);
                await cnx.CloseAsync();
                return rows;
            }
        }

        public static async Task<EjecucionReglaAutomaticaDTOModel> ObtenerDetonantes()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "[Sistema].[SP_GetAll_EjecucionAutomatica]";
                var rows = await cnx.QueryAsync<EjecucionReglaAutomaticaModel>(sql);
                await cnx.CloseAsync();
                EjecucionReglaAutomaticaDTOModel model = new EjecucionReglaAutomaticaDTOModel();
                model.dataRows = rows;
                return model;
            }
        }

        public static async Task<string> AplicarDetonante(object Id)
        {
            try
            {
                string cnxStr = LogsDataware.CnxStrDb();

                /*using (SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    if (cnx.State == ConnectionState.Closed)
                        await cnx.OpenAsync();
                    var sql = "Sistema.SP_Actualizacion_EjecucionAutomatica @EjecucionAutomaticaId='"+Id+"'";
                    var rows = await cnx.QueryFirstOrDefaultAsync<string>(sql);
                    await cnx.CloseAsync();
                    return rows;
                }*/

                string? dts = "";
                using (SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Sistema.SP_Actualizacion_EjecucionAutomatica", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@EjecucionAutomaticaId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(Id.ToString());
                        var dtos = await cmd.ExecuteScalarAsync();

                    }
                    await cnx.CloseAsync();
                }
                if (dts == null)
                {
                    return "NODATA";
                }
                else
                {
                    return "Tareja ejecutada correctamente";
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }


        #region ProcesosExtendidos
        /*
         * PROCESOS EXTENDIDOS
         * -------------------
         *
         * Estado Procesos
         * 0 = Creando Registro
         * 1 = Sin ejecutar
         * 2 = En Ejecución
         * 3 = Terminado
         * 4 = Error
         *
         */

        public static async Task<IEnumerable<ProcesosTareasAutomaticasModel>> ObtenerProcesosExtendidos()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                var sql = "EXEC [Logs].[SP_ProcesosTareasAutomaticas] @Accion='Get'";
                var rows = await cnx.QueryAsync<ProcesosTareasAutomaticasModel>(sql);
                await cnx.CloseAsync();
                return rows;
            }
        }

        public static async Task<long> UpdProcesoExtendido(long PTAId, int Proceso)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            long ret = 0;
            using(SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if(cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using(SqlCommand cmd = new SqlCommand("[Logs].[SP_ProcesosTareasAutomaticas]", cnx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@PTAId", System.Data.SqlDbType.BigInt).Value = PTAId;
                        cmd.Parameters.Add("@Proceso", SqlDbType.Int).Value = Proceso;
                        ret = await cmd.ExecuteNonQueryAsync();
                    }
                await cnx.CloseAsync();
            }
            return ret;
        }

        public static async Task<long> LogPTACancelacionGestoria(string vin, string solId, string contrato, string statusAcendes, object generalId)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            long ret = 0;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("[Logs].[SP_ProcesosTareasAutomaticas_CANCELACIONGESTORIA]", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GeneralId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(generalId.ToString());
                    cmd.Parameters.Add("@VIN", SqlDbType.NVarChar).Value = vin;
                    cmd.Parameters.Add("@sol_id", SqlDbType.NVarChar).Value = solId;
                    cmd.Parameters.Add("@contrato", SqlDbType.NVarChar).Value = contrato;
                    cmd.Parameters.Add("@StatusAcendes", SqlDbType.NVarChar).Value = statusAcendes;
                    ret = await cmd.ExecuteNonQueryAsync();
                }
                await cnx.CloseAsync();
            }
            return ret;
        }
        #endregion
    }
}
