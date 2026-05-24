namespace ReUse.Domain.Entities;

public class ProductComment : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Body { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public Guid? ParentCommentId { get; set; }
    public ProductComment? ParentComment { get; set; }
    public ICollection<ProductComment> Replies { get; set; } = new List<ProductComment>();
}