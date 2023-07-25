using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Consiss.DataWare.CrossCutting.Helpers;
using Consiss.DataWare.Functions.Utilities;
using DatawareConfig.Helpers;
using DatawareConfig.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Xml.Linq;

namespace DatawareConfig.Controllers
{
    public class DataDocsController : Controller
    {
        private string _uidUser = "9C0A37F3-937E-429A-AE5A-DF72885002C0"; //Para Pruebas
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("SubirDocumento")]
        public async Task<IActionResult> SubirDocumento([FromForm]SubirDatadocsModel model)
        {

            DocumentoDataDocsModel paramDatadocs = new DocumentoDataDocsModel
            {
                Folio = model.Folio,
                Nombre = model.Nombre,
                TipoDocumentoId = model.TipoDocumentoId,
                ParametroId = model.ParametroId,
                ParametroValor = model.ParametroValor,
                RevisionCalidad = true,
                TipoIndicadorId = 0,
                RevisionCalidadComentario = "",
                Original = model.Original,
                Activo = model.Activo,
                File = model.File

            };

            try
            {
                string tokenDataDocs = "";

                tokenDataDocs = await ApiHelper.GetTokenDatadocs("Get");
                var (result, _httpResponseMessage) =
                await HttpClientUtility.PostAsyncString<object>(ApiHelper.urlSubirDocumentoDatadocs, paramDatadocs, tokenDataDocs,"multipart/form-data");

                if (_httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var response = new ObjectResult(ResponseHelper.Response(200, result, "OK"));
                    return new OkObjectResult(response);
                }
                else if (_httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    tokenDataDocs = await ApiHelper.GetTokenDatadocs("UpdToken");
                    var (result2, _httpResponseMessage2) =
                    await HttpClientUtility.PostAsyncString<object>(ApiHelper.urlSubirDocumentoDatadocs, paramDatadocs, tokenDataDocs,"multipart/form-data");
                    if(_httpResponseMessage2.StatusCode == HttpStatusCode.OK)
                    {
                        var response = new ObjectResult(ResponseHelper.Response(200, result2, "OK"));
                        return new OkObjectResult(response);

                    }else if(_httpResponseMessage2.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return new ObjectResult(ResponseHelper.Response(403, "Se actualizó el token y aun asi falló", Messages.SaveError)) { StatusCode = 403 };
                    }
                    else
                    {
                        return new ObjectResult(ResponseHelper.Response(403, _httpResponseMessage2.RequestMessage, Messages.SaveError)) { StatusCode = 403 };
                    }

                }
                else
                {
                    return new ObjectResult(ResponseHelper.Response(403, _httpResponseMessage.RequestMessage, Messages.SaveError)) { StatusCode = 403 };
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(ResponseHelper.Response(403, null, ex.Message)) { StatusCode = 403 };
            }

        }

    }
}
