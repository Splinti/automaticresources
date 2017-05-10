using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using System.IO;
using System.Threading;

namespace spl_autoresources.Server
{
    class main : Script
    {
        Thread s;
        public main()
        {
            API.onResourceStart += API_onResourceStart;
            API.onResourceStop += API_onResourceStop;
            s = new Thread(new ThreadStart(checkForModifation));
        }

        private void API_onResourceStop()
        {
            s.Abort();
        }

        private string[] Resources;
        private Dictionary<string, Dictionary<string, DateTime>> ResourceStructure = new Dictionary<string, Dictionary<string, DateTime>>();
        private void API_onResourceStart()
        {
            foreach (var item in API.getRunningResources())
            {
                ResourceStructure.Add(item, new Dictionary<string, DateTime>());
                IndexFolders(item);
            }
            s.Start();
        }
        private void checkForModifation()
        {
            while (true)
            {
                foreach (var item in ResourceStructure)
                {
                    var currentResource = item.Key;
                    if (isResourceModified(item.Key))
                    {
                        API.stopResource(item.Key);
                        API.sleep(200);
                        API.startResource(item.Key);
                    }
                }
                Thread.Sleep(1000);
            }
            s.Abort();
        }
        private bool isResourceModified(string ResourceName)
        {
            var result = false;
            var dirs = Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/" + ResourceName, "*", SearchOption.AllDirectories);
            foreach (var item in dirs)
            {
                if(!ResourceStructure[ResourceName].ContainsKey(item)){
                    ResourceStructure[ResourceName].Add(item, File.GetLastWriteTime(item));
                    result = true;
                }
                else
                {
                    var lastWriteTime = ResourceStructure[ResourceName][item];
                    var newWriteTime = File.GetLastWriteTime(item);
                    if (lastWriteTime != newWriteTime)
                    {
                        result = true;
                    }
                    ResourceStructure[ResourceName][item] = newWriteTime;
                }
            }
            if(ResourceStructure[ResourceName].Count != dirs.Count())
            {
                foreach (var item in ResourceStructure[ResourceName])
                {
                    var d = dirs.Where(x => x == item.Key);
                    if (d.Count() == 0)
                    {
                        ResourceStructure[ResourceName].Remove(item.Key);
                        break;
                    }
                }
                result = true;
            }
            return result;
        }
        private void IndexFolders(string ResourceName)
        {
            foreach (var item in Directory.GetFiles(Directory.GetCurrentDirectory() + "/resources/" + ResourceName, "*", SearchOption.AllDirectories))
            {
                ResourceStructure[ResourceName].Add(item, File.GetLastWriteTime(item));
            }
        }
    }
}
