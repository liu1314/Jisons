using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Jisons;
using System.Linq;

namespace 迹I柳燕.文档整理
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\10.0")]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guid文档整理PkgString)]
    public sealed class 文档整理Package : Package
    {

        DTE dte;
        IVsUIShell uiShell;

        public 文档整理Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            dte = GetService(typeof(DTE)) as DTE;
            uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID menuCommandID = new CommandID(GuidList.guid文档整理CmdSet, (int)PkgCmdIDList.DocumentationCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        /// <summary> asdfsd
        ///           dsfg </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var activedocument = this.dte.GetActiveDocument();
            if (activedocument != null)
            {
                var activetextdocument = activedocument.GetTextDocument();
                var selection = (TextSelection)activedocument.Selection;
                if (!string.IsNullOrWhiteSpace(selection.Text))
                {
                    var colum = selection.CurrentColumn <= selection.AnchorColumn ? selection.CurrentColumn : selection.AnchorColumn;
                    var ds = DescriptionMethod.Documentation(selection.Text, colum);
                    activetextdocument.ReplacePattern(selection.Text, ds);
                    this.dte.OutPut(" summary 已格式化 ...");
                }
                else
                {
                    var text = activetextdocument.GetText();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var dischars = text.Distinct().ToList();
                        foreach (var c in dischars)
                        {
                            if ((int)c > 65000)
                            {
                                if (StringHelper.DoubleByte.Contains(c))
                                {
                                    var oldstr = c.ToString();
                                    var newstr = c.ConvertToSingleByte().ToString();
                                    activetextdocument.ReplacePattern(oldstr, newstr);
                                    this.dte.OutPut(oldstr + " => " + newstr);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
