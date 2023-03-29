using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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

                    string cnxStr = "Server=mssql-prod.c5zxdmjllybo.us-east-1.rds.amazonaws.com;Initial Catalog=DataWare_Dev;MultipleActiveResultSets=true;User Id=admin;password=*Consiss$2021;Connection Timeout=12000";
                    using (SqlConnection cnx = new SqlConnection(cnxStr))
                    {

                        DataTable table = result.Tables[0];
                        var sheet1 = table.TableName;

                        /*foreach (DataTable table in result.Tables)
                        {
                            await cnx.ExecuteAsync("INSERT INTO dbo.estado$ (IdEstado, NombreEstado, IdPais) VALUES (@Column1, @Column2, @Column3)", table.AsEnumerable().Select(r => new
                            {
                                Column1 = r[0].ToString(),
                                Column2 = r[1].ToString(),
                                Column3 = r[2].ToString()
                            }));
                        }*/



                    }

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
