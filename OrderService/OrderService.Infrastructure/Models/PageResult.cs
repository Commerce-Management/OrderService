namespace OrderService.Infrastructure.Models;

public class PageResult<T>
{
    public PageResult(int limit, int length, IEnumerable<T> items)
    {
        Items = items;
        MaxPages = length / limit;
    }
    public IEnumerable<T> Items { get; set; }
    public int MaxPages { get; set; }
}