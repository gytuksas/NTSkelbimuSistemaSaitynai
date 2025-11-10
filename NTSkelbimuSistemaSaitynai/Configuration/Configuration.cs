using System.Text.Json;

namespace NTSkelbimuSistemaSaitynai.Configuration
{
    public sealed class Configuration
    {
        public required JwtConfig Jwt { get; set; }

        public static Configuration GetConfiguration()
        {
            string text = null;
            try
            {
                text = File.ReadAllText("/run/secrets/config");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read configuration file, the error was: ", ex.Message);
            }
            Configuration config = JsonSerializer.Deserialize<Configuration>(text);
            if (config == null) 
            {
                throw (new Exception("Failed to deserialize configuration."));
            }
            return config;
        }
    }
}
