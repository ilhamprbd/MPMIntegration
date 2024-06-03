﻿using System;
using System.Collections.Generic;
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

        public async Task UpdateBatchListStatus(string strBatchId)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    tbl_placing_batch batchList = await Task.Run(() => db.tbl_placing_batch.SingleOrDefault<tbl_placing_batch>(d => d.id == strBatchId));

                    if (batchList != null)
                    {
                        batchList.batch_status = 1;
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SAVING BATCH LIST : " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task UpdateCoverNotesDetail(tbl_cover_notes data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    tbl_cover_notes tblCoverNotes = await Task.Run(() => db.tbl_cover_notes.SingleOrDefault(d => d.id == data.id));

                    if (tblCoverNotes != null)
                    {
                        // Update multiple values in the batchList object based on the dataModel

                        tblCoverNotes.id = data.id;
                        tblCoverNotes.number = data.number;
                        tblCoverNotes.countOfInsurables = data.countOfInsurables;
                        tblCoverNotes.sumOfPremium = data.sumOfPremium;
                        tblCoverNotes.status = data.status;
                        tblCoverNotes.sumOfAmountAll = data.sumOfAmountAll;
                        tblCoverNotes.administrationFee = data.administrationFee;
                        tblCoverNotes.createdTime = data.createdTime;
                        tblCoverNotes.finalizedTime = data.finalizedTime;
                        tblCoverNotes.batch_id = data.batch_id;

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

        public async Task<List<tbl_placing_batch>> getGenerateCovernotesBatches()
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<tbl_placing_batch> batchList = await Task.Run(() => db.tbl_placing_batch.Where<tbl_placing_batch>(d => d.status.ToUpper() == "ISSUED" && d.batch_status == 1 && d.totalInsurables > 0).ToList());
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

                    decimal? sumAdminFee = db.tbl_participant_list
                                        .Where(t => t.batch_id == strBatchId)
                                        .Sum(t => t.admin_fee);

                    int sumAdminFeeInt = (int)(sumAdminFee ?? 0); // Default to 0 if sumAdminFee is null

                    return sumAdminFeeInt;
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
                    Console.WriteLine("ERROR SAVING BATCH LIST : " + ex.Message);
                    throw ex;
                }
            }
        }

    }
}