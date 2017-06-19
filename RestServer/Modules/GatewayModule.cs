using RestServer.Common;
using RestServer.Common.Nancy;
using RestServer.Logic;

namespace RestServer.Modules
{
    public class GatewayModule : BaseModule
    {
        public GatewayModule():base("api/gateway")
        {
            Get["/"] = (_) =>
            {
//                this.RequiresAuthentication();
                var model = ValidateRequest<GetGatewayRequest>();
                var response =  new GetGatewayResponse()
                {
                    GatewayUrl=Program.currentFastestGateway
                };
                return this.Success(response);
            };
        }

    }


    public class GetGatewayRequest
    {
    }
    public class GetGatewayResponse
    {
        public string GatewayUrl { get; set; }
    }

}