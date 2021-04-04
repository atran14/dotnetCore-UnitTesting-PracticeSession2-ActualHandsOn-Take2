using System;
using System.Collections.Generic;
using System.Linq;
using EFProject.Models;
using Microsoft.EntityFrameworkCore;

namespace EFProject.BusinessLogic
{
    public class PostService
    {
        private readonly BloggingDbContext _context;

        public PostService(BloggingDbContext context)
        {
            _context = context;
        }

        public int Add(
            string title,
            string content,
            string url
        )
        {
            if (title == null || url == null)
                return 0;
            if (title.Count() == 0 || url.Count() == 0)
                return 0;

            var blog = _context.Blogs
                .Where(b => b.Url == url).Single();
            var post = new Post
            {
                Title = title,
                Content = content,
                BlogId = blog.BlogId,
                Blog = blog
            };
            _context.Posts.AddRangeAsync(post);
            _context.SaveChanges();
            return 1;
        }

        public IEnumerable<Post> Find(string term)
        {
            return _context.Posts
                .Include(b => b.Blog)
                .Where(b => b.Title.Contains(term))
                .ToList();
        }

        public IEnumerable<Post> GetAll()
        {
            return _context.Posts
                .Include(b => b.Blog)
                .OrderBy(b => b.Title)
                .ToList();
        }

        public int DeleteByTitle(string title)
        {
            if (title == null || title.Count() == 0)
                return 0;
            
            var postQueryResult = _context.Posts
                .Where(p => p.Title.Equals(title));
            if (postQueryResult.Count() < 1)
                return 0;

            var postToDelete = postQueryResult.Single();
            _context.Posts.Remove(postToDelete);
            _context.SaveChanges();
            return 1;
        }

        public void UpdateTitle(
            string title,
            string newTitle
        )
        {
            if (title == null || title.Count() == 0)
                throw new InvalidOperationException("Invalid title name");
            if (newTitle == null || newTitle.Count() == 0)
                throw new InvalidOperationException("Invalid title name");
            if (_context.Posts
                .Where(p => p.Title == title)
                .Count() < 1)
                throw new InvalidOperationException("Post with that title could not be found");

            var targetPost = _context.Posts
                .Where(p => p.Title.Contains(title))
                .Single();
            targetPost.Title = newTitle;
            _context.SaveChanges();

        }
        public void UpdateContent(
            string title,
            string newContent
        )
        {
            if (title == null || title.Count() == 0)
                throw new InvalidOperationException("Invalid title");
            if (_context.Posts
            .Where(p => p.Title == title)
            .Count() < 1)
                throw new InvalidOperationException("Post with that title could not be found");

            var targetPost = _context.Posts
                .Where(p => p.Title.Contains(title))
                .Single();
            targetPost.Content = newContent;
            _context.SaveChanges();
        }
        public void UpdateUrl(
            string title,
            string newUrl
        )
        {
            if (title == null || title.Count() == 0)
                throw new InvalidOperationException("Invalid title");
            if (newUrl == null || newUrl.Count() == 0)
                throw new InvalidOperationException("Invalid new URL");

            var newBlogQuery = _context.Blogs
                .Where(b => b.Url == newUrl);            
            if (newBlogQuery.Count() < 1)
                throw new InvalidOperationException("New URL does not already exist in the blog database. Please add it first");

            var newBlog = newBlogQuery.Single();
            var targetPost = _context.Posts
                .Where(p => p.Title.Contains(title))
                .Single();            

            targetPost.Blog = newBlog;
            targetPost.BlogId = newBlog.BlogId;
            _context.SaveChanges();
        }
    }

}