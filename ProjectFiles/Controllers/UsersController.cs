using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace MusicDB.Models
{
    public class UsersController : Controller
    {
        private MusicDBEntities db = new MusicDBEntities();

        // GET: Users/Account/5
        public ActionResult Account(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(User user)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                if(EmailExist(user.Email))
                {
                    ModelState.AddModelError("EmailExists", "Ten email jest już zajęty.");
                    return View(user);
                }

                db.Configuration.ValidateOnSaveEnabled = false;
                user.Password = PasswordHash.Hash(user.Password);
                db.Users.Add(user);
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                ViewBag.Message = "Rejestracja zakończona pomyślnie.";
            }

            return View(user);
        }

        // GET: Users/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Users/Register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (user.Password == null || user.Email == null)
            {
                return View(user);
            }

            var person = db.Users.Where(a => a.Email == user.Email).FirstOrDefault();

            if (person != null)
            {
                if (string.Compare(PasswordHash.Hash(user.Password), person.Password) == 0)
                {
                    Session["userID"] = person.UserID.ToString();
                    Session["userType"] = person.Type.ToString();
                    Session["userName"] = person.Name.ToString();

                    if (person.Avatar != null)
                    {
                        Session["userAvatar"] = Convert.ToBase64String(person.Avatar);
                    }
                    int timeout = 30;
                    var ticket = new FormsAuthenticationTicket(user.Email.ToString(), false, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Niepoprawne hasło.";
                }
            }
            else
            {
                ViewBag.Message = "Niepoprawny email.";
            }

            return View(user);
        }

        // POST: Users/Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Settings
        [Authorize]
        public ActionResult Settings()
        {
            return View();
        }
            

        // GET: Users/DetailsSettings
        [Authorize]
        public ActionResult DetailsSettings()
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int id = Int32.Parse(str);
            var user = db.Users.Where(a => a.UserID == id).FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/DetailsSettings
        public ActionResult DetailsSettings(User user)
        {
            if (ModelState.IsValid)
            {
                var person = db.Users.Where(a => a.Email == user.Email & a.UserID != user.UserID).FirstOrDefault();

                if (person != null)
                {
                    ModelState.AddModelError("EmailExists", "Ten email jest już zajęty.");
                    return View(user);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.Message = "Zaktualizowano dane.";
            }
            return View(user);
        }

        // GET: Users/DetailsSettings
        [Authorize]
        public ActionResult AvatarSettings()
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int id = Int32.Parse(str);
            var user = db.Users.Where(a => a.UserID == id).FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/AvatarSettings
        public ActionResult AvatarSettings(User user, string submitButton, HttpPostedFileBase postedFile)
        {
            switch(submitButton)
            {
                case "AddAvatar":
                    byte[] bytes;
                    if (postedFile != null)
                    {
                        using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                        {
                            var postedFileExtension = Path.GetExtension(postedFile.FileName);
                            if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
                            {
                                                    
                                ViewBag.Message = "Zły format pliku.";
                                return View(user);
                            }
                            bytes = br.ReadBytes(postedFile.ContentLength);
                        }

                        db.Entry(user).State = EntityState.Modified;
                        user.Avatar = bytes;
                        db.SaveChanges();
                        Session["userAvatar"] = Convert.ToBase64String(user.Avatar);
                        ViewBag.Message = "Dodano avatar.";
                    }
                    else
                    {
                        ViewBag.Message = "Pole nie może być puste.";
                    }
                    return View(user);
                case "DeleteAvatar":
                    if (ModelState.IsValid)
                    {
                        db.Entry(user).State = EntityState.Modified;
                        user.Avatar = null;
                        db.SaveChanges();
                        ViewBag.Message = "Usunięto avatar.";
                    }
                    return View(user);
                default:
                    return View(user);
            }
        }

        // GET: Users/PasswordSettings
        [Authorize]
        public ActionResult PasswordSettings()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/PasswordSettings
        public ActionResult PasswordSettings(Password password)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int id = Int32.Parse(str);
            var user = db.Users.Where(a => a.UserID == id).FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                db.Configuration.ValidateOnSaveEnabled = false;
                user.Password = PasswordHash.Hash(password.Text);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                ViewBag.Message = "Zaktualizowano hasło.";
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/DeleteRate/5
        public ActionResult DeleteRate(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Rating rating = db.Ratings.Find(id);

            if (rating == null)
            {
                return HttpNotFound();
            }

            db.Ratings.Remove(rating);
            db.SaveChanges();
            return RedirectToAction("Account", "Users", new { id = Session["userID"] });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/DeleteFav/5
        public ActionResult DeleteFav(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Favourite favourite = db.Favourites.Find(id);

            if (favourite == null)
            {
                return HttpNotFound();
            }

            db.Favourites.Remove(favourite);
            db.SaveChanges();
            return RedirectToAction("Account", "Users", new { id = Session["userID"] });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/DeleteReview/5
        public ActionResult DeleteReview(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Review review = db.Reviews.Find(id);

            if (review == null)
            {
                return HttpNotFound();
            }

            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction("Account", "Users", new { id = Session["userID"] });
        }

        [NonAction]
        public bool EmailExist(string Email)
        {
            var person = db.Users.Where(a => a.Email == Email).FirstOrDefault();
            return person != null;
        }
    }
}
