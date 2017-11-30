//
// © Copyright 2017 HP Development Company, L.P.
//

using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace CopyrightHeader
{
    [DataContract]
    public class CommentSpec
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string SingleComment { get; set; }
        [DataMember]
        public string CommentBegin { get; set; }
        [DataMember]
        public string CommentEnd { get; set; }
        [DataMember]
        public string[] Extensions { get; set; }
    }

    public static class CommentSpecExtension
    {
        public static CommentSpec Find(this CommentSpec[] specs, string extension)
        {
            if ((string.IsNullOrWhiteSpace(extension)) || specs == null)
            {
                return null;
            }
            var ext = extension.TrimStart('.');
            return specs.First(x => x.Extensions.Any(s => s == ext.ToLower(CultureInfo.InvariantCulture)));
        }

    }
}