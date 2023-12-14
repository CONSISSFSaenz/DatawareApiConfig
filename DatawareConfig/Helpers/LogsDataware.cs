using Microsoft.Data.SqlClient;
using System.Data;

namespace DatawareConfig.Helpers
{
    public static class LogsDataware
    {
        /*
         * Las Categorias y los Procesos pueden cambiar, ya que se toma el id desde BD
         * Los Id Modulos son demasiados asi que no se predefiniran aqui por ahora
         * Los Id Modulos se obtienen desde un metodo definido en esta misma clase
         */
        #region Categorias Predefinidas
        public const long OKListar = 1;
        public const long OKInsertar = 2;
        public const long OKActualizar = 3;
        public const long OKActualizarStatus = 4;
        public const long OKEliminar = 5;
        public const long OKExportar = 6;
        public const long ERRORListar = 7;
        public const long ERRORInsertar = 8;
        public const long ERRORActualizar = 9;
        public const long ERRORActualizarStatus = 10;
        public const long ERROREliminar = 11;
        public const long ERRORExportar = 12;
        #endregion

        #region Procesos Predefinidos
        public const long Dataware = 1;
        public const long Intelimotor = 2;
        public const long Datadocs = 3;
        public const long Acendes = 4;
        #endregion

        #region Methods
        public static string CnxStrDb()
        {
            string cnxAmazon = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=900000";
            string cnxAzureDev = "Server=consissqa.database.windows.net;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=administrador;password=C0nsiss2022+;Connection Timeout=900000";
            string cnxAzureQA = "Server=consissqa.database.windows.net;Initial Catalog=DataWare_QA;MultipleActiveResultSets=true;User Id=administrador;password=C0nsiss2022+;Connection Timeout=900000";
            return cnxAzureDev;
        }

        public static async Task<long> LogInterfaz(long ProcesoId, long CategoriaId, string Descripcion, string Contenido)
        {
            string cnxStr = CnxStrDb();
            DateTime FechaRegistro = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));  //DateTime.UtcNow;
            long Id = 0;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_Interfaz", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ProcesoId", System.Data.SqlDbType.BigInt).Value = ProcesoId;
                        cmd.Parameters.Add("@CategoriaId", System.Data.SqlDbType.BigInt).Value = CategoriaId;
                        cmd.Parameters.Add("@Descripcion", System.Data.SqlDbType.NVarChar).Value = Descripcion;
                        cmd.Parameters.Add("@Contenido", System.Data.SqlDbType.NVarChar).Value = Contenido;
                        cmd.Parameters.Add("@FechaRegistro", System.Data.SqlDbType.DateTime).Value = FechaRegistro;

                        Id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                    }
                await cnx.CloseAsync();
            }
            return Id;
        }

        public static async void LogInterfazDetalle(long LogInterfazId, string Accion, string Etiquetas, string Resultado)
        {
            string cnxStr = CnxStrDb();
            DateTime FechaRegistro = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));  //DateTime.UtcNow;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_Interfaz_Detalle", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@LogInterfazId", System.Data.SqlDbType.BigInt).Value = LogInterfazId;
                        cmd.Parameters.Add("@Accion", System.Data.SqlDbType.NVarChar).Value = Accion;
                        cmd.Parameters.Add("@Etiquetas", System.Data.SqlDbType.NVarChar).Value = Etiquetas;
                        cmd.Parameters.Add("@Resultado", System.Data.SqlDbType.NVarChar).Value = Resultado;
                        cmd.Parameters.Add("@FechaRegistro", System.Data.SqlDbType.DateTime).Value = FechaRegistro;

                        await cmd.ExecuteNonQueryAsync();
                    }
                await cnx.CloseAsync();
            }
        }

        public static async void LogSistema(long ProcesoId, long CategoriaId, string Descripcion, string Contenido)
        {
            string cnxStr = CnxStrDb();
            DateTime FechaRegistro = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));  //DateTime.UtcNow;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_Sistema", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ProcesoId", System.Data.SqlDbType.BigInt).Value = ProcesoId;
                        cmd.Parameters.Add("@CategoriaId", System.Data.SqlDbType.BigInt).Value = CategoriaId;
                        cmd.Parameters.Add("@Descripcion", System.Data.SqlDbType.NVarChar).Value = Descripcion;
                        cmd.Parameters.Add("@Contenido", System.Data.SqlDbType.NVarChar).Value = Contenido;
                        cmd.Parameters.Add("@FechaRegistro", System.Data.SqlDbType.DateTime).Value = FechaRegistro;

                        await cmd.ExecuteNonQueryAsync();
                    }
                await cnx.CloseAsync();
            }
        }

        public static async void LogUsuario(object UsuarioId, long ModuloId, long CategoriaId, string Descripcion, string Contenido)
        {
            string cnxStr = CnxStrDb();
            DateTime FechaRegistro = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));  //DateTime.UtcNow;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_Ins_Usuario", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@UsuarioId", System.Data.SqlDbType.UniqueIdentifier).Value = new Guid(UsuarioId.ToString());
                        cmd.Parameters.Add("@ModuloId", System.Data.SqlDbType.BigInt).Value = ModuloId;
                        cmd.Parameters.Add("@CategoriaId", System.Data.SqlDbType.BigInt).Value = CategoriaId;
                        cmd.Parameters.Add("@Descripcion", System.Data.SqlDbType.NVarChar).Value = Descripcion;
                        cmd.Parameters.Add("@Contenido", System.Data.SqlDbType.NVarChar).Value = Contenido;
                        cmd.Parameters.Add("@FechaRegistro", System.Data.SqlDbType.DateTime).Value = FechaRegistro;

                        await cmd.ExecuteNonQueryAsync();
                    }
                await cnx.CloseAsync();
            }
        }

        public static async void UpdExports(string tabla, string ids)
        {
            string cnxStr = CnxStrDb();
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_UpdExportLogs", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@TablaLog", System.Data.SqlDbType.NVarChar).Value = tabla;
                        cmd.Parameters.Add("@Ids", System.Data.SqlDbType.NVarChar).Value = ids;

                        await cmd.ExecuteNonQueryAsync();
                    }
                await cnx.CloseAsync();
            }
        }

        public static async Task<long> GetModuloId(string NombreModulo)
        {
            string cnxStr = CnxStrDb();
            DateTime FechaRegistro = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));  //DateTime.UtcNow;
            long Id = 0;
            using (SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if (cnx.State == ConnectionState.Closed)
                    await cnx.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Logs.SP_GetId_Modulo", cnx))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Nombre", System.Data.SqlDbType.NVarChar).Value = NombreModulo;

                        Id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                    }
                await cnx.CloseAsync();
            }
            return Id;
        }
        #endregion
    }
}
