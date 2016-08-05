using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoApp.Posts
{
    public static class PostRenderer
    {
        static Lazy<MarkdownSharp.Markdown> s_markdownProcessor = new Lazy<MarkdownSharp.Markdown>();

        public static string RenderPost(PostContent post)
        {
            return s_markdownProcessor.Value.Transform(post.Content);
        }
    }
}