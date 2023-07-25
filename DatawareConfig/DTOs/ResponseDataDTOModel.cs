using DatawareConfig.Models;

namespace DatawareConfig.DTOs
{
    public class ResponseDataDTOModel
    {
        public ResponseDataIdModel data { get; set; }
        public string error { get; set; }
    }
}
