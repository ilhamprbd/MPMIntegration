using Microsoft.VisualBasic.FileIO;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MPMIntegration.Libraries
{
    class WriteCSVtoServer
    {


        protected const string _truncateLiveTableCommandText = @"TRUNCATE TABLE tbl_participant_list";
        protected const int _batchSize = 100000;

        public void LoadCsvDataIntoSqlServer(string strFileName, string strBatchId)
        {
            // This should be the full path

            strFileName = @"D:\folderCSV\placing-batch-csv.2023-06-21-07-44-29";
            strBatchId = @"0188dce9-7da3-7ce5-8fb2-a52f7fd47fec";

            var createdCount = 0;

            using (var textFieldParser = new TextFieldParser(strFileName))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.Delimiters = new[] { "," };
                textFieldParser.HasFieldsEnclosedInQuotes = true;


                var connectionString = ConfigurationManager.ConnectionStrings["AJPCore"].ConnectionString;

                var dataTable = new DataTable("tbl_participant_list");

                //// Add the columns in the temp table

                dataTable.Columns.Add("id");
                dataTable.Columns.Add("clientRefId");
                dataTable.Columns.Add("clientCompany");
                dataTable.Columns.Add("lenderBranchCode");
                dataTable.Columns.Add("lenderBranchLabel");
                dataTable.Columns.Add("idNumber");
                dataTable.Columns.Add("name");
                dataTable.Columns.Add("dateOfBirth");
                dataTable.Columns.Add("gender");
                dataTable.Columns.Add("address");
                dataTable.Columns.Add("maritalStatus");
                dataTable.Columns.Add("fundManagementType");
                dataTable.Columns.Add("debtorId");
                dataTable.Columns.Add("loanId");
                dataTable.Columns.Add("lenderProductCode");
                dataTable.Columns.Add("lenderProductType");
                dataTable.Columns.Add("ceilingAmount");
                dataTable.Columns.Add("Suku_Bunga");
                dataTable.Columns.Add("termInWeeks");
                dataTable.Columns.Add("cycle");
                dataTable.Columns.Add("gracePeriodInWeeks");
                dataTable.Columns.Add("disbursementDate");
                dataTable.Columns.Add("dueDate");
                dataTable.Columns.Add("termDurationInWeeks");
                dataTable.Columns.Add("termStart");
                dataTable.Columns.Add("termEnd");
                dataTable.Columns.Add("ageAtTermStart");
                dataTable.Columns.Add("ageAtTermEnd");
                dataTable.Columns.Add("contract");
                dataTable.Columns.Add("premiumAmount");
                dataTable.Columns.Add("brokerageFeeAmount");
                dataTable.Columns.Add("coverNoteNumber");
                dataTable.Columns.Add("regno").DefaultValue = "PNM";
                dataTable.Columns.Add("batch_id").DefaultValue = strBatchId;
                dataTable.Columns.Add("participant_status").DefaultValue = 0;
                dataTable.Columns.Add("notif_status").DefaultValue = 0;
                

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    // Truncate the live table
                    using (var sqlCommand = new SqlCommand(_truncateLiveTableCommandText, sqlConnection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    // Create the bulk copy object
                    var sqlBulkCopy = new SqlBulkCopy(sqlConnection)
                    {
                        DestinationTableName = "tbl_participant_list"
                    };

                    //Setup the column mappings, anything ommitted is skipped
                    sqlBulkCopy.ColumnMappings.Add("id", "id");
                    sqlBulkCopy.ColumnMappings.Add("clientRefId", "clientRefId");
                    sqlBulkCopy.ColumnMappings.Add("clientCompany", "clientCompany");
                    sqlBulkCopy.ColumnMappings.Add("lenderBranchCode", "lenderBranchCode");
                    sqlBulkCopy.ColumnMappings.Add("lenderBranchLabel", "lenderBranchLabel");
                    sqlBulkCopy.ColumnMappings.Add("idNumber", "idNumber");
                    sqlBulkCopy.ColumnMappings.Add("name", "name");
                    sqlBulkCopy.ColumnMappings.Add("dateOfBirth", "dateOfBirth");
                    sqlBulkCopy.ColumnMappings.Add("gender", "gender");
                    sqlBulkCopy.ColumnMappings.Add("address", "address");
                    sqlBulkCopy.ColumnMappings.Add("maritalStatus", "maritalStatus");
                    sqlBulkCopy.ColumnMappings.Add("fundManagementType", "fundManagementType");
                    sqlBulkCopy.ColumnMappings.Add("debtorId", "debtorId");
                    sqlBulkCopy.ColumnMappings.Add("loanId", "loanId");
                    sqlBulkCopy.ColumnMappings.Add("lenderProductCode", "lenderProductCode");
                    sqlBulkCopy.ColumnMappings.Add("lenderProductType", "lenderProductType");
                    sqlBulkCopy.ColumnMappings.Add("ceilingAmount", "ceilingAmount");
                    sqlBulkCopy.ColumnMappings.Add("Suku_Bunga", "Suku_Bunga");
                    sqlBulkCopy.ColumnMappings.Add("termInWeeks", "termInWeeks");
                    sqlBulkCopy.ColumnMappings.Add("cycle", "cycle");
                    sqlBulkCopy.ColumnMappings.Add("gracePeriodInWeeks", "gracePeriodInWeeks");
                    sqlBulkCopy.ColumnMappings.Add("disbursementDate", "disbursementDate");
                    sqlBulkCopy.ColumnMappings.Add("dueDate", "dueDate");
                    sqlBulkCopy.ColumnMappings.Add("termDurationInWeeks", "termDurationInWeeks");
                    sqlBulkCopy.ColumnMappings.Add("termStart", "termStart");
                    sqlBulkCopy.ColumnMappings.Add("termEnd", "termEnd");
                    sqlBulkCopy.ColumnMappings.Add("ageAtTermStart", "ageAtTermStart");
                    sqlBulkCopy.ColumnMappings.Add("ageAtTermEnd", "ageAtTermEnd");
                    sqlBulkCopy.ColumnMappings.Add("contract", "contract");
                    sqlBulkCopy.ColumnMappings.Add("premiumAmount", "premiumAmount");
                    sqlBulkCopy.ColumnMappings.Add("brokerageFeeAmount", "brokerageFeeAmount");
                    sqlBulkCopy.ColumnMappings.Add("coverNoteNumber", "coverNoteNumber");
                    sqlBulkCopy.ColumnMappings.Add("regno", "regno");
                    sqlBulkCopy.ColumnMappings.Add("batch_id", "batch_id");
                    sqlBulkCopy.ColumnMappings.Add("participant_status", "participant_status");
                    sqlBulkCopy.ColumnMappings.Add("notif_status", "notif_status");
                    



                    // Loop through the CSV and load each set of 100,000 records into a DataTable
                    // Then send it to the LiveTable
                    while (!textFieldParser.EndOfData)
                    {
                        createdCount++;


                            dataTable.Rows.Add(textFieldParser.ReadFields());



                            if (createdCount % _batchSize == 0)
                            {
                                InsertDataTable(sqlBulkCopy, sqlConnection, dataTable);

                                break;
                          }

                    }

                    // Don't forget to send the last batch under 100,000
                    InsertDataTable(sqlBulkCopy, sqlConnection, dataTable);

                    sqlConnection.Close();
                }
            }
        }

        protected void InsertDataTable(SqlBulkCopy sqlBulkCopy, SqlConnection sqlConnection, DataTable dataTable)
        {
            sqlBulkCopy.WriteToServer(dataTable);

            dataTable.Rows.Clear();
        }


    }
}
