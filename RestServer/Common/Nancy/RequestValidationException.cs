using System.Collections.Generic;

namespace RestServer.Common
{
    public class RequestValidationException : RequestException
    {
        public IEnumerable<string> Errors { get; set; }

        public RequestValidationException(IEnumerable<string> errors)
            : base(RequestExceptionType.Validation)
        {
            Errors = errors;
        }

        public RequestValidationException(string error)
            : base(RequestExceptionType.Validation)
        {
            Errors = new[] { error };
        }
    }
}