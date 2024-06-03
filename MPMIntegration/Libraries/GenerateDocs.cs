using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using MPMIntegration.ServiceReference;

namespace MPMIntegration.Libraries
{
    public class GenerateDocsRepository
    {
        private readonly IHttpClientFactory _clientFactory;

        public GenerateDocsRepository(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<string> RenderReportAsync(int intReportCode, string strExportFormat, string strPath, List<it_report_list> lReportlist, string strInvoiceno)
        {
            string filename = "";

            string reportPath = "/" + lReportlist.FirstOrDefault().folder + "/" + lReportlist.FirstOrDefault().folder_name;
            string format = strExportFormat;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            var parameters = new ParameterValue[]
            {
                new ParameterValue
                {
                    Name = "INVOICENO",
                    Value = strInvoiceno
                }
            };

            var client = _clientFactory.CreateClient("ReportExecutionService");

            // Prepare the SOAP request
            var soapEnvelopeXml = new StringBuilder();
            soapEnvelopeXml.Append($@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rep=""http://schemas.microsoft.com/sqlserver/2005/06/30/reporting/reportingservices"">
                <soapenv:Header/>
                <soapenv:Body>
                    <rep:LoadReport>
                        <rep:Report>{reportPath}</rep:Report>
                        <rep:HistoryID>{null}</rep:HistoryID>
                    </rep:LoadReport>
                </soapenv:Body>
            </soapenv:Envelope>");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress)
            {
                Content = new StringContent(soapEnvelopeXml.ToString(), Encoding.UTF8, "application/soap+xml")
            };

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string resultXml = await response.Content.ReadAsStringAsync();
                XDocument doc = XDocument.Parse(resultXml);
                var execInfo = doc.Descendants().Where(x => x.Name.LocalName == "ExecutionInfo").FirstOrDefault();

                // Get the report's Execution ID
                string executionId = execInfo.Element("ExecutionID")?.Value;

                // Continue with rendering and handling the report...
                // (You'll need to create and send another SOAP request for rendering)
            }
            else
            {
                throw new Exception("Failed to load report.");
            }

            // The code to render and save the report remains to be written similarly
            // Handle the rest of the logic as per your original method

            return filename;
        }
    }
}

