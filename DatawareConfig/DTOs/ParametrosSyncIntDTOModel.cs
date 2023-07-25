using DatawareConfig.Models;

namespace DatawareConfig.DTOs
{
    public class ParametrosSyncIntDTOModel
    {
        public List<DataSyncIntModel> DSI { get; set; }
        public object syncId { get; set; }
        public long identifier { get; set; }
        public string userId { get; set; }
    }
}
