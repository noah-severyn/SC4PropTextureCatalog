using Amazon.S3;
using Amazon.S3.Model;

namespace SC4PropTextureCatalogBuilder {
    internal class ThumbnailUploader {
        private readonly AmazonS3Client _client;
        private readonly string _bucket = "prop-texture-catalog-thumbnails";

        public HashSet<string> ExistingThumbs { get; private set; }

        public ThumbnailUploader() {
            _client = new AmazonS3Client(Credentials.AccessKey, Credentials.SecretKey, new AmazonS3Config {
                ServiceURL = Credentials.Endpoint,
                ForcePathStyle = true
            });
            ExistingThumbs = [];
        }


        private async Task<HashSet<string>> LoadExistingKeysAsync() {
            var keys = new HashSet<string>();
            string? continuationToken = null;
            do {
                var request = new ListObjectsV2Request {
                    BucketName = _bucket,
                    ContinuationToken = continuationToken
                };
                var response = await _client.ListObjectsV2Async(request);
                foreach (var obj in response.S3Objects) keys.Add(obj.Key); 
                continuationToken = (bool) response.IsTruncated ? response.NextContinuationToken : null; 
            } while (continuationToken != null);
            return keys;
        }


        /// <summary>
        /// Uploads all files from the specified local folder to the remote storage, skipping files that already exist
        /// in the destination.
        /// </summary>
        /// <param name="thumbnailFolder">File folder containing the thumbnail images to upload.</param>
        /// <param name="filePrefix">Prefix to add to each file name, roughly equivalent to a folder name.</param>
        public async Task UploadFolderAsync(string thumbnailFolder, string filePrefix) {
            ExistingThumbs = await LoadExistingKeysAsync();
            Console.WriteLine($"  > found {ExistingThumbs.Count} thumbnails in R2 already");

            var files = Directory.GetFiles(thumbnailFolder, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                string fileName = filePrefix + "/" + Path.GetFileName(file);
                if (ExistingThumbs.Contains(fileName)) {
                    Console.WriteLine($"  > skipping {fileName} (already exists)");
                    continue;
                }
                await UploadFileAsync(file, fileName);
            }
        }


        private async Task UploadFileAsync(string localPath, string key) {
            Console.WriteLine($"  > uploading {key}");
            await using var fileStream = File.OpenRead(localPath);
            var put = new PutObjectRequest {
                BucketName = _bucket,
                Key = key,
                InputStream = fileStream,
                ContentType = "image/png",
                DisablePayloadSigning = true
            };
            await _client.PutObjectAsync(put);
        }
    }
}
