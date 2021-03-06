﻿using Newtonsoft.Json;

namespace simpleblog.Posts
{
    public class PostContent
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}