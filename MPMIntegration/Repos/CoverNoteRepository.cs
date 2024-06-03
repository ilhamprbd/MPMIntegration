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
                    .Where(p => p.batch_id == strBatchId)
                    .Select(p => p.coverNoteNumber)
                    .Distinct()
                    .ToListAsync();

                return invoiceList.Select(invoiceNo => new invoiceListModel { InvoiceNo = invoiceNo }).ToList();
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


    }
}
