using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dochazka.HelperClasses
{
    public class CSVResult : ActionResult
    {
        /// <summary>
        /// Converts the columns and rows from a data table into an Microsoft Excel compatible CSV file using custom DataTable Extension methods.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="fileName">The full file name including the extension.</param>
        public CSVResult(DataTable dataTable, string fileName)
        {
            Table = dataTable;
            FileName = fileName;
        }

        public string FileName { get; protected set; }
        public DataTable Table { get; protected set; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            response.ContentType = "text/csv";
            response.Headers.Add("Content-Disposition", "attachment;filename=" + this.FileName);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(Table.WriteToCsvString());
            response.Body.WriteAsync(data, 0, data.Length);
            return Task.CompletedTask;
        }
    }
}
