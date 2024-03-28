namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater
{
	using System;
	using System.Threading;

	/// <summary>
	/// Allows running commands on the shell
	/// </summary>
	public interface IShell
	{
		/// <summary>
		/// Runs the given command on the shell
		/// </summary>
		/// <param name="command">the command to run</param>
		/// <param name="output">Any output from running the command</param>
		/// <param name="errors">Any error from the command</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that controls the cancellation of the command</param>
		/// <param name="workingDirectory">Optional working directory in which the command should be run</param>
		/// <returns><see cref="bool.TrueString"/> if there were no errors with the command</returns>
		bool RunCommand(string command, out string output, out string errors, CancellationToken cancellationToken, string workingDirectory = "");
	}

	/// <summary>
	/// Helper methods for <see cref="IShell"/>
	/// </summary>
	public static class ShellFactory
	{
		/// <summary>
		/// Get your shiny shells here! Tailored specifically to your OS!
		/// </summary>
		/// <returns>Shiny shell</returns>
		/// <exception cref="NotSupportedException">Can't create a shell for this OS.</exception>
		public static IShell GetShell()
		{
			if (OperatingSystem.IsWindows())
			{
				return new WindowsShell();
			}

			if (OperatingSystem.IsLinux())
			{
				return new UnixShell();
			}

			throw new NotSupportedException($"The current operating system ({System.Runtime.InteropServices.RuntimeInformation.OSDescription}) is not supported.");
		}
	}
}