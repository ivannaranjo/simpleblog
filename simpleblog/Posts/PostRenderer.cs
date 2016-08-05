using System;
using System.Collections.Generic;
using System.Linq;
using HeyRed.MarkdownSharp;

namespace DemoApp.Posts
{
    public static class PostRenderer
    {
        static Lazy<Markdown> s_markdownProcessor = new Lazy<Markdown>();

        public static string RenderPost(PostContent post)
        {
            return s_markdownProcessor.Value.Transform(post.Content);
        }
    }
}