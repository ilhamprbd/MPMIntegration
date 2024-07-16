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

        public async Task UpdatePoliciesHolder(tbl_policies_holder data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Find the existing entity by its primary key (assuming `id` is the primary key)
                    var existingEntity = await db.tbl_policies_holder.FindAsync(data.id);

                    if (existingEntity != null)
                    {
                        // Update the properties of the existing entity with the new values
                        existingEntity.number = data.number;
                        existingEntity.status = data.status;
                        existingEntity.sumOfPremium = data.sumOfPremium;
                        existingEntity.sumOfAmountCovered = data.sumOfAmountCovered;
                        existingEntity.sumOfAmountAll = data.sumOfAmountAll;
                        existingEntity.sumOfBrokerageFee = data.sumOfBrokerageFee;
                        existingEntity.countOfInsurables = data.countOfInsurables;
                        existingEntity.createdTime = data.createdTime;
                        existingEntity.finalizedTime = data.finalizedTime;
                        existingEntity.issuedDate = data.issuedDate;

                        // Save the changes
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Policies holder not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR UpdatePoliciesHolder: " + ex.Message);
                    throw;
                }
            }
        }


        public async Task<List<tbl_policies_holder>> GetPoliciesFinalBatch(string strBatchId)
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_policies_holder> batchList = await Task.Run(() => db.tbl_policies_holder.Where<tbl_policies_holder>(d => d.batch_id == strBatchId).ToList());
                    //List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                        db.tbl_policies_holder
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
