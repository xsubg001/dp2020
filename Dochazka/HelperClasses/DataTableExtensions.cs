using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dochazka.HelperClasses
{
    public static class DataTableExtensions
    {
        public static string WriteToCsvString(this DataTable dataTable)
        {
            StringBuilder sbData = new StringBuilder();

            // Only return Null if there is no structure.
            if (dataTable.Columns.Count == 0)
                return string.Empty;

            foreach (var col in dataTable.Columns)
            {
                if (col == null)
                    sbData.Append(",");
                else
                    sbData.Append("\"" + col.ToString().Replace("\"", "\"\"") + "\",");
            }

            sbData.Replace(",", System.Environment.NewLine, sbData.Length - 1, 1);

            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (var column in dr.ItemArray)
                {
                    if (column == null)
                        sbData.Append(",");
                    else
                        sbData.Append("\"" + column.ToString().Replace("\"", "\"\"") + "\",");
                }
                sbData.Replace(",", System.Environment.NewLine, sbData.Length - 1, 1);
            }

            return sbData.ToString();
        }
    }
}
