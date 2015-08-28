using System.Collections.Generic;
using System.Web.UI.WebControls;
using Nancy.Responses.Negotiation;
using RestServer.Common;

namespace RestServer
{
    public class IndexModule : BaseModule
    {
        public IndexModule():base("api/index")
        {
            Get["/"] = parameters =>
            { 
                var model = ValidateRequest<AdminLoginRequest>();
                var response = LoginSomethign(model);
                return this.Success(response);
            };
        }

        private object LoginSomethign(AdminLoginRequest model)
        {
            return 12;
        }
    }
    public class AdminLoginRequest
    {
    }
}