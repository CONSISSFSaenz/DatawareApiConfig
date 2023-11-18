using Dapper;
using DatawareConfig.Helpers;
using Microsoft.Data.SqlClient;

namespace DatawareConfig.Servicios
{
    public class ResetearSeguros
    {
        public static async Task<(int,string)> RASegurosAValoracion()
        {
            string cnxStr = LogsDataware.CnxStrDb();
            int resultados = 0;
            string respuesta = "";

            using(SqlConnection cnx = new SqlConnection(cnxStr))
            {
                if(cnx.State == System.Data.ConnectionState.Closed)
                    await cnx.OpenAsync();

                var sql = "EXEC [Sistema].[SP_Valida_SeguroMensual]";
                var result = await cnx.QueryFirstOrDefaultAsync<int>(sql);

                if(result != null)
                {
                    if(result > 0)
                    {
                        resultados = result;
                        respuesta = "OK";
                    }
                    else
                    {
                        resultados = result;
                        respuesta = "NO";
                    }
                    
                }
                else
                {
                    resultados = 0;
                    respuesta = "KO";
                }

                await cnx.CloseAsync();
            }

            return (resultados, respuesta);

        }
    }
}
