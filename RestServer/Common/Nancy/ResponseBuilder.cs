using System.Collections.Generic;
using Nancy;
using Nancy.Responses.Negotiation;

namespace RestServer.Common
{
    public static class ResponseBuilder
    {
        public static Negotiator Success(this NancyModule module, object response = null, MetaData metadata = null)
        {
            return module.Negotiate.Success(response, metadata);
        }

        public static Negotiator Unauthorized(this NancyModule module)
        {
            return module.Negotiate.Unauthorized();
        }

        public static Negotiator ServerError(this NancyModule module, List<string> errors)
        {
            return module.Negotiate.ServerError(errors);
        }

        public static Negotiator ValidationError(this NancyModule module, List<string> errors)
        {
            return module.Negotiate.ValidationError(errors);
        }

        public static Negotiator Success(this Negotiator negotiator, object response = null, MetaData metadata = null)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.OK);
            return negotiate.WithModel(new AcgResponse(response, metadata ?? new DefaultMetaData()));
        }

        public static Negotiator SuccessfulCreation(this Negotiator negotiator, object response = null)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.Created);
            return response != null ? negotiate.WithModel(new AcgResponse(response, new DefaultMetaData())) : negotiate;
        }

        public static Negotiator Unauthorized(this Negotiator negotiator)
        {
            return
                negotiator.WithStatusCode(HttpStatusCode.Unauthorized)
                    .WithModel(new AcgResponse(null, new ErrorMetaData("Unauthorized")));
        }

        public static Negotiator ServerError(this Negotiator negotiator, IEnumerable<string> errors)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.InternalServerError);
            return errors != null ? negotiate.WithModel(new AcgResponse(null, new ErrorMetaData(errors))) : negotiate;
        }

        public static Negotiator ValidationError(this Negotiator negotiator, IEnumerable<string> errors)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.BadRequest);
            return errors != null ? negotiate.WithModel(new AcgResponse(null, new ErrorMetaData(errors))) : negotiate;
        }

        public static Negotiator ServerError(this Negotiator negotiator, string error)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.InternalServerError);
            return error != null ? negotiate.WithModel(new AcgResponse(null, new ErrorMetaData(error))) : negotiate;
        }

        public static Negotiator ValidationError(this Negotiator negotiator, string error)
        {
            var negotiate = negotiator.WithStatusCode(HttpStatusCode.BadRequest);
            return error != null ? negotiate.WithModel(new AcgResponse(null, new ErrorMetaData(error))) : negotiate;
        }
    }
}