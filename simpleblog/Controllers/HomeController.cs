using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using simpleblog.Posts;

namespace simpleblog.Controllers
{
     public class HomeController : Controller
    {
        // TODO: Make this a setting.
        private const string StorageBucket = "tryinggce.appspot.com";

        private readonly Lazy<PostStore> _postStore;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public HomeController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(HomeController));
            _loggerFactory = loggerFactory;
            _postStore = new Lazy<PostStore>(CreatePostStore);
        }

        private PostStore CreatePostStore()
        {
            return new PostStore(StorageBucket, _loggerFactory);
        }

        public async Task<ActionResult> Index()
        {
            var posts = await _postStore.Value.ListPostsAsync();
            return View(posts);
        }

        [HttpGet]
        public async Task<ActionResult> ShowPost(string id)
        {
            var post = await _postStore.Value.GetPostAsync(id);
            if (post == null)
            {
                _logger.LogError($"No post was read for {id}");
            }
            else
            {
                _logger.LogDebug($"Title: {post.Title}, Content: {post.Content}");
            }

            ViewBag.Id = id;
            return View(post);
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {   
            await _postStore.Value.DeletePostAsync(id);

            return Redirect("/");
        }

        public ActionResult Create()
        {   
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(string title, string content)
        {   
            // TODO: Check for validity of post.                                                                                                                                                                                              
            var newPost = new PostContent { Title = title, Content = content };
            var newPostRef = await _postStore.Value.SavePostAsync(newPost);

            // And go back to the main page.                                                                                                                                                                                                  
            return Redirect($"/Home/ShowPost/{newPostRef.Id}");
        }

        [HttpGet]
        public async Task<ActionResult> EditPost(string id)
        {
            var post = await _postStore.Value.GetPostAsync(id);
            return View(post);
        }

        [HttpPost]
        public async Task<ActionResult> EditPost(string id, string title, string content)
        {
            var newPost = new PostContent { Title = title, Content = content };
            var newPostRef = await _postStore.Value.SavePostAsync(id, newPost);

            return Redirect($"/Home/ShowPost/{newPostRef.Id}");
        }
    }
}
