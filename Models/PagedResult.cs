namespace BlazingBlog.Models
{
    public record PagedResult<TResult>(TResult[] Records, int TotalCount);
}
