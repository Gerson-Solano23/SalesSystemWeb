namespace SalesSystem.API.Utility
{
    public class Response<T>
    {
        public bool status { get; set; }
        public T data { get; set; }
        public string message { get; set; }
    }
}
