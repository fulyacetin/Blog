using BlogOdev.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogOdev.Models
{
    public class PostViewModel
    {
        public Post Post { get; set; }

        public IList<Image> PostImages { get; set; }
    }
}