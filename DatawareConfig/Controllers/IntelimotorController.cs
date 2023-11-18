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
using Microsoft.Win32;
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
            string ApiUrl = await LogSystem.GetKSB("Url");
            string ApiKey = await LogSystem.GetKSB("ApiKey");
            string ApiSecret = await LogSystem.GetKSB("ApiSecret");
            string BusinessUnit = await LogSystem.GetKSB("Business");
            string procUpd = "";
            string validarProc = await LogSystem.ValidarProcesoActivo("Check",0);
            if (validarProc == "ExisteUnProcesoActivo")
            {
                return new ObjectResult(ResponseHelper.Response(403, null, "Existe un proceso activo")) { StatusCode = 403 };
            }
            else
            {
                procUpd = await LogSystem.ValidarProcesoActivo("Upd", 1);
                #region Descarga TipoAdquisicion
                var syncIdTA = LogSystem.GetGuidDb();
                var identifierTA = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
                string useridTA = "admin@admin.com";

                //Log procesos automaticos
                long LogsInterfazIDTA = 0;
                if (!isManual)
                {
                    LogsInterfazIDTA = await LogsDataware.LogInterfaz(2, LogsDataware.OKActualizar, "Inicia Descarga datos Intelimotor TA", "IdTA: " + syncIdTA);
                }

                LogSystem.SyncsCatIntelimotor(syncIdTA, identifierTA, isManual ? "Manual TA" : "Automático TA", useridTA); //Log Proceso Manual
                LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Descarga datos Intelimotor", "0", "-");

                var (resultTA, _httpResponseMessageEntTA) =
                await HttpClientUtility.GetAsync<TipoAdquisicionDTOModel>($"{_urlIntelimotor}unit-types?apiKey={ApiKey}&apiSecret={ApiSecret}", null);

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
                            LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Descarga", "Descarga completa");
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
                            LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización TA fallida", "Registros: " + registros);
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
                            LogsDataware.LogSistema(2, LogsDataware.OKActualizar, "Sincronización TA Completada", "Registros: " + registros);
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
                        LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización TA fallida", e.Message);
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

                LogSystem.SyncsCatIntelimotor(syncId, identifier, isManual ? "Manual MO" : "Automático MO", userid); //Log Proceso Manual
                LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Descarga datos Intelimotor", "0", "-");

                var (result, _httpResponseMessageEnt) =
                await HttpClientUtility.GetAsync<TrimsDTO>($"{_urlIntelimotor}trims?apiKey={ApiKey}&apiSecret={ApiSecret}", null);

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
                            LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización MO fallida", "Registros: " + registros);
                            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización falló");
                        }
                        procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
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
                            LogsDataware.LogSistema(2, LogsDataware.OKActualizar, "Sincronización MO Completada", "Registros: " + registros);
                            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización se realizó con éxito");
                        }
                        procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
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
                        LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización MO fallida", ex.Message);
                        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", ex.Message);
                    }
                    procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
                    SendMailHelper.Notificaciones(1, "No se completó la sincronización Modelos, " + ex.Message);
                    return new OkObjectResult(ResponseHelper.Response(500, null, ex.Message)) { StatusCode = 500 };
                    throw;
                }
                #endregion
            }

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


        [HttpGet("TestSync")]
        public async Task<IActionResult> TestSync()
        {
            int total = 0;//await SendMailHelper.AltaInventarioIntelimotor(20230512160124);
            return new ObjectResult(ResponseHelper.Response(200, total, "TEST")) { StatusCode = 200 };
        }

        [HttpGet("SyncInvInt")]
        public async Task<IActionResult> SyncInvInt()
        {
            //string UrlIntelimotor = "https://app.intelimotor.com/api/business-units/";
            //$"{UrlIntelimotor}{BusinessUnit}/units?apiKey={ApiKey}&apiSecret={ApiSecret}&pageSize=100"
            //string ApiKey = "a546c5fe24b748374c836b4c016a7c7488e1be09f37695303ea3f80604d7eccf";
            //string ApiSecret = "62585c298dc6504fa86e5a1a89b808bc6360102677adfe594e06c6190af87ac9";
            //string BusinessUnit = "614222e2272bfa0013ac594a";
            string ApiUrl = await LogSystem.GetKSB("Url");
            string ApiKey = await LogSystem.GetKSB("ApiKey");
            string ApiSecret = await LogSystem.GetKSB("ApiSecret");
            string BusinessUnit = await LogSystem.GetKSB("Business");
            string UrlIntelimotor = $"{ApiUrl}inventory-units?apiKey={ApiKey}&apiSecret={ApiSecret}&getAll=true";
            string procUpd = "";
            string validarProc = await LogSystem.ValidarProcesoActivo("CheckInvInt",0);
            if(validarProc == "ExisteUnProcesoActivo")
            {
                return new ObjectResult(ResponseHelper.Response(403, null, "Existe un proceso activo")) { StatusCode = 403 };
            }
            else
            {
                procUpd = await LogSystem.ValidarProcesoActivo("UpdInvInt", 1);
                var syncId = LogSystem.GetGuidDb();
                var identifier = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
                string userid = "admin@admin.com";
                try
                {
                    LogSystem.SyncsInvIntelimotor(syncId, identifier, "Manual", userid); //Log Proceso Manual
                    LogSystem.SyncsDetailInvIntelimotor(syncId, identifier, "Descarga datos Intelimotor", "0", "-");

                    //var (resultSI, _httpResponseMessageEntSI) =
                    //await HttpClientUtility.GetAsync<DataSyncIntDTOModel>(UrlIntelimotor, null);

                    var (resultSI, _httpResponseMessageEntSI) =
                    await HttpClientUtility.GetAsync<List<DataSyncIntModel>>(UrlIntelimotor, null);

                    string resultadoApi;
                    if(_httpResponseMessageEntSI.StatusCode == HttpStatusCode.OK)
                    {
                        resultadoApi = "Completado con éxito";
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncIntelimotor Inventario",
                        "Descarga de datos completa");
                    }
                    else
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncIntelimotor Inventario",
                        "Error al descargar datos");
                        resultadoApi = _httpResponseMessageEntSI.StatusCode.ToString();
                        SendMailHelper.ErrorSyncInventarioIntelimotor(resultadoApi);
                    }
                    LogSystem.SyncsDetailInvIntelimotor(syncId, identifier, "Termina descarga datos Intelimotor", resultSI.Count().ToString(), resultadoApi);

                    ParametrosSyncIntDTOModel prams = new ParametrosSyncIntDTOModel
                    {
                        DSI = resultSI,
                        syncId = syncId,
                        identifier = identifier,
                        userId = userid
                    };

                    var registros = await InsertarSyncInt.InsSyncIntelimotor(prams);

                    if(registros == 0)
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.ERRORActualizar,
                        "SyncIntelimotor Inventario",
                        "La sincronización falló");
                        procUpd = await LogSystem.ValidarProcesoActivo("UpdInvInt",0);
                        SendMailHelper.ErrorSyncInventarioIntelimotor("La sincronización falló, no se obtuvieron resultados.");
                        return new ObjectResult(ResponseHelper.Response(403, null, Messages.Error)) { StatusCode = 403 };
                    }
                    else
                    {
                        LogsDataware.LogUsuario(
                        _uidUser,
                        await LogsDataware.GetModuloId("Configuración y Tablas"),
                        LogsDataware.OKActualizar,
                        "SyncIntelimotor Inventario",
                        "La sincronización se realizó con éxito");
                        
                        int total = await SendMailHelper.AltaInventarioIntelimotor(identifier);
                        int totalErrores = await SendMailHelper.AltaInventarioIntelimotorErrores(identifier);
                        int totalCreados = await ApiHelper.CrearFolderSyncInventario(identifier);
                        procUpd = await LogSystem.ValidarProcesoActivo("UpdInvInt", 0);
                        object dataobj = new
                        {
                            totalRegistros = resultSI.Count(),
                            totalAlta = total,
                            totalConErrores = totalErrores,
                            totalFolderCreados = totalCreados,
                            statusProceso = procUpd
                        };
                        return new OkObjectResult(ResponseHelper.Response(200, dataobj, Messages.SuccessMsg));
                    }
                    
                }
                catch (Exception ex)
                {
                    LogsDataware.LogUsuario(
                    _uidUser,
                    await LogsDataware.GetModuloId("Configuración y Tablas"),
                    LogsDataware.ERRORActualizar,
                    "SyncIntelimotor Inventario",
                    ex.Message);
                    SendMailHelper.ErrorSyncInventarioIntelimotor(ex.Message);
                    return new ObjectResult(ResponseHelper.Response(500, null, ex.Message)) { StatusCode = 500 };
                    throw ex;
                }
            }
        }

        [HttpGet("EliminarIntelimotorIds")]
        public async Task<IActionResult> EliminarIntelimotorIds()
        {
            string ApiUrl = await LogSystem.GetKSB("Url");
            string ApiKey = await LogSystem.GetKSB("ApiKey");
            string ApiSecret = await LogSystem.GetKSB("ApiSecret");
            //string BusinessUnit = await LogSystem.GetKSB("Business");
            string _userId = "9c0a37f3-937e-429a-ae5a-df72885002c0";

            try
            {
                string resultado = "";
                long logInterfazId = 0;
                var(respuesta,ids) = await EliminarRegistros.DelIntelimotorIds(ApiUrl, ApiKey, ApiSecret, _userId);
                if (respuesta == 0)
                {
                    if(ids != "")
                    {
                        resultado = "Error al eliminar registro(s) de intelimotor";
                        logInterfazId = await LogsDataware.LogInterfaz(LogsDataware.Intelimotor, LogsDataware.ERROREliminar, "Proceso automático", resultado);
                        LogsDataware.LogInterfazDetalle(logInterfazId, resultado, "Eliminar intelimotor", "IntelimotorId: " + ids);
                    }
                    
                }
                else
                {
                    resultado = "Registro(s) eliminado(s) de intelimotor correctamente";
                    logInterfazId = await LogsDataware.LogInterfaz(LogsDataware.Intelimotor, LogsDataware.OKEliminar, "Proceso automático", resultado);
                    LogsDataware.LogInterfazDetalle(logInterfazId, resultado, "Eliminar intelimotor", "IntelimotorId: " + ids);
                }

                return new OkObjectResult(ResponseHelper.Response(200, respuesta, resultado));

            }
            catch (Exception ex)
            {
                LogsDataware.LogSistema(LogsDataware.Intelimotor, LogsDataware.ERROREliminar, "Proceso automático", ex.Message);
                return new ObjectResult(ResponseHelper.Response(403, null, "Error al procesar registros")) { StatusCode = 403 };
                throw ex;

            }

            
        }

        [HttpGet("TestCn")]
        public async Task<IActionResult> TestCn()
        {
            return new ObjectResult(LogsDataware.CnxStrDb());
        }

        [HttpGet("SyncIntelimotorTEST")]
        public async Task<IActionResult> SyncIntelimotorTEST(bool isManual = false)
        {
            string ApiUrl = await LogSystem.GetKSB("Url");
            string ApiKey = await LogSystem.GetKSB("ApiKey");
            string ApiSecret = await LogSystem.GetKSB("ApiSecret");
            string BusinessUnit = await LogSystem.GetKSB("Business");
            string procUpd = "";
            string validarProc = await LogSystem.ValidarProcesoActivo("Check", 0);
            if (validarProc == "ExisteUnProcesoActivo")
            {
                return new ObjectResult(ResponseHelper.Response(403, null, "Existe un proceso activo")) { StatusCode = 403 };
            }
            else
            {
                procUpd = await LogSystem.ValidarProcesoActivo("Upd", 1);
                #region Descarga TipoAdquisicion
                var syncIdTA = LogSystem.GetGuidDb();
                var identifierTA = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
                string useridTA = "admin@admin.com";

                //Log procesos automaticos
                long LogsInterfazIDTA = 0;
                if (!isManual)
                {
                    //LogsInterfazIDTA = await LogsDataware.LogInterfaz(2, LogsDataware.OKActualizar, "Inicia Descarga datos Intelimotor TA", "IdTA: " + syncIdTA);
                }

                //LogSystem.SyncsCatIntelimotor(syncIdTA, identifierTA, isManual ? "Manual TA" : "Automático TA", useridTA); //Log Proceso Manual
                //LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Descarga datos Intelimotor", "0", "-");

                var (resultTA, _httpResponseMessageEntTA) =
                await HttpClientUtility.GetAsync<TipoAdquisicionDTOModel>($"{_urlIntelimotor}unit-types?apiKey={ApiKey}&apiSecret={ApiSecret}", null);

                try
                {
                    string resultadoApi;
                    if (_httpResponseMessageEntTA.StatusCode == HttpStatusCode.OK)
                    {
                        //if (isManual)
                        //{
                        //    LogsDataware.LogUsuario(
                        //    _uidUser,
                        //    await LogsDataware.GetModuloId("Configuración y Tablas"),
                        //    LogsDataware.OKActualizar,
                        //    "SyncCatIntelimotor TA",
                        //    "Descarga de datos completa");
                        //}
                        //else
                        //{
                        //    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Descarga", "Descarga completa");
                        //}
                        resultadoApi = "Completado con éxito";
                    }
                    else
                    {
                        //if (isManual)
                        //{
                        //    LogsDataware.LogUsuario(
                        //    _uidUser,
                        //    await LogsDataware.GetModuloId("Configuración y Tablas"),
                        //    LogsDataware.ERRORActualizar,
                        //    "SyncCatIntelimotor TA",
                        //    "Error al descargar datos");
                        //}
                        //else
                        //{
                        //    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Descarga", "ERROR Descarga no completada");
                        //}
                        ////SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, _httpResponseMessageEntTA.ReasonPhrase);
                        //SendMailHelper.Notificaciones(1, "No se completó la descarga Tipo Adquisición, " + _httpResponseMessageEntTA.StatusCode.ToString());
                        resultadoApi = _httpResponseMessageEntTA.StatusCode.ToString();
                    }
                    //LogSystem.SyncsDetailCatIntelimotor(syncIdTA, identifierTA, "Termina descarga datos Intelimotor", resultTA.data.Count.ToString(), resultadoApi);

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
                        //if (isManual)
                        //{
                        //    LogsDataware.LogUsuario(
                        //    _uidUser,
                        //    await LogsDataware.GetModuloId("Configuración y Tablas"),
                        //    LogsDataware.ERRORActualizar,
                        //    "SyncCatIntelimotor TA",
                        //    "La sincronización falló");
                        //}
                        //else
                        //{
                        //    LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización TA fallida", "Registros: " + registros);
                        //    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", "La sincronización falló");
                        //}
                        //SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MTA-" : "ATA-") + identifierTA, "La sincronización falló");
                    }
                    else
                    {
                        //if (isManual)
                        //{
                        //    LogsDataware.LogUsuario(
                        //    _uidUser,
                        //    await LogsDataware.GetModuloId("Configuración y Tablas"),
                        //    LogsDataware.OKActualizar,
                        //    "SyncCatIntelimotor TA",
                        //    "La sincronización se realizó con éxito");
                        //}
                        //else
                        //{
                        //    LogsDataware.LogSistema(2, LogsDataware.OKActualizar, "Sincronización TA Completada", "Registros: " + registros);
                        //    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", "La sincronización se realizó con éxito");
                        //}
                    }
                    return new OkObjectResult(ResponseHelper.Response(200, prams, Messages.SuccessMsg));

                }
                catch (Exception e)
                {
                    //if (isManual)
                    //{
                    //    LogsDataware.LogUsuario(
                    //    _uidUser,
                    //    await LogsDataware.GetModuloId("Configuración y Tablas"),
                    //    LogsDataware.ERRORActualizar,
                    //    "SyncCatIntelimotor TA",
                    //    e.Message);
                    //}
                    //else
                    //{
                    //    LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización TA fallida", e.Message);
                    //    LogsDataware.LogInterfazDetalle(LogsInterfazIDTA, "Sync Tipo Adquisicion", "Actualizar", e.Message);
                    //}
                    //SendMailHelper.Notificaciones(1, "No se completó la sincronización Tipo Adquisición, " + e.Message);

                    throw;
                }
                #endregion

                //#region Descarga Modelos
                //var syncId = LogSystem.GetGuidDb();
                //var identifier = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
                //string userid = "admin@admin.com";

                //long LogsInterfazIDMO = 0;
                //if (!isManual)
                //{
                //    LogsInterfazIDMO = await LogsDataware.LogInterfaz(2, LogsDataware.OKActualizar, "Inicia Descarga datos Intelimotor MO", "IdMO: " + syncId);
                //}

                //LogSystem.SyncsCatIntelimotor(syncId, identifier, isManual ? "Manual MO" : "Automático MO", userid); //Log Proceso Manual
                //LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Descarga datos Intelimotor", "0", "-");

                //var (result, _httpResponseMessageEnt) =
                //await HttpClientUtility.GetAsync<TrimsDTO>($"{_urlIntelimotor}trims?apiKey={ApiKey}&apiSecret={ApiSecret}", null);

                //try
                //{

                //    string resultadoApi;
                //    if (_httpResponseMessageEnt.StatusCode == HttpStatusCode.OK)
                //    {
                //        if (isManual)
                //        {
                //            LogsDataware.LogUsuario(
                //            _uidUser,
                //            await LogsDataware.GetModuloId("Configuración y Tablas"),
                //            LogsDataware.OKActualizar,
                //            "SyncCatIntelimotor MO",
                //            "Descarga de datos completa");
                //        }
                //        else
                //        {
                //            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Descarga", "Descarga completa");
                //        }
                //        resultadoApi = "Completado con éxito";
                //    }
                //    else
                //    {
                //        if (isManual)
                //        {
                //            LogsDataware.LogUsuario(
                //            _uidUser,
                //            await LogsDataware.GetModuloId("Configuración y Tablas"),
                //            LogsDataware.ERRORActualizar,
                //            "SyncCatIntelimotor MO",
                //            "Error al descargar datos");
                //        }
                //        else
                //        {
                //            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Descarga", "ERROR Descarga no completada");
                //        }

                //        //SendMailHelper.Send("ERROR", "Descarga Intelimotor - #Proceso: " + (isManual ? "MMO-" : "AMO-") + identifier, _httpResponseMessageEnt.ReasonPhrase);
                //        SendMailHelper.Notificaciones(1, "No se completó la descarga Modelos, " + _httpResponseMessageEntTA.StatusCode.ToString());
                //        resultadoApi = _httpResponseMessageEnt.StatusCode.ToString();
                //    }
                //    LogSystem.SyncsDetailCatIntelimotor(syncId, identifier, "Termina descarga datos Intelimotor", result.trims.Count.ToString(), resultadoApi);

                //    ParametrosInsCatDTOModel prams = new ParametrosInsCatDTOModel
                //    {
                //        TrimsDto = result,
                //        syncId = syncId,
                //        identifier = identifier,
                //        userId = userid
                //    };
                //    var registros = await InsertarRegistros.InsCatIntelimotor(prams);

                //    if (registros == 0)
                //    {
                //        if (isManual)
                //        {
                //            LogsDataware.LogUsuario(
                //            _uidUser,
                //            await LogsDataware.GetModuloId("Configuración y Tablas"),
                //            LogsDataware.ERRORActualizar,
                //            "SyncCatIntelimotor MO",
                //            "La sincronización falló");
                //        }
                //        else
                //        {
                //            LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización MO fallida", "Registros: " + registros);
                //            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización falló");
                //        }
                //        procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
                //        return new ObjectResult(ResponseHelper.Response(403, null, Messages.Error)) { StatusCode = 403 };
                //    }
                //    else
                //    {
                //        if (isManual)
                //        {
                //            LogsDataware.LogUsuario(
                //            _uidUser,
                //            await LogsDataware.GetModuloId("Configuración y Tablas"),
                //            LogsDataware.OKActualizar,
                //            "SyncCatIntelimotor MO",
                //            "La sincronización se realizó con éxito");
                //        }
                //        else
                //        {
                //            LogsDataware.LogSistema(2, LogsDataware.OKActualizar, "Sincronización MO Completada", "Registros: " + registros);
                //            LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", "La sincronización se realizó con éxito");
                //        }
                //        procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
                //        //SendMailHelper.Send("OK", "Descarga Intelimotor - #Proceso: " + (isManual ? "MMO-" : "AMO-") + identifier, "La sincronización se realizó con éxito");
                //        return new OkObjectResult(ResponseHelper.Response(200, result.trims[0], Messages.SuccessMsg));
                //    }

                //}
                //catch (Exception ex)
                //{
                //    if (isManual)
                //    {
                //        LogsDataware.LogUsuario(
                //        _uidUser,
                //        await LogsDataware.GetModuloId("Configuración y Tablas"),
                //        LogsDataware.ERRORActualizar,
                //        "SyncCatIntelimotor MO",
                //        ex.Message);
                //    }
                //    else
                //    {
                //        LogsDataware.LogSistema(2, LogsDataware.ERRORActualizar, "Sincronización MO fallida", ex.Message);
                //        LogsDataware.LogInterfazDetalle(LogsInterfazIDMO, "Sync Modelos", "Actualizar", ex.Message);
                //    }
                //    procUpd = await LogSystem.ValidarProcesoActivo("Upd", 0);
                //    SendMailHelper.Notificaciones(1, "No se completó la sincronización Modelos, " + ex.Message);
                //    return new OkObjectResult(ResponseHelper.Response(500, null, ex.Message)) { StatusCode = 500 };
                //    throw;
                //}
                //#endregion
            }

        }

    }

}
