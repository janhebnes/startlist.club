using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using FlightLog.Models;

namespace FlightLog.Controllers
{
    using System.Configuration;
    using System.Net;
    using System.Net.Configuration;
    using System.Net.Mail;

    public class AccountController : DataAvail.Mvc.Account.OAuthAccountController
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            base.OAuthBeforeLogOn();

            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //var isPilot = false;
                //using (var db = new FlightContext())
                //{
                //    isPilot = db.Pilots.Any(p => p.Name == model.UserName);
                //    if (!isPilot) 
                //    {
                //        ModelState.AddModelError("", "The user " + model.UserName + " does not have a matching pilot profil in the system.");
                //    }
                //}

                //isPilot && 

                if (model.UserName.Contains("@"))
                {
                    var result = MembershipService.FindUsersByEmail(model.UserName);
                    var membershipUsers = result as IList<MembershipUser> ?? result.ToList();
                    if (membershipUsers.Any())
                    {
                        if (membershipUsers.Count() == 1)
                        {
                            model.UserName = membershipUsers.First().UserName;
                        }
                    }
                }

                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {

                    this.FormsService.SignIn(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                MembershipCreateStatus createStatus;
                //using (var db = new FlightContext())
                //{
                //    var pilot = db.Pilots.FirstOrDefault(p => p.Name == model.UserName);
                //    if (pilot != null)
                //    {
                // Attempt to register the user
                createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);
                //    }
                //    else
                //    {
                //        ModelState.AddModelError("", "The user " + model.UserName + " does not have a matching pilot profil in the system.");
                //        createStatus = MembershipCreateStatus.UserRejected;
                //    }
                //}

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        // **************************************
        // URL: /Account/PasswordReset
        // **************************************

        public ActionResult PasswordReset()
        {
            if (!MembershipService.EnablePasswordReset)
            {
                throw new Exception("Password reset is not allowed");
            }
            return View();
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        [HttpPost]
        public ActionResult PasswordReset(string userName)
        {
            if (!MembershipService.EnablePasswordReset)
            {
                throw new Exception("Password reset is not allowed");
            }

            MembershipUser user = null;

            // Try by Email
            if (userName.Contains("@"))
            {
                var result = MembershipService.FindUsersByEmail(userName);
                var membershipUsers = result as IList<MembershipUser> ?? result.ToList();
                if (membershipUsers.Any())
                {
                    if (membershipUsers.Count() == 1)
                    {
                        user = membershipUsers.First();
                    }
                    else
                    {
                        ViewBag.Error = "<p class=\"error\">Multiple user accounts where found with user name: " +
                                        userName + ", please contact administrator for help.</p>";
                        return this.View();
                    }
                }
            }

            if (user == null)
            {
                // Try by Name
                var result = MembershipService.FindUsersByName(userName);
                var membershipUsers = result as IList<MembershipUser> ?? result.ToList();
                if (membershipUsers.Any())
                {
                    if (membershipUsers.Count() == 1)
                    {
                        user = membershipUsers.First();
                    }
                    else
                    {
                        ViewBag.Error = "<p class=\"error\">Multiple user accounts where found with user name: " +
                                        userName + ", please contact administrator for help.</p>";
                        return this.View();
                    }
                }
            }


            if (user != null)
            {
                if (!user.Email.Contains("@"))
                {
                    ViewBag.Error =
                        "<p class=\"error\">User does not have an associated e-mail, password recovery is not possible, please contact administrator for help.</p>";
                    return this.View();
                }

                try
                {
                    var newPassword = user.ResetPassword();
                    var subject = "Password Reset Request for " + GetWebAppRoot();
                    var body = "Your password has been reset to: " + newPassword +
                               " \n\nWe received a reset password request for your account: " + userName + ". \n\n" +
                               GetWebAppRoot() + "/Account/LogOn\n\nYou can change your password at\n" +
                               GetWebAppRoot() + "/Account/ChangePassword\n\nRegards,\nRobert Robot";

                    var smtp = new SmtpClient();
                    using (
                        var message = new MailMessage(GetSmtpFromAddress(), user.Email)
                            {
                                Subject = subject,
                                Body = body
                            })
                    {
                        smtp.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error =
                        "<p class=\"error\">System Error sending e-mail message to your e-mail.</p><textarea style=\"display:none\"> " +
                        ex + " </textarea>";
                    return this.View();
                }
                return RedirectToAction("PasswordResetSuccess");
            }

            ViewBag.Error = "<p class=\"error\">Could not find user name: " + userName + "</p><p>Would you like to <a href=\"/Account/Register\">Register</a>? </p>";
            return this.View();
        }

        public string GetSmtpFromAddress()
        {
            var config = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;

            return (config != null) ? config.From : null;
        }

        public string GetWebAppRoot()
        {
            if (HttpContext.Request.ApplicationPath == "/")
                return "http://" + HttpContext.Request.Url.Host;
            else
                return "http://" + HttpContext.Request.Url.Host + HttpContext.Request.ApplicationPath;
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult PasswordResetSuccess()
        {
            return View();
        }

    }
}

