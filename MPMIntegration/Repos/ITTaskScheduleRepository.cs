using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MPMIntegration;

namespace MPMIntegration.Libraries
{
    public class ITTaskScheduleRepository
    {
        public async Task<List<it_task_schedule>> GetAllExpiredTask(DateTime dtimeExpire, string strBPSId)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    var itschedule = await Task.Run(() =>
                        db.it_task_schedule
                            .Where(d => d.next_schedule <= (DateTime?)dtimeExpire && d.status == "Y" && d.stage == null && (d.bps_id ?? "000") == strBPSId)
                            .OrderBy(d => d.next_schedule)
                            .ToList());

                    return itschedule;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task UpdateStage(int ai_register_id, string ac_stage = null)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                string strInterUnit = "";

                try
                {
                    it_task_schedule task_update = await Task.Run(() =>
                        db.it_task_schedule.SingleOrDefault(d => d.register_id == ai_register_id));

                    task_update.stage = null;

                    if (task_update.schedule_type == "R")
                    {
                        strInterUnit = task_update.interval_unit;

                        if (!string.IsNullOrEmpty(strInterUnit))
                        {
                            switch (strInterUnit)
                            {
                                case "D":
                                    task_update.next_schedule = task_update.next_schedule.GetValueOrDefault(DateTime.Now).AddDays(task_update.interval);
                                    break;
                                case "H":
                                    task_update.next_schedule = task_update.next_schedule.GetValueOrDefault(DateTime.Now).AddHours(task_update.interval);
                                    break;
                                case "M":
                                    task_update.next_schedule = task_update.next_schedule.GetValueOrDefault(DateTime.Now).AddMonths(task_update.interval);
                                    break;
                                case "T":
                                    task_update.next_schedule = task_update.next_schedule.GetValueOrDefault(DateTime.Now).AddMinutes(task_update.interval);
                                    break;
                                case "Y":
                                    task_update.next_schedule = task_update.next_schedule.GetValueOrDefault(DateTime.Now).AddYears(task_update.interval);
                                    break;
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<it_task_schedule> Get(int ai_register_id)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    return await Task.Run(() =>
                        db.it_task_schedule.SingleOrDefault(d => d.register_id == ai_register_id));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task BeingExecuted(int ai_register_id)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    it_task_schedule task_update = await Task.Run(() =>
                        db.it_task_schedule.SingleOrDefault(d => d.register_id == ai_register_id));

                    task_update.stage = "E";
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
