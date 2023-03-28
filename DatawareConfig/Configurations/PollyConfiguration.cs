using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consiss.ConfigDataWare.CrossCutting.Configurations
{
    public class PollyConfiguration
    {
        public int MaxTrys { get; set; }
        public int TimeDelay { get; set; }

        public PollyConfiguration()
        {
            try
            {
                //var builder = new ConfigurationBuilder()
                //    .SetBasePath(Directory.GetCurrentDirectory())
                //    .AddJsonFile(ProjectConfiguration.JsonFile);
                //var configuration = builder.Build();

                MaxTrys = 1;//int.Parse(configuration["Polly:MaxTrys"]);               
                TimeDelay = 1; //int.Parse(configuration["Polly:TimeDelay"]);                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
