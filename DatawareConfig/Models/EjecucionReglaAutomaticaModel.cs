namespace DatawareConfig.Models
{
    public class EjecucionReglaAutomaticaModel
    {
        public Guid? EjecucionAutomaticaId { get; set; }
        public int? ReglasNegocioId { get; set; }
        public string? Regla { get; set; }
        public DateTime? FechaAplicacion { get; set; }
        public int? TipoDetonanteId { get; set; }
        public string? TipoDetonante { get; set; }
        public int? PlazoId { get; set; }
        public int? Tiempo { get; set; }
        public string? Momento { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Hora { get; set; }
        public int? Admin_AplicacionId { get; set; }
        public string? Detonamiento { get; set; }
    }
}
