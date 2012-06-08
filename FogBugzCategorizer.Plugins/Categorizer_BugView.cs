using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.UI.EditableTable;

namespace FogBugzCategorizer.Plugins
{
    public partial class CategorizerGridView : IPluginBugDisplay
	{
		#region Implementation of IPluginBugDisplay

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
            var table = new CEditableTable("table");
		    table.Header.AddCell("Project").sWidth = "330px";
            table.Header.AddCell("Task").sWidth = "330px";
            
		    var row = new CEditableTableRow();
		    row.AddCell(Forms.SelectInput("project", new[] {"Project 1", "Project 2", "Project 3"}));
            row.AddCell(Forms.SelectInput("task", new[] { "Task 1", "Task 2", "Task 3" }));
		    table.Body.AddRow(row);

            table.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(table.sId, "dlgAdd", "footer", string.Concat(rgbug[0].BugLink(), "&newSplit=1")));
            table.Footer.AddCell(CEditableTable.LinkShowDialog(table.sId, "dlgAdd", "footer", string.Concat(rgbug[0].BugLink(), "&newSplit=1"), "Add New Split"));

            var dialog = new CSingleColumnDialog { sTitle = "Add New Split" };
            dialog.Items.Add(new CDialogItem(Forms.CheckboxInput("checkbox", "Check Yes Or No", false)));
            dialog.Items.Add(CEditableTable.DialogItemOkCancel(table.sId));

		    var templateNew = new CDialogTemplate {Template = dialog};
		    table.AddDialogTemplate("dlgAdd", templateNew);

            var displayItem = new CBugDisplayDialogItem("BugDisplayViewTop")
            {
                sLabel = "Time-Split Categorization",
                iColumnSpan = 4,
                sContent = table.RenderHtml()
            };

			return new[] { displayItem };
		}

		public CBugDisplayDialogItem[] BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		#endregion
	}
}