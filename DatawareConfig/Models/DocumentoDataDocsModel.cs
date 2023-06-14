namespace DatawareConfig.Models
{
    public class DocumentoDataDocsModel
    {
        public string? Nombre { get; set; }
        public int? ParametroId { get; set; }
        public bool? original { get; set; }
        public string? Activo { get; set; }
        public string? TipoDocumentoId { get; set; }
        public string? Folio { get; set; }
        public byte[]? File { get; set; }
    }
}
