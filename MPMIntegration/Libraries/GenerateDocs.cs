
using MPMIntegration;
using MPMIntegration.ReportExecutionService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Net.Http;
using System.Text;

public class GenerateDocsRepository
{



    public async Task<string> RenderReportAsync(int intReportCode, string strExportFormat, string strPath, List<it_report_list> lReportlist, string strBatchid)
    {
        try
        {
            var report = lReportlist.FirstOrDefault(r => r.code == intReportCode);

            if (report == null)
            {
                Console.WriteLine("Report not found for code: " + intReportCode);
                return null;
            }

            ReportExecutionService rs = new ReportExecutionService
            {
                Credentials = System.Net.CredentialCache.DefaultCredentials,
                Url = report.url   //
            };

            string reportPath = $"/{report.folder}/{report.report_name}";
            string format = "PDF";
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            // Prepare report parameter.
            ParameterValue[] parameters = new ParameterValue[1];
            parameters[0] = new ParameterValue
            {
                Name = "BATCHID",
                Value = strBatchid
            };

            // Load report
            rs.LoadReport(reportPath, null);
            rs.SetExecutionParameters(parameters, "en-us");

            // Render report
            byte[] result = rs.Render(format, devInfo, out var extension, out var encoding, out var mimeType, out var warnings, out var streamIDs);

            // Save report to file
            string strFilename = $"{strBatchid}-{DateTime.Now:yyyyMMddhhmmss}";
            string filePath = Path.Combine(strPath, strFilename + ".pdf");

            using (var stream = File.Create(filePath))
            {
                await stream.WriteAsync(result, 0, result.Length);
                Console.WriteLine("Report saved to: " + filePath);
            }

            return filePath;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error rendering report: " + e.Message);
            return null;
        }
    }
}





