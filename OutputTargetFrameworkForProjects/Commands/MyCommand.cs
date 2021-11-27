using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace OutputTargetFrameworkForProjects
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            await CreateAndOpenOutputFileAsync().ConfigureAwait(false);

            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync().ConfigureAwait(false);
            if (docView?.TextView == null) return;
            SnapshotPoint position = docView.TextView.Caret.Position.BufferPosition;

            var projects = await VS.Solutions.GetAllProjectsAsync();
            projects.ToList()
                    .ForEach(project => docView.TextBuffer?.Insert(position, OutputProjectInfo(project)));
        }

        private static async Task CreateAndOpenOutputFileAsync()
        {
            var directory = @"C:\temp";
            var solution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(false);
            var solutionName = string.IsNullOrEmpty(solution.Name)
                                ? "Unknown solution"
                                : solution.Name.Replace(".sln", "");
            var fileName = solutionName + " project target frameworks.txt";
            var path = Path.Combine(directory, fileName);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (File.Exists(path))
                File.Delete(path);
            File.Create(path).Close();

            await VS.Documents.OpenAsync(path);
        }

        private string OutputProjectInfo(Project project)
        {
            var sb = new StringBuilder();
            sb.Append(project.Name);
            sb.Append("|");
            sb.Append(project.GetAttributeAsync("TargetFrameworkMoniker").GetAwaiter().GetResult());
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
