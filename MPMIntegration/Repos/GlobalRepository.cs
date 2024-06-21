using MPMIntegration.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    public class GlobalRepository
    {

        public ITTaskScheduleRepository repo_tsc = new ITTaskScheduleRepository();
        public ITTaskRegisterRepository repo_regi = new ITTaskRegisterRepository();
        public APIRepository repo_API = new APIRepository();
        public PlacingBatchRepository repo_placingBatch = new PlacingBatchRepository();
        public GenerateDocsRepository repo_generatedocs = new GenerateDocsRepository();
        public CoverNoteRepository repo_covernote = new CoverNoteRepository();
        public PoliciesHolderRepository repo_policies = new PoliciesHolderRepository();
    }
}
