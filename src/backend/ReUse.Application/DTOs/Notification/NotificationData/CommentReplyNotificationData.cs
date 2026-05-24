using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReUse.Application.DTOs.Notification.NotificationData;
public class CommentReplyNotificationData : INotificationData
{
    public Guid ReplierId { get; set; }
    public string ReplierName { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Guid ParentCommentId { get; set; }
    public Guid ReplyCommentId { get; set; }
}