/* Copyright 2009 Fog Creek Software, Inc. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

/* FogBugz namespaces-- make sure you add the neccesary assembly references to
 * the following DLL files contained in C:\Program Files\FogBugz\Website\bin\ 
 * FogBugz.dll, FogCreek.Plugins.dll, FogCreek.Plugins.InterfaceEvents.dll     */
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;

namespace IPluginGridColumn_Example
{
	/* Class Declaration: Inherit from Plugin, implement IPluginGridColumn, etc */
	public class IPluginGridColumn_Example : Plugin, IPluginGridColumn,
											 IPluginDatabase, IPluginBugJoin
	{
		private const string STABLENAME = "Kiwi";
		private const string PLUGIN_ID = "IPluginGridColumn_Example@fogcreek.com";
		private string sPrefixedTableName;
		/* Constructor: We'll just initialize the inherited Plugin class, which 
		 * takes the passed instance of CPluginApi and sets its "api" member variable. */
		public IPluginGridColumn_Example(CPluginApi api)
			: base(api)
		{
			/* database tables created by plugins are prefixed by a unique identifier.
			 * To use the table directly in a query, you need the full name */
			sPrefixedTableName = api.Database.PluginTableName(STABLENAME);
		}

		#region IPluginDatabase Members

		public CTable[] DatabaseSchema()
		{
			CTable kiwiTable = api.Database.NewTable(api.Database.PluginTableName(PLUGIN_ID));

			kiwiTable.sDesc = "A kiwi for every case.";
			/* don't use text column type so we can sort on it */
			kiwiTable.AddVarcharColumn("sName", 255, false);
			kiwiTable.AddDateColumn("dtBirth", false);
			kiwiTable.AddIntColumn("ixBug", true, 2);
			kiwiTable.AddAutoIncrementPrimaryKey("ixKiwi");
			return new CTable[] { kiwiTable };
		}

		public int DatabaseSchemaVersion()
		{
			return 3;
		}

		/* this is the first version: no upgrade */
		public void DatabaseUpgradeAfter(int ixVersionFrom, int ixVersionTo,
										 CDatabaseUpgradeApi apiUpgrade) { }

		public void DatabaseUpgradeBefore(int ixVersionFrom, int ixVersionTo,
										  CDatabaseUpgradeApi apiUpgrade) { }

		#endregion

		#region IPluginBugJoin Members

		public string[] BugJoinTables()
		{
			return new string[] { STABLENAME };
		}

		#endregion

		#region IPluginGridColumn Members

		public CGridColumn[] GridColumns()
		{
			CGridColumn gridCol1 = api.Grid.CreateGridColumn();
			/* the name displayed in the filter drop-down */
			gridCol1.sName = "Kiwi Name";
			/* the column title in grid view */
			gridCol1.sTitle = "Kiwi Name";
			/* every column you create needs to have a unique iType */
			gridCol1.iType = 0;

			CGridColumn gridCol2 = api.Grid.CreateGridColumn();
			gridCol2.sName = "Kiwi Birthdate";
			gridCol2.sTitle = "Kiwi Birthdate";
			/* every column you create needs to have a unique iType */
			gridCol2.iType = 1;

			return new CGridColumn[] { gridCol1, gridCol2 };
		}

		public CBugQuery GridColumnQuery(CGridColumn col)
		{
			/* Return a CBugQuery with the data you need joined
			 * in. If your table is already joined to bug in
			 * IPluginBugJoin, FogBugz does the work for you. */
			return api.Bug.NewBugQuery();
		}

		public string[] GridColumnDisplay(CGridColumn col,
										  CBug[] rgBug,
										  bool fPlainText)
		{
			string sTableColumn = "sName";
			switch (col.iType)
			{
				case 0:
					sTableColumn = "sName";
					break;
				case 1:
					sTableColumn = "dtBirth";
					break;
			}
			string[] sValues = new string[rgBug.Length];

			for (int i = 0; i < rgBug.Length; i++)
			{
				/* For tables joined in IPluginBugJoin, use
				 * GetPluginField to fetch the values you need
				 * for the GridColumn. */
				object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
				sValues[i] = (pluginField == null) ?
							 "" :
							 HttpUtility.HtmlEncode(pluginField.ToString());
			}
			return sValues;
		}

		public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending,
												bool fIncludeSelect)
		{
			string sTableColumn = "sName";
			switch (col.iType)
			{
				case 0:
					sTableColumn = "sName";
					break;
				case 1:
					sTableColumn = "dtBirth";
					break;
			}
			/* Return a CBugQuery with the data you need joined
			 * in and sorted appropriately. Include an explicit
			 * select if fIncludeSelect is true. If your table is
			 * already joined to bug in IPluginBugJoin, FogBugz
			 * does the work for you, ignore fIncludeSelect. */
			CBugQuery bugQuery = api.Bug.NewBugQuery();
			bugQuery.AddOrderBy(string.Format("{0}.{1} {2}",
											sPrefixedTableName,
											sTableColumn,
											(fDescending ? "DESC" : "ASC")
											)
							 );
			return bugQuery;
		}

		#endregion
	}
}

