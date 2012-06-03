using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using System.Web;

namespace FogBugzCategorizer.Plugins
{
    public class CategorizerGridView : FogBugzCategorizerBase, IPluginGridColumn
    {
        public CategorizerGridView(CPluginApi api) : base(api)
        {
        }

        #region IPluginGridColumn Members

        public CGridColumn[] GridColumns()
        {
            CGridColumn gridCol1 = api.Grid.CreateGridColumn();
            /* the name displayed in the filter drop-down */
            gridCol1.sName = "Split";
            /* the column title in grid view */
            gridCol1.sTitle = "Split";
            /* every column you create needs to have a unique iType */
            gridCol1.iType = 0;

            CGridColumn gridCol2 = api.Grid.CreateGridColumn();
            gridCol2.sName = "Hours";
            gridCol2.sTitle = "Hours";
            /* every column you create needs to have a unique iType */
            gridCol2.iType = 1;

            return new[] {gridCol1, gridCol2};
        }

        public string[] GridColumnDisplay(CGridColumn col, CBug[] rgBug, bool fPlainText)
        {
            string columnName = null;
            switch (col.iType)
            {
                case 0:
                    columnName = "Split";
                    break;
                case 1:
                    columnName = "Hours";
                    break;
            }
            var sValues = new string[rgBug.Length];

            for (int i = 0; i < rgBug.Length; i++)
            {
                object pluginField = rgBug[i].GetPluginField(PLUGIN_ID,string.Format("{0}",columnName));
                sValues[i] = (pluginField == null) ?
                             "" :
                             HttpUtility.HtmlEncode(pluginField.ToString());
            }
            return sValues;
        }

 
        public CBugQuery GridColumnQuery(CGridColumn col)
        {
            CBugQuery gridColumnQuery = api.Bug.NewBugQuery();
            string select = null;
            switch (col.iType)
            {
                case 0:
                    select = "case when ixbug % 2 = 0 then 'Y' else 'n' end as Split";
                    break;
                case 1:
                    select = "ixBug / 500 as Hours";
                    break;
            }
            gridColumnQuery.AddSelect(select);
            return gridColumnQuery;
        }

        public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending, bool fIncludeSelect)
        {
            CBugQuery gridColumnQuery = api.Bug.NewBugQuery();
            string orderby = null;
            switch (col.iType)
            {
                case 0:
                    orderby = "case when ixbug % 2 = 0 then 'Y' else 'n' end";
                    break;
                case 1:
                    orderby = "ixBug / 500";
                    break;
            }
            gridColumnQuery.AddOrderBy(string.Format("{0} {1}", orderby, (fDescending ? "DESC" : "ASC")));
            
            return gridColumnQuery;

        }

        #endregion
    }
}