/* Copyright 2009 Fog Creek Software, Inc. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;

/* FogBugz namespaces-- make sure you add the neccesary assembly references to
 * the following DLL files contained in C:\Program Files\FogBugz\Website\bin\ 
 * FogBugz.dll, FogCreek.Plugins.dll, FogCreek.Plugins.InterfaceEvents.dll     */
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.Globalization;
using System.Collections;

namespace IPluginFilterJoin_Display_Commit_Example
{
	/* Class Declaration: Inherit from Plugin, expose IPluginFilterJoin, IPluginFilterDisplay,
	 * IPluginFilterCommit */
	public class IPluginFilterJoin_Display_Commit_Example : Plugin, IPluginFilterJoin,
		IPluginFilterDisplay, IPluginFilterCommit, IPluginFilterOptions, IPluginBugJoin,
		IPluginBugDisplay, IPluginBugCommit, IPluginGridColumn, IPluginDatabase
	{
		/* The plugin Id is a required argument for CFilter.SetPluginField and 
		 * CFilter.GetPluginField */

		protected const string PLUGIN_ID =
			"IPluginFilterJoin_Display_Commit_Example@fogcreek.com";

		/* We'll need this member variable to store the "awesomeness" value in the 
		* pre-commit phase, so that if we need to roll back we can replace the original
		* value. */

		protected int preCommitAwesomenessLevel = 0;

		string[] rgsAwesomenessLevels = null;
		string[] rgsAwesomenessIxs = null;

		protected string sBugAwesomenessTableName;
		protected string sAwesomenessTableName;

		/* Constructor: We'll initialize the inherited Plugin class, which 
		 * takes the passed instance of CPluginApi and sets its "api" member variable. */
		public IPluginFilterJoin_Display_Commit_Example(CPluginApi api)
			: base(api)
		{
			/* Create a set of awesomeness levels for use by all plugin interfaces */
			rgsAwesomenessLevels = new string[] { "-- None specified --", "Low", "Moderate", "High", "Extreme" };
			rgsAwesomenessIxs = new string[] { "1", "2", "3", "4", "5" };

			/* Use the API to get our plugin database table names */
			sBugAwesomenessTableName = api.Database.PluginTableName("BugAwesomeness");
			sAwesomenessTableName = api.Database.PluginTableName("Awesomeness");
		}

		#region IPluginFilterJoin Members

		public string[] FilterJoinTables()
		{
			/* All tables specified here must have an integer ixFilter column and an
			 * ixPerson column so FogBugz can perform the necessary join. */

			return new string[] { "FilterAwesomeness" };
		}

		#endregion


		#region IPluginFilterDisplay Members

		public CDialogItem[] FilterDisplayEdit(CFilter Filter)
		{
			/* if you specify one or more CFilterOptions, they will appear
			 * on the edit filter page automatically. You don't need to
			 * explicitly add dialog items here. */
			return null;
		}

		public string[] FilterDisplayListFields(CFilter Filter)
		{
			int ixAwesomeness = Convert.ToInt32(
				Filter.GetPluginField(PLUGIN_ID, "ixAwesomeness"));
			/* including dashes to match those automatically added on the
			 * edit filter page, so the user knows it's not the literal
			 * awesomeness value "Any" */
			string s = (ixAwesomeness < 2) ?
					   "-- Any --" :
					   GetAwesomenessString(ixAwesomeness);

			return new string[] { s };
		}

		public string[] FilterDisplayListHeaders()
		{
			return new string[] { "Level of Awesomeness" };
		}

		#endregion


		#region IPluginFilterCommit Members

		public void FilterCommitAfter(CFilter Filter)
		{

		}

		public bool FilterCommitBefore(CFilter Filter)
		{
			/* Set the preCommitAwesomenessLevel member variable */

			if (api.Request[api.AddPluginPrefix("ixAwesomeness")] != null)
			{
				int ixAwesomenessNew = -1;

				preCommitAwesomenessLevel = Convert.ToInt32(Filter.GetPluginField(PLUGIN_ID,
						"ixAwesomeness"));

				/* use tryparse in case the URL querystring value isn't a valid integer */
				if (Int32.TryParse(api.Request[api.AddPluginPrefix("ixAwesomeness")].ToString(), out ixAwesomenessNew) &&
					(ixAwesomenessNew > -1)
				)
					Filter.SetPluginField(PLUGIN_ID, "ixAwesomeness",
						ixAwesomenessNew);
			}

			return true;

		}

		public void FilterCommitRollback(CFilter Filter)
		{
			/* if the filter commit was aborted, reset the awesomeness
			 * level to the pre-commit value */
			Filter.SetPluginField(PLUGIN_ID, "ixAwesomness", preCommitAwesomenessLevel);
		}

		#endregion

		#region IPluginFilterOptions Members

		public CFilterOption[] FilterOptions(CFilter filter)
		{
			/* Create a new filter option to be included in the list of options
			 * available in the "Filter:" drop-down at the top of the case
			 * list. */

			int ixAwesomeness =
				ToAwesomenessSelectRange(
				Convert.ToInt32(filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")));

			CFilterOption fo = api.Filter.NewFilterOption();

			/* Specify a single-select drop-down, and add a default value of "any" */

			fo.SetSelectOne(rgsAwesomenessLevels,
			   Convert.ToString(ixAwesomeness), rgsAwesomenessIxs);
			fo.SetDefault("0", "Any");
			fo.sQueryParam = api.PluginPrefix + "ixAwesomeness";
			fo.fShowDialogItem = true;
			fo.sHeader = "Level of Awesomeness";
			fo.sName = "awesomeness";
			return new CFilterOption[] { fo };
		}

		public CSelectQuery FilterOptionsQuery(CFilter filter)
		{
			/* Specify a query for the list of cases to be returned when the
			 * filter is imposed. */

			int ixAwesomeness =
				ToAwesomenessSelectRange(
				Convert.ToInt32(filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")));

			CSelectQuery query = api.Database.NewSelectQuery("Bug");
			if (ixAwesomeness == 0)
				return query;

			query.AddLeftJoin(sBugAwesomenessTableName, "Bug.ixBug = joinedBug.ixBug", "joinedBug");
			query.SetParamInt("ixAwesomeness", ixAwesomeness);

			if (ixAwesomeness == 1)
			{
				query.AddWhere(
					@"joinedBug.ixAwesomeness IS NULL OR
                    joinedBug.ixAwesomeness = @ixAwesomeness");
			}
			else
			{
				query.AddWhere("joinedBug.ixAwesomeness = @ixAwesomeness");
			}

			return query;
		}

		public CFilterStringList FilterOptionsString(CFilter filter)
		{
			int ixAwesomeness = Convert.ToInt32(
				filter.GetPluginField(PLUGIN_ID, "ixAwesomeness"));

			CFilterStringList list = new CFilterStringList();

			/* Return a string for the "Filter:" message at the top of the case list 
			 * so that the the user can clearly interpret current filter settings.
			 * ixAwesomeness = 0 will correspond to "any level of awesomeness," so
			 * no message is shown at all in that case. */

			string sFilterString;
			/* if there is an awesomeness in the range of possible values,
			 * generate the filter string */
			if (ixAwesomeness > 0 && ixAwesomeness <= rgsAwesomenessIxs.Length)
			{
				if (ixAwesomeness == 1)
				{
					sFilterString = "with no level of awesomeness specified";
				}
				else
				{
					sFilterString = String.Format(@"with awesomeness level ""{0}""",
						GetAwesomenessString(ixAwesomeness));
				}
				/* the second parameter to CFilterStringList.Add specifies the
				 * CFilterOption by name to display when the text is clicked */
				list.Add(sFilterString, "awesomeness");
			}
			return list;

		}

		public string FilterOptionsUrlParams(CFilter filter)
		{
			/* To make the filter saveable, we need to assign a querystring
			 * parameter to this filter setting. */

			int ixAwesomeness = Convert.ToInt32(
				filter.GetPluginField(PLUGIN_ID, "ixAwesomeness"));

			if (ixAwesomeness < 1)
				return "";
			else
				return api.PluginPrefix + "ixAwesomeness="
					+ Convert.ToString(ixAwesomeness);
		}

		#endregion

		#region IPluginFilterBugEntry Members

		public bool FilterBugEntryCanCreate(CFilter filter)
		{
			//For this plugin example, we won't offer users an "Add Case" control in the
			//grid view if the current filter has a level of awesomeness of "Low."

			if (Convert.ToInt32(filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")) == 2)
				return false;
			else
				return true;
		}

		public string FilterBugEntryUrlParams(CFilter filter)
		{
			return api.PluginPrefix +
				"ixAwesomeness=" + Convert.ToString(
				Convert.ToInt32(filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")));
		}

		#endregion

		protected string GetAwesomenessSelect(CFilter Filter)
		{
			List<string> rgsAwesomenessLevels;
			List<string> rgsAwesomenessIxs;

			int ixSelectedIndex = 0;

			CSelectQuery sq = api.Database.NewSelectQuery(sAwesomenessTableName);

			sq.AddSelect("*");

			/* Iterate through the data set and populate the drop-down */
			DataSet ds = sq.GetDataSet();

			if (ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
				return string.Empty;

			int numOptions = ds.Tables[0].Rows.Count;

			rgsAwesomenessLevels = new List<string>();
			rgsAwesomenessIxs = new List<string>();

			for (int i = 0; i < numOptions; i++)
			{
				rgsAwesomenessLevels.Add(ds.Tables[0].Rows[i]["sAwesomenessLevel"].ToString());
				rgsAwesomenessIxs.Add(ds.Tables[0].Rows[i]["ixAwesomeness"].ToString());
			}

			/* If there's already an "Awesomeness" value, set the selected index of the drop-down
			 * to the proper non-zero value */

			if (Convert.ToInt32(Filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")) != 0)
				ixSelectedIndex = Convert.ToInt32(
					Filter.GetPluginField(PLUGIN_ID, "ixAwesomeness")) - 1;

			return Forms.SelectInput(api.PluginPrefix + "ixAwesomeness",
				rgsAwesomenessLevels.ToArray(),
				rgsAwesomenessIxs[ixSelectedIndex],
				rgsAwesomenessIxs.ToArray());
		}

		protected string GetAwesomenessString(int ixAwesomeness)
		{

			if (ixAwesomeness < 1)
				return "-- Not Specified --";

			string s = "";

			CSelectQuery sq =
				api.Database.NewSelectQuery(sAwesomenessTableName);
			sq.AddSelect(sAwesomenessTableName + ".sAwesomenessLevel");
			sq.AddWhere
				(sAwesomenessTableName + ".ixAwesomeness = @ixAwesomeness");
			sq.SetParamInt("ixAwesomeness", ixAwesomeness);

			DataSet ds = sq.GetDataSet();

			if (ds.Tables[0] != null)
				s = ds.Tables[0].Rows[0][0].ToString();

			ds.Dispose();
			return s;

		}

		protected int ToAwesomenessSelectRange(int num)
		{
			if (num < 0)
				return 0;
			else if (num > rgsAwesomenessIxs.Length)
				return rgsAwesomenessIxs.Length;
			else
				return num;
		}

		#region IPluginDatabase Members

		public CTable[] DatabaseSchema()
		{
			/* for this plugin, we'll need a table repesenting the possible levels of
			 * Awesomeness, and a Filter-to-Awesomeness table to allow for a join */

			CTable Awesomeness = api.Database.NewTable(api.Database.PluginTableName("Awesomeness"));
			Awesomeness.sDesc = "Levels of Awesomeness";
			Awesomeness.AddAutoIncrementPrimaryKey("ixAwesomeness");
			Awesomeness.AddTextColumn("sAwesomenessLevel", "Level of Awesomeness");

			CTable FilterAwesomeness = api.Database.NewTable(api.Database.PluginTableName("FilterAwesomeness"));
			FilterAwesomeness.sDesc = "Assigns Filters to levels of Awesomeness";
			FilterAwesomeness.AddAutoIncrementPrimaryKey("ixFilterAwesomeness");
			FilterAwesomeness.AddIntColumn("ixFilter", true, 1);
			FilterAwesomeness.AddIntColumn("ixPerson", true, 1);
			FilterAwesomeness.AddIntColumn("ixAwesomeness", true, 0);

			CTable BugAwesomeness = api.Database.NewTable(api.Database.PluginTableName("BugAwesomeness"));
			BugAwesomeness.sDesc = "Assigns bugs to levels of Awesomeness";
			BugAwesomeness.AddAutoIncrementPrimaryKey("ixBugAwesomeness");
			BugAwesomeness.AddIntColumn("ixBug", true, 1);
			BugAwesomeness.AddIntColumn("ixAwesomeness", true, 0);

			return new CTable[] { Awesomeness, FilterAwesomeness, BugAwesomeness };

		}

		public int DatabaseSchemaVersion()
		{
			return 1;
		}

		public void DatabaseUpgradeAfter(int ixVersionFrom, int ixVersionTo,
			CDatabaseUpgradeApi apiUpgrade)
		{
			/* Create 5 different awesomeness levels. Note that we'll have a "None 
			 * specified" level, which our plugin will treat the same as NULL. */

			for (int i = 0; i < rgsAwesomenessLevels.Length; i++)
			{
				CInsertQuery iq = api.Database.NewInsertQuery(sAwesomenessTableName);

				iq.InsertString("sAwesomenessLevel", rgsAwesomenessLevels[i]);
				iq.Execute();
			}
		}

		public void DatabaseUpgradeBefore(int ixVersionFrom, int ixVersionTo,
			CDatabaseUpgradeApi apiUpgrade)
		{

		}

		#endregion

		#region IPluginBugJoin Members

		public string[] BugJoinTables()
		{
			/* All tables specified here mush have an integer ixBug column so FogBugz can
			 * perform the necessary join. */

			return new string[] { "BugAwesomeness" };
		}

		#endregion

		#region IPluginBugDisplay Members

		public CBugDisplayDialogItem[]
			BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			/* We're returning 1 dialog items: a drop-down box allowing the user to 
			 * select a level of awesomeness. */

			CBugDisplayDialogItem dItem1 = new CBugDisplayDialogItem("awesomeness");
			dItem1.sLabel = "Level of Awesomeness";
			dItem1.sContent = GetAwesomenessSelect(rgbug);

			return new CBugDisplayDialogItem[] { dItem1 };
		}

		public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug,
			BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			/* if GetPluginField returns null or -1 ("Not specified"), don't show anything
			 * in display mode. */

			int ixAwesomeness =
				Convert.ToInt32(rgbug[0].GetPluginField(PLUGIN_ID, "ixAwesomeness"));
			if (ixAwesomeness < 2) return null;

			/* Note: The plugin field may be NULL, but Convert.ToInt32 can be used in this
			 * case to make sure that NULL is converted to 0 without an exception */
			string sAwesomeness = GetAwesomenessString(
					Convert.ToInt32(rgbug[0].GetPluginField(PLUGIN_ID, "ixAwesomeness"))
			);
			CBugDisplayDialogItem dItem1 = new CBugDisplayDialogItem(
				"awesomeness",
				sAwesomeness,
				"Level of Awesomeness"
			);

			return new CBugDisplayDialogItem[] { dItem1 };
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		#endregion

		protected bool PluginFieldVaries(CBug[] rgBug, string sKey)
		{
			if (rgBug == null || rgBug.Length == 0) return false;

			object start = rgBug[0].GetPluginField(PLUGIN_ID, sKey);

			/* If we find a different value from the first, return true. */

			if (start != null)
			{
				for (int i = 1; i < rgBug.Length; i++)
					if (rgBug[i].GetPluginField(PLUGIN_ID, sKey) == null ||
						!rgBug[i].GetPluginField(PLUGIN_ID, sKey).Equals(start))
						return true;
			}
			else
			{
				for (int i = 1; i < rgBug.Length; i++)
					if (rgBug[i].GetPluginField(PLUGIN_ID, sKey) != null)
						return true;
			}

			return false;
		}

		protected string GetAwesomenessSelect(CBug[] rgbug)
		{
			List<string> rgsAwesomenessLevels;
			List<string> rgsAwesomenessIxs;

			/* If we're editing multiple bugs, then we need to put in a
			 * "no change" option if those bugs have varying values for
			 * ixAwesomeness. */

			bool fFieldVaries = PluginFieldVaries(rgbug, "ixAwesomeness");
			int ixSelectedIndex = 0;

			CSelectQuery sq = api.Database.NewSelectQuery(sAwesomenessTableName);

			sq.AddSelect("*");

			/* Iterate through the data set and populate the drop-down */
			DataSet ds = sq.GetDataSet();

			if (ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
				return string.Empty;

			int numOptions = ds.Tables[0].Rows.Count;

			rgsAwesomenessLevels = new List<string>();
			rgsAwesomenessIxs = new List<string>();

			for (int i = 0; i < numOptions; i++)
			{
				rgsAwesomenessLevels.Add(ds.Tables[0].Rows[i]["sAwesomenessLevel"].ToString());
				rgsAwesomenessIxs.Add(ds.Tables[0].Rows[i]["ixAwesomeness"].ToString());
			}

			if (fFieldVaries)
			{
				/* Insert a "No Change" value (-1); if this is selected we know
				 * not to change the plugin field value in BugCommitBefore */

				rgsAwesomenessLevels.Insert(0, "-- No Change --");
				rgsAwesomenessIxs.Insert(0, "-1");
			}

			/* If there's already an "Awesomeness" value, set the selected index of the drop-down
			 * to the proper non-zero value */

			if (rgbug.Length > 0 && !fFieldVaries &&
				Convert.ToInt32(rgbug[0].GetPluginField(PLUGIN_ID, "ixAwesomeness")) != 0)
				ixSelectedIndex = Convert.ToInt32(
					rgbug[0].GetPluginField(PLUGIN_ID, "ixAwesomeness")) - 1;

			return Forms.SelectInput(api.PluginPrefix + "ixAwesomeness",
				rgsAwesomenessLevels.ToArray(), rgsAwesomenessIxs[ixSelectedIndex],
				rgsAwesomenessIxs.ToArray());
		}

		#region IPluginBugCommit Members

		public void BugCommitAfter(CBug bug, BugAction nBugAction, CBugEvent bugevent,
			bool fPublic)
		{

		}

		public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
			bool fPublic)
		{
			/* Set the preCommitAwesomenessLevel member variable, but only set the plugin
			 * field value if it's not -1 (no change) */

			if (api.Request[api.AddPluginPrefix("ixAwesomeness")] != null)
			{
				preCommitAwesomenessLevel = Convert.ToInt32(bug.GetPluginField(PLUGIN_ID,
						"ixAwesomeness"));

				if (Convert.ToInt32(api.Request[api.AddPluginPrefix("ixAwesomeness")]) > 0)
					bug.SetPluginField(PLUGIN_ID, "ixAwesomeness",
						Convert.ToInt32(api.Request[api.AddPluginPrefix("ixAwesomeness")]));
			}

			return;
		}

		public void BugCommitRollback(CBug bug, BugAction nBugAction, bool fPublic)
		{

		}

		#endregion

		#region IPluginGridColumn Members

		public string[] GridColumnDisplay(CGridColumn col, CBug[] rgBug, bool fPlainText)
		{
			string[] sValues = new string[rgBug.Length];

			for (int i = 0; i < rgBug.Length; i++)
			{
				if (rgBug[i].GetPluginField(PLUGIN_ID, "ixAwesomeness") == null)
					sValues[i] = String.Empty;
				else
					sValues[i] = HttpUtility.HtmlEncode(
						GetAwesomenessString(
							Convert.ToInt32(rgBug[i].GetPluginField(PLUGIN_ID, "ixAwesomeness"))
						)
					);
			}
			return sValues;
		}

		public CBugQuery GridColumnQuery(CGridColumn col)
		{
			/* Return a CBugQuery with the data you need joined
			 * in. If your table is already joined to bug in
			 * IPluginBugJoin, FogBugz does the work for you. */
			return api.Bug.NewBugQuery();
		}

		public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending, bool fIncludeSelect)
		{
			/* It's good practice to switch on the unique column iType if your plugin
			 * specifies multiple custom columns. */

			string sTableColumn = "ixAwesomeness";
			switch (col.iType)
			{
				case 0:
					sTableColumn = "ixAwesomeness";
					break;
			}

			/* Return a CBugQuery with the data you need joined
			 * in and sorted appropriately. Include an explicit
			 * select if fIncludeSelect is true. If your table is
			 * already joined to bug in IPluginBugJoin, FogBugz
			 * does the work for you, ignore fIncludeSelect. */
			CBugQuery bugQuery = api.Bug.NewBugQuery();
			bugQuery.AddOrderBy(string.Format("{0}.{1} {2}",
											  sBugAwesomenessTableName,
											  sTableColumn,
											  (fDescending ? "DESC" : "ASC")
								)
			);

			return bugQuery;

		}

		public CGridColumn[] GridColumns()
		{
			/* Create the column and set the title */

			CGridColumn gridCol = api.Grid.CreateGridColumn();
			gridCol.sName = "Level of Awesomeness";
			gridCol.sTitle = "Level of Awesomeness";
			gridCol.iType = 0;

			return new CGridColumn[] { gridCol };
		}

		#endregion
	}
}