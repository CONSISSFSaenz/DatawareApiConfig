﻿namespace DatawareConfig.Models
{
    public class DocumentoDataDocsModel
    {
        public string Folio { get; set; }
        public string Nombre { get; set; }
        public string TipoDocumentoId { get; set; }
        public int ParametroId { get; set; }
        public string ParametroValor { get; set; }
        public bool RevisionCalidad { get; set; }
        public int TipoIndicadorId { get; set; }
        public string RevisionCalidadComentario { get; set; }
        public bool Original { get; set; }
        public bool Activo { get; set; }
        public IFormFile File { get; set; }
    }
}
