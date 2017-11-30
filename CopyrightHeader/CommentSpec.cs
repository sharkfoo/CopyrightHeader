//
// © Copyright 2017 HP Development Company, L.P.
//

using System.Globalization;
using System.Linq;

namespace CopyrightHeader
{
    public class CommentSpec
    {
        public string Name { get; set; }
        public string SingleComment { get; set; }
        public string CommentBegin { get; set; }
        public string CommentEnd { get; set; }
        public string[] Extensions { get; set; }
    }

    public static class CommentSpecExtension
    {
        public static CommentSpec Find(this CommentSpec[] specs, string extension)
        {
            var ext = extension.TrimStart('.');
            return specs.First(x => x.Extensions.Any(s => s == ext.ToLower(CultureInfo.InvariantCulture)));
        }

    }
}