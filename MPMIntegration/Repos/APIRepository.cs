using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMIntegration.Repos
{
    public class APIRepository
    {
        protected string is_Environment = ConfigurationManager.AppSettings["Environment"];

        public async Task<List<api_client_configuration>> GetClientConfigAPI()
        {
            if (string.IsNullOrEmpty(is_Environment))
                is_Environment = "DEV";

            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Using Task.Run to offload synchronous code to a background thread
                    var ListApiConfig = await Task.Run(() => db.api_client_configuration.Where(d => d.env_name == is_Environment).ToList());

                    return ListApiConfig;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<List<api_url>> GetURLAPI(int id)
        {
            if (string.IsNullOrEmpty(is_Environment))
                is_Environment = "DEV";

            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Using Task.Run to offload synchronous code to a background thread
                    var ListApiConfig = await Task.Run(() => db.api_url.Where(d => d.url_id == id).ToList());

                    return ListApiConfig;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task SaveLog(api_log_request data)
        {
            using (var db = new DashBoardMPMEntities1())
            {
                try
                {
                    // Using Task.Run to offload synchronous code to a background thread
                    await Task.Run(() =>
                    {
                        db.api_log_request.Add(data);
                        db.SaveChanges();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR SAVING: " + ex.Message);
                    throw ex;
                }
            }
        }




    }

}

