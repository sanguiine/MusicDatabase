namespace MusicDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Password
    {
        [Required(ErrorMessage = "Pole jest wymagane.")]
        public string Text { get; set; }
    }
}
