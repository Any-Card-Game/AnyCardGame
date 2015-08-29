using System.Collections.Generic;

namespace RestServer.Common
{
    public class ErrorMetaData : MetaData
    {
        public MetaDataType Type { get; set; }

        public ErrorMetaData(IEnumerable<string> errors)
            : base(MetaDataType.Error)
        {
            Errors = errors;
        }

        public ErrorMetaData(string error)
            : base(MetaDataType.Error)
        {
            Errors = new[] { error };
        }

        public ErrorMetaData()
            : base(MetaDataType.Error)
        {
            Errors = new string[0];
        }

        public IEnumerable<string> Errors { get; set; }
    }
}