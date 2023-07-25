namespace DatawareConfig.Models
{
    public class SubirDatadocsModel
    {
        public string Nombre { get; set; }
        public int ParametroId { get; set; }
        public string ParametroValor { get; set; }
        public bool Original { get; set; }
        public bool Activo { get; set; }
        public string TipoDocumentoId { get; set; }
        public string Folio { get; set; }
        public IFormFile File { get; set; }
        public Guid GeneralId { get; set; }
    }
}
