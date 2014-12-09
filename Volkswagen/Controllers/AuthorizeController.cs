using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Volkswagen.Models;
using Volkswagen.Providers;
using Volkswagen.Results;

namespace Volkswagen.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "Admin")]
    public class AuthorizeController : Controller
    {

        public AuthorizeController()
            : this(Startup.UserManagerFactory(), Startup.OAuthOptions.AccessTokenFormat)
        {
        }

        public AuthorizeController(UserManager<IdentityUser> userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public UserManager<IdentityUser> UserManager { get; private set; }
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //
        // GET: /Authorize/
        public ActionResult Index(string UserName)
        {
            if ((UserName == "") || (UserName == null))
            {
                UserName = User.Identity.Name;
            }
            var user = UserManager.FindByName(UserName);

            if (user == null)
            {
                return HttpNotFound();
            }

            var role = UserManager.GetRoles(user.Id);
            var model = new SelectUserRolesViewModel(user.UserName, role);
            return View(model);
        }

        public ActionResult ModifyRole(SelectRoleEditorViewModel srm)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(srm.UserName);
                var roles = UserManager.GetRoles(user.Id);
                foreach (var r in roles)
                {
                    UserManager.RemoveFromRole(user.Id, r);
                }

                UserManager.AddToRole(user.Id, srm.RoleName);

            }
            return RedirectToAction("index");
        }
	}
}