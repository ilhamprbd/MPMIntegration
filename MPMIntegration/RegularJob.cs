
using MPMIntegration.APIModel;
using MPMIntegration.Libraries;
using MPMIntegration.Repos;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace MPMIntegration
{
    public class RegularJob : IJob
    {
        private GlobalRepository repos = new GlobalRepository();
        private HitApi _hitApirepos = new HitApi();
        private GenerateCSV _generateCSV = new GenerateCSV();
        private SentEmail _sentEmail = new SentEmail();

        private string strBPSId = "000";

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime adt_expire_date = DateTime.Now;

            Console.Out.WriteLine("Expire Date {0}", (object)adt_expire_date);



            //checking BPS id
            try
            {
                strBPSId = ConfigurationManager.AppSettings["BPSID"];
                if (string.IsNullOrEmpty(strBPSId))
                    strBPSId = "000";

                List<it_task_schedule> itschedule = await repos.repo_tsc.GetAllExpiredTask(adt_expire_date, strBPSId);

                if (itschedule.Count<it_task_schedule>() > 0)
                {

                    foreach (var it_sche in itschedule)
                    {

                        var itRegis = await repos.repo_regi.GetTaskRegis(it_sche.task_code);

                        foreach (var it_reg in itRegis)
                        {
                            Console.WriteLine("Executing Task #:" + it_sche.task_code + "|:" + it_reg.task_type + " :| : " + it_sche.desc);
                            if (it_reg.task_type == "API")
                            {
                                List<api_client_configuration> lAPIConfig = await repos.repo_API.GetClientConfigAPI();
                                await repos.repo_tsc.BeingExecuted(it_sche.register_id);
                                TaskHitAPI(it_reg.task_code, lAPIConfig);
                                await repos.repo_tsc.UpdateStage(it_sche.register_id, null);
                            }
                            else if (it_reg.task_type == "RP")
                            {

                                await repos.repo_tsc.BeingExecuted(it_sche.register_id);

                                await repos.repo_tsc.UpdateStage(it_sche.register_id, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception {0}", (object)ex.Message);
            }


        }

        public async void TaskHitAPI(string strTaskCode, List<api_client_configuration> lAPIConfig)
        {
            string strReportPath = ConfigurationManager.AppSettings["DocsFilePath"];
            try
            {

                List<it_task_register> TaskRegis = await repos.repo_regi.GetTaskRegis(strTaskCode);
                List<api_url> lAPIUrl = await repos.repo_API.GetURLAPI(Int32.Parse(TaskRegis.FirstOrDefault().task_object));
                string strJwtToken = "";


                if (strTaskCode == "T000")
                {

                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);

                    if (strJwtToken.Length > 1)
                    {
                        List<tbl_placing_batch> lplacingBatch = await _hitApirepos.getBatchList(lAPIConfig, lAPIUrl, strJwtToken);
                        await repos.repo_placingBatch.SavePlacingBatch(lplacingBatch);
                    }


                }
                else if (strTaskCode == "T001")
                {

                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchExecParticipant();
                    if (lissuedBatch.Count() > 0)
                    {
                        foreach (var _batchId in lissuedBatch)
                        {
                            strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);

                            await _hitApirepos.GetParticipantDirectStream(lAPIConfig, lAPIUrl, strJwtToken, _batchId.id);
                            await repos.repo_placingBatch.UpdateBatchListStatus(_batchId.id, 1);

                        }

                    }
                    else
                    {
                        Console.WriteLine("No Batch Id Found!.");
                    }
                }
                else if (strTaskCode == "T002")
                {
                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchIssued();
                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    if (strJwtToken.Length > 0)
                    {
                        if (lissuedBatch != null && strJwtToken.Length > 1)
                        {
                            if (lissuedBatch != null && strJwtToken.Length > 1)
                                foreach (var _lbatch in lissuedBatch)
                                {
                                    tbl_placing_batch tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.id);
                                    await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);
                                }
                            Console.WriteLine("Updateing success !!");
                        }

                    }
                    else { Console.WriteLine("Token NOt Found at : " + strTaskCode); }
                }
                else if (strTaskCode == "T003")
                {
                    List<it_report_list> _urlReport = await repos.repo_covernote.GetURLReport(2);
                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.getBatchesForGenerateCovernotes();
                    List<invoiceListModel> lInvoice = await repos.repo_covernote.getInvoiceListCoverNote(lissuedBatch.FirstOrDefault().id);


                    if (lInvoice != null)
                    {
                        foreach (var _linvoice in lInvoice)
                        {
                            strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);

                            if (strJwtToken.Length > 0)
                            {
                                lAPIUrl = await repos.repo_API.GetURLAPI(5);
                                var invoiceList = new List<invoiceListModel> { _linvoice }; // Wrap _linvoice in a list
                                tbl_cover_notes _lCoverNotes = await _hitApirepos.generateCovernote(lAPIConfig, lAPIUrl, strJwtToken, invoiceList); // getting covernote from API
                                await repos.repo_placingBatch.SaveCoverNotes(_lCoverNotes);
                                ////ganti config dan URL untuk extract covernotes

                                string strPathCSV = _generateCSV.ParticipantCSV(invoiceList);
                                if (strPathCSV.Length > 1)
                                {
                                    lAPIUrl = await repos.repo_API.GetURLAPI(6);
                                    bool blUploadparticipantCSV = await _hitApirepos.uploadParticipantCoverNote(lAPIConfig, lAPIUrl, strJwtToken, _lCoverNotes.id, strPathCSV);  //upload participant csv to covernote

                                    if (blUploadparticipantCSV)
                                    {

                                        lAPIUrl = await repos.repo_API.GetURLAPI(7);
                                        _lCoverNotes = await _hitApirepos.extractParticipantCoverNote(lAPIConfig, lAPIUrl, strJwtToken, _lCoverNotes.id, _lCoverNotes.batch_id); //extract covernote participant
                                        await repos.repo_placingBatch.UpdateCoverNotesPath(_lCoverNotes, strPathCSV);

                                        lAPIUrl = await repos.repo_API.GetURLAPI(8); // upload docs url

                                        string strPathFileInvoice = await repos.repo_generatedocs.RenderReportInvoiceAsync(2, "PDF", strReportPath, _urlReport, _linvoice.InvoiceNo);
                                        await _hitApirepos.uploadDocumentCoverNote(lAPIConfig, lAPIUrl, strJwtToken, _lCoverNotes.id, strPathFileInvoice);  //upload Invoice 
                                        await repos.repo_covernote.UpdateInvoiceGenerateParti(_linvoice.InvoiceNo); // update  peserta notification

                                    }
                                }
                            }
                        }
                        Console.WriteLine("Updateing success !!"); ;
                    }
                    else { Console.WriteLine("Token NOt Found at : " + strTaskCode); }
                }

                else if (strTaskCode == "T004") // finalize covernote -- udah ga di pake karena biar user aja yg jalanin.. nanti dipake lagi klo udah full automatic
                {
                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    tbl_placing_batch tblBatch = new tbl_placing_batch();

                    if (strJwtToken.Length > 0)
                    {
                        List<tbl_cover_notes> lCoverNotes = await repos.repo_covernote.GetCoverNoteFinalize();
                        if (lCoverNotes != null)
                        {
                            foreach (var _lcoverNotes in lCoverNotes)
                            {
                                var invoiceList = new List<tbl_cover_notes> { _lcoverNotes }; // Wrap _linvoice in a list

                                lAPIUrl = await repos.repo_API.GetURLAPI(9);
                                tbl_cover_notes tbl_Cover_Notes = await _hitApirepos.finalizeCoverNote(lAPIConfig, lAPIUrl, strJwtToken, invoiceList); //finalize covernote
                                await repos.repo_placingBatch.UpdateCoverNotesDetail(invoiceList);


                            }
                            lAPIUrl = await repos.repo_API.GetURLAPI(12);
                            tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, lCoverNotes.FirstOrDefault().batch_id);
                            await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);

                            await repos.repo_placingBatch.UpdateBatchListStatus(lCoverNotes.FirstOrDefault().batch_id, 2);
                        }


                        List<tbl_participant_list> lRejectParticipant = await repos.repo_placingBatch.GetParticipantRejected();

                        if (lRejectParticipant.Count > 0)
                        {
                            string strPathCSV = _generateCSV.ParticipantRejectedCSV(lRejectParticipant.FirstOrDefault().batch_id);
                            if (strPathCSV.Length > 1)
                            {
                                lAPIUrl = await repos.repo_API.GetURLAPI(18);
                                bool blUploadparticipantCSV = await _hitApirepos.uploadParticipantCoverNote(lAPIConfig, lAPIUrl, strJwtToken, lRejectParticipant.FirstOrDefault().batch_id, strPathCSV);  //upload participant rejected csv to batch

                                if (blUploadparticipantCSV)
                                {
                                    lAPIUrl = await repos.repo_API.GetURLAPI(19);
                                    await _hitApirepos.extractParticipantRejected(lAPIConfig, lAPIUrl, strJwtToken, lRejectParticipant.FirstOrDefault().batch_id); //extract  participant rejected to batch

                                }
                            }
                        }
                        {
                            Console.WriteLine("Partcipant Rejected not Found!. Begin Finalized Batches");
                        }
                        List<tbl_placing_batch> ltblPlacingBatch = await repos.repo_placingBatch.GetBatchCoverNoteFinalize();

                        lAPIUrl = await repos.repo_API.GetURLAPI(10);

                        tblBatch = await _hitApirepos.finalizePlacingBatch(lAPIConfig, lAPIUrl, strJwtToken, ltblPlacingBatch.FirstOrDefault().id);  // finalize placingbatch
                                                                                                                                                     //disini update batch finalize

                        lAPIUrl = await repos.repo_API.GetURLAPI(12);
                        tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, ltblPlacingBatch.FirstOrDefault().id);
                        await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);
                        await repos.repo_placingBatch.UpdateBatchListStatus(ltblPlacingBatch.FirstOrDefault().id, 3);



                    }
                    else { Console.WriteLine("Token Not Found at : " + strTaskCode); }

                }

                else if (strTaskCode == "T005")
                {
                    tbl_placing_batch tblBatch = new tbl_placing_batch();
                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    if (strJwtToken.Length > 1)
                    {
                        lAPIUrl = await repos.repo_API.GetURLAPI(12);
                        List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchIssued();
                        strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                        if (strJwtToken.Length > 0)
                        {
                            if (lissuedBatch != null && strJwtToken.Length > 1)
                            {
                                if (lissuedBatch != null && strJwtToken.Length > 1)
                                    foreach (var _lbatch in lissuedBatch)
                                    {
                                        tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.id);
                                        await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);
                                    }
                                Console.WriteLine("Updateing success !!");
                            }

                        }
                        else { Console.WriteLine("Token Not Found at : " + strTaskCode); }

                        lissuedBatch = await repos.repo_placingBatch.GetPoliciesbatchGen();

                        if (lissuedBatch != null)
                        {

                            string strpolisissued = lissuedBatch?.FirstOrDefault().process_date?.ToString("yyyy-MM-dd") ?? string.Empty;  //"2021-01-08";


                            List<invoiceListModel> lInvoice = await repos.repo_covernote.getPoliciesGen(lissuedBatch.FirstOrDefault().id);
                            if (lInvoice != null)
                            {
                                foreach (var _linvoice in lInvoice)
                                {


                                    var invoiceList = new List<invoiceListModel> { _linvoice }; // Wrap _linvoice in a list
                                    //generate new policies hholder

                                    lAPIUrl = await repos.repo_API.GetURLAPI(13);
                                    tbl_policies_holder lpolHolder = await _hitApirepos.generatePolicyHolder(lAPIConfig, lAPIUrl, strJwtToken, _linvoice.BatchId, _linvoice.RegnoBatch, strpolisissued); // getting policyholder from API

                                    if (lpolHolder != null)
                                    {
                                        await repos.repo_policies.SavePoliciesHolder(lpolHolder);

                                        string strPathFileParticipant = _generateCSV.PoliciesParticipantCSV(invoiceList); //await repos.repo_covernote.getPathFileCSV(_linvoice.BatchId);

                                        if (strPathFileParticipant != null)
                                        {
                                            lAPIUrl = await repos.repo_API.GetURLAPI(14);
                                            bool blUploadparticipantDoc = await _hitApirepos.uploadParticipantPolicies(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, strPathFileParticipant);  //upload participant policies

                                            if (blUploadparticipantDoc)
                                            {
                                                lAPIUrl = await repos.repo_API.GetURLAPI(15);
                                                bool blExtractPolicies = await _hitApirepos.extractParticipantpolicies(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, _linvoice.BatchId); //extract policies participant



                                                if (blExtractPolicies)
                                                {

                                                    lAPIUrl = await repos.repo_API.GetURLAPI(17);
                                                    List<it_report_list> _urlReport = await repos.repo_covernote.GetURLReport(1);
                                                    List<string> strPathFileSertifikat = await repos.repo_generatedocs.RenderReportSertifikatAsync(1, strReportPath, _urlReport, _linvoice.RegnoBatch, (int)lissuedBatch.FirstOrDefault().totalCoveredInsurables);// render certif

                                                    if (strPathFileSertifikat == null || !strPathFileSertifikat.Any())
                                                    {
                                                        Console.WriteLine("No files generated.");
                                                        return;
                                                    }
                                                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);

                                                    // Upload each generated PDF file
                                                    foreach (var filePath in strPathFileSertifikat)
                                                    {
                                                        lAPIUrl = await repos.repo_API.GetURLAPI(16);
                                                        blExtractPolicies = await _hitApirepos.uploadPoliciesCertificate(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, filePath); // upload certificate per PDF to policies holder
                                                    }

                                                    //lAPIUrl = await repos.repo_API.GetURLAPI(17);
                                                    //lpolHolder = await _hitApirepos.finalizePoliciesHolder(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id); // finalize

                                                    // await repos.repo_policies.UpdatePoliciesHolder(lpolHolder);

                                                    string strReconReport = _generateCSV.ParticipantIssuedReport(invoiceList.FirstOrDefault().RegnoBatch);
                                                    if (strReconReport.Count() > 1)
                                                    {
                                                        _sentEmail.ParticipantRecon();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                lAPIUrl = await repos.repo_API.GetURLAPI(12);
                                tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, lissuedBatch.FirstOrDefault().id);
                                await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);
                                await repos.repo_placingBatch.UpdateBatchListStatus(lissuedBatch.FirstOrDefault().id, 2);
                            }



                        }
                        Console.WriteLine("Not Placing Batch Paid Yet!");
                    }


                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async void TaskReport(string strTaskCode)
        {
            try
            {
                string strPathFileParticipant = null;
                string strBatchId = null;
                if (strTaskCode == "T006")
                {

                    bool isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Friday;

                    if (!isWeekend)
                    {
                        strBatchId = await repos.repo_placingBatch.GetBatchRenderReport();

                        if (strBatchId != null)
                        {
                            List<invoiceListModel> lInvoice = await repos.repo_covernote.getInvoiceListCoverNote(strBatchId);
                            if (lInvoice != null)
                            {
                                foreach (var _linvoice in lInvoice)
                                {
                                    var invoiceList = new List<invoiceListModel> { _linvoice }; // Wrap _linvoice in a list

                                    strPathFileParticipant = _generateCSV.ParticipantIssuedReport(invoiceList.FirstOrDefault().RegnoBatch);
                                }

                                if (strPathFileParticipant != null)
                                {

                                    _sentEmail.ParticipantRecon();
                                }
                            }
                        }
                    }



                }

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

    }
}
