//
// © Copyright 2017 HP Development Company, L.P.
//

namespace CopyrightHeader
{
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

        public string Company { get; set; }
        public string CompanyPattern { get; set; }
        public string[] Header { get; set; }
        public CommentSpec CommentSpec { get; set; }
    }
}
