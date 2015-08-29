using System.Collections.Generic;

namespace RestServer.Common
{
    public class UserJwtModel
    {
        public string UserId { get; set; }

        public Dictionary<string, object> ToJwtPayload()
        {
            var payload = new Dictionary<string, object>();
            payload["userId"] = UserId;
            return payload;
        }

        public void HydrateFromJwt(Dictionary<string, object> payload)
        {
            UserId = payload["userId"].ToString();
        }
    }
}