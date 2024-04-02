namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater.Tests
{
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProjectFileProcessorTests
    {
        [TestMethod]
        public void ProcessTestRelease_OnlyVersion()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_OnlyVersion.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 0);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestPreRelease_OnlyVersion()
        {
            // Arrange
            string path = "TestProjects/ProcessTestPreRelease_OnlyVersion.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15.21-prerelease", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 0);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing_ClampHighTest()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("256.256.65536", 0);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("255.255.65535", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("255.255.65535", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("255.255.65535", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing_ClampLowTest3()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            Action action = () => processor.Process("-1,-1,-1", 0);

            // Assert
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing_Prerelease3Test()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.5-RC.1", 55757);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.5.55757", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.5.55757", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.5.55757-RC.1", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing_Prerelease4Test()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.5.55-RC.1", 55757);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.5.55757", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.5.55757", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.5.55757-RC.1", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_Nothing_PrereleaseTest()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            Action action = () => processor.Process("1.0.5-RC.1", 0);

            // Assert
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void ProcessTestPreRelease_Nothing()
        {
            // Arrange
            string path = "TestProjects/ProcessTestPreRelease_Nothing.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15.21-prerelease", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestRelease_OnlyPackageVersion()
        {
            // Arrange
            string path = "TestProjects/ProcessTestRelease_OnlyPackageVersion.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 0);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestPreRelease_OnlyPackageVersion()
        {
            // Arrange
            string path = "TestProjects/ProcessTestPreRelease_OnlyPackageVersion.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.AreEqual("1.0.15.21-prerelease", packageVersionElement?.Value, " packageVersionElement.Value");
        }

        [TestMethod]
        public void ProcessTestWix()
        {
            // Arrange
            string path = "TestProjects/ProcessTestWIX.wixproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.IsNull(packageVersionElement);
        }
        
        [TestMethod]
        public void ProcessTestAngular()
        {
            // Arrange
            string path = "TestProjects/ProcessTestAngular.esproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.IsNull(versionElement);
            Assert.IsNull(packageVersionElement);
            Assert.IsNull(productVersionElement);
        }

        [TestMethod]
        public void ProcessTestWeb()
        {
            // Arrange
            string path = "TestProjects/ProcessTestWeb.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.IsNull(packageVersionElement);
        }

        [TestMethod]
        public void ProcessTestAssembly()
        {
            // Arrange
            string path = "TestProjects/ProcessTestAssembly.csproj";
            ProjectFileProcessor processor = new ProjectFileProcessor(path);

            // Act
            processor.Process("1.0.15", 21);

            // Assert
            var doc = XDocument.Load(path);
            XNamespace ns = doc.Root!.GetDefaultNamespace();
            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("x", ns.NamespaceName);

            var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
            var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
            var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

            Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
            Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
            Assert.IsNull(packageVersionElement);
        }
    }
}