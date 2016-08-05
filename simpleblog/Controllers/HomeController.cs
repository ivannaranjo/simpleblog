using DemoApp.Posts;
using Google.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DemoApp.Controllers
{
    public class HomeController : Controller
    {
        private const string ProjectName = "my-demo-1331";

        private readonly Lazy<PostStore> _postStore = new Lazy<PostStore>();
       
        public async Task<ActionResult> Index()
        {
            var posts = await _postStore.Value.ListPostsAsync();
            return View(posts);
        }

        [HttpGet]
        public async Task<ActionResult> ShowPost(string id)
        {
            var post = await _postStore.Value.GetPostAsync(id);

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