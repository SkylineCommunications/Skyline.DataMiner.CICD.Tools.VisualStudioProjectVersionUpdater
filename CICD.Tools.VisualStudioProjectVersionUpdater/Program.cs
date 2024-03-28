namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater
{
	using System;
	using System.CommandLine;
	using System.IO;
	using System.Linq;
	using System.Threading;

	using Skyline.DataMiner.CICD.FileSystem;
	using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio;

	/// <summary>
	/// A program to update the version of Visual Studio projects.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The entry point of the program.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <returns>Returns 0 on success, non-zero on error.</returns>
		public static int Main(string[] args)
		{
			var versionOption = new Option<string>(
				"--version",
				"The version to apply to the projects. If not provided, an automatic version is generated based on the last stable Git tag and build information.  MSI product versions must have a major version less than 256, a minor version less than 256, and a build version less than 65536. The revision value and labels or metadata provided here are ignored. If not provided an automatic build version will be created based on last stable tag, git information.");
			versionOption.IsRequired = false;

			var revisionOption = new Option<int>(
				"--revision",
				"Often the run or build number of a pipeline. Used as the revision number. Use 0 for a full release.");
			revisionOption.SetDefaultValue(0);
			revisionOption.IsRequired = true;

			var workspaceOption = new Option<string>(
				"--workspace",
				"The workspace path where the solution is located.");
			workspaceOption.IsRequired = false;
			workspaceOption.LegalFilePathsOnly();

			var solutionFilepath = new Option<string?>(
				"--solution-filepath",
				"The filepath to the solution file.");
			solutionFilepath.IsRequired = false;
			solutionFilepath.LegalFilePathsOnly();

			var rootCommand = new RootCommand("Updates all executables and WiX projects within a solution to a specified version or an automatically generated version.")
			{
				versionOption,
				revisionOption,
				workspaceOption,
				solutionFilepath
			};

			rootCommand.SetHandler(ProcessProjects, versionOption, revisionOption, workspaceOption, solutionFilepath);

			return rootCommand.Invoke(args);
		}

		/// <summary>
		/// Attempts to generate an automatic version based on the last stable Git tag.
		/// </summary>
		/// <param name="workspace">The workspace directory where Git commands will be executed.</param>
		/// <returns>The generated version or null if unable to generate.</returns>
		private static string? GenerateAutomaticVersion(string workspace, int revision)
		{
			try
			{
				var shell = ShellFactory.GetShell();
				CancellationTokenSource cts = new CancellationTokenSource();

				var commandSuccess = shell.RunCommand("git tag --sort=-creatordate", out var output, out _, cts.Token, workspace);
				if (!commandSuccess)
				{
					return null;
				}
				string tag = output.Split(Environment.NewLine).FirstOrDefault(tag => !tag.Contains("-"));
				if (revision == 0)
				{
					// Increment the Build number if revision was 0
					var splitTag = tag.Split(".");
					splitTag[2] = Convert.ToString(Convert.ToInt32(splitTag[2]) + 1);
					tag = String.Join(".", splitTag);
				}

				return tag;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to create an automatic version. Exception: {ex}.");
				return null;
			}
		}

		/// <summary>
		/// Processes and updates the project files within the solution with the specified version and revision number.
		/// </summary>
		/// <param name="version">The version to set in the projects. If null or empty, attempts to generate an automatic version based on Git tags.</param>
		/// <param name="revision">The build number to set as the revision number in the projects. Ignored if zero.</param>
		/// <param name="workspace">The workspace directory containing the solution and projects.</param>
		/// <param name="solutionFilepath">The path to the solution file. If null or empty, searches the workspace for a solution file.</param>
		/// <exception cref="ArgumentException">Thrown if the workspace path is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if no solution file is found within the workspace.</exception>
		private static void ProcessProjects(string version, int revision, string workspace, string? solutionFilepath)
		{
			if (String.IsNullOrWhiteSpace(workspace))
			{
				workspace = Directory.GetCurrentDirectory();
			}

			if (String.IsNullOrWhiteSpace(solutionFilepath))
			{
				solutionFilepath = FileSystem.Instance.Directory.EnumerateFiles(workspace, "*.sln").FirstOrDefault();
			}

			if (String.IsNullOrWhiteSpace(solutionFilepath))
			{
				throw new InvalidOperationException($"No solution file found in workspace: {workspace}.");
			}

			if (String.IsNullOrWhiteSpace(version))
			{
				// Automatic version generation based on Git tags.
				version = GenerateAutomaticVersion(workspace, revision) ?? "1.0.0";
			}

			Solution solution = Solution.Load(solutionFilepath);
			var allProjects = solution.Projects.Select(p => p.AbsolutePath).ToList();

			foreach (var projectFile in allProjects)
			{
				var projectFileProcessor = new ProjectFileProcessor(projectFile);
				projectFileProcessor.Process(version, revision);
			}
		}
	}
}