using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DatawareConfig.Entities
{
    public class ModelosEntity
    {
        public long ModelosId { get; set; }
        public string ClaveYear { get; set; }
        public string NombreYear { get; set; }
        public string ClaveMarca { get; set; }
        public string NombreMarca { get; set; }
        public string ClaveModelo { get; set; }
        public string NombreModelo { get; set; }
        public string ClaveVersion { get; set; }
        public string NombreVersion { get; set; }
        public string UsuarioAlta { get; set; }
    }
}
