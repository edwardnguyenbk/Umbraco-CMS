﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Core.IO;

namespace Umbraco.Core.Logging.Viewer
{
    public class JsonLogViewer : LogViewerSourceBase
    {       

        public override IEnumerable<LogEvent> GetAllLogs(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var logs = new List<LogEvent>();

            //Open the JSON log file for the range of dates (and exclude machinename) Could be several for LB
            var dateRange = endDate - startDate;

            //Log Directory
            var logDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\App_Data\Logs\";

            //foreach full day in the range - see if we can find one or more filenames that end with
            //yyyyMMdd.json - Ends with due to MachineName in filenames - could be 1 or more due to load balancing
            for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
            {
                //Filename ending to search for (As could be multiple)
                var filesToFind = $"*{day.ToString("yyyyMMdd")}.json";

                var filesForCurrentDay = Directory.GetFiles(logDirectory, filesToFind);

                //Foreach file we find - open it
                foreach (var filePath in filesForCurrentDay)
                {
                    //Open log file & add contents to the log collection
                    //Which we then use LINQ to page over
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var stream = new StreamReader(fs))
                        {
                            var reader = new LogEventReader(stream);
                            LogEvent evt;
                            while (reader.TryRead(out evt))
                            {
                                logs.Add(evt);
                            }
                        }
                    }
                }
            }

            return logs;
        }
        
    }
}