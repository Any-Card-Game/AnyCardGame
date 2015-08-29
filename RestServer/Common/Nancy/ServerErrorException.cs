using System.Collections.Generic;

namespace RestServer.Common
{
    public class ServerErrorException : RequestException
    {
        public IEnumerable<string> Errors { get; set; }

        public ServerErrorException(IEnumerable<string> errors)
            : base(RequestExceptionType.ServerError)
        {
            Errors = errors;
        }

        public ServerErrorException(string error)
            : base(RequestExceptionType.ServerError)
        {
            Errors = new[] { error };
        }
    }
}