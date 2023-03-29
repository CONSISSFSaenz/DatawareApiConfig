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
        public IActionResult Index()
        {
            return View();
        }        
        [HttpPost("cargaExcel")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
            try
            {

                var stream = file.OpenReadStream();
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                {
                    //var result = reader.AsDataSet();
                    //DataSet dataset = reader.AsDataSet();
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
                    List<ColoniaEntity> lstColonia = new List<ColoniaEntity>();

                    foreach (DataTable item in result.Tables)
                    {
                        if(item.TableName != sheet1)
                        {
                            var municipioIdExcel = item.Select().FirstOrDefault().ItemArray[3];
                            using (SqlConnection cnx = new SqlConnection(cnxStr))
                            {
                                municipioId =  await cnx.QueryFirstOrDefaultAsync<int>("SELECT MunicipioId FROM Catalogos.Municipio WHERE Municipio=@municipioIdExcel", param: new { municipioIdExcel }, commandType: CommandType.Text);
                            }

                            foreach (var r in item.AsEnumerable())
                            {
                                var itemCol = new ColoniaEntity
                                {
                                    ColoniaId = Convert.ToInt32(r[0].ToString()),
                                    Colonia = r[1].ToString(),
                                    MunicipioId = municipioId,
                                    CodigoPostal = Convert.ToInt32(r[6].ToString()),
                                    Status = true
                                };
                                lstColonia.Add(itemCol);
                            }
                        }                                           
                    }
                    var lista = lstColonia;

                    try
                    {
                        int resultInsert = 0;
                        using (SqlConnection cnx = new SqlConnection(cnxStr))
                        {
                            foreach (var item in lstColonia)
                            {
                                var dynamicParameters = new DynamicParameters();
                                dynamicParameters.Add("@Accion", "Add");
                                dynamicParameters.Add("@Nombre", item.Colonia);
                                dynamicParameters.Add("@MunicipioId", item.MunicipioId);
                                dynamicParameters.Add("@CodigoPostal", item.CodigoPostal);
                                dynamicParameters.Add("@Estatus", item.Status);
                                resultInsert = await cnx.ExecuteScalarAsync<int>("Catalogos.SP_CRUD_Colonia",
                                    param: dynamicParameters,
                                    commandType: CommandType.StoredProcedure);

                            }                            
                        }
                        if (resultInsert > 1)
                            return new ObjectResult(ResponseHelper.Response(403, null, Messages.ErrorUpLoad)) { StatusCode = 403 };
                        
                        return new OkObjectResult(ResponseHelper.Response(200, null, Messages.SuccessMsgUp));
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    //string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
                    //using (SqlConnection cnx = new SqlConnection(cnxStr))
                    //{

                    //    //DataTable table = result.Tables[0];
                    //    //var sheet1 = table.TableName;

                    //    /*foreach (DataTable table in result.Tables)
                    //    {
                    //        await cnx.ExecuteAsync("INSERT INTO dbo.estado$ (IdEstado, NombreEstado, IdPais) VALUES (@Column1, @Column2, @Column3)", table.AsEnumerable().Select(r => new
                    //        {
                    //            Column1 = r[0].ToString(),
                    //            Column2 = r[1].ToString(),
                    //            Column3 = r[2].ToString()
                    //        }));
                    //    }*/



                    //}

                }

            }
            catch (Exception ex)
            {
                return new OkObjectResult(ex);
                throw;
            }



            return new OkResult();
        }
    }
}
