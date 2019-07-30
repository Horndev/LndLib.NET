using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LightningLib.lndrpc.Exceptions
{
    public class RestException : Exception
    {
        public string Content {get; set;}
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
    }
}
