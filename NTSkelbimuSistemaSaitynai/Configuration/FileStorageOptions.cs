namespace NTSkelbimuSistemaSaitynai.Configuration
{
    /// <summary>
    /// Configures persistent file storage used for broker picture uploads.
    /// </summary>
    public sealed class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        /// <summary>
        /// Absolute path to the directory that will hold uploaded media assets.
        /// </summary>
        public required string UploadRoot { get; set; }

        /// <summary>
        /// Public request path that maps to the upload directory.
        /// </summary>
        public string RequestPath { get; set; } = "/uploads";
    }
}
