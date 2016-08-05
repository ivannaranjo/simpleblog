using System;
using Microsoft.Extensions.Configuration;

namespace simpleblog
{
    public interface IAppConfiguration
    {
        string StorageBucket { get; }
    }

    public class AppConfiguration : IAppConfiguration
    {
        private const string StorageBucketEnvVariable = "SIMPLEBLOG_STORAGE_BUCKET";

        public string StorageBucket { get; private set; }

        public AppConfiguration(IConfiguration configuration)
        {
            var bucket = configuration[StorageBucketEnvVariable];
            if (bucket != null)
            {
                StorageBucket = bucket;
            }
            else
            {
                throw new InvalidProgramException("The storage bucket is not specified.");
            } 
        }
    }
}