using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Object = Google.Apis.Storage.v1.Data.Object;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Google.Apis.Auth.OAuth2;

namespace simpleblog.Posts
{
    public class PostStore
    {
        private const string StorageBucket = "storage-my-demo-1331";
        private const string TitleMetadataKey = "title";

        private readonly Lazy<Task<StorageService>> _storageClient = new Lazy<Task<StorageService>>(CreateStorageService);

        private static async Task<StorageService> CreateStorageService()
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            return new StorageService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "simpleblob1.0",
            });
        }

        public Task<PostReference> SavePostAsync(PostContent post)
        {
            return SavePostAsync(GetUniquePostId(), post);
        }

        public async Task<PostReference> SavePostAsync(string id, PostContent post)
        { 
            var client = await _storageClient.Value;
            var serialized = JsonConvert.SerializeObject(post);
            var buffer = Encoding.UTF8.GetBytes(serialized);

            var destination = new Object
            {
                Bucket = StorageBucket,
                Name = GetPostName(id),
                Metadata = new Dictionary<string, string> { { TitleMetadataKey, post.Title } },
            };

            using (var content = new MemoryStream(buffer))
            {
                await client.Objects.Insert(destination, StorageBucket, content, "").UploadAsync();
            }

            return new PostReference(id, post.Title);
        }

        public async Task DeletePostAsync(string id)
        {
            var client = await _storageClient.Value;
            await client.Objects.Delete(StorageBucket, GetPostName(id)).ExecuteAsync();
        }

        public async Task<IList<PostReference>> ListPostsAsync()
        {
            var client = await _storageClient.Value;
            var request = client.Objects.List(StorageBucket);
            request.Prefix = "post/";
            var objs = await request.ExecuteAsync();

            return objs.Items.Select(GetPostReference).Where(x => x.Title != null && x.Id != null).ToList();
        }

        public async Task<PostContent> GetPostAsync(string id)
        {
            var client = await _storageClient.Value;

            string serialized;
            using (var result = await client.Objects.Get(StorageBucket, GetPostName(id)).ExecuteAsStreamAsync())
            {
                using (var reader = new StreamReader(result))
                {
                    serialized = reader.ReadToEnd();
                }
            }

            return JsonConvert.DeserializeObject<PostContent>(serialized);
        }

        private PostReference GetPostReference(Object obj)
        {
            string title = null;
            obj.Metadata?.TryGetValue(TitleMetadataKey, out title);

            return new PostReference(GetPostIdFromName(obj.Name), title);
        }

        private static string GetPostName(string id) => $"post/{id}";

        private static string GetPostIdFromName(string name)
        {
            var parts = name.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException(nameof(name));
            }
            return parts[1];
        }

        private static string GetUniquePostId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}