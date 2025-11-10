using Microsoft.VisualStudio.Web.CodeGeneration.Utils;

namespace NTSkelbimuSistemaSaitynai.Configuration
{
    public class JwtConfig
    {
        public required string Issuer { get; set; }
        public required string Key {  get; set; }
    }
}
