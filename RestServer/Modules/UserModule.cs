using System.Collections.Generic;
using Common.Data;
using RestServer.Common;
using RestServer.Common.Nancy;
using RestServer.Logic;

namespace RestServer.Modules
{
    public class UserModule : BaseModule
    {
        public UserModule() : base("api/user")
        {
            Post["/login"] = _ =>
            {
                var model = ValidateRequest<UserLoginRequest>();
                var response = UserLogic.Login(model);
                return this.Success(response, new TokenMetaData(new JwtToken().Encode(new UserJwtModel() {UserId = response.UserId}.ToJwtPayload())));
            };
            Post["/register"] = _ =>
            {
                var model = ValidateRequest<UserRegisterRequest>();
                var response = UserLogic.Register(model);
                return this.Success(response, new TokenMetaData(new JwtToken().Encode(new UserJwtModel() { UserId = response.UserId }.ToJwtPayload())));
            };
        }

    }

      


    public class UserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UserLoginResponse
    {
        public string UserId { get; set; }
    }
    public class UserRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UserRegisterResponse
    {
        public string UserId { get; set; }
    }

}