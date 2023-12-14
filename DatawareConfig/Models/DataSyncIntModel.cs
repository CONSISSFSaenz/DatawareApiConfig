using DatawareConfig.DTOs;

namespace DatawareConfig.Models
{
    public class DataSyncIntModel
    {
        public string? Id { get; set; } // IntelimotorId
        public string? Ref { get; set; } // ExternoId
        public string? Vin { get; set; } // Vin
        public long? Kms { get; set; } // Kms
        public string? Type { get; set; } // ClaveTipoAdquisicion
        public string? ConsignmentFeeType { get; set; } // TipoComision
        public string? ConsignmentFee { get; set; } // Comision
        public long? BuyPrice { get; set; } // Costos
        public long? BuyDate { get; set; } // FechaAdquisicion

        public List<BrandModel>? brands { get; set; }
        public List<ModelModel>? models { get; set; }
        public List<YearModel>? years { get; set; }
        public List<TrimModel>? trims { get; set; }
        public bool? useCustomTrim { get; set; }
        public string? customTrim { get; set; }
        public List<CustomValuesSyncIntDTOModel> customValues { get; set; }
        public long? ListPrice { get; set; } // PrecioLista
        public string? Status { get; set; } // StatusIntelimotor (Activo,Inactivo)
        public string? SellChannel { get; set; }
    }
}
