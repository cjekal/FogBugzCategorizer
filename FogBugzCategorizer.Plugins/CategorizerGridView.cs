using System;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using System.Web;
using FogCreek.FogBugz.UI.Dialog;

namespace FogBugzCategorizer.Plugins
{
	public class CategorizerGridView : FogBugzCategorizerBase, IPluginGridColumn, IPluginDatabase, IPluginBugJoin, IPluginFilterDisplay, IPluginFilterOptions, IPluginFilterJoin, IPluginFilterCommit
    {
        public CategorizerGridView(CPluginApi api) : base(api)
        {
        }

		protected string preCommitHoursSelection = HoursFilterType.Unset.ToString();

        #region IPluginGridColumn Members

        public CGridColumn[] GridColumns()
        {
            CGridColumn hoursColumn = api.Grid.CreateGridColumn();
            hoursColumn.sName = "Hours";
            hoursColumn.sTitle = "Hours";
			hoursColumn.iType = 0;

			CGridColumn splitColumn = api.Grid.CreateGridColumn();
			splitColumn.sName = "Split";
			splitColumn.sTitle = "Split";
        	splitColumn.iType = 1;

			return new[] { hoursColumn, splitColumn };
        }

        public string[] GridColumnDisplay(CGridColumn col, CBug[] rgBug, bool fPlainText)
        {
            string columnName = "Hours";
            switch (col.iType)
            {
                case 0:
                    columnName = "Hours";
                    break;
				case 1:
            		columnName = "LastEditor";
            		break;
            }
            var valuesAsStrings = new string[rgBug.Length];

            for (int i = 0; i < rgBug.Length; i++)
            {
				object value = rgBug[i].GetPluginField(PLUGIN_ID, columnName);

				if (col.iType == 0)
				{
					valuesAsStrings[i] = (value == null) ? string.Empty : HttpUtility.HtmlEncode(Convert.ToDecimal(value).ToString("#,###.0"));
				}
				else
				{
					valuesAsStrings[i] = (value == null) ? "No" : "Yes";
				}
            }
            return valuesAsStrings;
        }

 
        public CBugQuery GridColumnQuery(CGridColumn col)
        {
            return api.Bug.NewBugQuery();
        }

        public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending, bool fIncludeSelect)
        {
            CBugQuery gridColumnQuery = api.Bug.NewBugQuery();
            string orderByColumn = null;
        	string tableName = null;
            switch (col.iType)
            {
                case 0:
                    orderByColumn = "Hours";
            		tableName = Tables.HOURS_TABLE;
                    break;
				case 1:
            		orderByColumn = "LastEditor";
            		tableName = Tables.SPLIT_TABLE;
            		break;
            }
        	var pluginTableName = api.Database.PluginTableName(tableName);
			gridColumnQuery.AddOrderBy(string.Format("{0}.{1} {2}", pluginTableName, orderByColumn, fDescending ? "DESC" : "ASC"));
            return gridColumnQuery;
        }

        #endregion

		#region Implementation of IPluginDatabase

		public int DatabaseSchemaVersion()
		{
			return 1;
		}

		public CTable[] DatabaseSchema()
		{
			CTable hoursTable = api.Database.NewTable(api.Database.PluginTableName(Tables.HOURS_TABLE));
			hoursTable.sDesc = "Hours billed to case";
			hoursTable.AddIntColumn("ixBug", true, 2);
			hoursTable.AddFloatColumn("Hours",true, 0);
			hoursTable.AddAutoIncrementPrimaryKey("Id");

			CTable splitTable = api.Database.NewTable(api.Database.PluginTableName(Tables.SPLIT_TABLE));
			splitTable.sDesc = "Categorization status for case";
			splitTable.AddIntColumn("ixBug", true, 2);
			splitTable.AddVarcharColumn("LastEditor", 255, false);
			splitTable.AddAutoIncrementPrimaryKey("Id");

			CTable hasHoursFilterTable = api.Database.NewTable(api.Database.PluginTableName(Tables.HOURS_FILTER_TABLE));
			hasHoursFilterTable.sDesc = "Save the filter selection for Has Hours?";
			hasHoursFilterTable.AddIntColumn("ixFilter", true, 1);
			hasHoursFilterTable.AddIntColumn("ixPerson", true, 1);
			hasHoursFilterTable.AddVarcharColumn("HoursFilterType", 50, true, "Unset");
			hasHoursFilterTable.AddAutoIncrementPrimaryKey("Id");

			//CTable hasSplitFilterTable = api.Database.NewTable(api.Database.PluginTableName(Tables.SPLIT_FILTER_TABLE));
			//hasHoursFilterTable.sDesc = "Save the filter selection for Has Hours?";
			//hasHoursFilterTable.AddIntColumn("ixFilter", true, 1);
			//hasHoursFilterTable.AddIntColumn("ixPerson", true, 1);
			//hasHoursFilterTable.AddVarcharColumn("HoursFilterType", 50, true, "Unset");
			//hasHoursFilterTable.AddAutoIncrementPrimaryKey("Id");

			return new[] { hoursTable, splitTable, hasHoursFilterTable };
		}

