#region Usings
using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BlogOdev.Core;
using BlogOdev.Core.Objects;
using BlogOdev.Models;
#endregion

namespace BlogOdev.Controllers
{
    /// <summary>
    /// Home controller that contain actions to return list/view pages and others.
    /// </summary>
    [AllowAnonymous]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogRepository;

        public BlogController(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        /// <summary>
        /// Return the list page with latest posts.
        /// </summary>
        /// <param name="p">pagination number</param>
        /// <returns></returns>
        public ViewResult Posts(int p = 1)
        {
            var viewModel = new ListViewModel(_blogRepository, p);
            ViewBag.Title = "Son Yazılar";
            return View("List", viewModel);
        }

        /// <summary>
        /// Return a particular post based on the puslished year, month and url slug.
        /// </summary>
        /// <param name="year">Published year</param>
        /// <param name="month">Published month</param>
        /// <param name="title">Url slug</param>
        /// <returns></returns>
        public ViewResult Post(int year, int month, string title)
        {
            var post = _blogRepository.Post(year, month, title);

            if (post == null)
                throw new HttpException(404, "Yazı Bulunamadı!");

            if (post.Published == false && User.Identity.IsAuthenticated == false)
                throw new HttpException(401, "Yazı yayınlanmadı!");

            var images = _blogRepository.ImagesForPost(post.Id);

            var viewModel = new PostViewModel
            {
                Post = post,
                PostImages = images
            };

            return View(viewModel);
        }

        /// <summary>
        /// Return the posts belongs to a category.
        /// </summary>
        /// <param name="category">Url slug</param>
        /// <param name="p">Pagination number</param>
        /// <returns></returns>
        public ViewResult Category(string category, int p = 1)
        {
            var viewModel = new ListViewModel(_blogRepository, category, "Category", p);

            if (viewModel.Category == null)
                throw new HttpException(404, "Kategori bulunamadı");

            ViewBag.Title = String.Format(@"""{0}"" kategorisi üzerindeki son mesajlar", viewModel.Category.Name);
            return View("List", viewModel);
        }

        /// <summary>
        /// Return the posts belongs to a tag.
        /// </summary>
        /// <param name="tag">Url slug</param>
        /// <param name="p">Pagination number</param>
        /// <returns></returns>
        public ViewResult Tag(string tag, int p = 1)
        {
            var viewModel = new ListViewModel(_blogRepository, tag, "Tag", p);

            if (viewModel.Tag == null)
                throw new HttpException(404, "Etiketi bulunamadı");

            ViewBag.Title = String.Format(@"""{0}"" etiketi üzerindeki son mesajlar", viewModel.Tag.Name);
            return View("List", viewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="author"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ViewResult Author(string author, int p = 1)
        {
            var viewModel = new ListViewModel(_blogRepository, author, "Author", p);

            ViewBag.Title = String.Format(@"""{0}"" yazar mesajları", viewModel.Author);

            return View("List", viewModel);
        }

        /// <summary>
        /// Return the posts that matches the search text.
        /// </summary>
        /// <param name="s">search text</param>
        /// <param name="p">Pagination number</param>
        /// <returns></returns>
        public ViewResult Search(string s, int p = 1)
        {
            ViewBag.Title = String.Format(@"Arama metni mesajları ""{0}""", s);

            var viewModel = new ListViewModel(_blogRepository, s, "Search", p);
            return View("List", viewModel);
        }

        /// <summary>
        /// Child action that returns the sidebar partial view.
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult Sidebars()
        {
            var widgetViewModel = new WidgetViewModel(_blogRepository);
            return PartialView("_Sidebars", widgetViewModel);
        }

        /// <summary>
        /// Generate and return RSS feed.
        /// </summary>
        /// <returns></returns>
        public ActionResult Feed()
        {
            var blogTitle = ConfigurationManager.AppSettings["BlogTitle"];
            var blogDescription = ConfigurationManager.AppSettings["BlogDescription"];
            var blogUrl = ConfigurationManager.AppSettings["BlogUrl"];

            var posts = _blogRepository.Posts(0, 25).Select
            (
                p => new SyndicationItem
                    (
                        p.Title,
                        p.Description,
                        new Uri(string.Concat(blogUrl, p.Href(Url)))
                    )
            );

            var feed = new SyndicationFeed(blogTitle, blogDescription, new Uri(blogUrl), posts)
            {
                Copyright = new TextSyndicationContent(String.Format("Copyright © {0}", blogTitle)),
                Language = "tr-TR"
            };

            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}
