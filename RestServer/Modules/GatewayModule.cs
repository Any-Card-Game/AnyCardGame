using RestServer.Common;
using RestServer.Logic;

namespace RestServer.Modules
{
    public class GatewayModule : BaseModule
    {
        public GatewayModule():base("api/gateway")
        {
            Get["/",true] = async (_, ct) =>
            { 
                var model = ValidateRequest<GetGatewayRequest>();
                var response = await GatewayLogic.GetFastestGateway(model);
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