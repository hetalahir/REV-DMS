using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace searchAndExportDocument
{
    public class ExportDoc
    {
        readonly revCore.Config _rev;
        readonly MyConfig _myconfig;

        public ExportDoc(revCore.Config revConfig, MyConfig myConfig)
        {
            _rev = revConfig;
            _myconfig = myConfig;

        }

        //code review Deepay attention to this error. If nothing is async just use TASK and return Task.completed.
        public async Task ExportDocAsync()
        {
            var rev = new revCore.Rev(_rev);

            // get list of documents
            var res = rev.SearchDocs(_myconfig.projectName, new Dictionary<string, string> { { _myconfig.searchIndex, _myconfig.searchIndexValue } }).ToArray();

            // Regular expression to remove special charater document name
            string pattern = @"([\\//;:?*><|])";

            foreach (var doc in res)
            {
                string docname = "";
                foreach (var i in doc.indexes)
                {
                    if (i.Key == _myconfig.docFolderName1 || i.Key == _myconfig.docFolderName2)
                    {
                        docname = docname + i.Value + " ";
                    }
                }


                string DocName = Regex.Replace(docname, pattern, "") + doc.pages.Count();

                createFolder($"{_myconfig.downFolder}\\{_myconfig.projectName}\\{DocName}");

                //dee code review this is inefficient
                /*
                foreach (var page in doc.pages)
                {
                    var pathS = $"{_rev.revUrl}{page.path}";
                    var docName = Regex.Replace(page.id, pattern, "");
                    var pathD = $"C:\\{_myconfig.downFolder}\\{_myconfig.projectName}\\{DocName}\\{docName} ";
                    await downloadFileAsync(pathS, pathD);                    
                }
                */

                /* this is compact code
                var done = await Task.WhenAll(doc.pages.Select(async page =>
                {
                    var pathS = $"{_rev.revUrl}{page.path}";
                    var docName = Regex.Replace(page.id, pattern, "");
                    var pathD = $"C:\\{_myconfig.downFolder}\\{_myconfig.projectName}\\{DocName}\\{docName} ";
                    await downloadFileAsync(pathS, pathD);
                    return true;
                }));
                */

                var downloadTasks = doc.pages.Select(async page =>
                {
                    var pathS = $"{_rev.revUrl}{page.path}";
                    var docName = Regex.Replace(page.id, pattern, "");
                    var pathD = $"C:\\{_myconfig.downFolder}\\{_myconfig.projectName}\\{DocName}\\{docName} ";
                    await downloadFileAsync(pathS, pathD);
                    return true;
                });

                var done = await Task.WhenAll(downloadTasks);

            }


        }

        public async Task downloadFileAsync(string src, string dest)
        {
            WebClient wc = new WebClient();
            try
            {
                //dee code review use DownloadFileAsync
                await wc.DownloadFileTaskAsync(new Uri(src), dest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void createFolder(string folderName)
        {
            Directory.SetCurrentDirectory(@"C:\");
            if (!Directory.Exists(folderName))
            {
                DirectoryInfo di = Directory.CreateDirectory(folderName);
            }
            else
            {
                Console.WriteLine(folderName + "Folder already exists.");
            }
        }
    }
}
