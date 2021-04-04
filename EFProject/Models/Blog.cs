using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFProject.Models
{
    [Table("Blog")]
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; } = new List<Post>();

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            Blog other = (Blog) obj;
            return
                this.BlogId == other.BlogId
                && this.Url == other.Url
                && this.Posts.Equals(other.Posts);
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            // throw new System.NotImplementedException();
            return base.GetHashCode();
        }
    }
}