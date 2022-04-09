using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using IISManagerCli.Models;
using IISManagerCli.Models.Dtos;
using IISManagerCli.Models.ResponseType;
using Newtonsoft.Json;
using RestSharp;

namespace IISManagerCli.Manager
{
    public class IISManager
    {
        private readonly string _host;
        private readonly int _port;

        public IISManager(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task<List<Site>> GetList()
        {
            var response = await HttpManager.Request(_host + ":" + _port, "/IIS/GetAll", Method.GET, null);
            if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

            var result = JsonConvert.DeserializeObject<ResponseType<List<Site>>>(response.Content);
            if (result is {IsSuccess: true})
            {
                var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                foreach (var site in result.Result)
                {
                    table.AddRow(site.Id, site.Name, site.Path, site.Port, site.Url, site.State);
                }

                table.Print();

                return result.Result;
            }

            throw new Exception(response.ErrorMessage);
        }

        public async Task<Site> Get(int id)
        {
            try
            {
                var response = await HttpManager.Request(_host + ":" + _port, "/IIS/Get/" + id, Method.GET, null);
                if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

                var result = JsonConvert.DeserializeObject<ResponseType<Site>>(response.Content);
                if (result is {IsSuccess: true})
                {
                    var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                    table.AddRow(result.Result.Id, result.Result.Name, result.Result.Path, result.Result.Port,
                        result.Result.Url, result.Result.State);
                    table.Print();

                    return result.Result;
                }

                if (result is {IsSuccess: false})
                {
                    throw new Exception(result.Error.Message);
                }

                throw new Exception(response.ErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Site> CreateSite(CreateSiteInput input)
        {
            try
            {
                var zipFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, input.Name + ".zip");
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                ZipFile.CreateFromDirectory(input.FilePath, zipFilePath);
                input.FilePath = zipFilePath;

                var response =
                    await HttpManager.CreateFromForm(_host + ":" + _port, "/IIS/CreateFormData", Method.POST, input);
                if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

                var result = JsonConvert.DeserializeObject<ResponseType<Site>>(response.Content);
                if (result is {IsSuccess: true})
                {
                    var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                    table.AddRow(result.Result.Id, result.Result.Name, result.Result.Path, result.Result.Port,
                        result.Result.Url ?? "", result.Result.State ?? "");
                    table.Print();

                    if (File.Exists(zipFilePath))
                    {
                        File.Delete(zipFilePath);
                    }

                    return result.Result;
                }

                if (result is {IsSuccess: false})
                {
                    if (File.Exists(zipFilePath))
                    {
                        File.Delete(zipFilePath);
                    }

                    throw new Exception(result.Error.Message);
                }

                throw new Exception(response.ErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Site> DeploySite(DeploySiteInput input)
        {
            try
            {
                await StopSite(new StopSiteInput() {Id = input.Id});

                var zipFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, input.Id + ".zip");
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                ZipFile.CreateFromDirectory(input.FilePath, zipFilePath);
                input.FilePath = zipFilePath;

                var success = false;

                while (!success)
                {
                    var response =
                        await HttpManager.DeployFromForm(_host + ":" + _port, "/IIS/Deploy", Method.POST, input);
                    if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

                    var result = JsonConvert.DeserializeObject<ResponseType<Site>>(response.Content);
                    if (result is {IsSuccess: true})
                    {
                        success = true;

                        var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                        table.AddRow(result.Result.Id, result.Result.Name, result.Result.Path, result.Result.Port,
                            result.Result.Url ?? "", result.Result.State ?? "");
                        table.Print();

                        if (File.Exists(zipFilePath))
                        {
                            File.Delete(zipFilePath);
                        }

                        await StartSite(new StartSiteInput() {Id = input.Id});

                        break;
                    }

                    Thread.Sleep(100);
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Site> StartSite(StartSiteInput startSiteInput)
        {
            try
            {
                var response =
                    await HttpManager.Request(_host + ":" + _port, "/IIS/Start", Method.POST, startSiteInput);
                if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

                var result = JsonConvert.DeserializeObject<ResponseType<Site>>(response.Content);
                if (result is {IsSuccess: true})
                {
                    var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                    table.AddRow(result.Result.Id, result.Result.Name, result.Result.Path, result.Result.Port,
                        result.Result.Url, result.Result.State);
                    table.Print();

                    return result.Result;
                }

                if (result is {IsSuccess: false})
                {
                    throw new Exception(result.Error.Message);
                }

                throw new Exception(response.ErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Site> StopSite(StopSiteInput stopSiteInput)
        {
            try
            {
                var response =
                    await HttpManager.Request(_host + ":" + _port, "/IIS/Stop", Method.POST, stopSiteInput);
                if (!response.IsSuccessful) throw new Exception(response.ErrorMessage);

                var result = JsonConvert.DeserializeObject<ResponseType<Site>>(response.Content);
                if (result is {IsSuccess: true})
                {
                    var table = new TablePrinter("Id", "Name", "Path", "Port", "Url", "State");
                    table.AddRow(result.Result.Id, result.Result.Name, result.Result.Path, result.Result.Port,
                        result.Result.Url, result.Result.State);
                    table.Print();

                    return result.Result;
                }

                if (result is {IsSuccess: false})
                {
                    throw new Exception(result.Error.Message);
                }

                throw new Exception(response.ErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}