namespace FluffyBunny4.Models
{
    public class ConsentBaseResponse
    {
        public class ConsentError
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
        }
        public ConsentError Error { get; set; }
    }
}
