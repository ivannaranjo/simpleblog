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
using Microsoft.Extensions.Logging;

namespace simpleblog.Posts
{
    public class PostStore
    {
        private const string TitleMetadataKey = "title";

        private readonly Lazy<Task<StorageService>> _storageClient = new Lazy<Task<StorageService>>(CreateStorageService);
        private readonly string _storageBucket;
        private readonly ILogger _logger;

        public PostStore(string storageBucket, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(PostStore));
            _storageBucket = storageBucket;
        }

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

            _logger.LogDebug($"Laving post {id}");

            var destination = new Object
            {
                Bucket = _storageBucket,
                Name = GetPostName(id),
                Metadata = new Dictionary<string, string> { { TitleMetadataKey, post.Title } },
            };

            using (var content = new MemoryStream(buffer))
            {
                await client.Objects.Insert(destination, _storageBucket, content, "").UploadAsync();
            }

            return new PostReference(id, post.Title);
        }

        public async Task DeletePostAsync(string id)
        {
            var client = await _storageClient.Value;
            _logger.LogDebug($"Deleting post {id}");
            await client.Objects.Delete(_storageBucket, GetPostName(id)).ExecuteAsync();
        }

        public async Task<IList<PostReference>> ListPostsAsync()
        {
            var client = await _storageClient.Value;
            var request = client.Objects.List(_storageBucket);
            request.Prefix = "post/";
           
            var objs = await request.ExecuteAsync();

            if (objs.Items != null)
            {
                _logger.LogDebug($"Got a list of {objs.Items.Count} items.");
                return objs.Items.Select(GetPostReference).Where(x => x.Title != null && x.Id != null)?.ToList();
            }
            else
            {
                _logger.LogDebug("No posts found.");
                return new List<PostReference>();
            }
        }

        public async Task<PostContent> GetPostAsync(string id)
        {
            var client = await _storageClient.Value;

            _logger.LogDebug($"Getting post {id}");

            string serialized;
            using (var result = new MemoryStream())
            {
                await client.Objects.Get(_storageBucket, GetPostName(id)).DownloadAsync(result);

                result.Position = 0;
                using (var reader = new StreamReader(result))
                {
                    serialized = reader.ReadToEnd();
                }
            }

            _logger.LogDebug($"Post content: {serialized}");

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