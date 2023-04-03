using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Media
{
  

    public class BannerUploadRequest
    {
        public string? BusinessId { get; set; }
        public IFormFile File { get; set; }
    }

    public class ImageUploadSignatureRequest
    {
        public string? BusinessId { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
    }
}
