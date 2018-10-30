//
// © Copyright 2017-2018 HP Development Company, L.P.
//

using System.Runtime.Serialization;

namespace CopyrightHeader
{
    [DataContract]
    public class CopyrightTemplate
    {
        public CopyrightTemplate()
        {
            Header = new[]
            {
                "{comment}",
                "{comment} {copyright} {year} {companyName}",
                "{comment}"
            };
        }

        [DataMember]
        public string Company { get; set; }
        [DataMember]
        public string CompanyPattern { get; set; }
        [DataMember]
        public string[] Header { get; set; }
        public CommentSpec CommentSpec { get; set; }
    }
}
