
using System.Web;
using System.Web.Security;

namespace BlogOdev.Providers
{
    public class AuthProvider : IAuthProvider
    {
        /// <summary>
        /// Return True if the user is already logged-in.
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        public System.Security.Principal.IIdentity GetUser()
        {
            return HttpContext.Current.User.Identity;
        }

        /// <summary>
        /// Authenticate an user and set cookie if user is valid.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            var result = FormsAuthentication.Authenticate(username, password); // TODO: User Membership APIs

            if (result)
                FormsAuthentication.SetAuthCookie(username, false);

            return result;

            //var users = ConfigurationManager.AppSettings["AllowedUsers"];
            //var splittedUsers = users.Split(';').Where(c => c.Contains(username));

            //foreach (var splittedUser in splittedUsers)
            //{
            //    var _ss = splittedUser.Split(',');
            //    var un = _ss[0];
            //    var pass = _ss[1];

            //    if (un == username && pass == password)
            //    {
            //        FormsAuthentication.SetAuthCookie(username, false);

            //        return true;
            //    }
            //}

            ////var result = FormsAuthentication.Authenticate(username, password); // TODO: User Membership APIs

            //return false;
        }

        /// <summary>
        /// Logout the user.
        /// </summary>
        public void Logout()
        {
            FormsAuthentication.SignOut();
        }
    }
}