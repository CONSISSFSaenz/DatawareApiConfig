using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consiss.DataWare.CrossCutting.Helpers
{
    public static class ResponseHelper
    {
        public static object Response(int statusCode, object data = null, string message = null)
        {
            var response = new
            {
                StatusCode = statusCode,
                data = data,
                Message = message
            };
            return response;
        }
    }
}
