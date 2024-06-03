using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    class ITReportListRepository
    {
        public async Task<List<it_report_list>> getReportList(int intCode)
        {
            try
            {
                using (var db = new DashBoardMPMEntities1())
                {
                    List<it_report_list> reportList = await Task.Run(() => db.it_report_list.Where<it_report_list>(d => d.code == intCode).ToList());
                    return reportList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
