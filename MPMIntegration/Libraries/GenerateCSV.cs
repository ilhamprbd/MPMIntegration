using MPMIntegration.APIModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Libraries
{
    class GenerateCSV
    {

        public string ParticipantCSV(List<invoiceListModel> invoiceLists)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DashboardMPM"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["CSVFilePath"];
            string query = "SELECT id FROM tbl_participant_list WHERE participant_status = 1  AND notif_status = 0 and coverNoteNumber = '" + invoiceLists.FirstOrDefault().InvoiceNo + "'";
            string countQuery = "SELECT COUNT(1) FROM tbl_participant_list WHERE  batch_id = @batchID and coverNoteNumber = '" + invoiceLists.FirstOrDefault().InvoiceNo + "'";
            string csvFilePath = Path.Combine(strPathFile, invoiceLists.FirstOrDefault().InvoiceNo + ".csv");

            try
            {
                int totalCount = 0;
                int participantCount = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the count of participants
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        countCommand.Parameters.AddWithValue("@batchID", invoiceLists.FirstOrDefault().BatchId);
                        totalCount = (int)countCommand.ExecuteScalar();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@batchID", invoiceLists.FirstOrDefault().BatchId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if file exists
                            if (File.Exists(csvFilePath))
                            {
                                File.Delete(csvFilePath); // Delete existing file
                            }


                            using (StreamWriter writer = new StreamWriter(csvFilePath))
                            {
                                // Write the data
                                while (reader.Read())
                                {
                                    writer.WriteLine(reader["id"].ToString());
                                    participantCount++;
                                }
                            }

                            // Log the number of participants written to the CSV
                            Console.WriteLine($"CSV file generated successfully at {csvFilePath}. Total participants written: {participantCount}");
                        }
                    }
                }

                // Log the total count of participants from the database
                Console.WriteLine($"Total participants from the database for covernote {invoiceLists.FirstOrDefault().InvoiceNo }: {totalCount}");
                Console.WriteLine($"Total participants rejected : {totalCount - participantCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return csvFilePath;
        }


        public string PoliciesParticipantCSV(List<invoiceListModel> invoiceLists)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DashboardMPM"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["CSVFilePath"];
            string query = "SELECT id,regno FROM tbl_participant_list WHERE participant_status = 1  AND notif_status = 1 and regno_batch = '" + invoiceLists.FirstOrDefault().RegnoBatch + "'";
            string countQuery = "SELECT COUNT(1) FROM tbl_participant_list WHERE  batch_id = @batchID  and regno_batch = '" + invoiceLists.FirstOrDefault().RegnoBatch + "'";
            string csvFilePath = Path.Combine(strPathFile, invoiceLists.FirstOrDefault().RegnoBatch + ".csv");

            try
            {
                int totalCount = 0;
                int participantCount = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the count of participants
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        countCommand.Parameters.AddWithValue("@batchID", invoiceLists.FirstOrDefault().BatchId);
                        totalCount = (int)countCommand.ExecuteScalar();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@batchID", invoiceLists.FirstOrDefault().BatchId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if file exists
                            if (File.Exists(csvFilePath))
                            {
                                File.Delete(csvFilePath); // Delete existing file
                            }


                            using (StreamWriter writer = new StreamWriter(csvFilePath))
                            {
                                // Write the data
                                while (reader.Read())
                                {
                                    writer.WriteLine(reader["id"].ToString() + "," + reader["regno"].ToString());
                                    //writer.WriteLine();
                                    participantCount++;
                                }
                            }

                            // Log the number of participants written to the CSV
                            Console.WriteLine($"CSV file generated successfully at {csvFilePath}. Total participants written: {participantCount}");
                        }
                    }
                }

                // Log the total count of participants from the database
                Console.WriteLine($"Total participants from the database for covernote {invoiceLists.FirstOrDefault().InvoiceNo }: {totalCount}");
                Console.WriteLine($"Total participants rejected : {totalCount - participantCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return csvFilePath;
        }


        public string ParticipantRejectedCSV(string strBatchID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DashboardMPM"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["CSVFilePath"];
            string query = "SELECT id FROM tbl_participant_list WHERE participant_status not in (0,1) AND batch_id = @batchID AND notif_status = 0";
            string countQuery = "SELECT COUNT(1) FROM tbl_participant_list WHERE  batch_id = @batchID AND notif_status = 0";
            string csvFilePath = Path.Combine(strPathFile, "R-" + strBatchID + ".csv");

            try
            {
                int totalCount = 0;
                int participantCount = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the count of participants
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        countCommand.Parameters.AddWithValue("@batchID", strBatchID);
                        totalCount = (int)countCommand.ExecuteScalar();
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@batchID", strBatchID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if file exists
                            if (File.Exists(csvFilePath))
                            {
                                File.Delete(csvFilePath); // Delete existing file
                            }


                            using (StreamWriter writer = new StreamWriter(csvFilePath))
                            {
                                // Write the data
                                while (reader.Read())
                                {
                                    writer.WriteLine(reader["id"].ToString());
                                    participantCount++;
                                }
                            }

                            // Log the number of participants written to the CSV
                            Console.WriteLine($"CSV file generated successfully at {csvFilePath}. Total rejected participants written: {participantCount}");
                        }
                    }
                }

                // Log the total count of participants from the database
                Console.WriteLine($"Total rejected participants from the database for batch {strBatchID}: {totalCount}");
                Console.WriteLine($"Total participants rejected : {totalCount - participantCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return csvFilePath;
        }


        public string ParticipantIssuedReport(string strRegnoBatch)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["AJPCore"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["DocsRecon"];
            strPathFile = Path.Combine(strPathFile, strRegnoBatch + ".csv");

            try
            {
                // Ensure directory exists for the output file
                string directoryPath = Path.GetDirectoryName(strPathFile);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Delete file if it already exists
                if (File.Exists(strPathFile))
                {
                    File.Delete(strPathFile);
                }

                // Create a SqlConnection
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Create a SqlCommand for the stored procedure
                    using (SqlCommand command = new SqlCommand("sp_get_penempatan_peserta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters if required
                         command.Parameters.AddWithValue("@regnoBatch", strRegnoBatch);

                        // Create a SqlDataReader to fetch the data
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Create a StreamWriter to write to the CSV file
                            using (StreamWriter writer = new StreamWriter(strPathFile, false, Encoding.UTF8))
                            {
                                // Write the header (optional)
                                WriteCsvHeader(writer, reader);

                                // Write the data
                                WriteCsvData(writer, reader);
                            }
                        }
                    }
                }

                Console.WriteLine("CSV file generated successfully: " + strPathFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating CSV file: " + ex.Message);
            }

            return strPathFile;
        }

        private void WriteCsvHeader(StreamWriter writer, SqlDataReader reader)
        {
            StringBuilder header = new StringBuilder();
            int fieldCount = reader.FieldCount;

            for (int i = 0; i < fieldCount; i++)
            {
                header.Append(reader.GetName(i));
                if (i < fieldCount - 1)
                    header.Append(",");
            }

            writer.WriteLine(header.ToString());
        }

        private void WriteCsvData(StreamWriter writer, SqlDataReader reader)
        {
            int fieldCount = reader.FieldCount;

            while (reader.Read())
            {
                StringBuilder rowData = new StringBuilder();

                for (int i = 0; i < fieldCount; i++)
                {
                    rowData.Append(reader[i].ToString());
                    if (i < fieldCount - 1)
                        rowData.Append(",");
                }

                writer.WriteLine(rowData.ToString());
            }
        }

        

    }
}