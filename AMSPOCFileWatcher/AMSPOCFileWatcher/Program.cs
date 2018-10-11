using AMSPOCFileWatcher.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace AMSPOCFileWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            //Watch OPLD files folder and call webservice to process that OPLD file
            var opldPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "OPLDFiles");
            FileSystemWatcher opldFileSystemWatcher = new FileSystemWatcher();
            opldFileSystemWatcher.Path = opldPath;
            opldFileSystemWatcher.Created += OPLDFileSystemWatcher_Created;
            opldFileSystemWatcher.EnableRaisingEvents = true;

            //Watch DIALS files folder and call webservice to process that DIALS file
            var dialsPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "DIALSFiles");
            FileSystemWatcher dialsFileSystemWatcher = new FileSystemWatcher();
            dialsFileSystemWatcher.Path = dialsPath;
            dialsFileSystemWatcher.Created += DIALSFileSystemWatcher_Created;
            dialsFileSystemWatcher.EnableRaisingEvents = true;

            Console.ReadLine();
        }

        public static OPLD ProcessOPLD(string opldString)
        {
            OPLD opldData = new OPLD();
            opldData.TrackingNumber = opldString.Length > 3533 ? opldString.Substring(3533, 35).Trim() : "";
            opldData.VersionNumber = opldString.Length > 2 ? opldString.Substring(2, 4).Trim() : "";
            opldData.ShiperNumber = opldString.Length > 55 ? opldString.Substring(55, 10).Trim() : "";
            opldData.ShiperCountry = opldString.Length > 65 ? opldString.Substring(65, 2).Trim() : "";
            opldData.AttentionName = opldString.Length > 229 ? opldString.Substring(229, 35).Trim() : "";
            opldData.AddressType = opldString.Length > 182 ? opldString.Substring(182, 2).Trim() : "";
            opldData.AddressLine1 = opldString.Length > 264 ? opldString.Substring(264, 35).Trim() : "";
            opldData.AddressLine2 = opldString.Length > 299 ? opldString.Substring(299, 35).Trim() : "";
            opldData.AddressLine3 = opldString.Length > 334 ? opldString.Substring(334, 35).Trim() : "";
            opldData.CityName = opldString.Length > 369 ? opldString.Substring(369, 30).Trim() : "";
            opldData.StateCode = opldString.Length > 399 ? opldString.Substring(399, 5).Trim() : "";
            opldData.ZipCode = opldString.Length > 404 ? opldString.Substring(404, 9).Trim() : "";
            opldData.CountryCode = opldString.Length > 413 ? opldString.Substring(413, 2).Trim() : "";
            opldData.PhoneNumber = opldString.Length > 415 ? opldString.Substring(415, 15).Trim() : "";

            return opldData;
        }

        private static void OPLDFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                var opldFolderPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "OPLDFiles");

                var files = Directory.GetFiles(opldFolderPath);

                if (files.Length > 0)
                {
                    foreach (string fileName in files)
                    {
                        string opldString = System.IO.File.ReadAllText(Path.Combine(opldFolderPath, fileName));

                        //Process OPLD data
                        var opldObject = ProcessOPLD(opldString);

                        string opldProcString = Newtonsoft.Json.JsonConvert.SerializeObject(opldObject);

                        HttpClient client = new HttpClient();
                        
                        var content = new StringContent(opldProcString, Encoding.UTF8, "application/json");

                        content.Headers.Add("opldProcString", opldProcString);
                                                
                        //System.Threading.Tasks.Task<HttpResponseMessage> response = client.PostAsync("http://54.173.238.145:8080/api/ProcessOPLD/ProcessOPLDNPushTOMQ1", content);
                        //System.Threading.Tasks.Task<HttpResponseMessage> response = client.PostAsync("http://localhost:59477/api/ProcessOPLD/ProcessOPLDNPushTOMQ1", content);

                        //System.Threading.Tasks.Task<HttpResponseMessage> response = client.PostAsync("http://54.173.238.145:8080/api/ProcessOPLD/ProcessOPLDNPushTOMQ1", content);
                        System.Threading.Tasks.Task<HttpResponseMessage> response = client.PostAsync("http://newamsdemo-amsdemo.apps.techm.name/api/ProcessOPLD/ProcessOPLDNPushTOMQ1", content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void DIALSFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent("Call Web API", Encoding.UTF8, "application/json");
            System.Threading.Tasks.Task<HttpResponseMessage> response = client.PostAsync("https://localhost:44334/api/ProcessDIALS/ProcessDIALSNWriteToDB", content);
        }
    }
}
