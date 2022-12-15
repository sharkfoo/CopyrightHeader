//
// © Copyright 2022 HP Development Company, L.P.
//
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CopyrightHeader;
using Task = System.Threading.Tasks.Task;

namespace CopyrightHeaderExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyrightHeaderCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b4b6c10f-c873-4bcc-b512-70e5e8038bdd");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyrightHeaderCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CopyrightHeaderCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CopyrightHeaderCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CopyrightHeaderCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CopyrightHeaderCommand(package, commandService);
        }

        public void Usage(string msg)
        {
            string title = "CopyrightHeaderCommand";

            VsShellUtilities.ShowMessageBox(
                this.package,
                msg,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var lineCount = 10;

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;

            if (dte?.ActiveDocument?.Object() is TextDocument activeDoc)
            {
                var inputFile = activeDoc.Parent.FullName;
                var outputFile = inputFile;
                if (!activeDoc.Parent.Saved)
                {
                    activeDoc.Parent.Save();
                }
                var template = CopyrightUtil.ReadTemplate(inputFile, "hp", Usage);
                if (template != null)
                {
                    var buffer = CopyrightUtil.ReadFile(inputFile);
                    if (buffer.Count > 0)
                    {
                        if (lineCount > buffer.Count)
                        {
                            lineCount = buffer.Count;
                        }
                        var copyright = new Copyright(template);
                        if (copyright.FindCurrentCopyright(buffer, lineCount))
                        {
                            return;
                        }

                        copyright.AddOrModifyCopyright(buffer, lineCount);
                        CopyrightUtil.WriteFile(outputFile, buffer);
                    }
                }
            }
        }
    }
}
