﻿using ASM.EF;
using ASM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ASM.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }
        public async Task<ActionResult> Index()
        {

            var context = new CMSContext();
            var store = new UserStore<UserInfor>(context);
            var manager = new UserManager<UserInfor>(store);
            var signInManager = new SignInManager<UserInfor, string>(manager, HttpContext.GetOwinContext().Authentication);

            var email = "thien@gmail.com";
            var password = "Tavip123";
            var phone = "0961119526";
            

            var user = await manager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new UserInfor
                {
                    UserName = email.Split('@')[0],
                    Email = email,
                    PhoneNumber = phone,
                    Name = email.Split('@')[0],
                };
                await manager.CreateAsync(user, password);
                await CreateRole(user.Email, "admin");
                return Content($"Welcome {user.UserName}. Your account has been Create!");

            }
            else
            {
                /* return Content($"Welcome {user.UserName}. You has login success to sysstem!");*/
                var result = await signInManager.PasswordSignInAsync(userName: user.UserName, password: password, isPersistent: false, shouldLockout: false);
                return Content($"Welcome back {user.UserName}. Your Account state is {result}");
            }


        }

        public async Task<ActionResult> CreateRole(string email, string role)
        {
            var context = new CMSContext();
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            var userStore = new UserStore<UserInfor>(context);
            var userManager = new UserManager<UserInfor>(userStore);

            if (!await roleManager.RoleExistsAsync(SecurityRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = SecurityRoles.Admin });
            }

            if (!await roleManager.RoleExistsAsync(SecurityRoles.Staff))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = SecurityRoles.Staff });
            }
            if (!await roleManager.RoleExistsAsync(SecurityRoles.Trainee))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = SecurityRoles.Staff });

            }
            if (!await roleManager.RoleExistsAsync(SecurityRoles.Trainer))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = SecurityRoles.Staff });
            }

            var User = await userManager.FindByEmailAsync(email);

            if (!await userManager.IsInRoleAsync(User.Id, SecurityRoles.Admin) && role == "admin")
            {
                userManager.AddToRole(User.Id, SecurityRoles.Admin);
            }
            if (!await userManager.IsInRoleAsync(User.Id, SecurityRoles.Staff) && role == "staff")
            {
                userManager.AddToRole(User.Id, SecurityRoles.Staff);
            }
            if (!await userManager.IsInRoleAsync(User.Id, SecurityRoles.Trainer) && role == "trainer")
            {
                userManager.AddToRole(User.Id, SecurityRoles.Trainer);
            }
            if (!await userManager.IsInRoleAsync(User.Id, SecurityRoles.Trainee) && role == "trainee")
            {
                userManager.AddToRole(User.Id, SecurityRoles.Trainee);
            }
            return Content("admin done!");
        }

        [HttpPost]
        public async Task<ActionResult> LogIn(UserInfor user)
        {
            var context = new CMSContext();
            var store = new UserStore<UserInfor>(context);
            var manager = new UserManager<UserInfor>(store);

            var signInManager
                = new SignInManager<UserInfor, string>(manager, HttpContext.GetOwinContext().Authentication);

            var fuser = await manager.FindByEmailAsync(user.Email);

            var result = await signInManager.PasswordSignInAsync(userName: user.Email.Split('@')[0], password: user.PasswordHash, isPersistent: false, shouldLockout: false);


            var userStore = new UserStore<UserInfor>(context);
            var userManager = new UserManager<UserInfor>(userStore);


            if (await userManager.IsInRoleAsync(fuser.Id, SecurityRoles.Admin))
            {
                return RedirectToAction("Test");
            }
            if (await userManager.IsInRoleAsync(fuser.Id, SecurityRoles.Staff))
            {
                return Content("dcmm");
            }
            else return RedirectToAction("Test2");


        }

        [HttpGet]
        public ActionResult AdminCreateStaff()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AdminCreateStaff(UserInfor staff)
        {

            var context = new CMSContext();
            var store = new UserStore<UserInfor>(context);
            var manager = new UserManager<UserInfor>(store);

            var user = await manager.FindByEmailAsync(staff.Email);

            if (user == null)
            {
                user = new UserInfor
                {
                    UserName = staff.Email.Split('@')[0],
                    Email = staff.Email,
                    PhoneNumber = "None!",
                    Name = staff.Email.Split('@')[0],
                    
                };
                await manager.CreateAsync(user, staff.PasswordHash);
                await CreateRole(staff.Email, "staff");
            }

            return RedirectToAction("LogIn");
        }

    }
}