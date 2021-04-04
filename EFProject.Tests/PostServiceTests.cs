using System;
using System.Collections.Generic;
using System.Linq;
using EFProject.BusinessLogic;
using EFProject.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EFProject.Tests
{
    [TestFixture]
    public class PostServiceTests
    {
        private DbContextOptions<BloggingDbContext> _contextOptions;

        [OneTimeSetUp]
        public void SetupOnce()
        {
            _contextOptions = new DbContextOptionsBuilder<BloggingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [SetUp]
        public void Setup()
        {
            SeedData();
        }

        [TearDown]
        public void TearDown()
        {
            QuickDumpAllTables();
        }

        [Test]
        public void Add_ValidInputData_ShouldSucceed()
        {
            Blog blog = new Blog
            {
                Url = "www.instagram.com"
            };
            Post post = new Post
            {
                Title = "How summer activities changed how we think about death",
                Content = "lksahfpwqohavskjlsbatuwqhaps;vddsah",
                Blog = blog
            };
            int beforeInsertCount;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                dbContext.Blogs.Add(blog);
                dbContext.SaveChangesAsync();
                beforeInsertCount = dbContext.Posts.Count();
            }

            int result;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                result = service.Add(post.Title, post.Content, blog.Url);
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                Assert.AreEqual(1, result);
                Assert.AreEqual(beforeInsertCount + 1, dbContext.Posts.Count());
                var newlyAddedPost = dbContext.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title.Contains("How summer activities changed how we think about death"))
                    .Single();
                Assert.AreEqual(post.Title, newlyAddedPost.Title);
                Assert.AreEqual(post.Content, newlyAddedPost.Content);
                Assert.IsNotNull(newlyAddedPost.Blog);
                Assert.AreEqual(post.Blog.Url, newlyAddedPost.Blog.Url);
            }
        }

        [Test]
        public void Add_EmptyTitle_ShouldFail()
        {
            Blog twitter;
            Post post;
            int beforeInsertCount;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                bool hasTwitter = dbContext.Blogs
                    .Where(b => b.Url.Contains("twitter"))
                    .Count() > 0;
                if (!hasTwitter)
                {
                    dbContext.Blogs.Add(new Blog
                    {
                        Url = "www.twitter.com"
                    });
                    dbContext.SaveChanges();
                }
                twitter = dbContext.Blogs
                    .Where(b => b.Url.Contains("twitter"))
                    .Single();

                post = new Post
                {
                    Title = "",
                    Content = "lahfpwqohavskjlsbatuwqhaps;vddsah",
                    Blog = twitter
                };
                beforeInsertCount = dbContext.Posts.Count();
            }

            int result;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                result = service.Add(post.Title, post.Content, twitter.Url);
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                Assert.AreEqual(0, result);
                Assert.AreEqual(beforeInsertCount, dbContext.Posts.Count());
            }
        }

        [TestCase("things")]
        [TestCase("boss")]
        public void Find_Always_ShouldReturnCorrectList(string term)
        {
            IEnumerable<Post> expectedList;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                expectedList = dbContext.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title.Contains(term))
                    .ToList();
            }

            IEnumerable<Post> returnedList;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                returnedList = service.Find(term);
            }

            Assert.AreEqual(expectedList.Count(), returnedList.Count());
            foreach (var expectedPost in expectedList)
            {
                var correspondingActualPost =
                    returnedList
                    .Where(p => p.PostId == expectedPost.PostId)
                    .Single();

                Assert.AreEqual(expectedPost.Title, correspondingActualPost.Title);
                Assert.AreEqual(expectedPost.Content, correspondingActualPost.Content);
                Assert.AreEqual(expectedPost.Blog.Url, correspondingActualPost.Blog.Url);
            }
        }

        [Test]
        public void GetAll_Always_ShouldReturnTheFullList()
        {
            IEnumerable<Post> expectedFullList;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                expectedFullList = dbContext.Posts
                    .Include(p => p.Blog)
                    .ToList();
            }

            IEnumerable<Post> actualFullList;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                actualFullList = service.GetAll();
            }

            Assert.AreEqual(expectedFullList.Count(), actualFullList.Count());
            foreach (var expectedPost in expectedFullList)
            {
                var correspondingActualPost =
                    actualFullList
                    .Where(p => p.PostId == expectedPost.PostId)
                    .Single();

                Assert.AreEqual(expectedPost.Title, correspondingActualPost.Title);
                Assert.AreEqual(expectedPost.Content, correspondingActualPost.Content);
                Assert.IsNotNull(correspondingActualPost.Blog);
                Assert.AreEqual(expectedPost.Blog.Url, correspondingActualPost.Blog.Url);
            }
        }

        [Test]
        public void UpdateTitle_Always_ShouldSucceed()
        {
            Blog facebook;
            Post post;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                facebook = dbContext.Blogs
                    .Where(b => b.Url.Contains("facebook"))
                    .Single();
                post = new Post
                {
                    Title = "Why game jobs are killing you",
                    Content = "ouvaisdhvsa",
                    Blog = facebook
                };
                dbContext.Posts.AddRange(post);
                dbContext.SaveChangesAsync();
            }

            string newTitle = "Why hybrid supercars are afraid of the truth";
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                service.UpdateTitle(
                    post.Title,
                    newTitle
                );
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                var updatedPost = dbContext.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title == newTitle)
                    .Single();
                Assert.AreEqual(post.Content, updatedPost.Content);
                Assert.AreEqual(post.PostId, updatedPost.PostId);
                Assert.AreEqual(post.BlogId, updatedPost.BlogId);
                Assert.AreEqual(post.Blog.Url, updatedPost.Blog.Url);
            }
        }

        [Test]
        public void UpdateTitle_OriginalTitleCannotBeFound_ShouldFail()
        {
            string nonexistentOldTitle = "avdsoiblnksdaigfuv";
            string newTitle = "Why hybrid supercars are afraid of the truth";

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);

                Assert.Throws(
                    typeof(InvalidOperationException),
                    () => service.UpdateTitle(nonexistentOldTitle, newTitle));
            }
        }

        [TestCase("")]
        [TestCase(null)]
        public void UpdateTitle_InvalidNewTitle_ShouldFail(string invalidNewTitle)
        {
            Post existingPost;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                existingPost = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid().ToString())
                    .FirstOrDefault();
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);

                Assert.Throws(
                    typeof(InvalidOperationException),
                    () => service.UpdateTitle(existingPost.Title, invalidNewTitle));
            }
        }

        [Test]
        public void UpdateContent_Always_ShouldSucceed()
        {
            Post existingPost;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                existingPost = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid())
                    .First();
            }
            var oldContent = existingPost.Content;
            var newContent = "vauprewhfsoivdlhbgreyfads;ivjnr";

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                service.UpdateContent(existingPost.Title, newContent);
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                var updatedPost = dbContext.Posts
                    .Where(p => p.PostId == existingPost.PostId)
                    .Single();
                Assert.AreNotEqual(oldContent, updatedPost.Content);
                Assert.AreEqual(newContent, updatedPost.Content);
            }
        }

        [TestCase("")]
        [TestCase(null)]
        public void UpdateContent_InvalidNewTitle_ShouldFail(string invalidNewTitle)
        {
            var newContent = "vauprewhfsoivdlhbgreyfads;ivjnr";

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);

                Assert.Throws(
                    typeof(InvalidOperationException),
                    () => service.UpdateContent(invalidNewTitle, newContent));
            }
        }

        [Test]
        public void UpdateContent_TitleCannotBeFound_ShouldFail()
        {
            var nonexistentTitle = "NonExistentTitle";
            var newContent = "vauprewhfsoivdlhbgreyfads;ivjnr";

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                Assert.Throws(typeof(InvalidOperationException),
                    () => service.UpdateContent(nonexistentTitle, newContent));
            }
        }

        [Test]
        public void UpdateUrl_Always_ShouldSucceed()
        {
            Blog instagram = new Blog
            {
                Url = "www.instagram.com"
            };
            Post existingPost;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                dbContext.Blogs.Add(instagram);
                dbContext.SaveChangesAsync();
                existingPost = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid().ToString())
                    .First();
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                service.UpdateUrl(
                    existingPost.Title,
                    instagram.Url
                );
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                var updatedPost = dbContext.Posts
                    .Include(p => p.Blog)
                    .Where(p => p.Title == existingPost.Title)
                    .Single();
                Assert.AreEqual(existingPost.Content, updatedPost.Content);
                Assert.AreEqual(existingPost.PostId, updatedPost.PostId);
                Assert.AreEqual(instagram.Url, updatedPost.Blog.Url);
            }
        }

        [TestCase("")]
        [TestCase(null)]
        public void UpdateUrl_InvalidUrl_ShouldFail(string invalidUrl)
        {
            Post existingPost;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                existingPost = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid().ToString())
                    .FirstOrDefault();
            }

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);

                Assert.Throws(
                    typeof(InvalidOperationException),
                    () => service.UpdateUrl(existingPost.Title, invalidUrl));
            }
        }

        [Test]
        public void UpdateUrl_BlogWithInputUrlDoesNotExistInOtherDatabase_ShouldFail()
        {
            Post existingPost;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                existingPost = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid().ToString())
                    .FirstOrDefault();
            }

            var nonexistentNewUrl = "localhost";

            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);

                Assert.Throws(
                    typeof(InvalidOperationException),
                    () => service.UpdateUrl(existingPost.Title, nonexistentNewUrl));
            }
        }

        [Test]
        public void DeleteByTitle_TitleExists_ShouldSucceed()
        {
            Post targetPostToDelete;
            int oldCount;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                targetPostToDelete = dbContext.Posts
                    .OrderBy(p => Guid.NewGuid().ToString())
                    .First();
                oldCount = dbContext.Posts
                    .Count();
            }

            int result;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                result = service.DeleteByTitle(targetPostToDelete.Title);
            }

            Assert.AreEqual(1, result);
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                int newCount = dbContext.Posts.Count();
                Assert.AreEqual(oldCount - 1, newCount);
                var afterDeletionQuery = dbContext.Posts
                    .Where(p => p.Title.Equals(targetPostToDelete.Title));
                Assert.True(afterDeletionQuery.Count() == 0);
            }
        }

        [TestCase("")]
        [TestCase(null)]
        public void DeleteByTitle_InvalidTitle_ShouldFailAndNotDeleteAnything(string invalidTitle)
        {
            IEnumerable<Post> beforeList;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                beforeList = dbContext.Posts
                    .ToList();
            }

            int result;
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                PostService service = new PostService(dbContext);
                result = service.DeleteByTitle(invalidTitle);
            }

            Assert.AreEqual(0, result);
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                IEnumerable<Post> afterList = dbContext.Posts
                    .ToList();

                Assert.AreEqual(beforeList.Count(), afterList.Count());
                foreach (var post in beforeList)
                {
                    Assert.True(afterList.Contains(post));
                }
            }
        }

        private void SeedData()
        {
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                dbContext.Database.EnsureCreatedAsync();

                Blog google = new Blog { Url = "www.google.com" };
                Blog facebook = new Blog { Url = "www.facebook.com" };
                Blog lttStore = new Blog { Url = "www.lttstore.com" };

                //adding blogs
                dbContext.Blogs.AddRangeAsync(
                    google,
                    facebook,
                    lttStore
                );

                dbContext.Posts.AddRangeAsync(
                    new Post
                    {
                        Title = "How to be unpopular in the business insurance world",
                        Content = "fslahfsdakjfds",
                        Blog = google
                    },
                    new Post
                    {
                        Title = "Why your accessory never works out the way you plan",
                        Content = "fslahfsdakjfds",
                        Blog = google
                    },
                    new Post
                    {
                        Title = "The 17 worst songs about special olympic world games",
                        Content = "fslahfsdakjfds",
                        Blog = google
                    },
                    new Post
                    {
                        Title = "The 8 worst home tech gadgets in history",
                        Content = "fslahfsdakjfds",
                        Blog = facebook
                    },
                    new Post
                    {
                        Title = "14 things your boss expects you know about football highlights",
                        Content = "fslahfsdakjfds",
                        Blog = facebook
                    },
                    new Post
                    {
                        Title = "10 things your boss expects you know about popular songs",
                        Content = "fslahfsdakjfds",
                        Blog = facebook
                    },
                    new Post
                    {
                        Title = "Why you'll never succeed at vaccination schedules",
                        Content = "fslahfsdakjfds",
                        Blog = lttStore
                    },
                    new Post
                    {
                        Title = "Why celebrity cruises will change your life",
                        Content = "fslahfsdakjfds",
                        Blog = lttStore
                    },
                    new Post
                    {
                        Title = "Why hairstyles are killing you",
                        Content = "fslahfsdakjfds",
                        Blog = lttStore
                    }
                );
                dbContext.SaveChangesAsync();
            }
        }

        private void QuickDumpAllTables()
        {
            using (var dbContext = new BloggingDbContext(_contextOptions))
            {
                dbContext.Database.EnsureDeletedAsync();
            }
        }
    }
}