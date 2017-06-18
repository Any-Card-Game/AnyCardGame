using System.Threading.Tasks;
using Common.Data;
using Common.Utils.Mongo;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class UserLogic
    {
        public static UserLoginResponse Login(UserLoginRequest model)
        {
            var user = MongoUser.Collection.GetOne(QueryField.FromExpression<MongoUser.User>(a => a.Email, model.Email), QueryField.FromExpression<MongoUser.User>(a => a.Password, model.Password));

            if (user == null)
            {
                throw new RequestValidationException("User not found.");
            }

            return new UserLoginResponse()
            {
                UserId=user.Id.ToString()
            };
        }
        public static UserRegisterResponse Register(UserRegisterRequest model)
        {
            var user = MongoUser.Collection.GetOne(QueryField.FromExpression<MongoUser.User>(a => a.Email, model.Email));

            if (user != null)
            {
                throw new RequestValidationException("Email Address In Use");
            }

            user = new MongoUser.User();
            user.Email = model.Email;
            user.Password = model.Password;
            user.Insert();

            return new UserRegisterResponse()
            {
                UserId=user.Id.ToString()
            };
        }
    }
}