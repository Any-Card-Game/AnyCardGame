namespace RestServer.Common
{
    public class AcgResponse
    {
        public AcgResponse(object data, MetaData meta)
        {
            Data = data;
            Meta = meta;
        }

        public MetaData Meta { get; set; }
        public object Data { get; set; }
    }
}