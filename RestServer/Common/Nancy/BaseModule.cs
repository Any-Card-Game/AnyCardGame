using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using RestServer.Modules;

namespace RestServer.Common.Nancy
{
    public abstract class BaseModule : NancyModule
    {
        public UserJwtModel JwtModel => (UserJwtModel)Context.Items["User"];

        protected BaseModule(string route)
            : base(route)
        {
        }

        protected T ValidateRequest<T>()
        {
            var request = this.Bind<T>(new BindingConfig() { IgnoreErrors = false });
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, new ValidationContext(request), validationResults);
            if (!isValid)
            {
                throw new Exception(validationResults.Select(a => a.ErrorMessage).Aggregate("", (a, b) => a + b));
            }
            return request;
        }
    }
}