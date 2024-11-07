using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using FlightJournal.Web.Configuration;
using FlightJournal.Web.Validators;
using Microsoft.Ajax.Utilities;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace FlightJournal.Web.Models
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 4, 
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        /// <summary>
        /// Method to add user to multiple roles
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="roles">list of role names</param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> AddUserToRolesAsync(string userId, IList<string> roles)
        {
            var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

            var user = await FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid user Id");
            }

            var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
            // Add user to each role using UserRoleStore
            foreach (var role in roles.Where(role => !userRoles.Contains(role)))
            {
                await userRoleStore.AddToRoleAsync(user, role).ConfigureAwait(false);
            }

            // Call update once when all roles are added
            return await UpdateAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove user from multiple roles
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="roles">list of role names</param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> RemoveUserFromRolesAsync(string userId, IList<string> roles)
        {
            var userRoleStore = (IUserRoleStore<ApplicationUser, string>) Store;

            var user = await FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid user Id");
            }

            var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
            // Remove user to each role using UserRoleStore
            foreach (var role in roles.Where(userRoles.Contains))
            {
                await userRoleStore.RemoveFromRoleAsync(user, role).ConfigureAwait(false);
            }

            // Call update once when all roles are removed
            return await UpdateAsync(user).ConfigureAwait(false);
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole,string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var manager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));

            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.

            try
            {
                // Local testing of email send operation can be done with e.g. Papercut (we lover papercut)
                // https://github.com/ChangemakerStudios/Papercut-SMTP/releases

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = Settings.MailSmtpHost;
                    smtpClient.Port = Settings.MailSmtpPort;
                    if (Settings.MailSmtpPort == 587)
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                    }
                    smtpClient.Credentials = new System.Net.NetworkCredential(Settings.MailSmtpUserName, Settings.MailSmtpPassword);
                    using (MailMessage mail = new MailMessage())
                    {
                        //mail.From = new MailAddress("no-reply@startlist.club", "Startlist.club");
                        mail.From = new MailAddress(Settings.MailFrom);
                        mail.To.Add(new MailAddress(message.Destination));
                        mail.Subject = message.Subject;
                        mail.Body = message.Body;

                        smtpClient.Send(mail);
                    }
                }

                return Task.FromResult(0);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Trace.TraceError($"Failed to send to {message.Destination} from {Settings.MailFrom} with host {Settings.MailSmtpHost}:{Settings.MailSmtpPort} and user {Settings.MailSmtpUserName} and exception {exception}");
                throw new Exception($"Failed to send to {message.Destination} from {Settings.MailFrom}", exception);
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            
            if (!string.IsNullOrWhiteSpace(Settings.TwilioAccountSid)
                && !string.IsNullOrWhiteSpace(Settings.TwilioAuthToken)
                && !string.IsNullOrWhiteSpace(Settings.TwilioFromNumber))
            {
                // Find your Account Sid and Auth Token at twilio.com/user/account 
                TwilioClient.Init(Settings.TwilioAccountSid, Settings.TwilioAuthToken);

                var smsmessage = MessageResource.Create(
                    message.Destination,
                    from: Settings.TwilioFromNumber,
                    body: message.Body
                );

                // v3 API 
                //var twilio = new TwilioRestClient(Settings.TwilioAccountSid, Settings.TwilioAuthToken);
                //var smsmessage = twilio.SendMessage(Settings.TwilioFromNumber, message.Destination, message.Body);

                if (smsmessage.Sid == null)
                {
                    Task.FromResult(1);

                    System.Diagnostics.Trace.TraceError($"Twilio did not respond with a valid message sid when sending to destination {message.Destination} - no sms sent");
                    throw new Exception($"Twilio did not respond with a valid message sid when sending to destination {message.Destination} - no sms sent");
                }
                if (smsmessage.ErrorMessage != null)
                {
                    Task.FromResult(1);

                    System.Diagnostics.Trace.TraceError(smsmessage.ErrorMessage);
                    throw new Exception(smsmessage.ErrorMessage);
                }
                return Task.FromResult(0);
            }
            else
            {
                return Task.FromResult(1);    
            }
        }
    }


    public enum SignInStatus
    {
        Success,
        UnConfirmed,
        LockedOut,
        RequiresTwoFactorAuthentication,
        Failure
    }

    // These help with sign and two factor (will possibly be moved into identity framework itself)
    public class SignInHelper
    {
        public SignInHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
        {
            UserManager = userManager;
            AuthenticationManager = authManager;
        }

        public ApplicationUserManager UserManager { get; private set; }
        public IAuthenticationManager AuthenticationManager { get; private set; }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
        {
            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            var userIdentity = await user.GenerateUserIdentityAsync(UserManager);
            if (rememberBrowser)
            {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id);
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
            }
            else
            {
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
            }
        }

        public async Task<bool> SendTwoFactorCode(string provider)
        {
            var userId = await GetVerifiedUserIdAsync();
            return await SendTwoFactorCode(provider, userId);
        }

        public async Task<bool> SendTwoFactorCode(string provider, string userId)
        {
            if (userId == null)
            {
                return false;
            }

            var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
            // See IdentityConfig.cs to plug in Email/SMS services to actually send the code
            await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
            return true;
        }

        public async Task<string> GetVerifiedUserIdAsync()
        {
            var result = await AuthenticationManager.AuthenticateAsync(DefaultAuthenticationTypes.TwoFactorCookie);
            if (result != null && result.Identity != null && !String.IsNullOrEmpty(result.Identity.GetUserId()))
            {
                return result.Identity.GetUserId();
            }
            return null;
        }

        public async Task<bool> HasBeenVerified()
        {
            return await GetVerifiedUserIdAsync() != null;
        }

        public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            var userId = await GetVerifiedUserIdAsync();
            return await TwoFactorSignIn(provider, code, isPersistent, rememberBrowser, userId);
        }

        public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent, bool rememberBrowser, string userId)
        {
            if (userId == null)
            {
                return SignInStatus.Failure;
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            if (!user.EmailConfirmed)
            {
                return SignInStatus.UnConfirmed;
            }
            if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
            {
                // When token is verified correctly, clear the access failed count used for lockout
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInAsync(user, isPersistent, rememberBrowser);
                return SignInStatus.Success;
            }
            // If the token is incorrect, record the failure which also may cause the user to be locked out
            await UserManager.AccessFailedAsync(user.Id);
            return SignInStatus.Failure;
        }

        public async Task<SignInStatus> ExternalSignIn(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (!user.EmailConfirmed)
            {
                return SignInStatus.UnConfirmed;
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            return await SignInOrTwoFactor(user, isPersistent);
        }

        private async Task<SignInStatus> SignInOrTwoFactor(ApplicationUser user, bool isPersistent)
        {
            UpdateLastLogonTimeStamp(user);

            // Validate that the user has valid TwoFactorProvider possible - otherwise the users access to the site is blocked 
            var validTwoFactorProviders = await UserManager.GetValidTwoFactorProvidersAsync(user.Id);

            if (validTwoFactorProviders.Any() && 
                await UserManager.GetTwoFactorEnabledAsync(user.Id) &&
                !await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id))
            {
                var identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                AuthenticationManager.SignIn(identity);
                return SignInStatus.RequiresTwoFactorAuthentication;
            }
            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;

        }

        /// <summary>
        /// Update LastLogonTimeStamp field on user 
        /// </summary>
        /// <param name="user"></param>
        private static void UpdateLastLogonTimeStamp(ApplicationUser user)
        {
            using (var context = new ApplicationDbContext())
            {
                var applicationUser = context.Users.Find(user.Id);
                if (applicationUser != null)
                {
                    applicationUser.LastLogonTimeStamp = DateTime.Now;
                }
                context.SaveChanges();
            }
        }

        public async Task<SignInStatus> PasswordSignIn(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            // Disallow mobil userName signin through Password SignIn
            if (MobilNumberValidator.IsValid(userName))
            {
                return SignInStatus.Failure;
            }

            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (!user.EmailConfirmed)
            {
                return SignInStatus.UnConfirmed;
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            if (await UserManager.CheckPasswordAsync(user, password))
            {
                return await SignInOrTwoFactor(user, isPersistent);
            }
            if (shouldLockout)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await UserManager.AccessFailedAsync(user.Id);
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }
            }
            return SignInStatus.Failure;
        }

        public async Task<SignInStatus> MobilSignIn(string userName, bool isPersistent)
        {
            // Restrict MobilSignIn to Only mobil UserName
            if (!MobilNumberValidator.IsValid(userName, true))
            {
                return SignInStatus.Failure;
            }
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                var pilots = MobilNumberValidator.FindPilotsByMobilNumber(userName); // userName is mobil number
                if (!pilots.Any())
                    return SignInStatus.Failure;

                // TODO: Handle multiple pilot profile registration
                if (pilots.Count() > 1)
                    return SignInStatus.Failure;

                // HACK: We only attach to the first pilot profil in this setup.
                var pilot = pilots.First();
                
                // Create mobilPhone Application User, Email is required and + is removed 
                user = new ApplicationUser()
                {
                    UserName = userName,
                    Email = userName.Substring(userName.Length-1) + pilot.Email.ToLower(),
                    EmailConfirmed = true,
                    BoundToPilotId = pilot.PilotId.ToString(),
                    PhoneNumberConfirmed = true,
                    PhoneNumber = MobilNumberValidator.ParseMobilNumber(userName),
                    TwoFactorEnabled = true
                };
                var result = UserManager.Create(user);
                if (!result.Succeeded)
                {
                    throw new SecurityException(string.Format("Failed to generate user {0} for {1}", userName, result.Errors.FirstOrDefault()));
                    //return SignInStatus.Failure;
                }
                result = UserManager.SetLockoutEnabled(user.Id, false);
                return await SignInOrTwoFactor(user, isPersistent);
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            
            // Password is null we require TwoFactor
            return await SignInOrTwoFactor(user, isPersistent);
        }
    }
}