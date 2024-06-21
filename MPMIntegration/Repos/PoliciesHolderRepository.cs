using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    public class PoliciesHolderRepository
    {


        public async Task SavePoliciesHolder(tbl_policies_holder data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        db.tbl_policies_holder.Add(data);
                        db.SaveChanges();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SavePoliciesHolder : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task<string> getPoliciesID(string strBatchId)
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
