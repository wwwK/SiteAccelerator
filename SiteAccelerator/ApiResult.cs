namespace SiteAccelerator
{
    public class ApiResult<T>
    {
        public bool Status { get; set; }

        public int Code { get; set; }

        public object Msg { get; set; }

        public T[] Data { get; set; }
    }

}
