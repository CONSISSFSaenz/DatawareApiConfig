using Consiss.DataWare.CrossCutting.Helpers;
using Microsoft.AspNetCore.Mvc;
using DatawareConfig.Servicios;
using DatawareConfig.Helpers;
using DatawareConfig.Models;
using System.Globalization;
using System.Xml.Linq;

namespace DatawareConfig.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatawareController : Controller
    {
        private string _uidUser = "9C0A37F3-937E-429A-AE5A-DF72885002C0"; //Para Pruebas

        public DatawareController()
        {
            
        }

        [HttpGet("ObtenerDetonante")]
        public async Task<IActionResult> ObtenerDetonante()
        {

            var datos = ReglasAutomaticas.ObtenerDetonante();


            object model = new
            {
                Horario = datos.Result.Hora,
                Id = datos.Result.EjecucionAutomaticaId,
                FechaDeAplicacion = datos.Result.FechaAplicacion
            };

            return new OkObjectResult(ResponseHelper.Response(200, model, "Detonante obtenido correctamente"));
        }

        [HttpGet("ObtenerDetonantes")]
        public async Task<IActionResult> ObtenerDetonantes()
        {

            var datos = await ReglasAutomaticas.ObtenerDetonantes();

            return new OkObjectResult(datos);
        }

        [HttpGet("AplicarDetonante")]
        public async Task<IActionResult> AplicarDetonante(string Id)
        {

            var result = await ReglasAutomaticas.AplicarDetonante(Id);

            return new OkObjectResult(ResponseHelper.Response(200, result, "Detonante obtenido correctamente"));
        }

        [HttpGet("ObtenerProcesosExtendidos")]
        public async Task<IActionResult> ObtenerProcesosExtendidos()
        {
            try
            {               
                var datos = await ReglasAutomaticas.ObtenerProcesosExtendidos();
                var ListProcesosACtivos = new List<ProcesosTareasAutomaticasModel>();
                if(datos.Count() > 0)
                    ListProcesosACtivos = datos.Where(x => x.Proceso == 0).ToList();
                else
                    return new ObjectResult(ResponseHelper.Response(403, "DatosNULL", "Sin datos")) { StatusCode = 403 };

                if (ListProcesosACtivos.Count() > 0) //datos != null && datos.Count() > 0
                {
                    foreach (var dts in ListProcesosACtivos)
                    {
                        if(dts.Proceso == 0)
                        {
                            if(dts.NombreTarea == "GestoriaCancelada" || dts.NombreTarea == "ReagendarEntrega")
                            {
                                //I-DM-CANCELACIONGESTORIA-AC
                                //Correo Notificacion
                                await ReglasAutomaticas.UpdProcesoExtendido(dts.PTAId, 2);
                                var cancelarGestoria = await ProcesosExtendidos.CancelarGestoriaAcendes(dts.PTAId,dts.NombreTarea,dts.CadenaIds);

                            }else if(dts.NombreTarea == "Pruebademanejo")
                            {
                                await ReglasAutomaticas.UpdProcesoExtendido(dts.PTAId, 2);
                                var notificaCambioEstatus = await ProcesosExtendidos.NotificaCambioEstatus(dts.PTAId,dts.NombreTarea,dts.CadenaIds);
                            }else if(dts.NombreTarea == "RenovacionVehiculo")
                            {
                                await ReglasAutomaticas.UpdProcesoExtendido(dts.PTAId, 2);
                                var notificaRenovarSeguro = await ProcesosExtendidos.NotificacionRenovarSeguro(dts.PTAId,dts.NombreTarea,dts.CadenaIds);
                            }
                        }
                    }

                    return new OkObjectResult(datos);
                }
                else
                {
                    return new ObjectResult(ResponseHelper.Response(500, "DatosNULL", "Sin datos")) { StatusCode = 403 };
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(ResponseHelper.Response(500, "TryCATCH", ex.Message)) { StatusCode = 500 };
            }

        }

        [HttpGet("EnviarRecordatorioCancelacionGestoria")]
        public async Task<IActionResult> EnviarRecordatorioCancelacionGestoria()
        {
            try
            {
                var datos = await ProcesosExtendidos.EnviarCorreoRecordatorio();
                if (datos != null)
                {
                    return new OkObjectResult(datos);
                }
                else
                {
                    return new ObjectResult(ResponseHelper.Response(500, "DatosNULL", "Sin datos")) { StatusCode = 403 };
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(ResponseHelper.Response(500, "TryCATCH", ex.Message)) { StatusCode = 500 };
            }

        }

        [HttpGet("ResetearSegurosCotizados")]
        public async Task<IActionResult> ResetearSegurosCotizados()
        {

            try
            {
                var (resultado, respuesta) = await ResetearSeguros.RASegurosAValoracion();

                if(respuesta == "OK")
                {
                    LogsDataware.LogSistema(
                        LogsDataware.Dataware,
                        LogsDataware.OKActualizar,
                        "Tarea ejecutada correctamente",
                        "Resetear Seguros Cotizados. Registros reseteados: " + resultado);
                }
                else if(respuesta == "NO")
                {
                    LogsDataware.LogSistema(
                        LogsDataware.Dataware,
                        LogsDataware.OKActualizar,
                        "Tarea ejecutada correctamente",
                        "Resetear Seguros Cotizados. No se encontraron registros");
                }
                else if(respuesta == "KO")
                {
                    LogsDataware.LogSistema(
                        LogsDataware.Dataware,
                        LogsDataware.ERRORActualizar,
                        "Error al ejecutar tarea automatica",
                        "Resetear Seguros Cotizados");
                }

                if (respuesta == "OK" && respuesta == "NO")
                {
                    return new OkObjectResult(ResponseHelper.Response(200, respuesta, "Tarea ejecutada correctamente"));
                }
                else
                {
                    object obj = new { resultadoRegistros = resultado, mensajeRespuesta = respuesta };
                    return new ObjectResult(ResponseHelper.Response(403, obj, "Error al ejecutar tarea automatica")) { StatusCode = 403 };
                }

            }
            catch (Exception ex)
            {
                LogsDataware.LogSistema(
                        LogsDataware.Dataware,
                        LogsDataware.ERRORActualizar,
                        "Error al ejecutar tarea automatica",
                        "Resetear Seguros Cotizados. Error: " + ex.Message);

                return new ObjectResult(ResponseHelper.Response(403, null, "Error al ejecutar tarea automatica")) { StatusCode = 403 };
            }

            
        }
    }
}
