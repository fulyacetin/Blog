
using BlogOdev.Core.Objects;
using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BlogOdev
{
    public static class ActionLinkExtensions
    {
        public static MvcHtmlString PostLink(this HtmlHelper helper, Post post)
        {
            return helper.ActionLink(post.Title, "Post", "Blog", new { year = post.PostedOn.Year, month = post.PostedOn.Month, title = post.UrlSlug }, new { title = post.Title });
        }

        public static MvcHtmlString CategoryLink(this HtmlHelper helper, Category category)
        {
            return helper.ActionLink(category.Name, "Category", "Blog", new { category = category.UrlSlug }, new { title = String.Format("{0} kategorisine bağlı tüme yazıları listele", category.Name) });
        }

        public static MvcHtmlString AuthorLink(this HtmlHelper helper, string author)
        {
            return helper.ActionLink(author, "Author", "Blog", new { author = author }, new { title = String.Format("{0} yazarına bağlı tüme yazıları listele", author) });
        }

        public static MvcHtmlString TagLink(this HtmlHelper helper, Tag tag)
        {
            return helper.ActionLink(tag.Name, "Tag", "Blog", new { tag = tag.UrlSlug }, new { title = String.Format("{0} etiketine bağlı tüme yazıları listele", tag.Name) });
        }
    }
}