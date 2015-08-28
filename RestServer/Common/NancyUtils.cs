using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace RestServer.Common
{
    public abstract class BaseModule : NancyModule
    {
        protected BaseModule(string route)
            : base(route)
        {
        }

        protected T ValidateRequest<T>()
        {
            var request = this.Bind<T>(new BindingConfig() { IgnoreErrors = false });
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, new ValidationContext(request, null, null), validationResults);
            if (!isValid)
            {
                throw new Exception(validationResults.Select(a => a.ErrorMessage).Aggregate("", (a, b) => a + b));
            }
            return request;
        }
    }


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


    public enum MetaDataType
    {
        Default,
        Token,
        Error
    }

    public abstract class MetaData
    {
        protected MetaData(MetaDataType type)
        {
            Type = type;
        }

        private MetaDataType Type { get; set; }
    }

    public class DefaultMetaData : MetaData
    {
        public DefaultMetaData()
            : base(MetaDataType.Default)
        {
        }
    }

    public class TokenMetaData : MetaData
    {
        public TokenMetaData(string jwt)
            : base(MetaDataType.Token)
        {
            Jwt = jwt;
        }

        public string Jwt { get; set; }
        public MetaDataType Type { get; set; }
    }

    public class ErrorMetaData : MetaData
    {
        public MetaDataType Type { get; set; }

        public ErrorMetaData(IEnumerable<string> errors)
            : base(MetaDataType.Error)
        {
            Errors = errors;
        }

        public ErrorMetaData(string error)
            : base(MetaDataType.Error)
        {
            Errors = new[] { error };
        }

        public ErrorMetaData()
            : base(MetaDataType.Error)
        {
            Errors = new string[0];
        }

        public IEnumerable<string> Errors { get; set; }
    }


    public class AcgResponse
    {
        public AcgResponse(object data, MetaData meta)
        {
            Data = data;
            Meta = meta;
        }

        public MetaData Meta { get; set; }
        public object Data { get; set; }
    }

}