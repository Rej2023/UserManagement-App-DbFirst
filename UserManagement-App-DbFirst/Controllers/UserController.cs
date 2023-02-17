using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI;
using UserManagement_App_DbFirst.Authorize;
using UserManagement_App_DbFirst.Models;
using UserManagement_App_DbFirst.Services;
using static UserManagement_App_DbFirst.Controllers.AccountController;

namespace UserManagement_App_DbFirst.Controllers
{
    [LoginAuthorizeAttribute(Roles="Admin")]
    public class UserController : Controller
    {
        UserServices services = new UserServices();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: Display User List
        public ActionResult Index(int? page)
        {

            if (page == null)
                page = 1;

            int PageSize = 10;
            UserViewModel model = new UserViewModel();
            model.Users = services.GetUsers((int)page,PageSize);
            model.TotalRecords = services.GetUserCount();
            
            model.Page = (int)page;
            var pager = new Pager(model.TotalRecords, page, PageSize);

            model.pager = pager;
            ViewBag.Page = page;
            ViewBag.Method = "Register";
            return View(model);
        }

        

        [HttpPost]
        public async Task<ActionResult> Index(UserViewModel _user)
        {

            bool status = CreateValidateModel(_user);
            if (!status)
            {
                _user.Users = services.GetUsers(_user.Page, 10);
                ViewBag.Method = "Register";
                return View(_user);
            }
            var user = UserManager.FindById(User.Identity.GetUserId());
            AccountController account = new AccountController(UserManager, _signInManager);
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.Email = _user.User_Data.Email;
            registerViewModel.Password = "Test@1234";
            registerViewModel.ConfirmPassword = "Test@1234";
            var result = await account.CreateUser(registerViewModel);
            if (result.Status)
            {
                User data = new User()
                {
                    FirstName = _user.User_Data.FirstName,
                    LastName = _user.User_Data.LastName,
                    Contact = _user.User_Data.Contact,
                    Email = _user.User_Data.Email
                };
                _ = services.Insert_data(data, user.Email);
            }
            else
            {
                ModelState.AddModelError("User_Data.Email", "Email Address is already available.");
                _user.Users = services.GetUsers(_user.Page, 10);
                ViewBag.Method = "Register";
                return View(_user);
            }
             return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult EditUser(int Id, int page)
        {
            UserViewModel model = new UserViewModel();
            model.Users = services.GetUsers(page,10);
            model.User_Data = services.GetUserById(Id);
            model.User_Data.OldEmail = model.User_Data.Email;
            model.TotalRecords = services.GetUserCount(); 
            var pager = new Pager(model.TotalRecords, page, 10);
            model.pager = pager;
            model.Page = page;
            ViewBag.Method = "Edit";
            ViewBag.Page = page;
            return View("Index", model);
        }

        [HttpPost]
        public async Task<ActionResult> EditUser(UserViewModel _user)
        {
            bool status = CreateValidateModel(_user);
            if (!status)
            {
                _user.Users = services.GetUsers(_user.Page, 10);
                _user.TotalRecords = services.GetUserCount(); 
                var pager = new Pager(_user.TotalRecords, _user.Page, 10);
                _user.pager = pager;
                ViewBag.Method = "Edit";
                return View("Index",_user);
            }
            AccountController account = new AccountController(UserManager, _signInManager);
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.OldEmail = _user.User_Data.OldEmail;
            registerViewModel.Email = _user.User_Data.Email;
            var result = await account.UpdateUser(registerViewModel);
            if (result.Status)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                User data = new User()
                {
                    UserID = _user.User_Data.UserID,
                    FirstName = _user.User_Data.FirstName,
                    LastName = _user.User_Data.LastName,
                    Contact = _user.User_Data.Contact,
                    Email = _user.User_Data.Email,
                    Updated = DateTime.Now,
                };
                _ = services.UpdateUser(data, user.Email);
                return RedirectToAction("Index", new {page= _user.Page });
            }
            else
            {
                _user.Users = services.GetUsers(_user.Page, 10);
                _user.TotalRecords = services.GetUserCount();
                var pager = new Pager(_user.TotalRecords, ViewBag.Page, 10);
                _user.pager = pager;
                ViewBag.Method = "Edit";
                ModelState.AddModelError("User_Data.Email", "Email address already available.");
                return View("Index",_user);
            }
        }

        [HttpGet]
        public ActionResult UpdateUserStatus(int id, int type, int page)
        {

            if (id > 0)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                User data = new User()
                {
                    UserID = id,
                    Active = type == 1 ? true : false,
                };
                _ = services.UpdateUserStatus(data, user.Email);

                return RedirectToAction("Index", new { page = page });
            }
            ViewBag.Method = "Register";
            return View("Index");
        }
        [HttpGet]
        public async Task<ActionResult> DeleteUser(int Id, int page)
        {
            var loginUser = UserManager.FindById(User.Identity.GetUserId());
            var selectedUser = services.GetUserById(Id);
            AccountController account = new AccountController(UserManager, _signInManager);
            var result = await account.DeleteUser(selectedUser.Email);
            if (result.Status)
            {
                services.Delete(Id, loginUser.Email);
            }
            return RedirectToAction("Index", new {page=page });
        }

        private bool CreateValidateModel(UserViewModel _user)
        {
            bool status = true;
            if (string.IsNullOrEmpty(_user.User_Data.FirstName))
            {
                status = false;
                ModelState.AddModelError("_user.User_Data.FirstName", "Please enter valid first name.");
            }
            else if (string.IsNullOrEmpty(_user.User_Data.LastName))
            {
                status = false;
                ModelState.AddModelError("_user.User_Data.LastName", "Please enter valid last name.");
            }
            else if (string.IsNullOrEmpty(_user.User_Data.Email))
            {
                status = false;
                ModelState.AddModelError("_user.User_Data.Email", "Please enter valid email.");
            }
            else if (!services.IsValidEmail(_user.User_Data.Email))
            {
                status = false;
                ModelState.AddModelError("User_Data.Email", "Email address is invalid.");
            }
            else if (string.IsNullOrEmpty(_user.User_Data.Contact))
            {
                status = false;
                ModelState.AddModelError("_user.User_Data.Contact", "Please enter valid contact number.");
            }
            else if (!IsPhoneNbr(_user.User_Data.Contact))
            {
                status = false;
                ModelState.AddModelError("User_Data.Contact", "Please enter valid contact number.");
            }
            return status;
        }
        public const string motif = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
        public static bool IsPhoneNbr(string number)
        {
            bool status = true;
            if (number != null)
            {
                status = Regex.IsMatch(number, motif);
            }
            else
            {
                return false;
            }
            return status;
        }
        public static void Show(string message, Control owner)
        {
            Page page = (owner as Page) ?? owner.Page;
            if (page == null) return;

            page.ClientScript.RegisterStartupScript(owner.GetType(),
                "ShowMessage", string.Format("<script type='text/javascript'>alert('{0}')</script>",
                message));

        }
    }
}