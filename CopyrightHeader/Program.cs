//
// © Copyright 2017 HP Development Company, L.P.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace CopyrightHeader
{
    public class Program
    {
        private static readonly Dictionary<string, Action<string>> paramList = new Dictionary<string, Action<string>>();
        private static string inputFile = "";
        private static string outputFile = "";
        private static string template = "";
        private static int lineCount = 10;
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
                        try
                        {
                            paramList[keyValue[0]].Invoke(keyValue[1]);
                        }
                        catch (Exception)
                        {
                            Usage($"Error in parameter {keyValue[0]}");
                        }
                    }
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

            companyTemplate = CopyrightUtil.ReadTemplate(inputFile, template, Usage);
        }

        private static void Main(string[] args)
        {
            paramList.Add("input", a => inputFile = a);
            paramList.Add("output", a => outputFile = a);
            paramList.Add("template", a => template = a);
            paramList.Add("linecount", a => lineCount = int.Parse(a));
            ParseArguments(args);

            CheckArguments();

            var buffer = CopyrightUtil.ReadFile(inputFile);
            if (buffer.Count > 0)
            {
                if (lineCount > buffer.Count)
                {
                    lineCount = buffer.Count;
                }
                var copyright = new Copyright(companyTemplate);
                if (copyright.FindCurrentCopyright(buffer, lineCount))
                {
                    Console.WriteLine("Current Copyright already exists.");
                    return;
                }

                copyright.AddOrModifyCopyright(buffer, lineCount);
                CopyrightUtil.WriteFile(outputFile, buffer);
            }
        }
    }
}
