using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogOdev.Core.Objects
{
    public class Image
    {
        [Required(ErrorMessage = "Id: Field is required")]
        public virtual int Id { get; set; }

        public virtual Post Post { get; set; }

        [Required(ErrorMessage = "Image: Field is required")]
        [StringLength(500, ErrorMessage = "Image: Length should not exceed 500 characters")]
        public virtual string ImagePath { get; set; }
    }
}
