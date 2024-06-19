using MPMIntegration.APIModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    public class CoverNoteRepository
    {

        public async Task <List<invoiceListModel>> getInvoiceList(string strBatchId)
        {


            using (var context = new DashBoardMPMEntities1()) // Replace YourDbContext with your actual DbContext class
            {
                var invoiceList = await context.tbl_participant_list
                    .Where(p => p.batch_id == strBatchId && p.notif_status == 0)
                    .Select(p => p.coverNoteNumber)
                    .Distinct()
                    .ToListAsync();

                return invoiceList.Select(invoiceNo => new invoiceListModel { InvoiceNo = invoiceNo }).ToList();
            }
        }

        public async Task UpdateInvoiceGenerateParti(string strInvoice)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    var batchLists = await db.tbl_participant_list
                          .Where(d => d.coverNoteNumber == strInvoice)
                          .ToListAsync();

                    foreach (var batchList in batchLists)
                    {
                        batchList.notif_status = 1;
                    }

                    // Save changes to the database
                    await db.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SAVING BATCH LIST : " + ex.Message);
                    throw ex;
                }
            }
        }




        public async Task<List<it_report_list>> GetURLReport(int intCode)
        {

            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Using Task.Run to offload synchronous code to a background thread
                    var ListApiConfig = await Task.Run(() => db.it_report_list.Where(d => d.code == intCode).ToList());

                    return ListApiConfig;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public async Task<string>  getCoverNoteId(string strBatchId)
        {

            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Assuming you want to retrieve the coverNoteId column
                    return await Task.Run(() =>
                        db.tbl_cover_notes
                          .Where(d => d.batch_id == strBatchId)
                          .Select(d => d.id)
                          .FirstOrDefault());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }

    }
}
