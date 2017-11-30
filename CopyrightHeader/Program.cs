//
// © Copyright 2017 HP Development Company, L.P.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CopyrightHeader
{
    public class Program
    {
        private static readonly Dictionary<string, Action<string>> paramList = new Dictionary<string, Action<string>>();
        private static string inputFile = "";
        private static string outputFile = "";
        private static string template = "";
        private const int lineCount = 10;
        private static CopyrightTemplate companyTemplate;

        private static void Usage(string msg = null)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                Console.WriteLine("");
                Console.WriteLine(msg);
            }
            Console.WriteLine("");
            Console.WriteLine("\tUsage:  CopyrightHeader input=<infile> output=<outfile> template=<template>");
            Console.WriteLine("");
            Environment.Exit(-1);
        }

        private static void ParseArguments(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var keyValue = arg.Split('=');
                if (keyValue.Length == 2)
                {
                    if (paramList.ContainsKey(keyValue[0]))
                    {
                        paramList[keyValue[0]].Invoke(keyValue[1]);
                    }
                }
            }
        }

        private static IList<string> ReadFile(string fileName)
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

        private static void WriteFile(string fileName, IEnumerable<string> buffer)
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
                    Console.WriteLine($"Writing file: {fileName}");
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

        private static void CheckArguments()
        {
            //If an output file is not specified, then use inputfile as the outputfile
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                outputFile = inputFile;
            }

            if (string.IsNullOrEmpty(inputFile))
            {
                Usage();
            }

            if (!File.Exists(inputFile))
            {
                Usage($"Input file does not exist: {inputFile}");
            }
            //If the outputfile exists and it is not the inputfile, then
            //We don't want to overwrite it.
            if ( (String.CompareOrdinal(inputFile, outputFile) != 0) && File.Exists(outputFile))
            {
                Usage($"Output file already exits: {outputFile}");
            }

            if (!string.IsNullOrWhiteSpace(template))
            {
                var path = Path.Combine(AssemblyDirectory, "templates");
                path = Path.Combine(path, template + ".json");

                if (!File.Exists(path))
                {
                    Usage($"Template does not exist: {path}");
                }
                Console.WriteLine($"Using template: {path}");
                try
                {
                    using (StreamReader file = File.OpenText(path))
                    {
                        var serializer = new JsonSerializer();
                        companyTemplate = (CopyrightTemplate)serializer.Deserialize(file, typeof(CopyrightTemplate));
                        if (companyTemplate == null)
                        {
                            Usage("Invalid template");
                        }
                    }
                }
                catch (Exception e)
                {
                    Usage(e.Message);
                }
            }
        }

        public static void GetCommentInfo()
        {
            try
            {
                var path = Path.Combine(AssemblyDirectory, "CommentSpecification.json");
                var ext = Path.GetExtension(inputFile);
                using (StreamReader file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    var commentSpecs = (CommentSpec[])serializer.Deserialize(file, typeof(CommentSpec[]));
                    var commentSpec = commentSpecs.Find(ext);
                    if (commentSpec == null)
                    {
                        Usage($"Unsupported file: {inputFile}");
                    }
                    Console.WriteLine($"Using {commentSpec?.Name} extension specification");
                    companyTemplate.CommentSpec = commentSpec;
                }
            }
            catch (Exception e)
            {
                Usage(e.Message);
            }
        }

        private static void Main(string[] args)
        {
            paramList.Add("input", a => inputFile = a);
            paramList.Add("output", a => outputFile = a);
            paramList.Add("template", a => template = a);
            ParseArguments(args);

            CheckArguments();

            GetCommentInfo();
            var buffer = ReadFile(inputFile);
            if (buffer.Count > 0)
            {
                var copyright = new Copyright(companyTemplate);
                if (copyright.FindCurrentCopyright(buffer, lineCount))
                {
                    Console.WriteLine("Current Copyright already exists.");
                    return;
                }

                copyright.AddOrModifyCopyright(buffer, lineCount);
                WriteFile(outputFile, buffer);
            }
        }
        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
