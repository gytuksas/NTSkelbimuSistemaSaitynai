namespace NTSkelbimuSistemaSaitynai.DbUtils
{
    public class DbConnection
    {
        public DbConnection()
        {
        }
        public string GetConnectionString()
        {
            string connString;
            try
            {
                var host = Environment.GetEnvironmentVariable("DB_HOST");
                var user = Environment.GetEnvironmentVariable("DB_USER");
#pragma warning disable CS8604 // Possible null reference argument.
                var password = new StreamReader(Environment.GetEnvironmentVariable("DB_PASS_FILE")).ReadToEnd();
#pragma warning restore CS8604 // Possible null reference argument.
                var database = Environment.GetEnvironmentVariable("DB_NAME");
                connString = "Host=" + host + ";Username=" + user + ";Password=" + password + ";Database=" + database;
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("Failed to generate connection string! Have you properply defined all environment variables?", ex.InnerException);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("Failed to generate connection string! File defined in environment variables not found!", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate connection string!", ex.InnerException);
            }

            return connString;
        }
    }
}
