namespace DemoApp.Posts
{
    public class PostReference
    {
        public string Id { get; }

        public string Title { get; }

        public PostReference(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}