using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace OutputTargetFrameworkForProjects
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var fileName = @"project-versions.txt";
            var path = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory, fileName);

            if (File.Exists(path))
                File.Delete(path);
            File.Create(path);
            await VS.Documents.OpenAsync(path);

            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView == null) return;
            SnapshotPoint position = docView.TextView.Caret.Position.BufferPosition;

            var projects = await VS.Solutions.GetAllProjectsAsync();
            projects.ToList().ForEach(project => docView.TextBuffer?.Insert(position, OutputProjectInfo(project)));
        }

        private string OutputProjectInfo(Project project)
        {
            var sb = new StringBuilder();
            sb.Append(project.Name + "|");
            sb.Append("|");
            sb.Append(project.GetAttributeAsync("TargetFrameworkMoniker").GetAwaiter().GetResult());
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}
