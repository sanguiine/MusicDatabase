using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.Ajax.Utilities;
using MusicDB.Models;
using PagedList;
using WebGrease.Css.Extensions;

namespace MusicDB.Controllers
{
    public class MusicController : Controller
    {
        private MusicDBEntities db = new MusicDBEntities();

        // GET: Music/Ranking
        public ActionResult Ranking(string GenreID, string YearID)
        {
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "Name", GenreID);
            ViewBag.YearID = new SelectList(Enumerable.Range(1950, (DateTime.Now.Year - 1950) + 1));

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

            if (!String.IsNullOrWhiteSpace(GenreID))
            {
                int id = int.Parse(GenreID);
                rank = rank.Where(s => s.Genres.Any(t => t.GenreID == id));
            }

            if (!String.IsNullOrWhiteSpace(YearID))
            {
                DateTime year;
                DateTime.TryParseExact(YearID, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out year);
                rank = rank.Where(s => s.ReleaseDate.Year == year.Year);
            }

            rank = rank.OrderByDescending(s => s.Average);
            return View(rank.ToList());
        }

        // GET: Music/Search
        public ActionResult Search(string searchString, string currentFilter, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchString))
            {
                var searchAlbums = db.Albums.Where(s => s.Title.Contains(searchString)).OrderByDescending(s => s.ReleaseDate);
                return View(searchAlbums.ToPagedList(pageNumber, pageSize));
            }

            var albums = db.Albums.OrderByDescending(s => s.ReleaseDate);
            return View(albums.ToPagedList(pageNumber, pageSize));
        }

        // GET: Music/SearchArtists
        public ActionResult SearchArtists(string searchString, string currentFilter, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchString))
            {
                var searchArtists = db.Artists.Where(s => s.Name.Contains(searchString)).OrderByDescending(s => s.Name);
                return View(searchArtists.ToPagedList(pageNumber, pageSize));
            }

            var artists = db.Artists.OrderByDescending(s => s.Name);
            return View(artists.ToPagedList(pageNumber, pageSize));
        }

        // GET: Music/Artist/5
        public ActionResult Artist(int? id)
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

            if (Session["userID"] != null)
            {
                string str = Session["userID"].ToString();
                int userID = Int32.Parse(str);
                var fav = db.Favourites.Where(s => s.ArtistID == id && s.UserID == userID);
                
                if(fav.FirstOrDefault() == null)
                {
                    ViewBag.Avaliable = "true";
                }
                else
                {
                    ViewBag.Avaliable = "false";
                }
            }

            return View(artist);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Music/ArtistFav/5
        public ActionResult ArtistFav(int id)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int userID = Int32.Parse(str);
            Favourite favourite = new Favourite();
            favourite.UserID = userID;
            favourite.ArtistID = id;
            db.Favourites.Add(favourite);
            db.SaveChanges();
            return RedirectToAction("Artist", "Music", new { id = id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Music/ArtistUnFav/5
        public ActionResult ArtistUnFav(int id)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int userID = Int32.Parse(str);
            var favourite = db.Favourites.Where(s => s.ArtistID == id && s.UserID == userID).FirstOrDefault();
            db.Favourites.Remove(favourite);
            db.SaveChanges();
            return RedirectToAction("Artist", "Music", new { id = id });
        }

        // GET: Music/Album/5
        public ActionResult Album(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Album album = db.Albums.Find(id);

            if (Session["userID"] != null)
            {
                string str = Session["userID"].ToString();
                int userID = Int32.Parse(str);
                Rating rating = db.Ratings.Where(s => s.AlbumID == id & s.UserID == userID).FirstOrDefault();

                if(rating != null)
                {
                    ViewBag.Rate = rating.Rating1;
                }
            }

            if (album == null)
            {
                return HttpNotFound();
            }

            return View(album);
        }

        // POST: Music/Rating
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rating(Album album, FormCollection form, String submitButton)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string str = Session["userID"].ToString();
            int userID = Int32.Parse(str);

            switch (submitButton)
            {
                case "AddRate":
                    Rating rating = new Rating();
                    rating.UserID = userID;
                    rating.AlbumID = album.AlbumID;
                    string selected = form["rating"].ToString();
                    rating.Rating1 = Int32.Parse(selected);
                    db.Ratings.Add(rating);
                    db.SaveChanges();
                    return RedirectToAction("Album", "Music", new { id = album.AlbumID });
                case "EditRate":
                    Rating rating2 = db.Ratings.Where(s => s.AlbumID == album.AlbumID & s.UserID == userID).FirstOrDefault();
                    string selected2 = form["rating"].ToString();
                    rating2.Rating1 = Int32.Parse(selected2);
                    db.Entry(rating2).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Album", "Music", new { id = album.AlbumID });
                case "DeleteRate":
                    Rating rating3 = db.Ratings.Where(s => s.AlbumID == album.AlbumID & s.UserID == userID).FirstOrDefault();
                    db.Ratings.Remove(rating3);
                    db.SaveChanges();
                    return RedirectToAction("Album", "Music", new { id = album.AlbumID });
            }

            return RedirectToAction("Album", "Music", new { id = album.AlbumID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Music/Comments
        public ActionResult Comments(Album album, String text)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Comment comment = new Comment();
            string str = Session["userID"].ToString();
            int id = Int32.Parse(str);
            comment.UserID = id;
            comment.AlbumID = album.AlbumID;
            comment.Comment1 = text;
            db.Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("Album", "Music", new { id = album.AlbumID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Music/Reviews
        public ActionResult Reviews(Album album, String text)
        {
            if (Session["userID"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Review review = new Review();
            string str = Session["userID"].ToString();
            int id = Int32.Parse(str);
            review.UserID = id;
            review.AlbumID = album.AlbumID;
            review.Review1 = text;
            db.Reviews.Add(review);
            db.SaveChanges();

            return RedirectToAction("Album", "Music", new { id = album.AlbumID });
        }
    }
}