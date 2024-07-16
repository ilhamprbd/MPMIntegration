

using CsvHelper;
using CsvHelper.Configuration;
using MPMIntegration.APIModel;
using MPMIntegration.Repos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;


namespace MPMIntegration.Libraries
{
    public class HitApi
    {

        private Stopwatch timer = new Stopwatch();
        private GlobalRepository repos = new GlobalRepository();
        protected const string _truncateLiveTableCommandText = @"TRUNCATE TABLE tbl_participant_list";
        protected const string strTblParticipant = "tbl_participant_list";


        public async Task<string> GetTokenAsyncClient(List<api_client_configuration> lApiConfig)
        {
            List<api_url> lAPIUrl = await repos.repo_API.GetURLAPI(11); // hardcode for get TOKEN
            timer.Start();
            string strStream = "";
            var jsonBody = JsonConvert.SerializeObject(lApiConfig.Select(c => new { apikey = c.api_key })).Replace("[", "").Replace("]", "");

            try
            {
                using (HttpClient httpClient = HttpClientCustomslHandling())
                {

                    using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                    {

                        // Use StringContent to send JSON in the request body
                        StringContent content = new StringContent(jsonBody, Encoding.UTF8, lApiConfig.FirstOrDefault().content_type);

                        // Send the request using HttpClient
                        HttpResponseMessage response = await HttpClientCustomslHandling().PostAsync(lApiConfig.FirstOrDefault().url + lAPIUrl.FirstOrDefault().url_value, content);

                        // Check if the response is successful (status code 200 OK)
                        if (response.IsSuccessStatusCode)
                        {
                            // Read the response content
                            strStream = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Generating Token...");

                            // Log the API request
                            //timer.Stop();
                            await LogApiRequestAsync(lApiConfig, jsonBody, strStream, response.StatusCode, lAPIUrl);


                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                            // Handle other HTTP status codes if needed
                        }

                    }

                    return strStream;
                }
            }
            catch (Exception ex)
            {


                Console.WriteLine($"Exception: {ex.Message}");
                // Handle other exceptions
                throw;
            }
        }

        private async Task LogApiRequestAsync(List<api_client_configuration> lApiConfig, string requestBody, string responseBody, HttpStatusCode statusCode, List<api_url> lAPIUrl)
        {
            timer.Stop();
            timer.Reset();
            string ipAddress = string.Empty;
            try
            {
                // Get the IP addresses of the local machine
                IPAddress[] localIpAddresses = Dns.GetHostAddresses(Dns.GetHostName());

                // Find the first IPv4 address
                IPAddress ipv4Address = localIpAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4Address != null)
                {
                    ipAddress = ipv4Address.ToString();
                }

                api_log_request data = new api_log_request
                {
                    // Populate your log data

                    request_direction = "IN",
                    request_url = lApiConfig.FirstOrDefault()?.url + lAPIUrl.FirstOrDefault()?.url_value,
                    request_method = lAPIUrl.FirstOrDefault()?.url_method,
                    request_header = null, // Set your request header
                    request_body = requestBody,
                    response_body = responseBody,
                    response_error = $"status: {statusCode}",
                    app_id = 0, // Set your app_id
                    time_load = timer.Elapsed.Seconds + (decimal)timer.Elapsed.Milliseconds / 1000M,
                    user_id = "Batch.Jobs", // Set your user_id
                    create_date = DateTime.Now,
                    ip_address = ipAddress // Set your ip_address
                };

                await Task.Run(() => new APIRepository().SaveLog(data));
            }
            catch (Exception ex)
            {
                Console.WriteLine("LogApiRequestAsync Error : " + ex);
            }
        }

