using Microsoft.Data.SqlClient;

namespace DatawareConfig.Helpers
{
    public static class LogSystem
    {
        public static string GetGuidDb()
        {
            return System.Guid.NewGuid().ToString("B").ToUpper().Replace("{", "").Replace("}", "");
        }

        public static async void SyncsCatIntelimotor(object syncId, long identifier, string tipoSync, string usuarioAlta)
        {
            string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            //Environment.GetEnvironmentVariable("sqldb_connection");

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
            string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";

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
    }
}
