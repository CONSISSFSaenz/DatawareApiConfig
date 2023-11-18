using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace DatawareConfig.Helpers
{
    public static class LogSystem
    {
        public static string GetGuidDb()
        {
            return System.Guid.NewGuid().ToString("B").ToUpper().Replace("{", "").Replace("}", "");
        }

        #region InventarioAutosCatalogos
        public static async void SyncsCatIntelimotor(object syncId, long identifier, string tipoSync, string usuarioAlta)
        {
            //string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            //Environment.GetEnvironmentVariable("sqldb_connection");
            string cnxStr = LogsDataware.CnxStrDb();

            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                cnx.Open();
                using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_Syncs", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(syncId.ToString());
                    cmd.Parameters.Add("@TipoSincronizacion", System.Data.SqlDbType.VarChar).Value = tipoSync;
                    cmd.Parameters.Add("@UsuarioAlta", System.Data.SqlDbType.VarChar).Value = usuarioAlta;
                    cmd.Parameters.Add("@Identifier", System.Data.SqlDbType.BigInt).Value = identifier;

                    cmd.ExecuteNonQuery();
                }
                cnx.Close();
            }
        }

        public static void SyncsDetailCatIntelimotor(object syncId, long identifier, string accion, string valor, string resultado)
        {
            //string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            string cnxStr = LogsDataware.CnxStrDb();

            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                cnx.Open();
                using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_SyncsDetail", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(syncId.ToString());
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.VarChar).Value = accion;
                    cmd.Parameters.Add("@Valor", System.Data.SqlDbType.VarChar).Value = valor;
                    cmd.Parameters.Add("@Resultado", System.Data.SqlDbType.VarChar).Value = resultado;
                    cmd.Parameters.Add("@Identifier", System.Data.SqlDbType.BigInt).Value = identifier;

                    cmd.ExecuteNonQuery();
                }
                cnx.Close();
            }
        }

        public static async Task<string> ValidarProcesoActivo(string accion, int valor)
        {
            //string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            string cnxStr = LogsDataware.CnxStrDb();

            var result = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Catalogos.SP_ValidarProcSync_InvCatAutos", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Accion", System.Data.SqlDbType.VarChar).Value = accion;
                    cmd.Parameters.Add("@Valor", System.Data.SqlDbType.Int).Value = valor;
                    result = Convert.ToString(await cmd.ExecuteScalarAsync());
                }
                await cnx.CloseAsync();
            }
            return result;
        }
        #endregion

        #region SyncInvIntelimotor
        public static async void SyncsInvIntelimotor(object syncId, long identifier, string tipoSync, string usuarioAlta)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            //Environment.GetEnvironmentVariable("sqldb_connection");
            try
            {
                using (SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    cnx.Open();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_SyncsInvInt", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(syncId.ToString());
                        cmd.Parameters.Add("@TipoSincronizacion", System.Data.SqlDbType.VarChar).Value = tipoSync;
                        cmd.Parameters.Add("@UsuarioAlta", System.Data.SqlDbType.VarChar).Value = usuarioAlta;
                        cmd.Parameters.Add("@Identifier", System.Data.SqlDbType.BigInt).Value = identifier;

                        cmd.ExecuteNonQuery();
                    }
                    cnx.Close();
                }
            }catch(Exception ex)
            {
                throw ex;
            }
            
        }

        public static void SyncsDetailInvIntelimotor(object syncId, long identifier, string accion, string valor, string resultado)
        {
            string cnxStr = LogsDataware.CnxStrDb();
            try
            {
                using (SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    cnx.Open();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_SyncsInvIntDetail", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(syncId.ToString());
                        cmd.Parameters.Add("@Accion", System.Data.SqlDbType.VarChar).Value = accion;
                        cmd.Parameters.Add("@Valor", System.Data.SqlDbType.VarChar).Value = valor;
                        cmd.Parameters.Add("@Resultado", System.Data.SqlDbType.VarChar).Value = resultado;
                        cmd.Parameters.Add("@Identifier", System.Data.SqlDbType.BigInt).Value = identifier;

                        cmd.ExecuteNonQuery();
                    }
                    cnx.Close();
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static async Task<string> GetKSB(string KeySecret)
        {
            string cnxStr = LogsDataware.CnxStrDb();

            string ksb = "";
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Sistema.SP_GetKeySecret", cnx))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Tipo", System.Data.SqlDbType.NVarChar).Value = KeySecret;

                    ksb = Convert.ToString(await cmd.ExecuteScalarAsync());
                }
                await cnx.CloseAsync();
            }
            return ksb;
        }
        #endregion
    }
}
