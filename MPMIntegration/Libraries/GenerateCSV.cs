using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Libraries
{
    class GenerateCSV
    {

        public string ParticipantCSV(string strBatchID)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["AJPCore"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["CSVFilePath"];
            //string connectionString = "your_connection_string";
            string query = "SELECT id FROM tbl_participant_list where participant_status = 1 and batch_id = '" + strBatchID + "'"  + " and notif_status = 0";
            string csvFilePath = Path.Combine( strPathFile,strBatchID + ".csv");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
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
                            }
                        }
                    }
                }

                Console.WriteLine("CSV file generated successfully." + csvFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return csvFilePath;
        }
    }
}
