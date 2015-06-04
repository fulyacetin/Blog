#region Usings
using BlogOdev.Core;
using BlogOdev.Core.Objects;
using BlogOdev.Models;
using BlogOdev.Providers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
#endregion

namespace BlogOdev.Controllers
{
    /// <summary>
    /// Contains actions required for admin.
    /// </summary>
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAuthProvider _authProvider;
        private readonly IBlogRepository _blogRepository;

        public AdminController(IAuthProvider authProvider, IBlogRepository blogRepository = null)
        {
            _authProvider = authProvider;
            _blogRepository = blogRepository;
        }

        /// <summary>
        /// Return the login page.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // If already logged-in redirect the user to manage page.
            if (_authProvider.IsLoggedIn)
                return RedirectToUrl(returnUrl);

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        /// <summary>
        /// Login POST action.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && _authProvider.Login(model.UserName, model.Password))
            {
                return RedirectToUrl(returnUrl);
            }

            ModelState.AddModelError("", "Sağlanan kullanıcı adı veya şifre yanlış.");
            return View(model);
        }

        /// <summary>
        /// Return page to manage posts, categories and tags.
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            return View();
        }

        /// <summary>
        /// Logout the user and return the login page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            _authProvider.Logout();

            return RedirectToAction("Login", "Admin");
        }

        private ActionResult RedirectToUrl(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Manage");
            }
        }

        #region Posts

        /// <summary>
        /// Return the posts based on jqgrid input parameters as JSON.
        /// </summary>
        /// <param name="jqParams"></param>
        /// <returns></returns>
        public ContentResult Posts(JqInViewModel jqParams)
        {
            var posts = _blogRepository.Posts(jqParams.page - 1, jqParams.rows, jqParams.sidx, jqParams.sord == "asc");
            var totalPosts = _blogRepository.TotalPosts(false);

            return Content(JsonConvert.SerializeObject(new
            {
                page = jqParams.page,
                records = totalPosts,
                rows = posts,
                total = Math.Ceiling(Convert.ToDouble(totalPosts) / jqParams.rows)
            }, new CustomDateTimeConverter()), "application/json");
        }

        /// <summary>
        /// Add a new post.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ContentResult AddPost(Post post, FormCollection form)
        {
            string json;

            ModelState.Clear();

            if (TryValidateModel(post))
            {
                post.Author = _authProvider.GetUser().Name;
                var id = _blogRepository.AddPost(post);

                json = JsonConvert.SerializeObject(new
                {
                    id = id,
                    success = true,
                    message = "Mesaj başarıyla eklendi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Yazı eklenemedi."
                });
            }

            return Content(json, "application/json");
        }

        public ActionResult ImagesForPost(int id)
        {
            ViewBag.PostId = id;
            var image = _blogRepository.ImagesForPost(id);

            return View(image);
        }

        [HttpPost]
        public ActionResult AddImage(HttpPostedFileBase yuklenecekDosya,int Id)
        {
            var post = _blogRepository.Post(Id);

            if(yuklenecekDosya!=null)
            {
                string dosyaYolu = Path.GetFileName(yuklenecekDosya.FileName);
                var yuklemeYeri = Path.Combine(Server.MapPath("~/Content/images"), dosyaYolu);

                if(!System.IO.File.Exists(yuklemeYeri))
                    yuklenecekDosya.SaveAs(yuklemeYeri);

                var image = new Image
                {
                    ImagePath = yuklenecekDosya.FileName,
                    Post = post
                };

                _blogRepository.AddImage(image);
            }

            return RedirectToAction("ImagesForPost", new { id = Id });
        }

        public ActionResult DeleteImage(int id)
        {
            var image = _blogRepository.Image(id);

            if(image!=null)
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/images"), image.ImagePath));
                _blogRepository.DeleteImage(id);
            }

            return RedirectToAction("ImagesForPost", new { id = image.Post.Id });
        }

        /// <summary>
        /// Edit an existing post.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ContentResult EditPost(Post post, FormCollection form, HttpPostedFileBase image)
        {
            string json;

            ModelState.Clear();

            if (TryValidateModel(post))
            {
                post.Author = _authProvider.GetUser().Name;
                _blogRepository.EditPost(post);
                json = JsonConvert.SerializeObject(new
                {
                    id = post.Id,
                    success = true,
                    message = "Değişiklikler başarıyla kaydedildi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Değişiklikleri kaydetme başarısız oldu."
                });
            }

            return Content(json, "application/json");
        }

        /// <summary>
        /// Delete an existing post.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult DeletePost(int id)
        {
            _blogRepository.DeletePost(id);

            var json = JsonConvert.SerializeObject(new
            {
                success = true,
                message = "Mesaj başarıyla silindi."
            });

            return Content(json, "application/json");
        }

        #endregion

        #region Categories

        /// <summary>
        /// Return all the categories.
        /// </summary>
        /// <returns></returns>
        public ContentResult Categories()
        {
            var categories = _blogRepository.Categories();

            return Content(JsonConvert.SerializeObject(new
            {
                page = 1,
                records = categories.Count,
                rows = categories,
                total = 1
            }), "application/json");
        }

        /// <summary>
        /// Add new category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult AddCategory([Bind(Exclude = "Id")]Category category)
        {
            string json;

            if (ModelState.IsValid)
            {
                var id = _blogRepository.AddCategory(category);
                json = JsonConvert.SerializeObject(new
                {
                    id = id,
                    success = true,
                    message = "Kategori başarıyla eklendi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Kategori ekleme başarısız oldu."
                });
            }

            return Content(json, "application/json");
        }

        /// <summary>
        /// Edit an existing category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult EditCategory(Category category)
        {
            string json;

            if (ModelState.IsValid)
            {
                _blogRepository.EditCategory(category);
                json = JsonConvert.SerializeObject(new
                {
                    id = category.Id,
                    success = true,
                    message = "Değişiklikler başarıyla kaydedildi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Değişiklikleri kaydetme başarısız oldu."
                });
            }

            return Content(json, "application/json");
        }

        /// <summary>
        /// Delete an existing category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult DeleteCategory(int id)
        {
            _blogRepository.DeleteCategory(id);

            var json = JsonConvert.SerializeObject(new
            {
                success = true,
                message = "Kategori başarıyla silindi."
            });

            return Content(json, "application/json");
        }

        /// <summary>
        /// Return html required to create category dropdown in jQGrid popup.
        /// </summary>
        /// <returns></returns>
        public ContentResult GetCategoriesHtml()
        {
            var categories = _blogRepository.Categories().OrderBy(s => s.Name);

            var sb = new StringBuilder();
            sb.AppendLine(@"<select>");

            foreach (var category in categories)
            {
                sb.AppendLine(string.Format(@"<option value=""{0}"">{1}</option>", category.Id, category.Name));
            }

            sb.AppendLine("<select>");
            return Content(sb.ToString(), "text/html");
        }

        #endregion

        #region Tags

        /// <summary>
        /// Return all the tags as JSON.
        /// </summary>
        /// <returns></returns>
        public ContentResult Tags()
        {
            var tags = _blogRepository.Tags();

            return Content(JsonConvert.SerializeObject(new
            {
                page = 1,
                records = tags.Count,
                rows = tags,
                total = 1
            }), "application/json");
        }

        /// <summary>
        /// Add a new tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult AddTag([Bind(Exclude = "Id")]Tag tag)
        {
            string json;

            if (ModelState.IsValid)
            {
                var id = _blogRepository.AddTag(tag);
                json = JsonConvert.SerializeObject(new
                {
                    id = id,
                    success = true,
                    message = "Etiket başarıyla eklendi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Etiket ekleme başarısız oldu."
                });
            }

            return Content(json, "application/json");
        }

        /// <summary>
        /// Edit an existing tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult EditTag(Tag tag)
        {
            string json;

            if (ModelState.IsValid)
            {
                _blogRepository.EditTag(tag);
                json = JsonConvert.SerializeObject(new
                {
                    id = tag.Id,
                    success = true,
                    message = "Değişiklikler başarıyla kaydedildi."
                });
            }
            else
            {
                json = JsonConvert.SerializeObject(new
                {
                    id = 0,
                    success = false,
                    message = "Değişiklikleri kaydetme başarısız oldu."
                });
            }

            return Content(json, "application/json");
        }

        /// <summary>
        /// Delete an existing tag.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ContentResult DeleteTag(int id)
        {
            _blogRepository.DeleteTag(id);

            var json = JsonConvert.SerializeObject(new
            {
                success = true,
                message = "Etiket başarıyla silindi."
            });

            return Content(json, "application/json");
        }

        /// <summary>
        /// Return html required to create tag dropdown in jQGrid popup.
        /// </summary>
        /// <returns></returns>
        public ContentResult GetTagsHtml()
        {
            var tags = _blogRepository.Tags().OrderBy(s => s.Name);

            var sb = new StringBuilder();
            sb.AppendLine(@"<select multiple=""multiple"">");

            foreach (var tag in tags)
            {
                sb.AppendLine(string.Format(@"<option value=""{0}"">{1}</option>", tag.Id, tag.Name));
            }

            sb.AppendLine("<select>");
            return Content(sb.ToString(), "text/html");
        }

        /// <summary>
        /// Navigate to particular post.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GoToPost(int id)
        {
            var post = _blogRepository.Post(id);
            return RedirectToRoute(new { controller = "Blog", action = "Post", year = post.PostedOn.Year, month = post.PostedOn.Month, title = post.UrlSlug });
        }

        #endregion
    }
}
