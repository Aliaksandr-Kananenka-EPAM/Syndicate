using Function.Models;

namespace Function.Entities
{
    public class Event
    {
        public string Id { get; set; }
        public int PrincipalId { get; set; }
        public string CreatedAt { get; set; }
        public Content Body { get; set; }
    }
}
