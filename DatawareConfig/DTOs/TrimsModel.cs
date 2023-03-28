using DatawareConfig.Models;

namespace DatawareConfig.DTOs
{
    public class TrimsModel
    {
        public YearModel year { get; set; }
        public BrandModel brand { get; set; }
        public ModelModel model { get; set; }
        public TrimModel trim { get; set; }
    }
}
