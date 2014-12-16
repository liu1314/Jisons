using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Jisons
{
    public static class DTEHelper
    {

        public static CommandEvents GetSaveSelectItemEvent(this DTE dte)
        {
            return dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 331];
        }

        public static CommandEvents GetSaveAllSelectItemEvent(this DTE dte)
        {
            return dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 224];
        }

        public static void OutPut(this DTE dte, string message, string panename = "")
        {
            OutPut(dte, message, true, panename);
        }

        public static void OutPut(this DTE dte, string message, bool isnewline, string panename = "")
        {
            var outputwindow = ((DTE2)dte).ToolWindows.OutputWindow;
            EnvDTE.OutputWindowPane pane = outputwindow.ActivePane;
            if (!string.IsNullOrWhiteSpace(panename))
            {
                pane = null;
                foreach (OutputWindowPane item in outputwindow.OutputWindowPanes)
                {
                    if (item.Name.Equals(panename))
                    {
                        pane = item;
                        break;
                    }
                }
                if (pane == null)
                {
                    pane = outputwindow.OutputWindowPanes.Add(panename);
                }
            }
            pane.OutputString(message + (isnewline ? Environment.NewLine : ""));
        }

    }
}
