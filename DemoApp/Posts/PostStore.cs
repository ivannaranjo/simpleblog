using Google.Storage.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Object = Google.Apis.Storage.v1.Data.Object;
using Google.Apis.Storage.v1.Data;

namespace DemoApp.Posts
{
    public class PostStore
    {
        private const string StorageBucket = "storage-my-demo-1331";
        private const string TitleMetadataKey = "title";

        private readonly Lazy<Task<StorageClient>> _storageClient = new Lazy<Task<StorageClient>>(() => StorageClient.CreateAsync());

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
                await client.UploadObjectAsync(destination, content);
            }

            return new PostReference(id, post.Title);
        }

        public async Task DeletePostAsync(string id)
        {
            var client = await _storageClient.Value;
            await client.DeleteObjectAsync(StorageBucket, GetPostName(id));
        }

        public async Task<IList<PostReference>> ListPostsAsync()
        {
            var client = await _storageClient.Value;
            var objs = await client.ListAllObjectsAsync(StorageBucket, "post/");

            return objs.Select(GetPostReference).Where(x => x.Title != null && x.Id != null).ToList();
        }

        public async Task<PostContent> GetPostAsync(string id)
        {
            var client = await _storageClient.Value;

            string serialized;
            using (var result = new MemoryStream())
            {
                await client.DownloadObjectAsync(StorageBucket, GetPostName(id), result);
                result.Position = 0;
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