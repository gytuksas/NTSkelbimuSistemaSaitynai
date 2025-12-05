namespace NTSkelbimuSistemaSaitynai.Models;

public class PublicViewingDto
{
    public long Id { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class PublicViewingRequest
{
    public required string From { get; set; }
    public required string To { get; set; }
}
