namespace DatawareConfig.Entities
{
    public class ColoniaEntity
    {
        public int? ColoniaId { get; set; }
        public int? MunicipioId { get; set; }
        public string? Colonia { get; set; }
        public int? CodigoPostal { get; set; }
        public bool Status { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
