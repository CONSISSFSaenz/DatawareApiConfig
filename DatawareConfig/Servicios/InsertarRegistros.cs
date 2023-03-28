using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Entities;
using DatawareConfig.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatawareConfig.Servicios
{
    public class InsertarRegistros
    {
        public static async Task<int> InsCatIntelimotor(ParametrosInsCatDTOModel p)
        {
            try
            {

                string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
                //Environment.GetEnvironmentVariable("sqldb_connection");
                int resultados;
                int totalRegistros = p.TrimsDto.trims.Count - 1;
                int filaRegistro = 1;
                using (SqlConnection cnx = new SqlConnection(cnxStr))
                {

                    cnx.OpenAsync();

                    var modelos = new List<ModelosEntity>();
                    var dt = new DataTable();
                    dt.Columns.Add("ClaveYear", typeof(string)).MaxLength = 50;
                    dt.Columns.Add("NombreYear", typeof(string)).MaxLength = 100;
                    dt.Columns.Add("ClaveMarca", typeof(string)).MaxLength = 50;
                    dt.Columns.Add("NombreMarca", typeof(string)).MaxLength = 100;
                    dt.Columns.Add("ClaveModelo", typeof(string)).MaxLength = 50;
                    dt.Columns.Add("NombreModelo", typeof(string)).MaxLength = 100;
                    dt.Columns.Add("ClaveVersion", typeof(string)).MaxLength = 50;
                    dt.Columns.Add("NombreVersion", typeof(string)).MaxLength = 100;
                    dt.Columns.Add("UsuarioAlta", typeof(string)).MaxLength = 100;
                    dt.Columns.Add("FilaRegistro", typeof(long));
                    dt.Columns.Add("TotalRegstros", typeof(long));
                    dt.Columns.Add("SyncsId", typeof(long));

                    for (int i = 0; i <= totalRegistros; i++)
                    {
                        dt.Rows.Add(
                            p.TrimsDto.trims[i].year.id, p.TrimsDto.trims[i].year.name,
                            p.TrimsDto.trims[i].brand.id, p.TrimsDto.trims[i].brand.name,
                            p.TrimsDto.trims[i].model.id, p.TrimsDto.trims[i].model.name,
                            p.TrimsDto.trims[i].trim.id, p.TrimsDto.trims[i].trim.name,
                            p.userId,
                            filaRegistro++,
                            totalRegistros,
                            p.identifier
                            );
                    }

                    var parameters = new
                    {
                        projects = dt.AsTableValuedParameter("[Catalogos].[ModelosCat]")
                    };

                    LogSystem.SyncsDetailCatIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", (filaRegistro - 1).ToString(), "Completado con éxito");
                    // execute Stored Procedure
                    resultados = await cnx.ExecuteScalarAsync<int>(
                        "[Catalogos].[SP_Add_Modelos]",
                        param: parameters, commandTimeout: 500,
                        commandType: CommandType.StoredProcedure);

                    cnx.CloseAsync();
                }
                Notificaciones.Enviar("OK", "Creación de registros - #Proceso: MMO-" + p.identifier, "La creación de registros en tablas temporales se ha realizado con éxito");
                return (filaRegistro - 1);

            }
            catch (Exception e)
            {
                Notificaciones.Enviar("ERROR", "Creación de registros - #Proceso: MMO-" + p.identifier, e.Message);
                LogSystem.SyncsDetailCatIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", "0", e.Message);
                return 0;
            }

        }
    }
}
