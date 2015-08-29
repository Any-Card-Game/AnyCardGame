namespace RestServer.Common
{
    public class TokenMetaData : MetaData
    {
        public TokenMetaData(string jwt)
            : base(MetaDataType.Token)
        {
            Jwt = jwt;
        }

        public string Jwt { get; set; }
        public MetaDataType Type { get; set; }
    }
}