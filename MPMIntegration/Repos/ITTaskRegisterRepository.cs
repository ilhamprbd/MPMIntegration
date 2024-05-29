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
    public class ITTaskRegisterRepository
    {
        public async Task<List<it_task_register>> GetTaskRegis(string strTaskCode)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    var itRegis = await Task.Run(() =>
                        db.it_task_register.Where(d => d.task_code == strTaskCode).ToList());

                    // string itRegis = db.it_task_register.Where(d => d.task_code == strTaskCode).Select(d => d.task_code ).ToString();


                    //string result = db.it_task_register
                    //           .Where(d => d.task_code == strTaskCode) // Apply the condition
                    //           .Select(d => d.task_code) // Select the specific column
                    //           .FirstOrDefault(); // Get the first item


                    return itRegis;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
