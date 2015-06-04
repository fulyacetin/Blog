using BlogOdev.Core.Objects;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogOdev.Core.Mappings
{
   public class ImageMap:ClassMap<Image>
    {
       public ImageMap()
       {
           Id(x => x.Id);
           Map(x => x.ImagePath).Length(50);
           References(x => x.Post).Column("Post").Not.Nullable();
       }
    }
}
