
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
using System.Configuration;

public class GenerateDocsRepository
{

    public async Task<string> RenderReportInvoiceAsync(int intReportCode, string strExportFormat, string strPath, List<it_report_list> lReportlist, string strInvoiceNo)
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
                Name = "INVOICENO",
                Value = strInvoiceNo
            };

            // Load report
            rs.LoadReport(reportPath, null);
            rs.SetExecutionParameters(parameters, "en-us");

            // Render report
            byte[] result = rs.Render(format, devInfo, out var extension, out var encoding, out var mimeType, out var warnings, out var streamIDs);

            // Save report to file
            string strFilename = $"{strInvoiceNo}-{DateTime.Now:yyyyMMddhhmmss}";
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



    public async Task<List<string>> RenderReportSertifikatAsync(int intReportCode, string strPath, List<it_report_list> lReportlist, string strBatchid, int intTotalRows)
    {
        try
        {
            //intTotalRows = 10000;
            var report = lReportlist.FirstOrDefault(r => r.code == intReportCode);

            if (report == null)
            {
                Console.WriteLine("Report not found for code: " + intReportCode);
                return null;
            }

            ReportExecutionService rs = new ReportExecutionService
            {
                Credentials = System.Net.CredentialCache.DefaultCredentials,
                Url = report.url
            };

            string reportPath = $"/{report.folder}/{report.report_name}";
            string format = "PDF";
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            List<string> generatedFiles = new List<string>();

            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            //int totalRows =    //Convert.ToInt32(ConfigurationManager.AppSettings["TotalRowsCertifPerGenerated"]);
            int totalPages = (intTotalRows + pageSize - 1) / pageSize; // Calculate total pages

            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                Console.WriteLine("starting  Generated PDF " + pageNumber + " / " + totalPages);
                // Prepare report parameters.
                ParameterValue[] parameters = new ParameterValue[3];
                parameters[0] = new ParameterValue { Name = "BATCHID", Value = strBatchid };
                parameters[1] = new ParameterValue { Name = "PageNumber", Value = pageNumber.ToString() };
                parameters[2] = new ParameterValue { Name = "PageSize", Value = pageSize.ToString() };

                // Load report
                rs.LoadReport(reportPath, null);
                rs.SetExecutionParameters(parameters, "en-us");

                // Render report
                byte[] result = rs.Render(format, devInfo, out var extension, out var encoding, out var mimeType, out var warnings, out var streamIDs);

                // Save report to file
                string strFilename = $"{strBatchid}-{pageNumber}";
                string filePath = Path.Combine(strPath, strFilename + ".pdf");

                using (var stream = File.Create(filePath))
                {
                    await stream.WriteAsync(result, 0, result.Length);
                    Console.WriteLine("Succed Generated PDF " + pageNumber + " / " + totalPages);
                    Console.WriteLine("Report saved to: " + filePath);

                }

                generatedFiles.Add(filePath);
            }

            return generatedFiles;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error rendering report: " + e.Message);
            return null;
        }
    }
}





