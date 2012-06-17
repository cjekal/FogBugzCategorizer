using System;
using FogBugz.Categorizer.Plugins.Business;
using FogCreek.Core;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;

namespace FogBugzCategorizer.Plugins
{
	public partial class Categorizer : Plugin, IPluginGridColumn, IPluginFilterDisplay, IPluginFilterOptions, IPluginFilterCommit
    {
		public Categorizer(CPluginApi api) : base(api)
        {
        }

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
				object value = rgBug[i].GetPluginField(Statics.PluginId, columnName);

				if (col.iType == 0)
				{
					valuesAsStrings[i] = (value == null) ? string.Empty : Html.Encode(Convert.ToDecimal(value).ToString("#,###.0"));
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
        	var pluginTableName = GetPluginTableName(tableName);
			gridColumnQuery.AddOrderBy(string.Format("{0}.{1} {2}", pluginTableName, orderByColumn, fDescending ? "DESC" : "ASC"));
            return gridColumnQuery;
        }

        #endregion

		#region Implementation of IPluginFilterDisplay

		public string[] FilterDisplayListHeaders()
		{
			return new[] { "Has Splits", "Has Hours Billed" };
		}

		public string[] FilterDisplayListFields(CFilter filter)
		{
			return new[] {"Has Hours?", "Has Split?"};
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
			return api.PluginPrefix + "HoursFilterType=" + filter.GetPluginField(Statics.PluginId, "HoursFilterType") + "&SplitFilterType=" + filter.GetPluginField(Statics.PluginId, "SplitFilterType");
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
			hasHoursOption.sHeader = "Has Hours?";
			hasHoursOption.sName = "hasHours";

			var hasSplitOption = api.Filter.NewFilterOption();
			hasSplitOption.fShowDialogItem = true;
			hasSplitOption.SetSelectOne(new[] {"Yes", "No"});
			hasSplitOption.SetDefault(string.Empty);
			hasSplitOption.sQueryParam = api.PluginPrefix + "SplitFilterType";
			hasSplitOption.sHeader = "Has Split?";
			hasSplitOption.sName = "hasSplit";
			return new[] { hasHoursOption, hasSplitOption };
		}

		public CFilterStringList FilterOptionsString(CFilter filter)
		{
			var filterList = new CFilterStringList();
			switch(GetHoursFilterType(filter))
			{
				case FilterType.Yes:
					filterList.Add("Has Hours", "HoursFilterType");
					break;
				case FilterType.No:
					filterList.Add("Has No Hours", "HoursFilterType");
					break;
				case FilterType.Unset:
					filterList.Add(string.Empty, "HoursFilterType");
					break;
			}

			switch(GetSplitFilterType(filter))
			{
				case FilterType.Yes:
					filterList.Add("Has Split", "SplitFilterType");
					break;
				case FilterType.No:
					filterList.Add("Has No Split", "SplitFilterType");
					break;
				case FilterType.Unset:
					filterList.Add(string.Empty, "SplitFilterType");
					break;
			}

			return filterList;
		}

		public CSelectQuery FilterOptionsQuery(CFilter filter)
		{
			var select = api.Database.NewSelectQuery("Bug");
			var pluginTableName = GetPluginTableName(Tables.HOURS_TABLE);
			switch(GetHoursFilterType(filter))
			{
				case FilterType.Yes:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NOT NULL", pluginTableName));
					break;
				case FilterType.No:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NULL", pluginTableName));
					break;
			}

			pluginTableName = GetPluginTableName(Tables.SPLIT_TABLE);
			switch (GetSplitFilterType(filter))
			{
				case FilterType.Yes:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NOT NULL", pluginTableName));
					break;
				case FilterType.No:
					select.AddLeftJoin(pluginTableName, string.Format("Bug.ixBug = {0}.ixBug", pluginTableName));
					select.AddWhere(string.Format("{0}.ixBug IS NULL", pluginTableName));
					break;
			}
			return select;
		}

		public string FilterOptionsUrlParams(CFilter filter)
		{
			var hours = string.Empty;
			switch (GetHoursFilterType(filter))
			{
				case FilterType.Yes:
					hours = api.PluginPrefix + "HoursFilterType=Yes";
					break;
				case FilterType.No:
					hours = api.PluginPrefix + "HoursFilterType=No";
					break;
			}

			var split = string.Empty;
			switch (GetSplitFilterType(filter))
			{
				case FilterType.Yes:
					split = api.PluginPrefix + "SplitFilterType=Yes";
					break;
				case FilterType.No:
					split = api.PluginPrefix + "SplitFilterType=No";
					break;
			}

			if (hours == string.Empty)
			{
				return split;
			}
			if (split == string.Empty)
			{
				return hours;
			}
			return hours + "&" + split;
		}

		#endregion

		#region Implementation of IPluginFilterCommit

		protected string PreCommitHoursSelection = FilterType.Unset.ToString();
		protected string PreCommitSplitSelection = FilterType.Unset.ToString();

		public bool FilterCommitBefore(CFilter filter)
		{
			if (api.Request[api.AddPluginPrefix("HoursFilterType")] != null)
			{
				var pluginField = filter.GetPluginField(Statics.PluginId, "HoursFilterType");
				PreCommitHoursSelection = pluginField == null ? "Unset" : pluginField.ToString();

				var newValue = api.Request[api.AddPluginPrefix("HoursFilterType")].ToLower();
				FilterType type = FilterType.Unset;
				switch (newValue)
				{
					case "yes":
						type = FilterType.Yes;
						break;
					case "no":
						type = FilterType.No;
						break;
				}
				filter.SetPluginField(Statics.PluginId, "HoursFilterType", type.ToString());
			}

			if (api.Request[api.AddPluginPrefix("SplitFilterType")] != null)
			{
				var pluginField = filter.GetPluginField(Statics.PluginId, "SplitFilterType");
				PreCommitSplitSelection = pluginField == null ? "Unset" : pluginField.ToString();

				var newValue = api.Request[api.AddPluginPrefix("SplitFilterType")].ToLower();
				FilterType type = FilterType.Unset;
				switch (newValue)
				{
					case "yes":
						type = FilterType.Yes;
						break;
					case "no":
						type = FilterType.No;
						break;
				}
				filter.SetPluginField(Statics.PluginId, "SplitFilterType", type.ToString());
			}
			return true;
		}

		public void FilterCommitAfter(CFilter filter)
		{
		}

		public void FilterCommitRollback(CFilter filter)
		{
			filter.SetPluginField(Statics.PluginId, "HoursFilterType", PreCommitHoursSelection);
			filter.SetPluginField(Statics.PluginId, "SplitFilterType", PreCommitSplitSelection);
		}

		#endregion
    
		protected virtual FilterType GetHoursFilterType(CFilter filter)
		{
			if (filter.GetPluginField(Statics.PluginId, "HoursFilterType") != null)
			{
				return (FilterType)Enum.Parse(typeof(FilterType), Convert.ToString(filter.GetPluginField(Statics.PluginId, "HoursFilterType")));
			}
			return FilterType.Unset;
		}

		protected virtual FilterType GetSplitFilterType(CFilter filter)
		{
			if (filter.GetPluginField(Statics.PluginId, "SplitFilterType") != null)
			{
				return (FilterType)Enum.Parse(typeof(FilterType), Convert.ToString(filter.GetPluginField(Statics.PluginId, "SplitFilterType")));
			}
			return FilterType.Unset;
		}
	}
}