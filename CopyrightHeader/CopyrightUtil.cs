//
// © Copyright 2017 HP Development Company, L.P.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace CopyrightHeader
{
    public class CopyrightUtil
    {
        public static IList<string> ReadFile(string fileName)
        {
            var buffer = new List<string>();
            if (File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    var line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        buffer.Add(line);
                    }
                }
            }
            return buffer;
        }

        public static void WriteFile(string fileName, IEnumerable<string> buffer)
        {
            var backup = fileName + ".cp.bak";
            //Make a backup just in case
            if (File.Exists(fileName))
            {
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
                File.Copy(fileName, backup);
            }

            try
            {
                using (var writer = new StreamWriter(fileName))
                {
                    foreach (var line in buffer)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception)
            {
                File.Copy(backup, fileName);
            }
            finally
            {
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
            }
        }
        private static void GetCommentInfo(string inputFile, CopyrightTemplate companyTemplate, Action<string> errorAction = null)
        {
            const string commentSpecFile = "CommentSpecification.json";
            try
            {
                var ext = Path.GetExtension(inputFile);
                using (var resource = CopyrightUtil.ResourceStream(commentSpecFile))
                {
                    if (resource != null)
                    {
                        var serializer = new DataContractJsonSerializer(typeof(CommentSpec[]));
                        var commentSpecs = (CommentSpec[])serializer.ReadObject(resource);
                        var commentSpec = commentSpecs.Find(ext);
                        if (commentSpec == null)
                        {
                            errorAction?.Invoke($"Unsupported file: {inputFile}");
                        }
                        Console.WriteLine($"Using {commentSpec?.Name} extension specification");
                        companyTemplate.CommentSpec = commentSpec;
                    }
                }
            }
            catch (Exception e)
            {
                errorAction?.Invoke(e.Message);
            }
        }

        public static CopyrightTemplate ReadTemplate(string inputFile, string templateName, Action<string> errorAction = null)
        {
            var copyrightTemplate = (CopyrightTemplate) null;
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                var path = "templates." + templateName + ".json";

                try
                {
                    using (var resource = ResourceStream(path))
                    {
                        if (resource != null)
                        {
                            var serializer = new DataContractJsonSerializer(typeof(CopyrightTemplate));
                            copyrightTemplate = (CopyrightTemplate)serializer.ReadObject(resource);
                            if (copyrightTemplate == null)
                            {
                                errorAction?.Invoke("Invalid template");
                            }
                        }
                        else
                        {
                            errorAction?.Invoke($"Bad or missing template: {path}");
                        }
                    }
                }
                catch (Exception e)
                {
                    errorAction?.Invoke(e.Message);
                }
            }
            if (copyrightTemplate != null)
            {
                GetCommentInfo(inputFile, copyrightTemplate, errorAction);
            }
            return copyrightTemplate;
        }

        public static Stream ResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.First(x => x.EndsWith(name));

            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }
    }
}
