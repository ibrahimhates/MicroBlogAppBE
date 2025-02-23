using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Core.Entities.Base;

public interface IEntity<TKey>
where TKey : struct
{
    public TKey Id { get; set; }
}