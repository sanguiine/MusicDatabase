using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicDB.Models;

namespace MusicDB.Controllers
{
    public class AdminController : Controller
    {
        private MusicDBEntities db = new MusicDBEntities();

        // GET: Admin
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            ViewBag.Current = "Index";
            ViewBag.UsersCount = db.Users.Count();
            ViewBag.RatingsCount = db.Ratings.Count();
            ViewBag.CommentsCount = db.Comments.Count();
            ViewBag.ReviewsCount = db.Reviews.Count();

            var rank = from album in db.Albums
                       join top in db.Top100Albums on album.AlbumID equals top.AlbumID
                       select new Top
                       {
                           AlbumID = album.AlbumID,
                           Title = album.Title,
                           ReleaseDate = album.ReleaseDate,
                           Cover = album.Cover,
                           Artists = album.Artists,
                           Genres = album.Genres,
                           Average = top.Average
                       };

            rank = rank.OrderByDescending(s => s.Average).Take(10);
            return View(rank.ToList());
        }

        // GET: Admin/Comments
        [Authorize(Roles = "admin")]
        public ActionResult Comments()
        {
            ViewBag.Current = "Comments";
            return View(db.Comments.ToList());
        }

        // POST: Admin/DeleteComment/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Comments", "Admin");
        }

        // GET: Admin/Reviews
        [Authorize(Roles = "admin")]
        public ActionResult Reviews()
        {
            ViewBag.Current = "Reviews";
            return View(db.Reviews.ToList());
        }

        // POST: Admin/DeleteReview/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReview(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction("Reviews", "Admin");
        }

        // GET: Admin/Genres
        [Authorize(Roles = "admin")]
        public ActionResult Genres()
        {
            ViewBag.Current = "Genres";
            return View(db.Genres.ToList());
        }

        // POST: Admin/DeleteGenre/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGenre(int id)
        {
            Genre genre = db.Genres.Find(id);
            db.Genres.Remove(genre);
            db.SaveChanges();
            return RedirectToAction("Genres", "Admin");
        }

        // GET: Admin/AddGenre
        [Authorize(Roles = "admin")]
        public ActionResult AddGenre()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddGenre
        public ActionResult AddGenre(Genre genre)
        {
            if (ModelState.IsValid)
            {
                db.Genres.Add(genre);
                db.SaveChanges();
                return RedirectToAction("Genres", "Admin");
            }

            return View(genre);
        }

        [Authorize(Roles = "admin")]
        // GET: Admin/EditGenre/5
        public ActionResult EditGenre(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                return HttpNotFound();
            }
            return View(genre);
        }

        // POST: Admin/EditGenre
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGenre(Genre genre)
        {
            if (ModelState.IsValid)
            {
                var item = db.Genres.Where(a => a.Name == genre.Name & a.GenreID != genre.GenreID).FirstOrDefault();

                if (item != null)
                {
                    ModelState.AddModelError("GenreExists", "Ten gatunek już istnieje.");
                    return View(genre);
                }

                db.Entry(genre).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Genres", "Admin");
            }
            return View(genre);
        }

        // GET: Admin/Users
        [Authorize(Roles = "admin")]
        public ActionResult Users()
        {
            ViewBag.Current = "Users";
            return View(db.Users.ToList());
        }

        // POST: Admin/DeleteUser/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Users", "Admin");
        }

        // GET: Admin/AddUser
        [Authorize(Roles = "admin")]
        public ActionResult AddUser()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddUser
        public ActionResult AddUser(User user, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                if (EmailExist(user.Email))
                {
                    ModelState.AddModelError("EmailExists", "Ten email jest już zajęty.");
                    return View(user);
                }

                db.Configuration.ValidateOnSaveEnabled = false;
                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }

                    user.Avatar = bytes;
                }

                user.Password = PasswordHash.Hash(user.Password);
                db.Users.Add(user);
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                return RedirectToAction("Users", "Admin");
            }

            return View(user);
        }

        // GET: Admin/EditUser/5
        [Authorize(Roles = "admin")]
        public ActionResult EditUser(int? id)
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

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult EditUser(User user, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                var item = db.Users.Where(a => a.Email == user.Email & a.UserID != user.UserID).FirstOrDefault();

                if (item != null)
                {
                    ModelState.AddModelError("EmailExists", "Ten email jest już zajęty.");
                    return View(user);
                }

                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }
                    user.Avatar = bytes;
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Users", "Admin");
            }
            return View(user);
        }

        // GET: Admin/Artists
        [Authorize(Roles = "admin")]
        public ActionResult Artists()
        {
            ViewBag.Current = "Artists";
            return View(db.Artists.ToList());
        }

        // POST: Admin/DeleteArtist/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteArtist(int id)
        {
            Artist artist = db.Artists.Find(id);
            db.Artists.Remove(artist);
            db.SaveChanges();
            return RedirectToAction("Artists", "Admin");
        }

        // GET: Admin/AddArtist
        [Authorize(Roles = "admin")]
        public ActionResult AddArtist()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        // POST: Admin/AddArtist
        public ActionResult AddArtist(Artist artist, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                db.Configuration.ValidateOnSaveEnabled = false;
                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }

                    artist.Photo = bytes;
                }

                db.Artists.Add(artist);
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                return RedirectToAction("Artists", "Admin");
            }

            return View(artist);
        }

        // GET: Admin/EditArtist/5
        [Authorize(Roles = "admin")]
        public ActionResult EditArtist(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }
            return View(artist);
        }

        // POST: Admin/EditArtist
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public ActionResult EditArtist(Artist artist, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }
                    artist.Photo = bytes;
                }

                db.Entry(artist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Artists", "Admin");
            }
            return View(artist);
        }

        // GET: Admin/Musicians/5
        [Authorize(Roles = "admin")]
        public ActionResult Musicians(int? id)
        {
            ViewBag.Current = "Artists";
            var artist = db.Artists.Where(s => s.ArtistID == id).FirstOrDefault();
            ViewBag.Artist = artist.Name;
            ViewBag.ArtistID = artist.ArtistID;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var musician = db.Musicians.Where(s => s.ArtistID == id);
            return View(musician.ToList());
        }

        // POST: Admin/DeleteMusicians/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMusician(int id, int artistID)
        {
            Musician musician = db.Musicians.Find(id);
            db.Musicians.Remove(musician);
            db.SaveChanges();
            return RedirectToAction("Musicians", "Admin", new { id = artistID });
        }

        // GET: Admin/AddMusicians/5
        [Authorize(Roles = "admin")]
        public ActionResult AddMusician(int id)
        {
            ViewBag.Current = "Artists";
            var artist = db.Artists.Where(s => s.ArtistID == id).FirstOrDefault();
            ViewBag.Artist = artist.Name;
            ViewBag.ArtistID = artist.ArtistID;
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddMusicians/5
        public ActionResult AddMusician(int id, Musician musician)
        {
            if (ModelState.IsValid)
            {
                musician.ArtistID = id;
                db.Musicians.Add(musician);
                db.SaveChanges();
                return RedirectToAction("Musicians", "Admin", new { id = id });
            }

            ViewBag.Current = "Artists";
            var artist = db.Artists.Where(s => s.ArtistID == id).FirstOrDefault();
            ViewBag.Artist = artist.Name;
            ViewBag.ArtistID = artist.ArtistID;
            return View(musician);
        }

        // GET: Admin/Albums
        [Authorize(Roles = "admin")]
        public ActionResult Albums()
        {
            ViewBag.Current = "Albums";
            return View(db.Albums.ToList());
        }

        // POST: Admin/DeleteAlbum/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAlbum(int id)
        {
            Album album = db.Albums.Find(id);
            db.Albums.Remove(album);
            db.SaveChanges();
            return RedirectToAction("Albums", "Admin");
        }

        // GET: Admin/AddAlbum
        [Authorize(Roles = "admin")]
        public ActionResult AddAlbum()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddAlbum
        public ActionResult AddAlbum(Album album, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                db.Configuration.ValidateOnSaveEnabled = false;
                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }

                    album.Cover = bytes;
                }

                db.Albums.Add(album);
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                return RedirectToAction("Albums", "Admin");
            }

            return View(album);
        }

        // GET: Admin/EditAlbum/5
        [Authorize(Roles = "admin")]
        public ActionResult EditAlbum(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            return View(album);
        }

        // POST: Admin/EditAlbum
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAlbum(Album album, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                byte[] bytes;

                if (postedFile != null)
                {
                    using (BinaryReader br = new BinaryReader(postedFile.InputStream))
                    {
                        bytes = br.ReadBytes(postedFile.ContentLength);
                    }
                    album.Cover = bytes;
                }

                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Albums", "Admin");
            }
            return View(album);
        }

        // GET: Admin/AlbumTracks/5
        [Authorize(Roles = "admin")]
        public ActionResult AlbumTracks(int? id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var track = db.Tracks.Where(s => s.AlbumID == id);
            return View(track.ToList());
        }

        // POST: Admin/DeleteAlbumTrack/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAlbumTrack(int id, int albumID)
        {
            Track track = db.Tracks.Find(id);
            db.Tracks.Remove(track);
            db.SaveChanges();
            return RedirectToAction("AlbumTracks", "Admin", new { id = albumID });
        }

        // GET: Admin/AddAlbumTrack/5
        [Authorize(Roles = "admin")]
        public ActionResult AddAlbumTrack(int id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddAlbumTrack/5
        public ActionResult AddAlbumTrack(int id, Track track)
        {
            if (ModelState.IsValid)
            {
                track.AlbumID = id;
                db.Tracks.Add(track);
                db.SaveChanges();
                return RedirectToAction("AlbumTracks", "Admin", new { id = id });
            }

            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            return View(track);
        }

        // GET: Admin/AlbumArtists/5
        [Authorize(Roles = "admin")]
        public ActionResult AlbumArtists(int? id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var artist = db.Artists.Where(s => s.Albums.Any(t => t.AlbumID == id));
            return View(artist.ToList());
        }

        // POST: Admin/DeleteAlbumArtist/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAlbumArtist(int id, int albumID)
        {
            Artist artist = db.Artists.Find(id);
            Album album = db.Albums.Find(albumID);
            album.Artists.Remove(artist);
            db.SaveChanges();
            return RedirectToAction("AlbumArtists", "Admin", new { id = albumID });
        }

        // GET: Admin/AddAlbumArtist/5
        [Authorize(Roles = "admin")]
        public ActionResult AddAlbumArtist(int id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            ViewBag.List = new SelectList(db.Artists, "ArtistID", "Name");
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddAlbumArtist/5
        public ActionResult AddAlbumArtist(int id, FormCollection form)
        {
            Album album = db.Albums.Find(id);

            if (ModelState.IsValid)
            {
                string selected = form["selectedValue"].ToString();

                if(String.IsNullOrEmpty(selected))
                {
                    ModelState.AddModelError("EmptyField", "Pole nie może być puste.");
                    return View();
                }
                    
                int artistID = int.Parse(selected);
                Artist artist = db.Artists.Find(artistID);
                album.Artists.Add(artist);
                db.SaveChanges();
                return RedirectToAction("AlbumArtists", "Admin", new { id = id });
            }

            ViewBag.Current = "Albums";
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            return View();
        }

        // GET: Admin/AlbumGenres/5
        [Authorize(Roles = "admin")]
        public ActionResult AlbumGenres(int? id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var genre = db.Genres.Where(s => s.Albums.Any(t => t.AlbumID == id));
            return View(genre.ToList());
        }

        // POST: Admin/DeleteAlbumGenre/5
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAlbumGenre(int id, int albumID)
        {
            Genre genre = db.Genres.Find(id);
            Album album = db.Albums.Find(albumID);
            album.Genres.Remove(genre);
            db.SaveChanges();
            return RedirectToAction("AlbumGenres", "Admin", new { id = albumID });
        }

        // GET: Admin/AddAlbumGenre/5
        [Authorize(Roles = "admin")]
        public ActionResult AddAlbumGenre(int id)
        {
            ViewBag.Current = "Albums";
            var album = db.Albums.Where(s => s.AlbumID == id).FirstOrDefault();
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            ViewBag.List = new SelectList(db.Genres, "GenreID", "Name");
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/AddAlbumGenre/5
        public ActionResult AddAlbumGenre(int id, FormCollection form)
        {
            Album album = db.Albums.Find(id);

            if (ModelState.IsValid)
            {
                string selected = form["selectedValue"].ToString();

                if (String.IsNullOrEmpty(selected))
                {
                    ModelState.AddModelError("EmptyField", "Pole nie może być puste.");
                    return View();
                }

                int genreID = int.Parse(selected);
                Genre genre = db.Genres.Find(genreID);
                album.Genres.Add(genre);
                db.SaveChanges();
                return RedirectToAction("AlbumGenres", "Admin", new { id = id });
            }

            ViewBag.Current = "Albums";
            ViewBag.Album = album.Title;
            ViewBag.AlbumID = album.AlbumID;
            return View();
        }

        [NonAction]
        public bool EmailExist(string Email)
        {
            var person = db.Users.Where(a => a.Email == Email).FirstOrDefault();
            return person != null;
        }
    }
}
