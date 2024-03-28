namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater
{
	using System;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;

	using Skyline.DataMiner.CICD.FileSystem;

	/// <summary>
	/// Processes Visual Studio project files to update version and product version attributes.
	/// </summary>
	public class ProjectFileProcessor
	{
		private readonly IFileSystem fs;
		private readonly XNamespace ns;
		private readonly string projectFile;
		private readonly XDocument projectFileDocument;
		private readonly XElement root;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFileProcessor"/> class.
		/// </summary>
		/// <param name="projectFile">The path to the project file to be processed.</param>
		public ProjectFileProcessor(string projectFile)
		{
			this.projectFile = projectFile;
			fs = FileSystem.Instance;

			var projectRaw = fs.File.ReadAllText(projectFile);
			projectFileDocument = XDocument.Parse(projectRaw);
			root = projectFileDocument.Root ?? throw new InvalidOperationException($"Unexpected content in '{projectFile}': Root element is null.");
			ns = root.Name.Namespace;
		}

		/// <summary>
		/// Processes the project file to update its version and product version based on specified parameters.
		/// </summary>
		/// <param name="version">The new version to set in the project file.</param>
		/// <param name="buildNumber">The build number to use in the version.</param>
		public void Process(string version, int buildNumber)
		{
			var sdkAttribute = root.Attribute("Sdk");
			if (sdkAttribute == null) return;


			if (sdkAttribute.Value.StartsWith("Microsoft.NET.Sdk") || sdkAttribute.Value.StartsWith("WixToolset.Sdk"))
			{
				XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
				xnm.AddNamespace("x", ns.NamespaceName);
				var generatePackageOnBuildElement = projectFileDocument.XPathSelectElement("/x:Project/x:PropertyGroup/x:GeneratePackageOnBuild", xnm);

				if (generatePackageOnBuildElement != null && generatePackageOnBuildElement.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					var propGroup = generatePackageOnBuildElement.Parent;
					SetNuGetVersion(version, buildNumber, propGroup);
				}
				else
				{
					var propGroup = projectFileDocument.Root.Elements(ns + "PropertyGroup").FirstOrDefault();
					SetVersion(version, buildNumber, propGroup);
				}
			}
			fs.File.WriteAllText(projectFile, projectFileDocument.ToString());
		}

		/// <summary>
		/// Updates or adds a new project property in the specified property group.
		/// </summary>
		/// <param name="elementName">The name of the property to update.</param>
		/// <param name="value">The value to set for the property.</param>
		/// <param name="propertyGroupElement">The property group element where the property resides.</param>
		private static void UpdateProjectProperty(XName elementName, string value, XElement propertyGroupElement)
		{
			var element = propertyGroupElement.Element(elementName);
			if (element == null)
			{
				propertyGroupElement.Add(new XElement(elementName, value));
			}
			else
			{
				element.Value = value;
			}
		}

		/// <summary>
		/// Sets the version and product version in the project file.
		/// </summary>
		/// <param name="version">The new version to apply. Can include a pre-release suffix separated by a dash ('-').</param>
		/// <param name="revision">The revision number to incorporate into the version. Required if a pre-release suffix is present.</param>
		/// <param name="propertyGroupElement">The property group element to update.</param>
		/// <exception cref="ArgumentException">Thrown when the version format is invalid or a pre-release version does not include a revision number.</exception>
		private void SetVersion(string version, int revision, XElement propertyGroupElement)
		{
			if (propertyGroupElement == null) throw new ArgumentNullException(nameof(propertyGroupElement));

			// Split version string to handle pre-release versions
			var splitVersion = version.Split('-');
			if (splitVersion.Length > 2)
			{
				throw new ArgumentException($"Invalid version format: {version}. Expected format: 'Major.Minor.Build[-Suffix]'.", nameof(version));
			}

			var baseVersion = splitVersion[0];
			bool hasPreRelease = splitVersion.Length == 2;
			if (hasPreRelease && revision == 0)
			{
				throw new ArgumentException("Pre-release version requires a non-zero revision number.", nameof(revision));
			}

			// Attempt to parse the base version
			if (!Version.TryParse(baseVersion, out Version parsedVersion))
			{
				parsedVersion = new Version(1, 0, 0); // Default version if parsing fails
			}

			// Create a new version object with clamped values to ensure compatibility
			Version newVersion;
			if (revision != 0)
			{
				newVersion = new Version(
				   Math.Clamp(parsedVersion.Major, 1, 255),
				   Math.Clamp(parsedVersion.Minor, 0, 255),
				   Math.Clamp(parsedVersion.Build, 0, 65535),
				   revision);
			}
			else
			{
				newVersion = new Version(
					Math.Clamp(parsedVersion.Major, 1, 255),
					Math.Clamp(parsedVersion.Minor, 0, 255),
					Math.Clamp(parsedVersion.Build, 0, 65535));
			}

			// Update the project properties
			UpdateProjectProperty(ns + "Version", newVersion.ToString(), propertyGroupElement);
			UpdateProjectProperty(ns + "ProductVersion", newVersion.ToString(), propertyGroupElement);

			Console.WriteLine($"Updated File {FileSystem.Instance.Path.GetFileName(projectFile)} to version:{newVersion.ToString()}");
		}


		private void SetNuGetVersion(string version, int revision, XElement propertyGroupElement)
		{
			if (propertyGroupElement == null) throw new ArgumentNullException(nameof(propertyGroupElement));

			// Split version string to handle pre-release versions
			var splitVersion = version.Split('-');
			if (splitVersion.Length > 2)
			{
				throw new ArgumentException($"Invalid version format: {version}. Expected format: 'Major.Minor.Build[-Suffix]'.", nameof(version));
			}

			var baseVersion = splitVersion[0];
			bool hasPreRelease = splitVersion.Length == 2;
			if (hasPreRelease && revision == 0)
			{
				throw new ArgumentException("Pre-release version requires a non-zero revision number.", nameof(revision));
			}

			// Attempt to parse the base version
			if (!Version.TryParse(baseVersion, out Version parsedVersion))
			{
				parsedVersion = new Version(1, 0, 0); // Default version if parsing fails
			}

			// Create a new version object with clamped values to ensure compatibility
			Version newVersion;
			if (revision != 0)
			{
				newVersion = new Version(
				   Math.Clamp(parsedVersion.Major, 1, 255),
				   Math.Clamp(parsedVersion.Minor, 0, 255),
				   Math.Clamp(parsedVersion.Build, 0, 65535),
				   revision);
			}
			else
			{
				newVersion = new Version(
					Math.Clamp(parsedVersion.Major, 1, 255),
					Math.Clamp(parsedVersion.Minor, 0, 255),
					Math.Clamp(parsedVersion.Build, 0, 65535));
			}

			// Update the project properties
			UpdateProjectProperty(ns + "Version", newVersion.ToString(), propertyGroupElement);
			UpdateProjectProperty(ns + "ProductVersion", newVersion.ToString(), propertyGroupElement);

			if (hasPreRelease)
			{
				UpdateProjectProperty(ns + "PackageVersion", newVersion.ToString() + "-" + splitVersion[1], propertyGroupElement);
			}
			else
			{
				UpdateProjectProperty(ns + "PackageVersion", newVersion.ToString(), propertyGroupElement);
			}

			Console.WriteLine($"Updated File {FileSystem.Instance.Path.GetFileName(projectFile)} to version:{newVersion.ToString()}");
		}
	}
}