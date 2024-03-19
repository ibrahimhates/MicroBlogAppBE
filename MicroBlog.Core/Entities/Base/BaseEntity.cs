namespace MicroBlog.Core.Entities.Base;

public abstract class BaseEntity
{
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
}