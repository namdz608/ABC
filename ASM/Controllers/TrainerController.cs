﻿using ASM.EF;
using ASM.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static ASM.Models.UserInfor;

namespace ASM.Controllers
{
    [Authorize(Roles = SecurityRoles.Trainer)]
    public class TrainerController : Controller
    {
        

        // GET: Trainer
        public async Task<ActionResult>  Index()
        {
            TempData["username"] = TempData["username"];
            var context = new CMSContext();
            var store = new UserStore<UserInfor>(context);
            var manager = new UserManager<UserInfor>(store);

            var user = await manager.FindByEmailAsync(TempData["username"].ToString()+"@gmail.com");
            return View(user);
        }




        [HttpGet]
        public ActionResult EditTrainer(string username)
        {
            using (var FAPCtx = new EF.CMSContext())
            {
                var trainer = FAPCtx.Users.FirstOrDefault(c => c.UserName == username);

                if (trainer != null)
                {
                    TempData["username"] = username;
                    return View(trainer);
                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
        }
        [HttpPost]
        public async Task<ActionResult> EditTrainer(string username, UserInfor trainer)
        {

            CustomValidationTrainer(trainer);

            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                var context = new CMSContext();
                var store = new UserStore<UserInfor>(context);
                var manager = new UserManager<UserInfor>(store);

                var user = await manager.FindByEmailAsync(trainer.Email);

                if (user != null)
                {
                    user.UserName = trainer.Email.Split('@')[0];
                    user.Email = trainer.Email;                  
                    user.Name = trainer.Name;
                    user.Type = trainer.Type;               
                    user.Education = trainer.Education;
                    user.WorkingPlace = trainer.WorkingPlace;
                    await manager.UpdateAsync(user);
                    TempData["username"] = TempData["username"];
                    @TempData["alert"] = "Change Profile successful";
                    return RedirectToAction("Index", "Trainer");
                }
                TempData["username"] = TempData["username"];
                @TempData["alert"] = "E-mail is being used";
                return RedirectToAction("Index");
            }
        }


        private void SetViewBag()
        {
            using (var bwCtx = new EF.CMSContext())
            {
                ViewBag.Publishers = bwCtx.Courses
                                  .Select(p => new SelectListItem
                                  {
                                      Text = p.Name,
                                      Value = p.Id.ToString()
                                  })
                                  .ToList();

                ViewBag.Formats = bwCtx.Courses.ToList(); //select *
            }
        }

        [HttpGet]
        public async Task<ActionResult> ShowCourseAssign()
        {
            TempData["username"] = TempData["username"];
            var context = new CMSContext();
            var store = new UserStore<UserInfor>(context);
            var manager = new UserManager<UserInfor>(store);

            var user = await manager.FindByEmailAsync(TempData["username"].ToString() + "@gmail.com");

            var a = manager.Users.Include(x => x.listCourse).FirstOrDefault(b => b.Id == user.Id);

            if (a != null)
            {
                SetViewBag();
                return View(a);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        private void CustomValidationTrainer(UserInfor trainer)
        {
            if (string.IsNullOrEmpty(trainer.Email))
            {
                ModelState.AddModelError("Email", "Please input Email");
            }
            if (string.IsNullOrEmpty(trainer.Name))
            {
                ModelState.AddModelError("Name", "Please input Name");
            }
            if (!string.IsNullOrEmpty(trainer.Email))
            {
                if (!trainer.Email.Contains("@") || (trainer.Email.Split('@')[0] == "") || (trainer.Email.Split('@')[1] == "") || trainer.Email.Split('@')[1] != "gmail.com")
                {
                    ModelState.AddModelError("Email", "Please use a valid Email (abc@gmail.com)");
                }
            }
            if (!string.IsNullOrEmpty(trainer.Email) && (trainer.Email.Length >= 30))
            {
                ModelState.AddModelError("Email", "Email length must be less than 30 characters!");
            }

        }


        [HttpGet]
        public ActionResult ChangePass(string username)
        {
            TempData["username"] = username;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ChangePass(FormCollection fc, UserInfor userInfor)
        {

            CustomValidationTrainerPass(userInfor);

            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                var context = new CMSContext();
                var store = new UserStore<UserInfor>(context);
                var manager = new UserManager<UserInfor>(store);

                var user = await manager.FindByEmailAsync(TempData["username"].ToString() + "@gmail.com");
                var result = manager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, userInfor.PasswordHash);

                if (user != null)
                {
                    if (result == PasswordVerificationResult.Success)
                    {
                        String newPassword = userInfor.PassTemp;
                        String hashedNewPassword = manager.PasswordHasher.HashPassword(newPassword);
                        user.PasswordHash = hashedNewPassword;
                        await store.UpdateAsync(user);
                        @TempData["alert"] = "Change PassWord successful";
                    }
                    else
                    {
                        ModelState.AddModelError("PasswordHash", "Old Password incorrect!");
                        TempData["username"] = TempData["username"];
                        return View();
                    }

                }
                TempData["username"] = TempData["username"];
                return RedirectToAction("Index", "Trainer");
            }
        }
        public void CustomValidationTrainerPass(UserInfor staff)
        {
            if (string.IsNullOrEmpty(staff.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Please input old Password");
            }
            if (string.IsNullOrEmpty(staff.PassTemp))
            {
                ModelState.AddModelError("PassTemp", "Please input new Password");
            }
            
            if (string.IsNullOrEmpty(staff.PassTempConfirm))
            {
                ModelState.AddModelError("PassTempConfirm", "Please input Confirm Password");
            }
            if (!string.IsNullOrEmpty(staff.PassTempConfirm) && !string.IsNullOrEmpty(staff.PassTemp) && (staff.PassTemp != staff.PassTempConfirm))
            {
                ModelState.AddModelError("PassTempConfirm", "New password and Confirm password not match");
            }
            if (!string.IsNullOrEmpty(staff.PassTempConfirm) && !string.IsNullOrEmpty(staff.PassTemp) && (staff.PassTemp.Length <=7))
            {
                ModelState.AddModelError("PassTempConfirm", "New password must longer than 7 character");
            }
        }

        public ActionResult ShowCourse()
        {
            using (var classes = new EF.CMSContext())
            {
                var Course = classes.Courses.OrderBy(a => a.Id).ToList();
                return View(Course);
            }
        }
    }
}