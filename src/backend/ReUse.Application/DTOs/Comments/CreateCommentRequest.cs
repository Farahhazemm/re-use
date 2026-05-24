using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Comments;

public record CreateCommentRequest(string Body, Guid? ParentCommentId = null);