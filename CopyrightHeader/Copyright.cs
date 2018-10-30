//
// © Copyright 2017-2018 HP Development Company, L.P.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CopyrightHeader
{
    public class Copyright
    {
        private readonly int currentYear = DateTime.Now.Date.Year;

        private readonly CopyrightTemplate template;
        private readonly string copyrightName;
        private readonly string yearSpanPattern;
        private readonly string yearPattern;
        private readonly string multiYearCopyrightPattern;

        public Copyright(CopyrightTemplate companyTemplate)
        {
            this.template = companyTemplate;

            if (string.IsNullOrWhiteSpace(template.CompanyPattern))
            {
                template.CompanyPattern = template.Company;
            }

            var companyPattern = "(?'company'" + template.CompanyPattern + ")";
            copyrightName = "© Copyright";
            var spacePattern = "(\\s*)";
            var singleYearPattern = "(?'singleYear'(19|20)?[0-9][0-9])";
            var beginPattern = "(?'begin'.*)";
            var endPattern = "(?'end'.*)";
            yearSpanPattern = $"(?'spanYear'(?'fromYear'{singleYearPattern})-(?'toYear'{singleYearPattern}))";
            yearPattern = $"(({singleYearPattern}|{yearSpanPattern}),{spacePattern})*(?'endYear'({singleYearPattern}|{yearSpanPattern}))";
            var copyrightPattern = $"(?'copyright'(©|\\([Cc]\\)){spacePattern}(([Cc]opyright|[Cc]opr.))?)";
            multiYearCopyrightPattern = beginPattern + copyrightPattern + spacePattern + yearPattern + spacePattern + companyPattern + endPattern;
        }

        public void AddOrModifyCopyright(IList<string> buffer, int lineCount)
        {
            var line = FindCopyright(buffer, lineCount);
            if (line >= 0)
            {
                buffer[line] = ModifyHeader(buffer[line]);
            }
            else
            {
                InsertDocumentHeader(buffer);
            }
        }

        public string ModifyHeader(string header)
        {
            var regex = new Regex(multiYearCopyrightPattern);
            if (regex.IsMatch(header))
            {
                var m = Regex.Match(header, multiYearCopyrightPattern);
                if (m.Success)
                {
                    //Fix company
                    header = ReplaceGroup(regex, header, "company", template.Company);

                    var yearString = m.Groups["endYear"].Value;
                    if (!string.IsNullOrEmpty(yearString))
                    {
                        var spanYear = Regex.Match(yearString, (yearSpanPattern + "$"));
                        if (spanYear.Success)
                        {
                            var year = spanYear.Groups["toYear"].Value;
                            if (int.Parse(year) == currentYear - 1)
                            {
                                header = ReplaceGroup(regex, header, "toYear", $"{currentYear}");
                            }
                            else if (int.Parse(year) != currentYear)
                            {
                                header = ReplaceGroup(regex, header, "toYear", $"{year}, {currentYear}");
                            }
                        }
                        else
                        {
                            var singleYear = Regex.Match(yearString, yearPattern + "$");
                            if (singleYear.Success)
                            {
                                var year = singleYear.Groups["singleYear"].Value;
                                if (int.Parse(yearString) == currentYear - 1)
                                {
                                    header = ReplaceGroup(regex, header, "endYear", $"{year}-{currentYear}");
                                }
                                else if (int.Parse(yearString) != currentYear)
                                {
                                    header = ReplaceGroup(regex, header, "endYear", $"{year}, {currentYear}");
                                }
                            }
                        }
                    }
                }
            }
            return header;
        }

        private void InsertDocumentHeader(IList<string> buffer)
        {
            var year = DateTime.Now.Date.Year;

            for (int i = template.Header.Length - 1; i >= 0; i--)
            {
                var line = template.Header[i];
                line = line.Replace("{SingleComment}", template.CommentSpec.SingleComment).
                    Replace("{CommentBegin}", template.CommentSpec.CommentBegin).
                    Replace("{CommentEnd}", template.CommentSpec.CommentEnd).
                    Replace("{Copyright}", copyrightName).
                    Replace("{Year}", year.ToString()).
                    Replace("{CompanyName}", template.Company);
                buffer.Insert(0, line);
            }
        }

        public int FindCopyright(IList<string> buffer, int lineCount)
        {
            for (var index = 0; index < lineCount; index++)
            {
                if (FindCopyright(buffer[index]))
                {
                    return index;
                }
            }
            return -1;
        }
        public bool FindCurrentCopyright(IList<string> buffer, int lineCount)
        {
            for (var index = 0; index < lineCount; index++)
            {
                if (FindCurrentCopyright(buffer[index]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool FindCopyright(string line)
        {
            return Regex.IsMatch(line, multiYearCopyrightPattern);
        }

        public bool FindCurrentCopyright(string line)
        {
            var match = Regex.Match(line, multiYearCopyrightPattern);
            if (match.Success)
            {
                var endYear = match.Groups["endYear"].Value;
                if (endYear.Contains(currentYear.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private static string ReplaceGroup(Regex regex, string input, string groupName, string replacement)
        {
            var m = regex.Match(input);
            var replaceString = ReplaceLastNamedGroup(input, groupName, replacement, m);
            return regex.Replace(input, replaceString);
        }

        private static string ReplaceLastNamedGroup(string input, string groupName, string replacement, Match m)
        {
            Capture capt = null;

            foreach (Capture c in m.Groups[groupName].Captures)
            {
                capt = c;
            }
            if (capt == null)
                return m.Value;
            var sb = new StringBuilder(input);
            sb.Remove(capt.Index, capt.Length);
            sb.Insert(capt.Index, replacement);
            return sb.ToString();
        }
    }
}
