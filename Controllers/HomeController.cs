using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Windows.Forms;
using Rotativa;

namespace LoginUserMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserDbEntities db = new UserDbEntities();
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(tblUser User)
        {
            if (ModelState.IsValid)
            {
                using (db)
                {
                    var obj = db.tblUsers.Where(x => x.UserName.Equals(User.UserName) && x.Password.Equals(User.Password)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["UserID"] = obj.ID.ToString();
                        Session["UserName"] = obj.UserName.ToString();
                        return RedirectToAction("UserDashBoard");
                    }
                    else
                    {
                        ViewBag.Message = string.Format("Invalid Username or Password");
                    }
                }
            }
            return View(User);
        }

        public ActionResult UserDashBoard(string sortOrder)
        {
            if (Session["UserID"] != null)
            {
                ViewBag.FirstNameSortParm = (sortOrder == "firstname_asc") ? "firstname_desc" : "firstname_asc";
                ViewBag.LastNameSortParm = (sortOrder == "lastname_asc") ? "lastname_desc" : "lastname_asc";

                var users = from a in db.tblUsers
                            select a;

                switch (sortOrder)
                {
                    case "firstname_asc":
                        users = users.OrderBy(x => x.FirstName);
                        break;
                    case "lastname_asc":
                        users = users.OrderBy(x => x.LastName);
                        break;
                    case "firstname_desc":
                        users = users.OrderByDescending(x => x.FirstName);
                        break;
                    case "lastname_desc":
                        users = users.OrderByDescending(x => x.LastName);
                        break;
                    default:
                        users = users.OrderBy(x => x.ID);
                        break;
                }
                return View(users.ToList());
            }
            else
            {
                return RedirectToAction("Login");
            }

        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(tblUser User)
        {
            if (string.IsNullOrEmpty(User.Oib) || User.Oib.Length > 11)
            {
                ModelState.AddModelError("Oib", "Oib is empty or over 11 numbers.");
            }
            if (!string.IsNullOrEmpty(User.Email))
            {
                string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                         @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(User.Email))
                {
                    ModelState.AddModelError("Email", "Email is not valid.");
                }
            }
            else
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            if (ModelState.IsValid)
            {
                db.tblUsers.Add(User);
                db.SaveChanges();
                return RedirectToAction("UserDashBoard");
            }
            return View(User);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblUser User = db.tblUsers.Find(id);
            if (User == null)
            {
                return HttpNotFound();
            }
            return View(User);
        }
        [HttpPost]
        public ActionResult Edit(tblUser User)
        {
            if (ModelState.IsValid)
            {
                db.Entry(User).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("UserDashBoard");
            }

            return View();
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tblUser user = db.tblUsers.SingleOrDefault(x => x.ID == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var report = new PartialViewAsPdf("~/Views/Home/Details.cshtml", user);
            return report;
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tblUser user = db.tblUsers.SingleOrDefault(x => x.ID == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            tblUser user = db.tblUsers.SingleOrDefault(x => x.ID == id);
            db.tblUsers.Remove(user ?? throw new InvalidOperationException());
            db.SaveChanges();
            return RedirectToAction("UserDashBoard");
        }
        public ActionResult PrintViewToPdf()
        {
            var users = from a in db.tblUsers
                        select a;
            var report = new PartialViewAsPdf("~/Views/Home/UserDashBoard.cshtml",users);
            return report;
        }
    }
}
