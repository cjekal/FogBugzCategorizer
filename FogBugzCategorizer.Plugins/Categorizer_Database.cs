using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Interfaces;

namespace FogBugzCategorizer.Plugins
{
	public partial class Categorizer : IPluginDatabase, IPluginBugJoin, IPluginFilterJoin
	{
		#region Implementation of IPluginDatabase

		public int DatabaseSchemaVersion()
		{
			return 1;
		}

		public CTable[] DatabaseSchema()
		{
			CTable hoursTable = api.Database.NewTable(GetPluginTableName(Tables.HOURS_TABLE));
			hoursTable.sDesc = "Hours billed to case";
			hoursTable.AddIntColumn("ixBug", true, 2);
			hoursTable.AddFloatColumn("Hours", true, 0);
			hoursTable.AddAutoIncrementPrimaryKey("Id");

			CTable splitTable = api.Database.NewTable(GetPluginTableName(Tables.SPLIT_TABLE));
			splitTable.sDesc = "Categorization status for case";
			splitTable.AddIntColumn("ixBug", true, 2);
			splitTable.AddVarcharColumn("LastEditor", 255, false);
			splitTable.AddAutoIncrementPrimaryKey("Id");

			CTable hasHoursFilterTable = api.Database.NewTable(GetPluginTableName(Tables.HOURS_FILTER_TABLE));
			hasHoursFilterTable.sDesc = "Save the filter selection for Has Hours?";
			hasHoursFilterTable.AddIntColumn("ixFilter", true, 1);
			hasHoursFilterTable.AddIntColumn("ixPerson", true, 1);
			hasHoursFilterTable.AddVarcharColumn("HoursFilterType", 50, true, "Unset");
			hasHoursFilterTable.AddAutoIncrementPrimaryKey("Id");

			CTable hasSplitFilterTable = api.Database.NewTable(GetPluginTableName(Tables.SPLIT_FILTER_TABLE));
			hasSplitFilterTable.sDesc = "Save the filter selection for Has Split?";
			hasSplitFilterTable.AddIntColumn("ixFilter", true, 1);
			hasSplitFilterTable.AddIntColumn("ixPerson", true, 1);
			hasSplitFilterTable.AddVarcharColumn("SplitFilterType", 50, true, "Unset");
			hasSplitFilterTable.AddAutoIncrementPrimaryKey("Id");

			return new[] { hoursTable, splitTable, hasHoursFilterTable, hasSplitFilterTable };
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

		#region Implementation of IPluginFilterJoin

		public string[] FilterJoinTables()
		{
			//return null;
			return new[] { Tables.HOURS_FILTER_TABLE, Tables.SPLIT_FILTER_TABLE };
		}

		#endregion

		protected virtual string GetPluginTableName(string tableName)
		{
			return api.Database.PluginTableName(PLUGIN_ID, tableName);
		}
	}
}