		public void DatabaseUpgradeBefore(int ixVersionFrom, int ixVersionTo, CDatabaseUpgradeApi apiUpgrade)
		{
		}

		public void DatabaseUpgradeAfter(int ixVersionFrom, int ixVersionTo, CDatabaseUpgradeApi apiUpgrade)
		{
		}

		#endregion

		#region Implementation of IPluginBugJoin 

		public string[] BugJoinTables()
		{
			return new[] { Tables.HOURS_TABLE, Tables.SPLIT_TABLE };
		}

		#endregion

		#region Implementation of IPluginFilterDisplay

		public string[] FilterDisplayListHeaders()
		{
			return new[] { "Has Splits", "Has Hours Billed" };
		}

		public string[] FilterDisplayListFields(CFilter filter)
		{
			return new[] {"Has Hours?"};
		}

		public CDialogItem[] FilterDisplayEdit(CFilter filter)
		{
			return null;
		}

		#endregion

		#region Implementation of IPluginFilterBugEntry

		public bool FilterBugEntryCanCreate(CFilter filter)
		{
			return false;
		}

		public string FilterBugEntryUrlParams(CFilter filter)
		{
			return api.PluginPrefix + "HoursFilterType=1";
		}

		#endregion

		#region Implementation of IPluginFilterOptions

		public CFilterOption[] FilterOptions(CFilter filter)
		{
			var hasHoursOption = api.Filter.NewFilterOption();
			hasHoursOption.fShowDialogItem = true;
			hasHoursOption.SetSelectOne(new[]{"Yes", "No"});
			hasHoursOption.SetDefault(string.Empty);
			hasHoursOption.sQueryParam = api.PluginPrefix + "HoursFilterType";
			hasHoursOption.fShowDialogItem = true;
			hasHoursOption.sHeader = "Has Hours?";
			hasHoursOption.sName = "hasHours";
			return new[] {hasHoursOption};
		}

		public CFilterStringList FilterOptionsString(CFilter filter)
		{
			var hasHoursList = new CFilterStringList();
			switch(GetHoursFilterType(filter))
			{
				case HoursFilterType.Yes:
					hasHoursList.Add("Has Hours", "HoursFilterType");
					break;
				case HoursFilterType.No:
					hasHoursList.Add("Has No Hours", "HoursFilterType");
					break;
				case HoursFilterType.Unset:
					hasHoursList.Add(string.Empty, "HoursFilterType");
					break;
			}
			return hasHoursList;
		}

		public CSelectQuery FilterOptionsQuery(CFilter filter)
		{
			var select = api.Database.NewSelectQuery("Bug");
			var pluginTableName = GetPluginTableName(Tables.HOURS_TABLE);
			switch(GetHoursFilterType(filter))
			{
				case HoursFilterType.Yes:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NOT NULL", pluginTableName));
					break;
				case HoursFilterType.No:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NULL", pluginTableName));
					break;
				//default:
					//throw new Exception(GetHoursFilterType(filter).ToString());
			}
			return select;
		}

		public string FilterOptionsUrlParams(CFilter filter)
		{
			switch (GetHoursFilterType(filter))
			{
				case HoursFilterType.Yes:
					return api.PluginPrefix + "HoursFilterType=Yes";
				case HoursFilterType.No:
					return api.PluginPrefix + "HoursFilterType=No";
				default:
					return string.Empty;
			}
		}

		#endregion

		#region Implementation of IPluginFilterJoin

		public string[] FilterJoinTables()
		{
			//return null;
			return new[] {Tables.HOURS_FILTER_TABLE};
		}

		#endregion

		protected virtual string GetPluginTableName(string tableName)
		{
			return api.Database.PluginTableName(PLUGIN_ID, tableName);
		}

		protected virtual HoursFilterType GetHoursFilterType(CFilter filter)
		{
			if (filter.GetPluginField(PLUGIN_ID, "HoursFilterType") != null)
			{
				return (HoursFilterType)Enum.Parse(typeof(HoursFilterType), Convert.ToString(filter.GetPluginField(PLUGIN_ID, "HoursFilterType")));
			}
			return HoursFilterType.Unset;
		}

		#region Implementation of IPluginFilterCommit

		public bool FilterCommitBefore(CFilter filter)
		{
			if (api.Request[api.AddPluginPrefix("HoursFilterType")] != null)
			{
				var pluginField = filter.GetPluginField(PLUGIN_ID, "HoursFilterType");
				preCommitHoursSelection = pluginField == null ? "Unset" : pluginField.ToString();

				var newValue = api.Request[api.AddPluginPrefix("HoursFilterType")].ToLower();
				HoursFilterType type = HoursFilterType.Unset;
				switch (newValue)
				{
					case "yes":
						type = HoursFilterType.Yes;
						break;
					case "no":
						type = HoursFilterType.No;
						break;
				}
				filter.SetPluginField(PLUGIN_ID, "HoursFilterType", type.ToString());
			}
			return true;
		}

		public void FilterCommitAfter(CFilter filter)
		{
		}

		public void FilterCommitRollback(CFilter filter)
		{
			filter.SetPluginField(PLUGIN_ID, "HoursFilterType", preCommitHoursSelection);
		}

		#endregion
    }
}