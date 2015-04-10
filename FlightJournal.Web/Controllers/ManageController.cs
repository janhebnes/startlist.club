using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Globalization;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FlightJournal.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

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

        //
        // GET: /Account/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two factor provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "The phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.BindToPilotSuccess ? "Pilot has been bound to account."
                : "";

            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(User.Identity.GetUserId()),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(User.Identity.GetUserId()),
                Logins = await UserManager.GetLoginsAsync(User.Identity.GetUserId()),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(User.Identity.GetUserId()),
                HasPilotBinding = HasPilotBinding()
            };
            return View(model);
        }
        
        //
        // GET: /Account/RemoveLogin
        public ActionResult RemoveLogin()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return View(linkedAccounts);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // POST: /Manage/RemovePilotBinding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePilotBinding(string currentPilotIdBinding)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                using (var context = new FlightContext())
                {
                    if (user.BoundToPilotId != null)
                    {
                        var userPilotBinding = context.Pilots.Find(Convert.ToInt32(user.BoundToPilotId));
                        if (userPilotBinding.PilotId.ToString(CultureInfo.InvariantCulture) == currentPilotIdBinding)
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var dbUser = db.Users.Find(user.Id);
                                if (dbUser != null)
                                {
                                    dbUser.BoundToPilotId = null;
                                    db.SaveChanges();
                                    message = ManageMessageId.RemovePilotBindingSuccess;        
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("ManagePilotBinding", new { Message = message });
        }

        //
        // GET: /Account/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // GET: /Manage/RememberBrowser
        public ActionResult RememberBrowser()
        {
            var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, rememberBrowserIdentity);
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/ForgetBrowser
        public ActionResult ForgetBrowser()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/EnableTFA
        public async Task<ActionResult> EnableTFA()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/DisableTFA
        public async Task<ActionResult> DisableTFA()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Account/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Number))
            {
                model.Number = model.Number.Replace(" ", "");

                if (model.Number.Length == 8)
                {
                    model.Number = "+45" + model.Number;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Send result of: UserManager.GetPhoneNumberCodeAsync(User.Identity.GetUserId(), phoneNumber);
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // GET: /Account/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            // This code allows you exercise the flow without actually sending codes
            // For production use please register a SMS provider in IdentityConfig and generate a code here.
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            if (HttpContext.IsDebuggingEnabled)
            {
                ViewBag.Status = "For DEMO purposes only, the current code is " + code ;
            }
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Account/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Account/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new {Message = ManageMessageId.Error});
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Manage
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        private static string ManageMessageText(ManageMessageId? message)
        {
            return message == ManageMessageId.RemovePilotBindingSuccess ? "Pilot relationen er nu fjernet."
                : message == ManageMessageId.BindToPilotSuccess ? string.Format("Pilot relationen oprettet! Du kan nu oprette og redigere flyvninger samt se din personlige logbog.")
                    : message == ManageMessageId.Error ? "Der opstod en fejl."
                        : "";
        }
        //
        // GET: /Account/Manage
        public PartialViewResult ManagePilotBindingPartial(ManageMessageId? message)
        {
            ViewBag.StatusMessage = ManageMessageText(message);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return PartialView("Error");
            }

            var result = ManagePilotBindingViewModel(user);

            return PartialView("ManagePilotBinding",result);
        }

        public ViewResult ManagePilotBinding(ManageMessageId? message)
        {
            ViewBag.StatusMessage = ManageMessageText(message);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }

            var result = ManagePilotBindingViewModel(user);

            return View(result);
        }

        private static ManagePilotBindingViewModel ManagePilotBindingViewModel(ApplicationUser user)
        {
            Pilot userPilotBinding = null;
            List<Pilot> otherPilots = new List<Pilot>();
            using (var context = new FlightContext())
            {
                if (user.BoundToPilotId != null)
                {
                    userPilotBinding = context.Pilots.Find(Convert.ToInt32(user.BoundToPilotId));
                    // Load club reference information 
                    if (userPilotBinding != null)
                    {
                        context.Entry(userPilotBinding).Reference(p => p.Club).Load();
                    }
                }

                if (user.EmailConfirmed)
                {
                    otherPilots = context.Pilots.Where(p => p.Email == user.Email).Include(c => c.Club).ToList();
                }

                if (user.PhoneNumberConfirmed)
                {
                    otherPilots = context.Pilots.Where(p => p.MobilNumber == user.PhoneNumber).Include(c => c.Club).ToList();
                }

                // Remove the existing pilot
                if (userPilotBinding != null && otherPilots.Any())
                {
                    otherPilots = otherPilots.Where(p => p.PilotId != userPilotBinding.PilotId).ToList();
                }

                // Auto set primary pilot binding information
                if (string.IsNullOrEmpty(user.BoundToPilotId) && otherPilots.Count == 1)
                {
                    user.BoundToPilotId = otherPilots.First().PilotId.ToString();
                    // Save to database
                    using (var appcontext = new ApplicationDbContext())
                    {
                        var appUser = appcontext.Users.Find(user.Id);
                        if (appUser != null)
                        {
                            appUser.BoundToPilotId = user.BoundToPilotId;
                            appcontext.SaveChanges();
                        }
                    }
                    userPilotBinding = otherPilots.First();
                    otherPilots = new List<Pilot>();
                }

            }

            var result = new ManagePilotBindingViewModel
            {
                CurrentPilotBinding = userPilotBinding,
                PotentialPilotBindings = otherPilots
            };

            return result;
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LinkPilot(string pilot)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                using (var context = new FlightContext())
                {
                    var userPilotBinding = context.Pilots.Find(Convert.ToInt32(pilot));
                    if (userPilotBinding != null)
                    {
                        // Validate User and Pilot have Email or Phone Number in Common

                        using (var db = new ApplicationDbContext())
                        {
                            var dbUser = db.Users.Find(user.Id);
                            if (dbUser != null)
                            {
                                dbUser.BoundToPilotId = userPilotBinding.PilotId.ToString(CultureInfo.InvariantCulture);
                                db.SaveChanges();
                                message = ManageMessageId.BindToPilotSuccess;
                            }
                        }
                    }
                }
            }
            return RedirectToAction("ManagePilotBinding", new { Message = message });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }
        
        private bool HasPilotBinding()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.BoundToPilotId != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePilotBindingSuccess,
            RemovePhoneSuccess,
            Error,
            BindToPilotSuccess
        }

        #endregion
    }
}