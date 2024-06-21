
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
                                hitAPI(it_reg.task_code, lAPIConfig);
                                await repos.repo_tsc.UpdateStage(it_sche.register_id, null);
                            }
                            else if (it_reg.task_type == "SP")
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

        public async void hitAPI(string strTaskCode, List<api_client_configuration> lAPIConfig)
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

                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchIssued();
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
                        Console.WriteLine("NO Batch Id Found!.");
                    }
                }
                else if (strTaskCode == "T002")
                {
                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchIssued();
                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    if (lissuedBatch != null && strJwtToken.Length > 1)
                        foreach (var _lbatch in lissuedBatch)
                        {
                            tbl_placing_batch tblBatch = await _hitApirepos.getBatchListDetail(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.id);
                            await repos.repo_placingBatch.UpdateBatchListDetail(tblBatch);
                        }
                    Console.WriteLine("Updateing success !!");
                }
                else if (strTaskCode == "T003")
                {
                    List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.getGenerateCovernotesBatches();
                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    if (lissuedBatch != null && strJwtToken.Length > 1)

                        foreach (var _lbatch in lissuedBatch)
                        {

                            int intAdminFee = await repos.repo_placingBatch.getSumAdminFee(_lbatch.id); // getting sumAdminfee --> PNM need to set 0 or need calculate?

                            tbl_cover_notes _lCoverNotes = await _hitApirepos.generateCovernote(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.gen_id, intAdminFee, _lbatch.id); // getting covernote from API
                            await repos.repo_placingBatch.SaveCoverNotes(_lCoverNotes);
                            ////ganti config dan URL untuk extract covernotes

                            string strPathCSV = _generateCSV.ParticipantCSV(_lbatch.id);
                            if (strPathCSV.Length > 1)
                            {
                                lAPIUrl = await repos.repo_API.GetURLAPI(6);
                                bool blUploadparticipantCSV = await _hitApirepos.uploadParticipantCoverNote(lAPIConfig, lAPIUrl, strJwtToken, _lCoverNotes.id, strPathCSV);  //upload participant csv to covernote

                                if (blUploadparticipantCSV)
                                {
                                    lAPIUrl = await repos.repo_API.GetURLAPI(7);
                                    _lCoverNotes = await _hitApirepos.extractParticipantCoverNote(lAPIConfig, lAPIUrl, strJwtToken, _lCoverNotes.id, _lbatch.id); //extract covernote participant
                                    await repos.repo_placingBatch.UpdateCoverNotesPath(_lCoverNotes, strPathCSV);

                                    // adding covernte invoice docs

                                    List<it_report_list> _urlReport = await repos.repo_covernote.GetURLReport(2);


                                    string strCovernoteId = await repos.repo_covernote.getCoverNoteId(_lbatch.id);
                                    List<invoiceListModel> _invoiceList = await repos.repo_covernote.getInvoiceList(_lbatch.id);


                                    lAPIUrl = await repos.repo_API.GetURLAPI(8);
                                    foreach (var _invoice in _invoiceList)
                                    {
                                        string strPathFileInvoice = await repos.repo_generatedocs.RenderReportInvoiceAsync(2, "PDF", strReportPath, _urlReport, _invoice.InvoiceNo);
                                        await _hitApirepos.uploadDocumentCoverNote(lAPIConfig, lAPIUrl, strJwtToken, strCovernoteId, strPathFileInvoice);  //upload Invoice 

                                        await repos.repo_covernote.UpdateInvoiceGenerateParti(_invoice.InvoiceNo); // update finalize peserta notification
                                    }



                                    lAPIUrl = await repos.repo_API.GetURLAPI(9);
                                    _lCoverNotes = await _hitApirepos.finalizeCoverNote(lAPIConfig, lAPIUrl, strJwtToken, strCovernoteId, _lbatch.id); //finalize covernote

                                    //dsini update covernote finalize
                                    await repos.repo_placingBatch.UpdateCoverNotesDetail(_lCoverNotes);


                                    lAPIUrl = await repos.repo_API.GetURLAPI(10);
                                    tbl_placing_batch _tblplacingbatch = await _hitApirepos.finalizePlacingBatch(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.id); // finalize placingbatch


                                    //disini update batch finalize
                                    await repos.repo_placingBatch.UpdateBatchListDetail(_tblplacingbatch);
                                    await repos.repo_placingBatch.UpdateBatchListStatus(_lbatch.id, 2);




                                }
                            }

                        }
                    Console.WriteLine("Updateing success !!"); ;
                }
                else if (strTaskCode == "T004")
                {

                    strJwtToken = await _hitApirepos.GetTokenAsyncClient(lAPIConfig);
                    if (strJwtToken.Length > 1)
                    {

                        List<tbl_placing_batch> lissuedBatch = await repos.repo_placingBatch.GetBatchFinalize();
                        tbl_policies_holder lpolHolder = new tbl_policies_holder();

                        lAPIUrl = await repos.repo_API.GetURLAPI(13);


                        foreach (var _lbatch in lissuedBatch)
                        {
                            string strPolisNo = ConfigurationManager.AppSettings["PolisNo"];
                            string strpolisissued = "2021-01-08";
                            //generate new policies hholder

                            lpolHolder = await _hitApirepos.gerneratePolicyHolder(lAPIConfig, lAPIUrl, strJwtToken, _lbatch.id, strPolisNo, strpolisissued); // getting policyholder from API
                            await repos.repo_policies.SavePoliciesHolder(lpolHolder);



                            string strPathFileParticipant = await repos.repo_covernote.getPathFileCSV(_lbatch.id);
                            if (strPathFileParticipant != null)
                            {
                                //string strCovernoteId = await repos.repo_covernote.getCoverNoteId(_lbatch.id);
                                lAPIUrl = await repos.repo_API.GetURLAPI(14);
                                bool blUploadparticipantDoc = await _hitApirepos.uploadParticipantPolicies(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, strPathFileParticipant);  //upload participant 

                                if (blUploadparticipantDoc)
                                {
                                    lAPIUrl = await repos.repo_API.GetURLAPI(15);

                                    string strPoliciesID = await repos.repo_policies.getPoliciesID(_lbatch.id);
                                    bool blExtractPolicies = await _hitApirepos.extractParticipantpolicies(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, _lbatch.id); //extract covernote participant

                                    if (blExtractPolicies)
                                    {
                                        //upload & render certificate
                                        List<it_report_list> _urlReport = await repos.repo_covernote.GetURLReport(1);
                                        string strPathFileSertifikat = await repos.repo_generatedocs.RenderReportSertifikatAsync(1, "PDF", strReportPath, _urlReport, _lbatch.id);

                                        lAPIUrl = await repos.repo_API.GetURLAPI(16);
                                        bool blUploadCertifcate = await _hitApirepos.uploadPoliciesCertificate(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id, strPathFileSertifikat);  //upload certificate 
                                        if (blUploadparticipantDoc)
                                        {
                                            lAPIUrl = await repos.repo_API.GetURLAPI(17);
                                            lpolHolder = await _hitApirepos.finalizePoliciesHolder(lAPIConfig, lAPIUrl, strJwtToken, lpolHolder.id); // finalize

                                            await repos.repo_policies.SavePoliciesHolder(lpolHolder);
                                            await repos.repo_placingBatch.UpdateBatchListStatus(_lbatch.id, 3);
                                        }

                                    }

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
