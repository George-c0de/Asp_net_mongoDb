namespace WebApplication1.Models
{
    public class IndexViewModel
    {
        public FilterViewModel Filter { get; set; }
        public IEnumerable<Users> Users { get; set; }
    }
}
