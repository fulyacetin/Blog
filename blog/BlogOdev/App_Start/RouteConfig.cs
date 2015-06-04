using System.Web.Mvc;
using System.Web.Routing;

namespace BlogOdev
{
  public class RouteConfig
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Admin", action = "FileUpload" });

      routes.MapRoute(
        "Post",
        "Archive/{year}/{month}/{title}",
        new { controller = "Blog", action = "Post" }
      );

      routes.MapRoute(
        "Archive",
        "Archive/{year}/{month}",
        new { controller = "Blog", action = "Archive", year = UrlParameter.Optional, month = UrlParameter.Optional }
      );

      routes.MapRoute(
        "Category",
        "Category/{category}",
        new { controller = "Blog", action = "Category" }
      );

      routes.MapRoute(
        "Tag",
        "Tag/{tag}",
        new { controller = "Blog", action = "Tag" }
      );

      routes.MapRoute(
      "Author",
      "Author/{author}",
      new { controller = "Blog", action = "Author" }
    );

      routes.MapRoute(
        "Login",
        "Login",
        new { controller = "Admin", action = "Login" }
      );

      routes.MapRoute(
        "Logout",
        "Logout",
        new { controller = "Admin", action = "Logout" }
      );

      routes.MapRoute(
        "Manage",
        "Manage",
        new { controller = "Admin", action = "Manage" }
      );

      

      routes.MapRoute(
        "AdminAction",
        "Admin/{action}",
        new { controller = "Admin", action = "Login" }
      );

      routes.MapRoute(
        "Action",
        "{action}",
        new { controller = "Blog", action = "Posts" }
      );

     
    }
  }
}