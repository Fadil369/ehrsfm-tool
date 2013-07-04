﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MAX_EA.ClassLibrary.Tests
{
    [TestClass]
    public class UnitTests
    {
        EA.Repository Repos;

        [TestInitialize]
        public void TestInit()
        {
            Repos = new EA.Repository();
            Repos.OpenFile(@"D:\Develop\ehrsfm_profile\trunk\MAX_EA.ClassLibrary.Tests\MAX-TestCases.eap");
        }

        [TestMethod]
        public void TestPackageWithOneClass()
        {
            TestRoundtrip("{19465808-9DC6-49b0-88C4-15C608A8B1C7}");
        }

        [TestMethod]
        public void TestPackageWithPackageAndElementWithChilds()
        {
            TestRoundtrip("{57922E3A-1184-4d06-A612-A5A1AE369639}");
        }

        [TestMethod]
        public void TestPackageWithSupportedElementTypes()
        {
            TestRoundtrip("{C36ABB39-D44F-489b-8066-5F7B77BB187A}");
        }

        [TestMethod]
        public void TestPackageWithUnsupportedElementTypes() // DataType
        {
            try
            {
                TestRoundtrip("{A6BFF475-EF36-4526-8F75-2FE63D64D30E}");
                Assert.Fail("Expected an ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("De aangevraagde waarde DataType is niet gevonden.", ex.Message);
            }
        }

        [TestMethod]
        public void TestMultipleStereotypes()
        {
            // TODO: e.g. in DCM's "data,enumeration"
        }

        private void TestRoundtrip(string guidPackageToTest)
        {
            // Export TestCase
            string fileNameExport1 = @"d:\tmp\max-export1.xml";
            MAXExporter3 exporter1 = new MAXExporter3();
            exporter1.exportPackage(Repos, Repos.GetPackageByGuid(guidPackageToTest), fileNameExport1);

            // Import Exported TestCase in Package with name "Temp (used in the UnitTests)"
            string guidView = "{1CC3E100-9474-4f2c-9FBD-75B7C4289FC6}";
            EA.Package selectedPackage = (EA.Package)Repos.GetPackageByGuid(guidView).Packages.AddNew(Guid.NewGuid().ToString(), "Package");
            selectedPackage.Update();
            string guidExport2 = selectedPackage.PackageGUID;
            MAXImporter3 importer = new MAXImporter3();
            importer.import(Repos, selectedPackage, fileNameExport1);
            selectedPackage.Packages.Refresh();

            // Export Imported TestCase
            string guidImport2 = ((EA.Package)selectedPackage.Packages.GetAt(0)).PackageGUID;
            string fileNameExport2 = @"d:\tmp\max-export2.xml";
            MAXExporter3 exporter2 = new MAXExporter3();
            exporter2.exportPackage(Repos, Repos.GetPackageByGuid(guidImport2), fileNameExport2);

            // compare export1 with export2
            // IGNORE @exportDate & <modified> elements. We know those will be different.
            XElement export1 = XElement.Load(fileNameExport1);
            export1.Attribute("exportDate").Remove();
            foreach (XElement mod in export1.XPathSelectElements("//object/modified")) { mod.Remove(); }
            export1.Save(fileNameExport1);

            XElement export2 = XElement.Load(fileNameExport2);
            export2.Attribute("exportDate").Remove();
            foreach (XElement mod in export2.XPathSelectElements("//object/modified")) { mod.Remove(); }
            export2.Save(fileNameExport2);

            Assert.AreEqual(export1.ToString(), export2.ToString());

            // delete tempPackage & temp files
        }
    }
}
