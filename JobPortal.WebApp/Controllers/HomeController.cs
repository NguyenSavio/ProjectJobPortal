﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using JobPortal.WebApp.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JobPortal.Data.ViewModel;

namespace JobPortal.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly RoleManager<AppRole> roleManager;
        private readonly UserManager<AppUser> userManager;
        private readonly DataDbContext _context;

        public HomeController(SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, DataDbContext dataDbContext)
        {
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this._context = dataDbContext;
        }

        public IActionResult Index()
        {
			//for random value
			var random = new Random();

            //for model
            var jobs = _context.Jobs.ToList();

            //For search filter area
            ViewBag.FilterProvinces = _context.Provinces.OrderBy(p => p.Id).Where(p => p.Jobs.Count > 0).ToList();
            ViewBag.FilterSkills = _context.Skills.OrderBy(s => s.Name).Where(s => s.Jobs.Count > 0).ToList();

            //random employers - 4
            var employerList = _context.Users.Where(e => e.Status == 2).Include(e => e.Province).Include(e => e.Jobs).ToList();
            ViewBag.RandomEmployers = employerList.OrderBy(e => Guid.NewGuid()).Where(e => e.Jobs.Count > 0).Take(4).ToList();

            //random skills - 6
            var skillList = _context.Skills.ToList();
            ViewBag.RandomSkills = skillList.OrderBy(s => random.Next()).Take(6).ToList();

            //random jobs - 6
            var jobList = _context.Jobs
                .Include(j => j.Province)
                .Include(j => j.AppUser)
                .Include(j => j.Title)
                .Include(j => j.Time)
                .Include(j => j.Skills)
                .ToList();
            var randomJobs = jobList.OrderBy(j => random.Next()).Take(6).ToList();
            ViewBag.RandomJobs = randomJobs;

            return View(jobs);
        }

        [Route("about-us")]
        public IActionResult AboutUs()
        {
            return View();
        }

        [Route("price")]
        public IActionResult Price()
        {
            return View();
        }

        [Route("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("elements")]
        public IActionResult Elements()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}