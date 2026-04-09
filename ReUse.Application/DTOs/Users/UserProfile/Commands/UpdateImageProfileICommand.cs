using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace ReUse.Application.DTOs.Users.UserProfile.Commands;

public class UpdateImageProfileDto
{
    public IFormFile Image { get; set; } = null!;
}