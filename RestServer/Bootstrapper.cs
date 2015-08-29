using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using JWT;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Json;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public Bootstrapper()
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext requestContext)
        {
            pipelines.OnError.AddItemToEndOfPipeline((context, exception) =>
            {
                if (exception is RequestException)
                {
                    var hyException = (RequestException)exception;
                    Negotiator negotiator;
                    switch (hyException.Type)
                    {
                        case RequestExceptionType.Validation:
                            var hyRequestValidationException = (RequestValidationException)hyException;
                            negotiator = new Negotiator(context);
                            negotiator.ValidationError(hyRequestValidationException.Errors);
                            return container.Resolve<IResponseNegotiator>().NegotiateResponse(negotiator, context);
                        case RequestExceptionType.ServerError:
                            var hyServerErrorException = (ServerErrorException)hyException;
                            negotiator = new Negotiator(context);
                            negotiator.ServerError(hyServerErrorException.Errors);
                            return container.Resolve<IResponseNegotiator>().NegotiateResponse(negotiator, context);
                        case RequestExceptionType.Unauthorized:
                            negotiator = new Negotiator(context);
                            negotiator.Unauthorized();
                            return container.Resolve<IResponseNegotiator>().NegotiateResponse(negotiator, context);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    var negotiator = new Negotiator(context);
                    negotiator.ServerError(exception.Message);
                    return container.Resolve<IResponseNegotiator>().NegotiateResponse(negotiator, context);
                }
            });

            base.RequestStartup(container, pipelines, requestContext);
        }


    }
    public static class ModuleSecurity
    {
        /// <summary>
        /// This module requires authentication
        /// </summary>
        /// <param name="module">Module to enable</param>
        public static void RequiresAuthentication(this NancyModule module)
        {
            module.AddBeforeHookOrExecute(RequiresAuthentication);
        }

        /// <summary>
        /// This module requires authentication and certain claims to be present.
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="requiredClaims">Claim(s) required</param>
        public static void RequiresClaims(this NancyModule module, IEnumerable<string> requiredClaims)
        {
            module.Before.AddItemToEndOfPipeline(RequiresAuthentication);
            module.Before.AddItemToEndOfPipeline(RequiresClaims(requiredClaims));
        }

        /// <summary>
        /// This module requires claims to be validated
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="isValid">Claims validator</param>
        public static void RequiresValidatedClaims(this NancyModule module, Func<IEnumerable<string>, bool> isValid)
        {
            module.Before.AddItemToStartOfPipeline(RequiresValidatedClaims(isValid));
            module.Before.AddItemToStartOfPipeline(RequiresAuthentication);
        }

        /// <summary>
        /// Ensure that the module requires authentication.
        /// </summary>
        /// <param name="context">Current context</param>
        /// <returns>Unauthorized response if not logged in, null otherwise</returns>
        public static Response RequiresAuthentication(NancyContext context)
        {
            try
            {
                var token = context.Request.Headers.Authorization;

                var decodedtoken =JsonWebToken.DecodeToObject(token, ConfigurationManager.AppSettings["jwt:cryptkey"]) as Dictionary<string, object>;

                if (decodedtoken != null)
                {
                    var jwt = new UserJwtModel();
                    jwt.HydrateFromJwt(decodedtoken);
                    context.Items.Add("User", jwt);
                }
            }
            catch (Exception exc)
            {
                Console.Out.WriteLine("Exception! " + exc.Message);
                throw new RequestUnauthorizedException("Invalid Authorization Token");
            }

            return null;
        }

        /// <summary>
        /// Gets a request hook for checking claims
        /// </summary>
        /// <param name="claims">Required claims</param>
        /// <returns>Before hook delegate</returns>
        private static Func<NancyContext, Response> RequiresClaims(IEnumerable<string> claims)
        {
            return (ctx) =>
            {
                Response response = null;
                if (ctx.CurrentUser == null
                    || ctx.CurrentUser.Claims == null
                    || claims.Any(c => !ctx.CurrentUser.Claims.Contains(c)))
                {
                    response = new Response { StatusCode = HttpStatusCode.Forbidden };
                }

                return response;
            };
        }

        /// <summary>
        /// Gets a pipeline item for validating user claims
        /// </summary>
        /// <param name="isValid">Is valid delegate</param>
        /// <returns>Pipeline item delegate</returns>
        private static Func<NancyContext, Response> RequiresValidatedClaims(Func<IEnumerable<string>, bool> isValid)
        {
            return (ctx) =>
            {
                Response response = null;
                var userClaims = ctx.CurrentUser.Claims;
                if (ctx.CurrentUser == null || ctx.CurrentUser.Claims == null || !isValid(ctx.CurrentUser.Claims))
                {
                    response = new Response { StatusCode = HttpStatusCode.Forbidden };
                }

                return response;
            };
        }
    }


    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonNetSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _serializer = JsonSerializer.Create(settings);
        }

        public bool CanSerialize(string contentType)
        {
            return contentType == "application/json";
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(outputStream)))
            {
                _serializer.Serialize(writer, model);
                writer.Flush();
            }
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
    public class JwtToken
    {
        private int issued;
        private int exp;

        public JwtToken()
        {
            issued = (int) Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            exp = (int) Math.Round((DateTime.UtcNow.AddMonths(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        public string Encode(Dictionary<string, object> payload)
        {
            //fixed payload values
            payload.Add("iss", "http://acg.io");
            payload.Add("aud", "http://acg.io");
            payload.Add("iat", issued);
            payload.Add("exp", exp);
            var token = JsonWebToken.Encode(payload, ConfigurationManager.AppSettings["jwt:cryptkey"], JwtHashAlgorithm.HS256);
            return token;
        }
    }

}