using System.Collections.Generic;

namespace RestServer.Common
{
    public class RequestUnauthorizedException : RequestException
    {
        public IEnumerable<string> Errors { get; set; }

        public RequestUnauthorizedException(IEnumerable<string> errors)
            : base(RequestExceptionType.Unauthorized)
        {
            Errors = errors;
        }

        public RequestUnauthorizedException(string error)
            : base(RequestExceptionType.Unauthorized)
        {
            Errors = new[] { error };
        }
    }
}