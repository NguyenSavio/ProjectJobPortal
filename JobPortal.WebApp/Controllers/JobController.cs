﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using System;
using Microsoft.AspNetCore.Identity;
using X.PagedList;

namespace JobPortal.WebApp.Controllers
{
    [Route("job")]
    public class JobController : Controller
    {
        private readonly DataDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public JobController(DataDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Route("")]
        public IActionResult Index(string slug, int? page)
        {
            int pageSize = 5; //number of jobs per page

            //for random value
            var random = new Random();

            //random jobs - 6
            var jobList = _context.Jobs.Include(j => j.Province).ToList();
            ViewBag.ListJobs = jobList.OrderBy(j => random.Next()).Take(6).ToList();

            //random skills - 7
            var skillList = _context.Skills.Include(s => s.Jobs).ToList();
            ViewBag.ListSkills = skillList.OrderBy(s => random.Next()).Where(s => s.Jobs.Count > 0).Take(7).ToList();

            //provinces - 4
            ViewBag.ListProvinces = _context.Provinces.Include(p => p.Jobs).Where(p => p.Jobs.Count > 0).Take(4).ToList();

            var jobs = _context.Jobs
                .OrderByDescending(j => j.Id)
                .Include(j => j.AppUser)
                .Include(j => j.Title)
                .Include(j => j.Time)
                .Include(j => j.Skills)
                .ToList();
            ViewBag.jobCount = _context.Jobs.Count();

            if (slug != null)
            {
                var time = _context.Times.FirstOrDefault(t => t.Slug == slug);
                var province = _context.Provinces.FirstOrDefault(p => p.Slug == slug);
                var employer = _context.AppUsers.FirstOrDefault(e => e.Slug == slug);

                if (time != null)
                {
                    jobs = (from t in _context.Times
                                  join job in _context.Jobs on t.Id equals job.TimeId
                                  orderby job.Id descending
                                  where t.Slug == slug
                                  select job).ToList();
                    ViewBag.Time = time;
                }
                else if (province != null)
                {
                    jobs = (from p in _context.Provinces
                                  join job in _context.Jobs on p.Id equals job.ProvinceId
                                  orderby job.Id descending
                                  where p.Slug == slug
                                  select job).ToList();
                    ViewBag.Province = province;
                }
                else if (employer != null)
                {
                    jobs = (from e in _context.AppUsers
                                  join job in _context.Jobs on e.Id equals job.AppUserId
                                  orderby job.Id descending
                                  where e.Slug == slug
                                  select job).ToList();
                    ViewBag.Employer = employer;
                }
                else
                {
                    var skill = _context.Skills.FirstOrDefault(s => s.Slug == slug);
                    if (skill != null)
                    {
                        jobs = jobs.Where(j => j.Skills.Any(s => s.Slug == slug)).ToList();
                        ViewBag.Skill = skill;
                    }
                }
                return View(jobs.ToPagedList(page ?? 1, pageSize)); //phan trang se lam mat danh sach
            }
            return View(jobs.ToPagedList(page ?? 1, pageSize));
        }

        [Route("{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            //for random value
            var random = new Random();

            //random jobs - 6
            var jobList = _context.Jobs.Include(j => j.Province).ToList();
            ViewBag.ListJobs = jobList.OrderBy(j => random.Next()).Take(6).ToList();

            //random skills - 7
            var skillList = _context.Skills.Include(s => s.Jobs).ToList();
            ViewBag.ListSkills = skillList.OrderBy(s => random.Next()).Where(s => s.Jobs.Count > 0).Take(7).ToList();

            //provinces - 4
            ViewBag.ListProvinces = _context.Provinces.Include(p => p.Jobs).Where(p => p.Jobs.Count > 0).Take(4).ToList();

            var job = await _context.Jobs
                .Where(j => j.Slug == slug)
                .Include(j => j.AppUser)
                .Include(j => j.Time)
                .Include(j => j.Title)
                .Include(j => j.Skills)
                .FirstOrDefaultAsync();
            job.Popular++;
            await _context.SaveChangesAsync();

            //check existing CV
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id != null ? Guid.Parse(user.Id.ToString()) : Guid.Empty;
            var hasSubmittedCV = userId != Guid.Empty ? await HasSubmittedCV(userId, slug) : false;
            ViewBag.HasSubmittedCV = hasSubmittedCV;

            return View(job);
        }

        private async Task<bool> HasSubmittedCV(Guid userId, string jobSlug)
        {
            return await _context.CVs.AnyAsync(cv => cv.Job.Slug == jobSlug && cv.AppUserId == userId);
        }

    }
}