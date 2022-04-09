using System;
using System.Linq;
using System.Threading.Tasks;
using IISManagerCli.Enums;
using IISManagerCli.Manager;
using IISManagerCli.Models.Dtos;
using Microsoft.Extensions.Configuration;
using Sharprompt;

namespace IISManagerCli
{
    class Program
    {
        private static string _hostAddress;
        private static int _port;

        static async Task Main(string[] args)
        {

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var childProperties = config.GetChildren();
            var profiles = childProperties.Select(children => children.Key).ToList();

            var selectedProfile = Prompt.Select("Select Profile",
                profiles.ToArray(), pageSize: 3);

            _hostAddress = config.GetSection(selectedProfile + ":" + "IpAddress").Value;
            _port = Convert.ToInt32(config.GetSection(selectedProfile + ":" + "Port").Value);
            //_hostAddress = Prompt.Input<string>("Host Address");
            //_port = Prompt.Input<int>("Port");

            var completed = false;
            var iisManager = new IISManager(_hostAddress, _port);
            await iisManager.GetList();
            while (!completed)
            {
                var processType = Prompt.Select<ProcessType>("Select Process");

                switch (processType)
                {
                    case ProcessType.Get:
                        var siteId = Prompt.Input<int>("Site Id");
                        await iisManager.Get(siteId);
                        break;
                    case ProcessType.GetAll:
                        Console.Clear();
                        await iisManager.GetList();
                        break;
                    case ProcessType.Create:
                        Console.Clear();
                        var createSiteInput = ValidateCreateSiteInput();
                        await iisManager.CreateSite(createSiteInput);
                        break;
                    case ProcessType.Deploy:
                        Console.Clear();
                        var deploySite = ValidateDeploySiteInput();
                        await iisManager.DeploySite(deploySite);
                        break;
                    case ProcessType.Start:
                        var siteIdToStart = Prompt.Input<int>("Site Id");
                        var startSiteInput = new StartSiteInput()
                        {
                            Id = siteIdToStart
                        };
                        Console.Clear();
                        await iisManager.StartSite(startSiteInput);
                        break;
                    case ProcessType.Stop:
                        var siteIdToStop = Prompt.Input<int>("Site Id");
                        var stopSiteInput = new StopSiteInput()
                        {
                            Id = siteIdToStop
                        };
                        Console.Clear();
                        await iisManager.StopSite(stopSiteInput);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static DeploySiteInput ValidateDeploySiteInput()
        {
            var site = new DeploySiteInput();
            site.Id = Prompt.Input<long>("Site Id");
            site.FilePath = Prompt.Input<string>("Publish Folder Path");
            return site;
        }

        private static CreateSiteInput ValidateCreateSiteInput()
        {
            var site = new CreateSiteInput();
            site.Name = Prompt.Input<string>("Site Name");
            site.BindingInformation = Prompt.Input<string>("IP Address", "*");
            site.Port = Prompt.Input<int>("Port Name");
            site.FilePath = Prompt.Input<string>("Publish Folder Path");
            return site;
        }
    }
}