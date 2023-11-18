namespace DatawareConfig.Models
{
    public class ProcesosTareasAutomaticasModel
    {
        public long PTAId { get; set; }
        public string? NombreTarea { get; set; }
        public DateTime? FechaAlta { get; set; }
        public int? Proceso { get; set; }
        public int? TiempoDif { get; set; }
        public string? CadenaIds { get; set; }
    }
}
