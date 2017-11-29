//
// © Copyright 2017 HP Development Company, L.P.
//
using System;
using System.Collections.Generic;
using CopyrightHeader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CopyrightHeaderTest
{
    [TestClass]
    public class HeaderTest
    {
        private static readonly int currentYear = DateTime.Now.Date.Year;
        private static string alternateCompany = "Acme, Inc.";
        private static readonly List<string> headerList = new List<string>
        {
            $"// ©Copyright 1995 Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 1995-1996 Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 1995, 1997 Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 1995, 1997-1999 Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 1995, 2010-{currentYear-1} Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 2012 Hewlett-Packard Development Company, L.P.",
            $"// ©Copyright 2012 Hewlett-Packard Development Co., L.P.",
            $"// ©Copyright 2012 HPDC",
            $"// ©Copyright 1990, 1993, 1997-1999 Hewlett-Packard Development Company, L.P.",
            $"// (C)Copyright 1990, 1993, 1997-1999 Hewlett-Packard Development Company, L.P.",
            $"// Copyright © {currentYear} HPDC"
        };

        private static readonly List<string> modifiedHeaderList = new List<string>
        {
            $"// ©Copyright 1995, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 1995-1996, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 1995, 1997, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 1995, 1997-1999, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 1995, 2010-{currentYear} HP Development Company, L.P.",
            $"// ©Copyright 2012, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 2012, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 2012, {currentYear} HP Development Company, L.P.",
            $"// ©Copyright 1990, 1993, 1997-1999, {currentYear} HP Development Company, L.P.",
            $"// (C)Copyright 1990, 1993, 1997-1999, {currentYear} HP Development Company, L.P.",
            $"// Copyright © {currentYear} HP Development Company, L.P."
        };

        private static readonly List<string> altHeaderList = new List<string>
        {
            $"// ©Copyright 1995 {alternateCompany}",
            $"// ©Copyright 1995-1996 {alternateCompany}",
            $"// ©Copyright 1995, 1997 {alternateCompany}",
            $"// ©Copyright 1995, 1997-1999 {alternateCompany}",
            $"// ©Copyright 1995, 2010-{currentYear-1} {alternateCompany}",
            $"// ©Copyright 2012 {alternateCompany}",
            $"// ©Copyright 1990, 1993, 1997-1999 {alternateCompany}",
            $"// (C)Copyright 1990, 1993, 1997-1999 {alternateCompany}",
        };

        private static readonly List<string> altModifiedHeaderList = new List<string>
        {
            $"// ©Copyright 1995, {currentYear} {alternateCompany}",
            $"// ©Copyright 1995-1996, {currentYear} {alternateCompany}",
            $"// ©Copyright 1995, 1997, {currentYear} {alternateCompany}",
            $"// ©Copyright 1995, 1997-1999, {currentYear} {alternateCompany}",
            $"// ©Copyright 1995, 2010-{currentYear} {alternateCompany}",
            $"// ©Copyright 2012, {currentYear} {alternateCompany}",
            $"// ©Copyright 1990, 1993, 1997-1999, {currentYear} {alternateCompany}",
            $"// (C)Copyright 1990, 1993, 1997-1999, {currentYear} {alternateCompany}",
        };

        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void TestValidHeaders()
        {
            var copyright = new Copyright(hp);
            foreach (var header in headerList)
            {
                Assert.IsTrue(copyright.FindCopyright(header));
            }
        }
        [TestMethod]
        public void TestValidHeadersAlternate()
        {
            var copyright = new Copyright(acme);
            foreach (var header in altHeaderList)
            {
                Assert.IsTrue(copyright.FindCopyright(header));
            }
        }

        [TestMethod]
        public void TestModifiedHeaders()
        {
            var copyright = new Copyright(hp);
            for (int index = 0; index < headerList.Count; index++)
            {
                var newHeader = copyright.ModifyHeader(headerList[index]);
                Assert.AreEqual(modifiedHeaderList[index], newHeader);
            }
        }
        [TestMethod]
        public void TestModifiedHeadersAlternate()
        {
            var copyright = new Copyright(acme);
            for (int index = 0; index < altHeaderList.Count; index++)
            {
                var newHeader = copyright.ModifyHeader(altHeaderList[index]);
                Assert.AreEqual(altModifiedHeaderList[index], newHeader);
            }
        }

        private readonly CopyrightTemplate hp = new CopyrightTemplate
        {
            Company = "HP Development Company, L.P.",
            CompanyPattern = "(Hewlett-Packard|HP) (Development|Dev.) (Company|Co.), L.P.|HPDC",
            Header = new string[]
            {
                "//",
                "// {copyright} {year} {companyName}",
                "//"
            }
        };
        private readonly CopyrightTemplate acme = new CopyrightTemplate
        {
            Company = "Acme, Inc.",
            Header = new string[]
            {
                "//",
                "// {copyright} {year} {companyName}",
                "//"
            }
        };
    }
}
