using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.APIModel
{
    class placingBatchesModel
    {
        public string id { get; set; }
        public DateTime createdTime { get; set; }
        public string status { get; set; }
        public int totalInsurables { get; set; }
        public int totalCoveredInsurables { get; set; }
        public int totalRejectedInsurables { get; set; }
        public int totalUncoveredInsurables { get; set; }
        public int totalCoveredInsurablesUnderPolicy { get; set; }
        public int totalCoveredInsurablesNotUnderPolicy { get; set; }


    }
}
