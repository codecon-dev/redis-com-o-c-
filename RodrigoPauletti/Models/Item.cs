public class Item
{
    public string Value { get; set; }
    public int Ttl { get; set; }

    public Item(string value, int ttl)
    {
        Value = value;
        Ttl = ttl;
    }
}