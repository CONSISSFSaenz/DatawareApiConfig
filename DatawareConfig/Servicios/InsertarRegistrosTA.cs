using DatawareConfig.DTOs;
using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Entities;
using DatawareConfig.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Office.Interop.Excel;
using System.Data;

namespace DatawareConfig.Servicios
{
    public class InsertarRegistrosTA
    {
        public static async Task<int> InsTAIntelimotor(ParametrosInsTADTOModel p)
        {
            try
            {
                string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=900000";
                //Environment.GetEnvironmentVariable("sqldb_connection");
                int resultados;
                int totalRegistros = p.TipoAdDto.data.Count - 1;
                int filaRegistro = 1;

                var dt = new System.Data.DataTable();
                dt.Columns.Add("Clave", typeof(string)).MaxLength = 50;
                dt.Columns.Add("TipoAdquisicion", typeof(string)).MaxLength = 100;
                dt.Columns.Add("UsuarioAlta", typeof(string)).MaxLength = 100;
                dt.Columns.Add("FilaRegistro", typeof(long));
                dt.Columns.Add("TotalRegstros", typeof(long));
                dt.Columns.Add("SyncsId", typeof(long));

                for (int i = 0; i <= totalRegistros; i++)
                {
                    dt.Rows.Add(
                        p.TipoAdDto.data[i].Id,
                        p.TipoAdDto.data[i].Name,
                        p.userId,
                        filaRegistro++,
                        totalRegistros,
                        p.identifier
                        );
                }

                using(SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    await cnx.OpenAsync();

                    var parameters = new
                    {
                        projects = dt.AsTableValuedParameter("[Catalogos].[TipoAdquisicionCat]")
                    };

                    LogSystem.SyncsDetailCatIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", (filaRegistro - 1).ToString(), "Completado con éxito");
                    if (cnx.State == ConnectionState.Closed)
                        cnx.Open();

                    resultados = await cnx.ExecuteScalarAsync<int>("[Catalogos].[SP_Add_TAdquisicionIM]",
                        param: parameters, commandTimeout: 500,
                        commandType: CommandType.StoredProcedure);
                    await cnx.CloseAsync();
                }

                return (filaRegistro - 1);


            }
            catch(Exception e)
            {
                SendMailHelper.Send("ERROR", "Creación de registros - #Proceso: MTA-" + p.identifier, e.Message);
                LogSystem.SyncsDetailCatIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", "0", e.Message);
                return 0;
            }
        }
    }
}