        public async Task<List<tbl_placing_batch>> getBatchList(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken)
        {

            timer.Start();

            var jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            List<tbl_placing_batch> lPlacing_batch = new List<tbl_placing_batch>();

            try
            {
                HttpWebRequest webRequest = WebRequest.CreateHttp(lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value);
                webRequest.Method = lApiURL.FirstOrDefault().url_method;
                webRequest.ContentType = lApiConfig.FirstOrDefault().content_type;



                webRequest.Headers.Add("Authorization", $"Bearer {loginModel.token}");


                //var httpresponse = (HttpWebResponse)webRequest.GetResponse();
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    Encoding ascii = Encoding.ASCII;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Do stuff with response.GetResponseStream();
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), ascii))
                        {
                            string strEnd = await streamReader.ReadToEndAsync();
                            Console.WriteLine("PopulateBatch List.. ");

                            await LogApiRequestAsync(lApiConfig, jsonbody, response.StatusDescription, response.StatusCode, lApiURL);


                            // Deserialize the JSON data into a list of your data model
                            lPlacing_batch = JsonConvert.DeserializeObject<List<tbl_placing_batch>>(strEnd);

                        }
                    }

                }


            }
            catch (Exception ex)
            {

                throw ex;

            }
            return lPlacing_batch;
        }

        public async Task<string> getParticipantList(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID)
        {
            timer.Start();

            var jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            string strCSVFile = ConfigurationManager.AppSettings["CSVFilePath"];

            try
            {
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID));
                webRequest.Method = lApiURL.FirstOrDefault().url_method;
                webRequest.Accept = "text/csv";
                webRequest.Headers.Add("Authorization", $"Bearer {loginModel.token}");


                //var httpresponse = (HttpWebResponse)webRequest.GetResponse();
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    Encoding ascii = Encoding.ASCII;
                    string fileName = response.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Do stuff with response.GetResponseStream();

                        using (Stream responseStream = response.GetResponseStream())
                        {
                            // Save the response content to a local file with the specified filename
                            strCSVFile = Path.Combine(strCSVFile, fileName); // Replace with your desired local directory
                            using (FileStream fileStream = File.Create(strCSVFile))
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                }
                            }

                            Console.WriteLine($"File '{fileName}' downloaded and saved to '{strCSVFile}'");

                            Console.WriteLine("Saving To Log ");

                            await LogApiRequestAsync(lApiConfig, strJwtToken, fileName, response.StatusCode, lApiURL);


                        }
                    }
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

            return strCSVFile;
        }

        public async Task GetParticipantDirectStream(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID)
        {
            timer.Start();

            var jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string connectionString = ConfigurationManager.ConnectionStrings["DashboardMPM"].ConnectionString;
            int chunkSize = Int32.Parse(ConfigurationManager.AppSettings["chunkSize"]);
            int bufferSize = Int32.Parse(ConfigurationManager.AppSettings["bufferSize"]);

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Set headers
                    client.DefaultRequestHeaders.Add("Accept", "text/csv");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginModel.token}");

                    HttpResponseMessage response = await client.GetAsync(apiUrl, HttpCompletionOption.ResponseHeadersRead);


                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, bufferSize))
                        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { MissingFieldFound = null }))
                        {
                            // Read and skip the header
                            await csv.ReadAsync();
                            csv.ReadHeader();

                            // Create a DataTable to hold the CSV data
                            DataTable dataTable = DtParticipantlist(strBatchID);

                            while (await csv.ReadAsync())
                            {
                                string idValue = csv.GetField<string>("id");

                                if (string.IsNullOrEmpty(idValue) || idValue.ToLower() == "end of file")
                                {
                                    // Skip this row
                                    continue;
                                }


                                DataRow row = dataTable.NewRow();
                                row["id"] = csv.GetField<string>("id");
                                row["clientRefId"] = csv.GetField<string>("clientRefId");
                                row["clientCompany"] = csv.GetField<string>("clientCompany");
                                row["lenderBranchCode"] = csv.GetField<string>("lenderBranchCode");
                                row["lenderBranchLabel"] = csv.GetField<string>("lenderBranchLabel");
                                row["idNumber"] = csv.GetField<string>("idNumber");
                                row["name"] = csv.GetField<string>("name");
                                row["dateOfBirth"] = csv.GetField<DateTime>("dateOfBirth");
                                row["gender"] = csv.GetField<string>("gender");
                                row["address"] = csv.GetField<string>("address");
                                row["maritalStatus"] = csv.GetField<string>("maritalStatus");
                                row["fundManagementType"] = csv.GetField<string>("fundManagementType");
                                row["debtorId"] = csv.GetField<string>("debtorId");
                                row["loanId"] = csv.GetField<string>("loanId");
                                row["lenderProductCode"] = csv.GetField<string>("lenderProductCode");
                                row["lenderProductType"] = csv.GetField<string>("lenderProductType");
                                row["ceilingAmount"] = csv.GetField<float>("ceilingAmount");
                                row["Suku_Bunga"] = csv.GetField<float>("Suku Bunga");
                                row["termInWeeks"] = csv.GetField<int>("termInWeeks");
                                row["cycle"] = csv.GetField<int>("cycle");
                                row["gracePeriodInWeeks"] = csv.GetField<int>("gracePeriodInWeeks");
                                row["disbursementDate"] = csv.GetField<DateTime>("disbursementDate");
                                row["dueDate"] = csv.GetField<DateTime>("dueDate");
                                row["termDurationInWeeks"] = csv.GetField<int>("termDurationInWeeks");
                                row["termStart"] = csv.GetField<DateTime>("termStart");
                                row["termEnd"] = csv.GetField<DateTime>("termEnd");
                                row["ageAtTermStart"] = csv.GetField<float>("ageAtTermStart");
                                row["ageAtTermEnd"] = csv.GetField<float>("ageAtTermEnd");
                                row["contract"] = csv.GetField<string>("contract");
                                row["premiumAmount"] = csv.GetField<float>("premiumAmount");
                                row["brokerageFeeAmount"] = csv.GetField<float>("brokerageFeeAmount");
                                row["coverNoteNumber"] = csv.GetField<string>("coverNoteNumber");

                                dataTable.Rows.Add(row);


                                if (dataTable.Rows.Count >= chunkSize)
                                {
                                    InsertChunk(dataTable, connectionString, strTblParticipant);
                                    Console.WriteLine("Inserting data .. " + dataTable.Rows.Count.ToString() + " Rows");
                                    dataTable.Clear();
                                }
                            }

                            // Insert any remaining rows
                            if (dataTable.Rows.Count > 0)
                            {
                                InsertChunk(dataTable, connectionString, strTblParticipant);
                                Console.WriteLine("Finalize Inserting data .. " + dataTable.Rows.Count.ToString() + " Rows");
                                dataTable.Clear();

                                Console.WriteLine("Saving To Log : " + response.Content.Headers.ContentDisposition.FileName);
                                await LogApiRequestAsync(lApiConfig, strJwtToken, response.Content.Headers.ContentDisposition.FileName, response.StatusCode, lApiURL);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Request timed out: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public async Task<tbl_placing_batch> getBatchListDetail(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID)
        {
            timer.Start();

            var jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            tbl_placing_batch lPlacing_batch = new tbl_placing_batch();

            try
            {
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID));
                webRequest.Method = lApiURL.FirstOrDefault().url_method;
                webRequest.ContentType = lApiConfig.FirstOrDefault().content_type;
                webRequest.Headers.Add("Authorization", $"Bearer {loginModel.token}");


                var httpresponse = (HttpWebResponse)webRequest.GetResponse();
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    Encoding ascii = Encoding.ASCII;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Do stuff with response.GetResponseStream();
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), ascii))
                        {
                            string strEnd = streamReader.ReadToEnd();
                            Console.WriteLine("Getting New Info : " + strEnd);

                            // Deserialize the JSON data into a list of your data model
                            lPlacing_batch = JsonConvert.DeserializeObject<tbl_placing_batch>(strEnd);
                            //Console.WriteLine("Saving To Log ");
                            await LogApiRequestAsync(lApiConfig, strJwtToken, response.StatusDescription, response.StatusCode, lApiURL);
                        }
                    }
                }


            }
            catch (Exception ex)
            {

                throw ex;

            }
            return lPlacing_batch;
        }

        public async Task<tbl_cover_notes> generateCovernote(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, List<invoiceListModel> invoiceLists )
        {
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();


            coverNotesModel coverNotesModel = new coverNotesModel
            {
                number = invoiceLists.FirstOrDefault().InvoiceNo  ,
                administrationFee ="0"
            };


            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            string coverNotesJson = JsonConvert.SerializeObject(coverNotesModel);
            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", invoiceLists.FirstOrDefault().BatchId);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");
                    request.Content = new StringContent(coverNotesJson, System.Text.Encoding.UTF8, "application/json");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        tblCoverNotes.id = responseObject.id;
                        tblCoverNotes.number =  responseObject.number;
                        tblCoverNotes.countOfInsurables = responseObject.countOfInsurables;
                        tblCoverNotes.sumOfPremium = responseObject.sumOfPremium.amount;
                        tblCoverNotes.status = responseObject.status;
                        tblCoverNotes.sumOfAmountAll = responseObject.sumOfAmountAll.amount;
                        tblCoverNotes.administrationFee = responseObject.administrationFee.amount;
                        tblCoverNotes.createdTime = responseObject.createdTime;
                        tblCoverNotes.finalizedTime = responseObject.finalizedTime;
                        tblCoverNotes.batch_id = invoiceLists.FirstOrDefault().BatchId;
                        tblCoverNotes.finalize_status = 0;

                        await LogApiRequestAsync(lApiConfig, coverNotesJson, responseString, response.StatusCode, lApiURL);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                    
                }
                
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            

            return tblCoverNotes;
        }

        public async Task<tbl_policies_holder> generatePolicyHolder(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID , string strPolisNo, string strpolisissued)
        {
            timer.Start();
            tbl_policies_holder tblPolciesHolder = new tbl_policies_holder();


            policiesHolderModel policiesHolderModel = new policiesHolderModel
            {
                number = strPolisNo,
                issuedDate = strpolisissued
            };


            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            string coverNotesJson = JsonConvert.SerializeObject(policiesHolderModel);
            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");
                    request.Content = new StringContent(coverNotesJson, System.Text.Encoding.UTF8, "application/json");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        tblPolciesHolder.id = responseObject.id;
                        tblPolciesHolder.number = responseObject.number;
                        tblPolciesHolder.status = responseObject.status;
                        tblPolciesHolder.createdTime = responseObject.createdTime;
                        tblPolciesHolder.issuedDate = responseObject.issuedDate;
                        tblPolciesHolder.batch_id = strBatchID;


                        await LogApiRequestAsync(lApiConfig, coverNotesJson, responseString, response.StatusCode, lApiURL);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }


            return tblPolciesHolder;
        }

        public async Task<bool> uploadParticipantRejection(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID, string strPathCSV)
        {
            bool blUploadSuccess = false;
            timer.Start();



            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    var content = new MultipartFormDataContent();

                    var fileStream = new FileStream(strPathCSV, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(strPathCSV));

                    request.Content = content;
                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                        blUploadSuccess = true;

                        await LogApiRequestAsync(lApiConfig, strPathCSV, responseString, response.StatusCode, lApiURL);
                        Console.WriteLine("Success Uploading Participant Rejection !");

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return blUploadSuccess;

        }
        public async Task <bool> uploadParticipantCoverNote(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strCovernote, string strPathCSV)
        {
            bool blUploadSuccess = false;
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();



            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            
            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strCovernote);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    var content = new MultipartFormDataContent();
                   
                    var fileStream = new FileStream(strPathCSV, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(strPathCSV));

                    request.Content = content;
                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
    
                        blUploadSuccess = true;

                        await LogApiRequestAsync(lApiConfig, strPathCSV, responseString, response.StatusCode, lApiURL);
                        
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return blUploadSuccess;

        }

        public async Task<bool> uploadParticipantPolicies(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID, string strPathCSV)
        {
            bool blUploadSuccess = false;
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();



            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    var content = new MultipartFormDataContent();

                    var fileStream = new FileStream(strPathCSV, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(strPathCSV));

                    request.Content = content;
                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                        blUploadSuccess = true;

                        await LogApiRequestAsync(lApiConfig, strPathCSV, responseString, response.StatusCode, lApiURL);
                        Console.WriteLine("Successfully uploaded: " + strPathCSV + " for policies certificate");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return blUploadSuccess;

        }

        public async Task<bool> uploadPoliciesCertificate(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strPoliciesId, string strPathCertif)
        {
            bool blUploadSuccess = false;
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();



            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strPoliciesId);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    var content = new MultipartFormDataContent();

                    var fileStream = new FileStream(strPathCertif, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(strPathCertif));

                    request.Content = content;
                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                        blUploadSuccess = true;

                        await LogApiRequestAsync(lApiConfig, strPathCertif, responseString, response.StatusCode, lApiURL);
                        Console.WriteLine("Succed Upload Certificate :" + strPathCertif);

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return blUploadSuccess;

        }

        public async Task<bool> uploadDocumentCoverNote(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID, string strPathCovernoteDocs)
        {
            bool blUploadSuccess = false;
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();



            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    var content = new MultipartFormDataContent();

                    var fileStream = new FileStream(strPathCovernoteDocs, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream), "file", Path.GetFileName(strPathCovernoteDocs));

                    request.Content = content;
                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                        blUploadSuccess = true;

                        await LogApiRequestAsync(lApiConfig, strPathCovernoteDocs, responseString, response.StatusCode, lApiURL);

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return blUploadSuccess;

        }

        public async Task<tbl_cover_notes> extractParticipantCoverNote(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strCoverNote ,string strBatchID)
        {
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();


            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strCoverNote);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        tblCoverNotes.id = responseObject.id;
                        tblCoverNotes.number = responseObject.number;
                        tblCoverNotes.countOfInsurables = responseObject.countOfInsurables;
                        tblCoverNotes.sumOfPremium = responseObject.sumOfPremium.amount;
                        tblCoverNotes.status = responseObject.status;
                        tblCoverNotes.sumOfAmountAll = responseObject.sumOfAmountAll.amount;
                        tblCoverNotes.administrationFee = responseObject.administrationFee.amount;
                        tblCoverNotes.createdTime = responseObject.createdTime;
                        tblCoverNotes.finalizedTime = responseObject.finalizedTime;
                        tblCoverNotes.batch_id = strBatchID;

                        await LogApiRequestAsync(lApiConfig, responseString, responseString, response.StatusCode, lApiURL);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }


            return tblCoverNotes;
        }



        public async Task<bool> extractParticipantRejected(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchID)
        {
            timer.Start();

            bool blExtractStatus = false;

            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                        await LogApiRequestAsync(lApiConfig, strBatchID, responseString, response.StatusCode, lApiURL);
                        blExtractStatus = true;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return blExtractStatus;
        }


        public async Task<tbl_cover_notes> finalizeCoverNote(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, List<tbl_cover_notes> lTblCoverNotes )
        {
           
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();

            coverNoteFinalizeModel coverNotesFinalizeModel = new coverNoteFinalizeModel
            {
                coverNoteId = lTblCoverNotes.FirstOrDefault().id
            };


            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            string coverNotesJson = JsonConvert.SerializeObject(coverNotesFinalizeModel);


            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", lTblCoverNotes.FirstOrDefault().id);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");
                    request.Content = new StringContent(coverNotesJson, System.Text.Encoding.UTF8, "application/json");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        tblCoverNotes.id = responseObject.id;
                        tblCoverNotes.number = responseObject.number;
                        tblCoverNotes.countOfInsurables = responseObject.countOfInsurables;
                        tblCoverNotes.sumOfPremium = responseObject.sumOfPremium.amount;
                        tblCoverNotes.status = responseObject.status;
                        tblCoverNotes.sumOfAmountAll = responseObject.sumOfAmountAll.amount;
                        tblCoverNotes.administrationFee = responseObject.administrationFee.amount;
                        tblCoverNotes.createdTime = responseObject.createdTime;
                        tblCoverNotes.finalizedTime = responseObject.finalizedTime;
                        tblCoverNotes.batch_id = lTblCoverNotes.FirstOrDefault().batch_id;
                        tblCoverNotes.finalize_status = 1;

                        await LogApiRequestAsync(lApiConfig, responseString, responseString, response.StatusCode, lApiURL);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return tblCoverNotes;
        }

        public async Task<tbl_placing_batch> finalizePlacingBatch(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strBatchId)
        {

            timer.Start();
//            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();

            placingBatchFinalizeModel placingBatchFinalizeModel = new placingBatchFinalizeModel
            {
                PlacingBatchId = strBatchId
            };

            tbl_placing_batch lPlacing_batch = new tbl_placing_batch();

            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);
            string placingBatchJson = JsonConvert.SerializeObject(placingBatchFinalizeModel);


            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strBatchId);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");
                    request.Content = new StringContent(placingBatchJson, System.Text.Encoding.UTF8, "application/json");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        lPlacing_batch.id = responseObject.id;
                        lPlacing_batch.createdTime = responseObject.createdTime;
                        lPlacing_batch.status = responseObject.status;
                        lPlacing_batch.totalInsurables = responseObject.totalInsurables;
                        lPlacing_batch.totalUncoveredInsurables = responseObject.totalUncoveredInsurables;
                        lPlacing_batch.totalRejectedInsurables = responseObject.totalRejectedInsurables;
                        lPlacing_batch.totalCoveredInsurables = responseObject.totalCoveredInsurables;
                        lPlacing_batch.totalCoveredInsurablesUnderPolicy = responseObject.totalCoveredInsurablesUnderPolicy;
                        lPlacing_batch.totalCoveredInsurablesNotUnderPolicy = responseObject.totalCoveredInsurablesNotUnderPolicy;

                        await LogApiRequestAsync(lApiConfig, responseString, responseString, response.StatusCode, lApiURL);

                        Console.WriteLine($"Succes Finalizing batch id : {responseObject.id} . Next Step Waiting MPM paid The Batches");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return lPlacing_batch;
        }

        public async Task<tbl_policies_holder> finalizePoliciesHolder(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strPoliciesId)
        {

            timer.Start();


            tbl_policies_holder tblPoliciesHolder = new tbl_policies_holder();

            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);


            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strPoliciesId);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");
                   // request.Content = new StringContent(placingBatchJson, System.Text.Encoding.UTF8, "application/json");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        tblPoliciesHolder.id = strPoliciesId;
                        tblPoliciesHolder.number = responseObject.number;
                        tblPoliciesHolder.status = responseObject.status;
                        tblPoliciesHolder.sumOfPremium = (string)responseObject.sumOfPremium.amount;
                        tblPoliciesHolder.sumOfAmountCovered = (string)responseObject.sumOfAmountCovered.amount;
                        tblPoliciesHolder.sumOfAmountAll = (string)responseObject.sumOfAmountAll.amount;
                        tblPoliciesHolder.sumOfBrokerageFee = (string)responseObject.sumOfBrokerageFee.amount;
                        tblPoliciesHolder.countOfInsurables = responseObject.countOfInsurables;
                        tblPoliciesHolder.createdTime = responseObject.createdTime;
                        tblPoliciesHolder.finalizedTime = responseObject.finalizedTime;
                        tblPoliciesHolder.issuedDate = responseObject.issuedDate;

                        await LogApiRequestAsync(lApiConfig, responseString, responseString, response.StatusCode, lApiURL);

                        Console.WriteLine("Finalizing Policies Holder Succed :" + responseObject.finalizedTime + " | " +strPoliciesId);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return tblPoliciesHolder;
        }


        public async Task<bool> extractParticipantpolicies(List<api_client_configuration> lApiConfig, List<api_url> lApiURL, string strJwtToken, string strpolciesID, string strBatchID)
        {
            timer.Start();
            tbl_cover_notes tblCoverNotes = new tbl_cover_notes();
            bool blReturn = false;

            string jsonbody = strJwtToken;
            loginModel loginModel = JsonConvert.DeserializeObject<loginModel>(jsonbody);

            string responseString;

            try
            {
                using (HttpClient client = HttpClientCustomslHandling())
                {
                    // Replace the URL with your API endpoint
                    string apiUrl = lApiConfig.FirstOrDefault().url + lApiURL.FirstOrDefault().url_value.Replace("{id}", strpolciesID);

                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("Authorization", $"Bearer {loginModel.token}");

                    // Send the request
                    HttpResponseMessage response = await client.SendAsync(request);

                    Console.WriteLine("Open Communication : " + apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("status Code = " + response.StatusCode);
                        responseString = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        // Map the response to tbl_cover_notes object
                        blReturn = true;

                        await LogApiRequestAsync(lApiConfig, responseString, responseString, response.StatusCode, lApiURL);

                        Console.WriteLine("success extracting CSV | Waiting Core MPM Finalized Extracting file !" );
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }

                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }


            return blReturn;
        }

        //Setup skeleton for populate data
        #region etc
        private DataTable DtParticipantlist(string strBatchId)
        {
            DataTable dataTable = new DataTable(strTblParticipant);

            dataTable.Columns.Add("regno", typeof(string)).DefaultValue = "";
            dataTable.Columns.Add("regno_batch", typeof(string)).DefaultValue = "";
            dataTable.Columns.Add("batch_id", typeof(string)).DefaultValue = strBatchId;
            dataTable.Columns.Add("participant_status", typeof(int)).DefaultValue = 0;
            dataTable.Columns.Add("notif_status", typeof(int)).DefaultValue = 0;
            dataTable.Columns.Add("id", typeof(string));
            dataTable.Columns.Add("clientRefId", typeof(string));
            dataTable.Columns.Add("clientCompany", typeof(string));
            dataTable.Columns.Add("lenderBranchCode", typeof(string));
            dataTable.Columns.Add("lenderBranchLabel", typeof(string));
            dataTable.Columns.Add("idNumber", typeof(string));
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Columns.Add("dateOfBirth", typeof(DateTime));
            dataTable.Columns.Add("gender", typeof(string));
            dataTable.Columns.Add("address", typeof(string));
            dataTable.Columns.Add("maritalStatus", typeof(string));
            dataTable.Columns.Add("fundManagementType", typeof(string));
            dataTable.Columns.Add("debtorId", typeof(string));
            dataTable.Columns.Add("loanId", typeof(string));
            dataTable.Columns.Add("lenderProductCode", typeof(string));
            dataTable.Columns.Add("lenderProductType", typeof(string));
            dataTable.Columns.Add("ceilingAmount", typeof(decimal));
            dataTable.Columns.Add("Suku_Bunga", typeof(decimal));
            dataTable.Columns.Add("termInWeeks", typeof(int));
            dataTable.Columns.Add("cycle", typeof(int));
            dataTable.Columns.Add("gracePeriodInWeeks", typeof(int));
            dataTable.Columns.Add("disbursementDate", typeof(DateTime));
            dataTable.Columns.Add("dueDate", typeof(DateTime));
            dataTable.Columns.Add("termDurationInWeeks", typeof(int));
            dataTable.Columns.Add("termStart", typeof(DateTime));
            dataTable.Columns.Add("termEnd", typeof(DateTime));
            dataTable.Columns.Add("ageAtTermStart", typeof(float));
            dataTable.Columns.Add("ageAtTermEnd", typeof(float));
            dataTable.Columns.Add("contract", typeof(string));
            dataTable.Columns.Add("premiumAmount", typeof(float));
            dataTable.Columns.Add("brokerageFeeAmount", typeof(float));
            dataTable.Columns.Add("coverNoteNumber", typeof(string));


            return dataTable;
        }

        static bool IsConnectionStringValid(string connectionString)
        {
            try
            {
                // Attempt to create a SqlConnectionStringBuilder
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

                // Optionally, you can check specific properties of the builder to ensure they are valid

                return true;
            }
            catch (ArgumentException)
            {
                // The connection string is not valid
                return false;
            }
        }


        static void InsertChunk(DataTable dataTable, string connectionString, string strTblParticipant)
        {
            try
            {

                bool blConntrue = IsConnectionStringValid(connectionString);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {

                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection))
                        {
                            sqlBulkCopy.DestinationTableName = strTblParticipant;

                            foreach (DataColumn column in dataTable.Columns)
                            {
                                //Setup the column mappings, anything ommitted is skipped
                                sqlBulkCopy.BulkCopyTimeout = 600; // Set the timeout in seconds
                                                                   // Automatically generate column mappings based on column names
                                sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                            }
                            try
                            {

                                sqlBulkCopy.WriteToServer(dataTable);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error inserting chunk: " + ex.Message);
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine("Connection is not open.");
                        // Handle the situation where the connection is not open
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during connection: {ex.Message}");
                //  throw ex;
            }
        }

        private static HttpClient HttpClientCustomslHandling()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateServerCertificate
            };

            // Set TLS version to 1.2
            //handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            System.Net.ServicePointManager.Expect100Continue = false;

            // ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //ServicePointManager.DnsRefreshTimeout = 0;


            return new HttpClient(handler);
        }

        private static bool ValidateServerCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Add your custom validation logic here
            // You may want to check the certificate's issuer, expiration, etc.

            // For example, you can implement a basic check by requiring the certificate to be issued by a trusted CA
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // Check if the certificate is issued by a trusted CA
                X509ChainStatus[] chainStatus = chain.ChainStatus;
                bool isCertificateTrusted = chainStatus == null || chainStatus.Length == 0;

                if (!isCertificateTrusted)
                {
                    Console.WriteLine("Certificate validation failed: Not issued by a trusted CA.");
                }

                return isCertificateTrusted;
            }

            Console.WriteLine($"Certificate validation failed with errors: {sslPolicyErrors}");
            return false;
        }
        #endregion etc

    }

}