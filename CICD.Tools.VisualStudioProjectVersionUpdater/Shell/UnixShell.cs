namespace Skyline.DataMiner.CICD.Tools.VisualStudioProjectVersionUpdater
{
	using System.Diagnostics;
	using System.Text;
	using System.Threading;

	/// <summary>
	/// Allows running commands on the unix shell
	/// </summary>
	internal class UnixShell : IShell
	{
		/// <inheritdoc/>
		public bool RunCommand(string command, out string output, out string errors, CancellationToken cancellationToken, string workingDirectory = "")
		{
			StringBuilder outputStream = new StringBuilder();
			StringBuilder errorStream = new StringBuilder();
			string escapedArgs = command.Replace("\"", "\\\"");
			bool success = true;
			using Process cmd = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					FileName = "/bin/bash",
					Arguments = $"-c \"{escapedArgs}\"",
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					WorkingDirectory = workingDirectory
				}
			};

			cmd.OutputDataReceived += (sender, args) => { outputStream.Append(args.Data); };
			cmd.ErrorDataReceived += (sender, args) => { errorStream.Append(args.Data); };
			cmd.Start();
			cmd.BeginOutputReadLine();
			cmd.BeginErrorReadLine();
			cmd.WaitForExitAsync(cancellationToken).GetAwaiter().GetResult();
			if (!cmd.HasExited)
			{
				success = false;
				cmd.Kill();
			}

			output = outputStream.ToString();
			errors = errorStream.ToString();

			success &= cmd.ExitCode == 0;
			return success;
		}
	}
}