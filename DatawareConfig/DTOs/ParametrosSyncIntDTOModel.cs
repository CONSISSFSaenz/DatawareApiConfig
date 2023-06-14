namespace DatawareConfig.DTOs
{
    public class ParametrosSyncIntDTOModel
    {
        public DataSyncIntDTOModel DSI { get; set; }
        public object syncId { get; set; }
        public long identifier { get; set; }
        public string userId { get; set; }
    }
}
