using Amazon.S3;
using Amazon.S3.Model;
using static SC4PropTextureCatalogBuilder.DatabaseBuilder;

namespace SC4PropTextureCatalogBuilder {
    internal class ThumbnailUploader {
        private readonly AmazonS3Client _client;
        private readonly string _bucket = "prop-texture-catalog-thumbnails";
        private readonly string _thumbnailFolder;

        public HashSet<string> ExistingThumbs { get; private set; }

        public ThumbnailUploader(string thumbnailFolder) {
            _client = new AmazonS3Client(Credentials.AccessKey, Credentials.SecretKey, new AmazonS3Config {
                ServiceURL = Credentials.Endpoint,
                ForcePathStyle = true
            });
            _thumbnailFolder = thumbnailFolder;
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
        /// <param name="filePrefix">Prefix to add to each file name, roughly equivalent to a folder name.</param>
        public async Task UploadFolderAsync(string filePrefix) {
            int filesUploaded = 0;
            if (ExistingThumbs.Count == 0) {
                ExistingThumbs = await LoadExistingKeysAsync();
            }
            Console.WriteLine($"  > found {ExistingThumbs.Count} thumbnails in R2 already");

            var files = Directory.GetFiles(Path.Combine(_thumbnailFolder, filePrefix), "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                string fileName = filePrefix + "/" + Path.GetFileName(file);
                if (ExistingThumbs.Contains(fileName)) {
                    continue;
                }
                filesUploaded++;
                Console.WriteLine($"  > uploading {fileName} ({filesUploaded})");
                await UploadFileAsync(file, fileName);
            }
        }


        private async Task UploadFileAsync(string localPath, string key) {
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

        /// <summary>
        /// Fills the current count of each thumbnail type in the storage bucket.
        /// </summary>
        /// <returns>A new <see cref="ThumbnailCountItem"/> with the counts of each thumbnail type.</returns>
        public ThumbnailCountItem GetThumbnailCount() {
            int textures = Directory.EnumerateFiles(Path.Combine(_thumbnailFolder, "textures")).Count();
            int props = Directory.EnumerateFiles(Path.Combine(_thumbnailFolder, "props")).Count();
            int flora = Directory.EnumerateFiles(Path.Combine(_thumbnailFolder, "flora")).Count();
            int buildings = Directory.EnumerateFiles(Path.Combine(_thumbnailFolder, "buildings")).Count();
            return new ThumbnailCountItem(textures, props, flora, buildings);
        }

    }
}
