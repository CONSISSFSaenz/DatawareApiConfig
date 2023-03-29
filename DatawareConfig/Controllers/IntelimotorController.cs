using Aspose.Cells;
using Consiss.ConfigDataWare.CrossCutting.Utilities;
using Consiss.DataWare.CrossCutting.Helpers;
using Consiss.DataWare.Functions.Utilities;
using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Entities;
using DatawareConfig.Helpers;
using DatawareConfig.Models;
using DatawareConfig.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace DatawareConfig.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntelimotorController : Controller
    {
        //private readonly DbContext _dbContext;

        private string _urlIntelimotor;
        private string _apiKey;
        private string _apiSecret;

        public IntelimotorController()
        {
            //_dbContext = dbContext;
            _urlIntelimotor = "https://app.intelimotor.com/api/";
            _apiKey = "5bd6f126752e85cb72cbe8083ce81f281753072b73d44e718f8d73541121672a";
            _apiSecret = "cd956b40c566ebac02ea5744f6bea6eaba8428e30a01c3135db57f799c291dcb";
        }

        [HttpGet("SyncIntelimotor")]
        public async Task<IActionResult> SyncIntelimotor(bool isManual = false)
        {
            var syncId = LogSystem.GetGuidDb();
            var identifier = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
            string userid = "admin@admin.com";
            LogSystem.SyncsCatIntelimotor(syncId, identifier, isManual ? "Manual MO":"Autommatico MO", userid); //Log Proceso Manual
            LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Descarga datos Intelimotor", "0", "-");

            var (result, _httpResponseMessageEnt) =
            await HttpClientUtility.GetAsync<TrimsDTO>($"{_urlIntelimotor}trims?apiKey={_apiKey}&apiSecret={_apiSecret}", null);

            try
            {

                string resultadoApi;
                if (_httpResponseMessageEnt.StatusCode == HttpStatusCode.OK)
                {
                    resultadoApi = "Completado con éxito";
                    //SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: MMO-" + identifier, "La descarga de datos se ha completado con éxito");
                    //Notificaciones.Enviar("OK", "Descarga Intelimotor - #Proceso: MMO-" + identifier, "La descarga de datos se ha completado con éxito");
                }
                else
                {
                    SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: MMO-" + identifier, _httpResponseMessageEnt.ReasonPhrase);
                    resultadoApi = _httpResponseMessageEnt.StatusCode.ToString();
                }
                LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Termina descarga datos Intelimotor", result.trims.Count.ToString(), resultadoApi);

                ParametrosInsCatDTOModel prams = new ParametrosInsCatDTOModel
                {
                    TrimsDto = result,
                    syncId = syncId,
                    identifier = identifier,
                    userId = userid
                };
                var registros = await InsertarRegistros.InsCatIntelimotor(prams);
                
                if (registros == 0)
                    return new ObjectResult(ResponseHelper.Response(403, null, Messages.Error)) { StatusCode = 403 };

                SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: MMO-" + identifier, "La sincronización se realizó con éxito");
                return new OkObjectResult(ResponseHelper.Response(200, result.trims[0], Messages.SuccessMsg));

            }
            catch (Exception ex)
            {
                return new OkObjectResult(ResponseHelper.Response(500, null, ex.Message)) { StatusCode = 500 };
                throw;
            }

        }

       
    }

}
