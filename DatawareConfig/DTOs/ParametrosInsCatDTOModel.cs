namespace DatawareConfig.DTOs
{
    public class ParametrosInsCatDTOModel
    {
        public TrimsDTO TrimsDto { get; set; }
        //TrimsDTO l, object syncId, long identifier, string userId
        public object syncId { get; set; }
        public long identifier { get; set; }
        public string userId { get; set; }

    }
}
