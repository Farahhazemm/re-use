using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace ReUse.Application.DTOs.Categories;

public class UploadIconRequest
{
    public IFormFile File { get; set; }
}