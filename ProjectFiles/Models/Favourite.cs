//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Favourite
    {
        public int FavouriteID { get; set; }
        public int UserID { get; set; }
        public int ArtistID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:g}")]
        public System.DateTime Date { get; set; } = DateTime.Now;

        public virtual Artist Artist { get; set; }
        public virtual User User { get; set; }
    }
}
