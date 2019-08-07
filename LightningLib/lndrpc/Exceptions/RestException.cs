using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LightningLib.lndrpc.Exceptions
{
    [Serializable]
    public class RestException : Exception
    {
        public RestException()
        {
        }

        public RestException(string message, string content, string statusDescription)
            : base(message)
        {
            Content = content;
            StatusDescription = statusDescription;
        }

        public RestException(string message)
            : base(message)
        {
        }

        public RestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Content {get; set;}
        public string StatusDescription { get; set; }
    }
}
