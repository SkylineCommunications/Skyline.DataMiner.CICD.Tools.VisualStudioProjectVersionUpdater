namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater.Tests
{
	using Skyline.DataMiner.CICD.FileSystem;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater;
	using System.Xml.Linq;
	using System.Xml;
	using System.Xml.XPath;
	using FluentAssertions;

	[TestClass()]
	public class ProjectFileProcessorTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext)
		{
			var fs = FileSystem.Instance;
			if (!fs.Directory.Exists(fs.Path.GetFullPath("TestProjects")))
			{
				System.IO.Compression.ZipFile.ExtractToDirectory(fs.Path.GetFullPath("TestProjects.zip"), "TestProjects");
			}
		}

		[TestInitialize]
		public void TestInitialize()
		{
			var fs = FileSystem.Instance;
			if (fs.Directory.Exists("TempFolder"))
			{
				fs.Directory.DeleteDirectory("TempFolder");
			}

			fs.Directory.CopyRecursive("TestProjects", "TempFolder");
		}


		[TestMethod]
		public void ProcessTestRelease_OnlyVersion()
		{
			// Arrange
			string path = "TempFolder/ProcessTestRelease_OnlyVersion.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 0);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestPreRelease_OnlyVersion.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
			XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
			xnm.AddNamespace("x", ns.NamespaceName);

			var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
			var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
			var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

			Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
			Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
			Assert.AreEqual("1.0.15.21", packageVersionElement?.Value, " packageVersionElement.Value");
		}

		[TestMethod]
		public void ProcessTestRelease_Nothing()
		{
			// Arrange
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 0);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("256.256.65536", 0);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			Action A = () => processor.Process("-1,-1,-1", 0);

			// Assert
			A.Should().ThrowExactly<ArgumentException>();
		}

		[TestMethod]
		public void ProcessTestRelease_Nothing_Prerelease3Test()
		{
			// Arrange
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.5-RC.1", 55757);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.5.55-RC.1", 55757);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			Action A = () => processor.Process("1.0.5-RC.1", 0);

			// Assert
			A.Should().ThrowExactly<ArgumentException>();
		}


		[TestMethod]
		public void ProcessTestPreRelease_Nothing()
		{
			// Arrange
			string path = "TempFolder/ProcessTestPreRelease_Nothing.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
			XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
			xnm.AddNamespace("x", ns.NamespaceName);

			var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
			var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
			var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

			Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
			Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
			Assert.AreEqual("1.0.15.21", packageVersionElement?.Value, " packageVersionElement.Value");
		}

		[TestMethod]
		public void ProcessTestRelease_OnlyPackageVersion()
		{
			// Arrange
			string path = "TempFolder/ProcessTestRelease_OnlyPackageVersion.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 0);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestPreRelease_OnlyPackageVersion.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
			XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
			xnm.AddNamespace("x", ns.NamespaceName);

			var versionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:Version", xnm);
			var packageVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:PackageVersion", xnm);
			var productVersionElement = doc.XPathSelectElement("/x:Project/x:PropertyGroup/x:ProductVersion", xnm);

			Assert.AreEqual("1.0.15.21", versionElement?.Value, "versionElement.Value");
			Assert.AreEqual("1.0.15.21", productVersionElement?.Value, " productVersionElement.Value");
			Assert.AreEqual("1.0.15.21", packageVersionElement?.Value, " packageVersionElement.Value");
		}

		[TestMethod]
		public void ProcessTestWix()
		{
			// Arrange
			string path = "TempFolder/ProcessTestWIX.wixproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestAngular.esproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestWeb.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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
			string path = "TempFolder/ProcessTestAssembly.csproj";
			ProjectFileProcessor processor = new ProjectFileProcessor(path);

			// Act
			processor.Process("1.0.15", 21);

			// Assert
			var doc = XDocument.Load(path);
			XNamespace ns = doc.Root.GetDefaultNamespace();
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