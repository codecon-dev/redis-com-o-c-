public class Entry<T>
{
    public T Value { get; set; }
    public DateTime ExpirationUTC { get; set; }
    public TimeSpan ExpiresIn => ExpirationUTC - DateTime.UtcNow;
}
