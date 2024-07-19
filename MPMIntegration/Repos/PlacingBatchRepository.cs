using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    public class PlacingBatchRepository
    {
        public async Task SavePlacingBatch(List<tbl_placing_batch> reconList)
        {
            List<tbl_placing_batch> cleanList = new List<tbl_placing_batch>();

            using (var db = new DashBoardMPMEntities1())
            {
                DateTime dtParam = DateTime.Now.AddMonths(-3);

                // Get existing batches from the database based on your conditions
                List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.insert_date >= dtParam).ToList());

                // Identify items in the new list that do not exist in the existing list
                List<tbl_placing_batch> nonExistingBatchList = reconList
                    .Where(newBatch => !batchList.Any(existingBatch => existingBatch.id == newBatch.id))
                    .ToList();

                foreach (var _data in nonExistingBatchList)
                {
                    //if (_data.status.ToUpper() == "ISSUED")
                    //{
                    _data.insert_date = DateTime.Now;
                    _data.batch_status = 0;
                    await SaveBatchList(_data);
                    //}
                }
            }
        }

        public async Task<List<tbl_placing_batch>> GetBatchIssued()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.totalInsurables > 0).ToList());
                    //List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<tbl_participant_list>> GetParticipantRejected()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_participant_list> batchList = await Task.Run(() => db.tbl_participant_list.Where<tbl_participant_list>(d => d.participant_status != 1 && d.participant_status != 0 && d.notif_status == 0 ).ToList());
                    //List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<tbl_placing_batch>> GetBatchExecParticipant()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.batch_status == 0).ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<tbl_placing_batch>> GetPoliciesbatchGen()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.status.ToUpper() == "paid" && d.batch_status == 1).ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<tbl_placing_batch>> GetPoliciesFinalize()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.status.ToUpper() == "PAID" && d.batch_status == 4).ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<tbl_placing_batch>> GetBatchCoverNoteFinalize()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.batch_status == 2 && d.status.ToUpper()  == "ISSUED").ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task SaveBatchList(tbl_placing_batch data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        db.tbl_placing_batch.Add(data);
                        db.SaveChanges();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SAVING BATCH LIST : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task UpdateBatchListStatus(string strBatchId, int intBatchStatus)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    tbl_placing_batch batchList = await Task.Run(() => db.tbl_placing_batch.SingleOrDefault<tbl_placing_batch>(d => d.id == strBatchId));

                    if (batchList != null)
                    {
                        batchList.batch_status = intBatchStatus;
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR UpdateBatchListStatus : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task UpdateCoverNotesDetail(List<tbl_cover_notes> data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    foreach (var item in data)
                    {
                        // Fetch the existing entity from the database
                        var existingEntity = await db.tbl_cover_notes.SingleOrDefaultAsync(d => d.id == item.id);

                        if (existingEntity != null)
                        {
                            // Update the entity with new values
                            existingEntity.number = item.number;
                            existingEntity.countOfInsurables = item.countOfInsurables;
                            existingEntity.sumOfPremium = item.sumOfPremium;
                            existingEntity.status = item.status;
                            existingEntity.sumOfAmountAll = item.sumOfAmountAll;
                            existingEntity.administrationFee = item.administrationFee;
                            existingEntity.createdTime = item.createdTime;
                            existingEntity.finalizedTime = item.finalizedTime;
                            existingEntity.batch_id = item.batch_id;

                            Console.WriteLine("Finalized CoverNote number " + item.number + " Success.");
                        }
                        else
                        {
                            Console.WriteLine("Cover note with ID " + item.id + " not found.");
                        }
                    }

                    // Save all changes to the database
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR UpdateCoverNotesDetail: " + ex.Message);
                    throw;
                }
            }
        }

        public async Task UpdateCoverNotesPath(tbl_cover_notes data, string strPathFileCsv)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    tbl_cover_notes tblCoverNotes = await Task.Run(() => db.tbl_cover_notes.SingleOrDefault(d => d.id == data.id));

                    if (tblCoverNotes != null)
                    {
                        // Update multiple values in the batchList object based on the dataModel

                        tblCoverNotes.path_file_csv = strPathFileCsv; ;

                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine("Batch with ID " + data.id + " not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR UpdateCoverNotesDetail : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task UpdateBatchListDetail(tbl_placing_batch data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    tbl_placing_batch batchList = await Task.Run(() => db.tbl_placing_batch.SingleOrDefault(d => d.id == data.id));

                    if (batchList != null)
                    {
                        // Update multiple values in the batchList object based on the dataModel
                        batchList.status = data.status;
                        batchList.totalInsurables = data.totalInsurables;
                        batchList.totalCoveredInsurables = data.totalCoveredInsurables;
                        batchList.totalRejectedInsurables = data.totalRejectedInsurables;
                        batchList.totalUncoveredInsurables = data.totalUncoveredInsurables;
                        batchList.totalCoveredInsurablesUnderPolicy = data.totalCoveredInsurablesUnderPolicy;
                        batchList.totalCoveredInsurablesNotUnderPolicy = data.totalCoveredInsurablesNotUnderPolicy;

                        // You can continue to update other fields as needed
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine("Batch with ID " + data.id + " not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SAVING BATCH LIST : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task<List<tbl_placing_batch>> getBatchesForGenerateCovernotes()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.status.ToUpper() == "ISSUED" && d.batch_status == 1 && d.totalInsurables > 0 ).ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<tbl_placing_batch>> get()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.status.ToUpper() == "ISSUED" && d.batch_status == 0 && d.totalInsurables > 0).ToList());
                    //List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.ToList());
                    return batchList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> getSumAdminFee(string strBatchId)
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {

                    decimal? sumAdminFee = await Task.Run(() =>  db.tbl_participant_list
                                        .Where(t => t.batch_id == strBatchId)
                                        .Sum(t => t.admin_fee));

                    int sumAdminFeeInt = (int)(sumAdminFee ?? 0); // Default to 0 if sumAdminFee is null

                    return sumAdminFeeInt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<string> GetBatchRenderReport()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {

                    string batchId = await db.tbl_placing_batch
                          .Where(t => t.batch_status == 2 && t.status.ToUpper() == "PAID")
                          .Select(t => t.id)
                          .FirstOrDefaultAsync();


                    return batchId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveCoverNotes(tbl_cover_notes data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        db.tbl_cover_notes.Add(data);
                        db.SaveChanges();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SaveCoverNotes : " + ex.Message);
                    throw ex;
                }
            }
        }

    }
}