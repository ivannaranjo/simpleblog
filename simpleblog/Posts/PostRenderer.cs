using System;
using HeyRed.MarkdownSharp;

namespace simpleblog.Posts
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