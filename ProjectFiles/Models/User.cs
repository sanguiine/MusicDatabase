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

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Comments = new HashSet<Comment>();
            this.Favourites = new HashSet<Favourite>();
            this.Ratings = new HashSet<Rating>();
            this.Reviews = new HashSet<Review>();
        }

        public int UserID { get; set; }
        [Required(ErrorMessage = "Pole jest wymagane.")]
        [EmailAddress(ErrorMessage = "Niepoprawny adres email.")]
        [StringLength(50, ErrorMessage = "Adres email nie mo�e przekracza� 50 znak�w.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Pole jest wymagane.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Pole jest wymagane.")]
        [StringLength(50, ErrorMessage = "Nazwa konta nie mo�e przekracza� 50 znak�w.")]
        public string Name { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Required(ErrorMessage = "Pole jest wymagane.")]
        public System.DateTime Birthdate { get; set; }
        [Required(ErrorMessage = "Pole jest wymagane.")]
        public string Type { get; set; } = "user";
        public byte[] Avatar { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Favourite> Favourites { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rating> Ratings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
