using Microsoft.AspNetCore.Diagnostics;
using MusicDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;

namespace MusicDB.Controllers
{
    public class HomeController : Controller
    {
        private MusicDBEntities db = new MusicDBEntities();

        public ActionResult Index()
        {
            var albums = db.Albums.OrderByDescending(s => s.ReleaseDate).Take(8);
            return View(albums);
        }
    }
}