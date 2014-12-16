using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Jisons
{
    public static class DocumentHelper
    {

        public static string GetText(this EditPoint startPoint, EditPoint endPoint)
        {
            return startPoint.GetText(endPoint);
        }

        public static string GetText(this TextDocument textDocument)
        {
            var startPoint = textDocument.StartPoint.CreateEditPoint();
            var endPoint = textDocument.EndPoint.CreateEditPoint();
            return startPoint.GetText(endPoint);

            //startPoint.ReplaceText(endPoint, xamlSource, 0);
        }

        public static Document GetActiveDocument(this DTE dte)
        {
            return dte.ActiveDocument.ActiveWindow != null ? dte.ActiveDocument.ActiveWindow.Document : null;
        }

        public static TextDocument GetTextDocument(this Document document)
        {
            return (TextDocument)document.Object("TextDocument");
        }

        public static TextDocument GetActiveTextDocument(this DTE dte)
        {
            return (TextDocument)dte.GetActiveDocument().Object("TextDocument");
        }

        public static void ActiveDocumentsAction(this DTE dte, Func<TextDocument, bool> func, bool issave = true)
        {
            foreach (var item in dte.ActiveDocument.Collection)
            {
                var document = item as Document;
                if (document != null)
                {
                    var selection = document.Selection;
                    if (selection != null)
                    {
                        var textDocument = document.GetTextDocument();
                        //var abbb = textDocument.ReplaceText("！", "!");
                        if (func.Invoke(textDocument) && issave)
                        {
                            document.Save();
                        }
                    }
                }
            }
        }

    }
}
