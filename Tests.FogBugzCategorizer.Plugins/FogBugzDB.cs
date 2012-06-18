using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FogBugz.Categorizer.Plugins.Business;

namespace Tests.FogBugzCategorizer.Plugins
{
	public class FogBugzDB
	{
		public static void Restore()
		{
			const string sql = @"
truncate table Plugin_25_ARPC_SPLIT
truncate table Plugin_25_ARPC_SPLIT_DETAILS

set identity_insert Plugin_25_ARPC_SPLIT on
insert Plugin_25_ARPC_SPLIT (Id, ixBug, LastEditor) values (1, 1, 'Mark King')
set identity_insert Plugin_25_ARPC_SPLIT off

set identity_insert Plugin_25_ARPC_SPLIT_DETAILS on
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (1, 1, 'ABAT-002 Actuarial Analysis', 'Cashflow analysis')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (2, 1, 'ABAT-002 Actuarial Analysis', 'Compile database(s)')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (3, 1, 'ABAT-002 Actuarial Analysis', 'Finalize')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (4, 1, 'ABAT-002 Actuarial Analysis', 'Historical data analysis - model building')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (5, 1, 'ABAT-002 Actuarial Analysis', 'Reporting')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (6, 1, 'AC&S-003 Accounting', '1099/W9')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (7, 1, 'AC&S-003 Accounting', 'Audit')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (8, 1, 'AC&S-003 Accounting', 'Budgeting')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (9, 1, 'AC&S-003 Accounting', 'General Ledger')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (10, 1, 'AC&S-003 Accounting', 'Payment processing and tracking')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (11, 1, 'AC&S-003 Accounting', 'Reporting')
insert Plugin_25_ARPC_SPLIT_DETAILS (Id, SplitId, Project, Task) values (12, 1, 'AC&S-003 Accounting', 'Tax')
set identity_insert Plugin_25_ARPC_SPLIT_DETAILS off

delete PluginKeyValue where ixPlugin = 25
set identity_insert PluginKeyValue on
insert PluginKeyValue (ixPluginKeyValue, ixPlugin, sKey, sValue) values (9, 25, 'AuthorizedCategorizers', '[""Administrator"",""Mark King""]')
set identity_insert PluginKeyValue off
";

			using(var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["fogbugz"].ConnectionString))
			{
				using(var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					connection.Open();
					command.ExecuteNonQuery();
				}
			}
		}

		public static List<Task> GetSelected(int bugzId)
		{
			const string sql = @"
select
	 d.Project
	,d.Task
from Plugin_25_ARPC_SPLIT s
	join Plugin_25_ARPC_SPLIT_DETAILS d
		on s.Id = d.SplitId
where
	s.ixBug = @BugzId
";

			using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["fogbugz"].ConnectionString))
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.Parameters.AddWithValue("BugzId", bugzId);
					connection.Open();
					using (var reader = command.ExecuteReader())
					{
						var selected = new List<Task>();
						while (reader.Read())
						{
							selected.Add(new Task{ Project = new Project{ Name = reader.GetString(0) }, Name = reader.GetString(1) });
						}
						return selected;
					}
				}
			}
		}

		public static string GetLastEditor(int bugzId)
		{
			const string sql = @"
select
	LastEditor
from Plugin_25_ARPC_SPLIT
where
	ixBug = @BugzId
";

			using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["fogbugz"].ConnectionString))
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.Parameters.AddWithValue("BugzId", bugzId);
					connection.Open();
					return (string) command.ExecuteScalar();
				}
			}
		}
	}
}