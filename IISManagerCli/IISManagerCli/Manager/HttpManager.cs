using System.Threading.Tasks;
using IISManagerCli.Models.Dtos;
using Newtonsoft.Json;
using RestSharp;

namespace IISManagerCli.Manager
{
    public static class HttpManager
    {
        public static async Task<IRestResponse> Request(string urlSection, string endpoint, Method method, object body)
        {
            var client = new RestClient(urlSection + endpoint)
            {
                Timeout = -1
            };
            var request = new RestRequest(method);
            request.AddHeader("Content-Type", "application/json");
            if (body != null)
            {
                request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);
            }

            return await client.ExecuteAsync(request);
        }

        public static async Task<IRestResponse> CreateFromForm(string urlSection, string endpoint, Method method,
            CreateSiteInput body)
        {
            var client = new RestClient(urlSection + endpoint)
            {
                Timeout = -1
            };
            var request = new RestRequest(method);
            if (body != null)
            {
                request.AddFile("file", body.FilePath);
                request.AddParameter("name", body.Name);
                request.AddParameter("port", body.Port);
                request.AddParameter("bindingInformation", body.BindingInformation);
            }

            return await client.ExecuteAsync(request);
        }

        public static async Task<IRestResponse> DeployFromForm(string urlSection, string endpoint, Method method,
            DeploySiteInput body)
        {
            var client = new RestClient(urlSection + endpoint)
            {
                Timeout = -1
            };
            var request = new RestRequest(method);
            if (body != null)
            {
                request.AddFile("file", body.FilePath);
                request.AddParameter("id", body.Id);
            }

            return await client.ExecuteAsync(request);
        }
    }
}