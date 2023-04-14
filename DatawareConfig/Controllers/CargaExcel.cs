using Consiss.DataWare.CrossCutting.Helpers;
using Consiss.DataWare.Functions.Utilities;
using Dapper;
using DatawareConfig.Entities;
using DatawareConfig.Helpers;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.Data;
using System.Xml.Linq;
using static Dapper.SqlMapper;
using DataTable = System.Data.DataTable;

namespace DatawareConfig.Controllers
{
    public class CargaExcel : Controller
    {
        private string _uidUser = "9C0A37F3-937E-429A-AE5A-DF72885002C0"; //Para Pruebas
        public IActionResult Index()
        {
            return View();
        }        
        [HttpPost("cargaExcel")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            
            var stream = file.OpenReadStream();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            int resultInsert = 0;
            var lista = new List<ColoniaEntity>();
            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });
                DataTable table = result.Tables[0];
                var sheet1 = table.TableName;
                int municipioId;
                int edoId;
                List<ColoniaEntity> lstColonia = new List<ColoniaEntity>();
                long registrosInsertados = 0;
                foreach (DataTable item in result.Tables)
                {
                    if(item.TableName != sheet1)
                    {
                        var municipioIdExcel = item.Select().FirstOrDefault().ItemArray[3];
                        var edoIdExcel = item.Select().FirstOrDefault().ItemArray[4];
                        using (SqlConnection cnx = new SqlConnection(cnxStr))
                        {
                            edoId = await cnx.QueryFirstOrDefaultAsync<int>("SELECT EdoMexicoId FROM Catalogos.EdoMexico WHERE Estado=@edoIdExcel", param: new { edoIdExcel }, commandType: CommandType.Text);
                            municipioId =  await cnx.QueryFirstOrDefaultAsync<int>("SELECT MunicipioId FROM Catalogos.Municipio WHERE Municipio=@municipioIdExcel AND EdoMexicoId=@edoId", param: new { municipioIdExcel,edoId }, commandType: CommandType.Text);
                        }

                        foreach (var r in item.AsEnumerable())
                        {
                            var itemCol = new ColoniaEntity
                            {
                                Colonia = r[1].ToString(),
                                MunicipioId = municipioId,
                                CodigoPostal = Convert.ToInt32(r[0].ToString()),
                                Status = true
                            };
                            lstColonia.Add(itemCol);
                        }

                        lista = lstColonia;
                        using (SqlConnection cnx = new SqlConnection(cnxStr))
                        {
                            foreach (var colonias in lstColonia)
                            {
                                var dynamicParameters = new DynamicParameters();
                                dynamicParameters.Add("@Accion", "Ins");
                                dynamicParameters.Add("@Nombre", colonias.Colonia);
                                dynamicParameters.Add("@MunicipioId", colonias.MunicipioId);
                                dynamicParameters.Add("@CodigoPostal", colonias.CodigoPostal);
                                dynamicParameters.Add("@Estatus", colonias.Status);
                                resultInsert = await cnx.ExecuteScalarAsync<int>("Catalogos.SP_CRUD_Colonia",
                                    param: dynamicParameters,commandTimeout:800,
                                    commandType: CommandType.StoredProcedure);
                                registrosInsertados++;
                            }
                        }

                    }
                    
                }
                

                try
                {
                    if(registrosInsertados > 0)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKInsertar,
                        "CargaExcelColonias",
                        "TotalFilas: " + registrosInsertados);
                        return new OkObjectResult(ResponseHelper.Response(200, registrosInsertados, Messages.SuccessMsgUp));
                    }
                    else
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORInsertar,
                        "CargaExcelColonias",
                        "TotalFilas: 0");
                        return new ObjectResult(ResponseHelper.Response(403, null, Messages.ErrorUpLoad)) { StatusCode = 403 };
                    }

                    /*if (resultInsert > 1)
                        return new ObjectResult(ResponseHelper.Response(403, null, Messages.ErrorUpLoad)) { StatusCode = 403 };
                        
                    return new OkObjectResult(ResponseHelper.Response(200, registrosInsertados, Messages.SuccessMsgUp));*/
                }
                catch (Exception ex)
                {
                    LogsDataware.LogUsuario(
                    _uidUser,
                    await LogsDataware.GetModuloId("Configuración y Tablas"),
                    LogsDataware.ERRORInsertar,
                    "CargaExcelColonias",
                    ex.Message);
                    return new ObjectResult(ResponseHelper.Response(403, null, ex.Message)) { StatusCode = 403 };
                }
                    
            }

            
        }
    }
}
