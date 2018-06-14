﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using pBrainTrain.APIS.Models;
using pBrainTrain.Domain;
using System;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace pBrainTrain.APIS.Helpers
{
    public class UsersHelper : IDisposable
    {
        //see this on my github, you can download it, and study it them
        private static readonly ApplicationDbContext UserContext = new ApplicationDbContext();
        private static readonly DataContext Db = new DataContext();

        public static bool DeleteUser(string userName, string roleName)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));
            var userAsp = userManager.FindByEmail(userName);
            if (userAsp == null)
            {
                return false;
            }

            var response = userManager.RemoveFromRole(userAsp.Id, roleName);
            return response.Succeeded;
        }

        public static bool UpdateEmail(string currentUserEmail, string newUserEmail)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));
            var userAsp = userManager.FindByEmail(currentUserEmail);
            if (userAsp == null)
            {
                return false;
            }

            userAsp.UserName = newUserEmail;
            userAsp.Email = newUserEmail;
            var response = userManager.Update(userAsp);
            return response.Succeeded;
        }

        public static void CheckRole(string roleName)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(UserContext));

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }
        }

        public static void CheckSuperUser()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));
            var email = WebConfigurationManager.AppSettings["AdminUser"];
            var password = WebConfigurationManager.AppSettings["AdminPassWord"];
            var userAsp = userManager.FindByName(email);
            if (userAsp == null)
            {
                CreateUserAsp(email, "Admin", password);
                return;
            }

            userManager.AddToRole(userAsp.Id, "Admin");
        }

        public static void CreateUserAsp(string email, string roleName)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));
            var userAsp = userManager.FindByEmail(email);
            if (userAsp == null)
            {
                userAsp = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                };

                userManager.Create(userAsp, email);
            }

            userManager.AddToRole(userAsp.Id, roleName);
        }

        public static void CreateUserAsp(string email, string roleName, string password)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));

            var userAsp = new ApplicationUser
            {
                Email = email,
                UserName = email,
            };

            var result = userManager.Create(userAsp, password);
            if (result.Succeeded)
            {
                userManager.AddToRole(userAsp.Id, roleName);
            }
        }

        public static async Task PasswordRecovery(string email)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(UserContext));
            var userAsp = userManager.FindByEmail(email);
            if (userAsp == null)
            {
                return;
            }

            var random = new Random();
            var newPassword = string.Format("{0}", random.Next(100000, 999999));
            var response = await userManager.AddPasswordAsync(userAsp.Id, newPassword);
            if (response.Succeeded)
            {
                var subject = "Torneo Predicciones App - Recuperación de contraseña";
                var body = string.Format(@"
                    <h1>Torneo Predicciones - Recuperación de contraseña</h1>
                    <p>Su nueva contraseña es: <strong>{0}</strong></p>
                    <p>Por favor no olvide cambiarla por una de fácil recordación",
                    newPassword);

                //await Emails.SendMail(email, subject, body);
            }
        }

        public void Dispose()
        {
            UserContext.Dispose();
            Db.Dispose();
        }
    }
}