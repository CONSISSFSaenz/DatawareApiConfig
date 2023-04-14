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
        private string _uidUser = "9C0A37F3-937E-429A-AE5A-DF72885002C0"; //Para Pruebas
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

            #region Descarga TipoAdquisicion
            var syncIdTA = LogSystem.GetGuidDb();
            var identifierTA = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
            string useridTA = "admin@admin.com";

            //Log procesos automaticos
            long LogsInterfazIDTA = 0;
            if (!isManual)
            {
                LogsInterfazIDTA = await LogsDataware.LogInterfaz(2,LogsDataware.OKActualizar,"Inicia Descarga datos Intelimotor TA","IdTA: " + syncIdTA);
            }

            LogSystem.SyncsCatIntelimotor(syncIdTA, identifierTA, isManual ? "Manual TA" : "Automático TA", useridTA); //Log Proceso Manual
            LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Descarga datos Intelimotor", "0", "-");

            var (resultTA, _httpResponseMessageEntTA) =
            await HttpClientUtility.GetAsync<TipoAdquisicionDTOModel>($"{_urlIntelimotor}unit-types?apiKey={_apiKey}&apiSecret={_apiSecret}", null);

            try
            {
                string resultadoApi;
                if (_httpResponseMessageEntTA.StatusCode == HttpStatusCode.OK)
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncCatIntelimotor TA",
                        "Descarga de datos completa");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDTA,"Sync Tipo Adquisicion","Descarga","Descarga completa");
                    }
                    resultadoApi = "Completado con éxito";
                }
                else
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncCatIntelimotor TA",
                        "Error al descargar datos");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Descarga", "ERROR Descarga no completada");
                    }
                    //SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, _httpResponseMessageEntTA.ReasonPhrase);
                    SendMailHelper.Notificaciones(1, "No se completó la descarga Tipo Adquisición, " + _httpResponseMessageEntTA.StatusCode.ToString());
                    resultadoApi = _httpResponseMessageEntTA.StatusCode.ToString();
                }
                LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Termina descarga datos Intelimotor", resultTA.data.Count.ToString(), resultadoApi);

                ParametrosInsTADTOModel prams = new ParametrosInsTADTOModel
                {
                    TipoAdDto = resultTA,
                    syncId = syncIdTA,
                    identifier = identifierTA,
                    userId = useridTA,
                };
                var registros = await InsertarRegistrosTA.InsTAIntelimotor(prams);

                if (registros == 0)
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncCatIntelimotor TA",
                        "La sincronización falló");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", "La sincronización falló");
                    }
                    SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, "La sincronización falló");
                }
                else
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncCatIntelimotor TA",
                        "La sincronización se realizó con éxito");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", "La sincronización se realizó con éxito");
                    }
                    //SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, "La sincronización se realizó con éxito");
                }
                
            }
            catch (Exception e)
            {
                //SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, "La sincronización falló. "+e.Message);
                if (isManual)
                {
                    LogsDataware.LogUsuario(
                    _uidUser,
                    await LogsDataware.GetModuloId("Configuración y Tablas"),
                    LogsDataware.ERRORActualizar,
                    "SyncCatIntelimotor TA",
                    e.Message);
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", e.Message);
                }
                SendMailHelper.Notificaciones(1, "No se completó la sincronización Tipo Adquisición, " + e.Message);

                throw;
            }
            #endregion

            #region Descarga Modelos
            var syncId = LogSystem.GetGuidDb();
            var identifier = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
            string userid = "admin@admin.com";

            long LogsInterfazIDMO = 0;
            if (!isManual)
            {
                LogsInterfazIDMO = await LogsDataware.LogInterfaz(2, LogsDataware.OKActualizar, "Inicia Descarga datos Intelimotor MO", "IdMO: " + syncId);
            }

            LogSystem.SyncsCatIntelimotor(syncId, identifier, isManual ? "Manual MO": "Automático MO", userid); //Log Proceso Manual
            LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Descarga datos Intelimotor", "0", "-");

            var (result, _httpResponseMessageEnt) =
            await HttpClientUtility.GetAsync<TrimsDTO>($"{_urlIntelimotor}trims?apiKey={_apiKey}&apiSecret={_apiSecret}", null);

            try
            {

                string resultadoApi;
                if (_httpResponseMessageEnt.StatusCode == HttpStatusCode.OK)
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncCatIntelimotor MO",
                        "Descarga de datos completa");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Descarga", "Descarga completa");
                    }
                    resultadoApi = "Completado con éxito";
                }
                else
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncCatIntelimotor MO",
                        "Error al descargar datos");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Descarga", "ERROR Descarga no completada");
                    }
                    //SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MMO-" : "AMO-") + identifier, _httpResponseMessageEnt.ReasonPhrase);
                    SendMailHelper.Notificaciones(1, "No se completó la descarga Modelos, " + _httpResponseMessageEntTA.StatusCode.ToString());
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
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncCatIntelimotor MO",
                        "La sincronización falló");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización falló");
                    }
                    return new ObjectResult(ResponseHelper.Response(403, null, Messages.Error)) { StatusCode = 403 };
                }
                else
                {
                    if (isManual)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncCatIntelimotor MO",
                        "La sincronización se realizó con éxito");
                    }
                    else
                    {
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización se realizó con éxito");
                    }
                    //SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: " + (isManual ? "MMO-" : "AMO-") + identifier, "La sincronización se realizó con éxito");
                    return new OkObjectResult(ResponseHelper.Response(200, result.trims[0], Messages.SuccessMsg));
                }
                
            }
            catch (Exception ex)
            {
                if (isManual)
                {
                    LogsDataware.LogUsuario(
                    _uidUser,
                    await LogsDataware.GetModuloId("Configuración y Tablas"),
                    LogsDataware.ERRORActualizar,
                    "SyncCatIntelimotor MO",
                    ex.Message);
                }
                else
                {
                    LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", ex.Message);
                }
                SendMailHelper.Notificaciones(1, "No se completó la sincronización Modelos, " + ex.Message);
                return new OkObjectResult(ResponseHelper.Response(500, null, ex.Message)) { StatusCode = 500 };
                throw;
            }
            #endregion

        }

        [HttpGet("TipoAdquisicion")]
        public async Task<IActionResult> SyncTipoAd(bool isManual = false)
        {
            var syncIdTA = LogSystem.GetGuidDb();
            var identifierTA = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
            string useridTA = "admin@admin.com";
            LogSystem.SyncsCatIntelimotor(syncIdTA, identifierTA, isManual ? "Manual TA" : "Automático TA", useridTA); //Log Proceso Manual
            LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Descarga datos Intelimotor", "0", "-");

            var (result, _httpResponseMessageEnt) =
            await HttpClientUtility.GetAsync<TipoAdquisicionDTOModel>($"{_urlIntelimotor}unit-types?apiKey={_apiKey}&apiSecret={_apiSecret}", null);

            try
            {
                string resultadoApi;
                if(_httpResponseMessageEnt.StatusCode == HttpStatusCode.OK)
                {
                    resultadoApi = "Completado con éxito";
                }
                else
                {
                    SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, _httpResponseMessageEnt.ReasonPhrase);
                    resultadoApi = _httpResponseMessageEnt.StatusCode.ToString();
                }
                LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Termina descarga datos Intelimotor", result.data.Count.ToString(), resultadoApi);

                ParametrosInsTADTOModel prams = new ParametrosInsTADTOModel
                {
                    TipoAdDto = result,
                    syncId= syncIdTA,
                    identifier=identifierTA,
                    userId  =useridTA,
                };
                var registros = await InsertarRegistrosTA.InsTAIntelimotor(prams);

                if(registros == 0)
                    return new ObjectResult(ResponseHelper.Response(403, null, Messages.Error)) { StatusCode = 403 };

                SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, "La sincronización se realizó con éxito");
                return new OkObjectResult(ResponseHelper.Response(200, result.data, Messages.SuccessMsg));

            }
            catch(Exception e)
            {
                return new OkObjectResult(ResponseHelper.Response(500, null, e.Message)) { StatusCode = 500 };
                throw;
            }

        }


    }

}
