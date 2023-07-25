using Dapper;
using DatawareConfig.DTOs;
using DatawareConfig.Helpers;
using DatawareConfig.Utilities;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DatawareConfig.Servicios
{
    public static class InsertarSyncInt
    {
        public static async Task<int> InsSyncIntelimotor(ParametrosSyncIntDTOModel p)
        {
            try
            {
                string cnxStr = LogsDataware.CnxStrDb();
                int resultados;
                int totalRegistros = p.DSI.Count() - 1;
                int filaRegistro = 1;

                #region PropsCustomField
                string? CFColorId = "614b78e4b969910013d205a6";
                string? CFColorName = "Color";
                string? CVColorValue = string.Empty;

                string? CFTenenciaId = "614b78ffec3914001383d217";
                string? CFTenenciaName = "Tenencia";
                string? CVTenenciaValue = string.Empty;

                string? CFCambioPropId = "614b790a5958f0001334da5d";
                string? CFCambioPropName = "Cambio de Prop.";
                string? CVCambioPropValue = string.Empty;

                string? CFSeguroId = "614b78f4ec3914001383d214";
                string? CFSeguroName = "Seguro";
                string? CVSeguroValue = string.Empty;

                string? CFTipoCompraId = "614b78b2ec3914001383cd8b";
                string? CFTipoCompraName = "Tipo de Compra";
                string? CVTipoCompraValue = string.Empty;

                string? CFProveedorId = "614c98dd4527dc0013cfddb8";
                string? CFProveedorName = "Proveedor";
                string? CVProveedorValue = string.Empty;

                string? CFNomPagoId = "614c9c1b4527dc0013d00e56";
                string? CFNomPagoName = "Nom. de Pago";
                string? CVNomPagoValue = string.Empty;

                string? CFTipoPagoId = "614c9928e0a8580013f69da4";
                string? CFTipoPagoName = "Tipo de Pago";
                string? CVTipoPagoValue = string.Empty;

                string? CFNumPagoId = "614c9de44d1ba2001352cda8";
                string? CFNumPagoName = "Numero de Pago";
                string? CVNumPagoValue = string.Empty;

                string? CFTipoFacturaId = "614c9eb6b901f90013f737ad";
                string? CFTipoFacturaName = "Tipo de Factura";
                string? CVTipoFacturaValue = string.Empty;

                string? CFMontoFinanciadoId = "614b7794b969910013d1ef8e";
                string? CFMontoFinanciadoName = "Monto Financiado";
                string? CVMontoFinanciadoValue = string.Empty;

                string? CFUbicacionId = "614cfe36e62db00012f16aff";
                string? CFUbicacionName = "Ubicacion";
                string? CVUbicacionValue = string.Empty;
                #endregion

                #region PropsMarcaModeloYearVersion
                string? MarcaId = "";
                string? MarcaNombre = "";
                string? ModeloId = "";
                string? ModeloNombre = "";
                string? YearId = "";
                string? YearNombre = "";
                string? VersionId = "";
                string? VersionNombre = "";
                #endregion

                #region DtTable
                var dt = new System.Data.DataTable();
                dt.Columns.Add("IntelimotorId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("Ref", typeof(string)).MaxLength = 100;
                dt.Columns.Add("Vin", typeof(string)).MaxLength = 100;
                dt.Columns.Add("Kms", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ClaveTipoAdquisicion", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ConsigmentFeeType", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ConsigmentFee", typeof(string)).MaxLength = 100;
                dt.Columns.Add("BuyPrice", typeof(long));
                dt.Columns.Add("BuyDate", typeof(long));
                dt.Columns.Add("ClaveMarca", typeof(string)).MaxLength = 100;
                dt.Columns.Add("NombreMarca", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ClaveModelo", typeof(string)).MaxLength = 100;
                dt.Columns.Add("NombreModelo", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ClaveYear", typeof(string)).MaxLength = 100;
                dt.Columns.Add("NombreYear", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ClaveVersion", typeof(string)).MaxLength = 100;
                dt.Columns.Add("NombreVersion", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFColorId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFColorName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVColorValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTenenciaId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTenenciaName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVTenenciaValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFCambioPropId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFCambioPropName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVCambioPropValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFSeguroId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFSeguroName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVSeguroValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoCompraId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoCompraName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVTipoCompraValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFProveedorId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFProveedorName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVProveedorValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFNomPagoId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFNomPagoName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVNomPagoValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoPagoId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoPagoName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVTipoPagoValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFNumPagoId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFNumPagoName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVNumPagoValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoFacturaId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFTipoFacturaName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVTipoFacturaValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFMontoFinanciadoId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFMontoFinanciadoName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVMontoFinanciadoValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFUbicacionId", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CFUbicacionName", typeof(string)).MaxLength = 100;
                dt.Columns.Add("CVUbicacionValue", typeof(string)).MaxLength = 100;
                dt.Columns.Add("ListPrice", typeof(long));
                dt.Columns.Add("StatusIntelimotor", typeof(string)).MaxLength = 100;
                dt.Columns.Add("SellChannel", typeof(string)).MaxLength = 100;
                dt.Columns.Add("InterfazId", typeof(long));
                dt.Columns.Add("FilaRegistro", typeof(long));
                dt.Columns.Add("TotalRegistros", typeof(long));
                dt.Columns.Add("SyncsId", typeof(long));
                #endregion

                for (int i = 0; i<= totalRegistros; i++)
                {
                    #region CustomValues
                    for (int j = 0; j < p.DSI[i].customValues.Count(); j++)
                    {
                        #region CFColor
                        if (p.DSI[i].customValues[j].CustomField.Name == CFColorName)
                        {
                            CFColorName = p.DSI[i].customValues[j].CustomField.Name;
                            CVColorValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFTenencia
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFTenenciaName)
                        {
                            CFTenenciaName = p.DSI[i].customValues[j].CustomField.Name;
                            CVTenenciaValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFCambioProp
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFCambioPropName)
                        {
                            CFTenenciaName = p.DSI[i].customValues[j].CustomField.Name;
                            CVTenenciaValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFSeguro
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFSeguroName)
                        {
                            CFSeguroName = p.DSI[i].customValues[j].CustomField.Name;
                            CVSeguroValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFTipoCompra
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFTipoCompraName)
                        {
                            CFTipoCompraName = p.DSI[i].customValues[j].CustomField.Name;
                            CVTipoCompraValue = FuncUtils.Reemplazar(Convert.ToString(p.DSI[i].customValues[j].Value));
                        }
                        #endregion

                        #region CFProveedor
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFProveedorName)
                        {
                            CFProveedorName = p.DSI[i].customValues[j].CustomField.Name;
                            CVProveedorValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFNomPago
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFNomPagoName)
                        {
                            CFNomPagoName = p.DSI[i].customValues[j].CustomField.Name;
                            CVNomPagoValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFTipoPago
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFTipoPagoName)
                        {
                            CFTipoPagoName = p.DSI[i].customValues[j].CustomField.Name;
                            CVTipoPagoValue = FuncUtils.Reemplazar(Convert.ToString(p.DSI[i].customValues[j].Value));
                        }
                        #endregion

                        #region CFNumPago
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFNumPagoName)
                        {
                            CFNumPagoName = p.DSI[i].customValues[j].CustomField.Name;
                            CVNumPagoValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFTipoFactura
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFTipoFacturaName)
                        {
                            CFTipoFacturaName = p.DSI[i].customValues[j].CustomField.Name;
                            CVTipoFacturaValue = FuncUtils.Reemplazar(Convert.ToString(p.DSI[i].customValues[j].Value));
                        }
                        #endregion

                        #region CFMontoFinanciado
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFMontoFinanciadoName)
                        {
                            CFMontoFinanciadoName = p.DSI[i].customValues[j].CustomField.Name;
                            CVMontoFinanciadoValue = Convert.ToString(p.DSI[i].customValues[j].Value);
                        }
                        #endregion

                        #region CFUbicacion
                        else if (p.DSI[i].customValues[j].CustomField.Name == CFUbicacionName)
                        {
                            CFUbicacionName = p.DSI[i].customValues[j].CustomField.Name;
                            CVUbicacionValue = FuncUtils.Reemplazar(Convert.ToString(p.DSI[i].customValues[j].Value));
                        }
                        #endregion

                    }
                    #endregion

                    #region MarcaModeloYearVersion
                    if (p.DSI[i].brands != null)
                    {
                        if (p.DSI[i].brands.Count() > 0)
                        {
                            MarcaId = p.DSI[i].brands[0].id;
                            MarcaNombre = p.DSI[i].brands[0].name;
                        }
                    }
                    if (p.DSI[i].models != null)
                    {
                        if (p.DSI[i].models.Count() > 0)
                        {
                            ModeloId = p.DSI[i].models[0].id;
                            ModeloNombre = p.DSI[i].models[0].name;
                        }
                    }
                    if (p.DSI[i].years != null)
                    {
                        if (p.DSI[i].years.Count() > 0)
                        {
                            YearId = p.DSI[i].years[0].id;
                            YearNombre = p.DSI[i].years[0].name;
                        }
                    }
                    if (p.DSI[i].trims != null)
                    {
                        if (p.DSI[i].trims.Count() > 0)
                        {
                            VersionId = p.DSI[i].trims[0].id;
                            VersionNombre = p.DSI[i].trims[0].name;
                        }
                    }
                    #endregion

                    dt.Rows.Add(
                        p.DSI[i].Id,
                        p.DSI[i].Ref,
                        p.DSI[i].Vin,
                        p.DSI[i].Kms,
                        p.DSI[i].Type,
                        p.DSI[i].ConsignmentFeeType,
                        p.DSI[i].ConsignmentFee,
                        p.DSI[i].BuyPrice,
                        p.DSI[i].BuyDate,
                        MarcaId,
                        MarcaNombre,
                        ModeloId,
                        ModeloNombre,
                        YearId,
                        YearNombre,
                        VersionId,
                        VersionNombre,
                        CFColorId,
                        CFColorName,
                        CVColorValue,
                        CFTenenciaId,
                        CFTenenciaName,
                        CVTenenciaValue,
                        CFCambioPropId,
                        CFCambioPropName,
                        CVCambioPropValue,
                        CFSeguroId,
                        CFSeguroName,
                        CVSeguroValue,
                        CFTipoCompraId,
                        CFTipoCompraName,
                        CVTipoCompraValue,
                        CFProveedorId,
                        CFProveedorName,
                        CVProveedorValue,
                        CFNomPagoId,
                        CFNomPagoName,
                        CVNomPagoValue,
                        CFTipoPagoId,
                        CFTipoPagoName,
                        CVTipoPagoValue,
                        CFNumPagoId,
                        CFNumPagoName,
                        CVNumPagoValue,
                        CFTipoFacturaId,
                        CFTipoFacturaName,
                        CVTipoFacturaValue,
                        CFMontoFinanciadoId,
                        CFMontoFinanciadoName,
                        CVMontoFinanciadoValue,
                        CFUbicacionId,
                        CFUbicacionName,
                        CVUbicacionValue,
                        p.DSI[i].ListPrice,
                        p.DSI[i].Status,
                        p.DSI[i].SellChannel,
                        2,
                        filaRegistro++,
                        totalRegistros,
                        p.identifier
                        );
                }

                using(SqlConnection cnx = new SqlConnection(cnxStr))
                {
                    await cnx.OpenAsync();

                    var parameters = new
                    {
                        projects = dt.AsTableValuedParameter("[Sistema].[SyncInventarioIntelimotor]")
                    };

                    LogSystem.SyncsDetailInvIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", (filaRegistro - 1).ToString(), "Completado con éxito");
                    if (cnx.State == System.Data.ConnectionState.Closed)
                        cnx.Open();

                    resultados = await cnx.ExecuteScalarAsync<int>(
                        "Sistema.SP_Sync_InvIntelimotor",
                        param: parameters, commandTimeout: 1500,
                        commandType: System.Data.CommandType.StoredProcedure
                        );

                    await cnx.CloseAsync();
                }

                return (filaRegistro - 1);

            }catch(Exception e)
            {
                //SendMailHelper.Send("ERROR", "Creación de registros - #Proceso: MMO-" + p.identifier, e.Message);
                LogSystem.SyncsDetailInvIntelimotor(p.syncId, p.identifier, "Almacenar informacion en tablas temporales", "0", e.Message);
                return 0;
            }
        }
    }
}
