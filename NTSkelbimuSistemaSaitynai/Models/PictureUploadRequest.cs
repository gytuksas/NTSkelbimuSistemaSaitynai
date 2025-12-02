using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NTSkelbimuSistemaSaitynai.Models;

public class PictureUploadRequest
{
    [Required]
    public long ApartmentId { get; set; }

    public bool Public { get; set; } = false;

    [Required]
    [MinLength(1)]
    public List<IFormFile> Files { get; set; } = new();
}
