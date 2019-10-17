using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class StatisticsHelper
{
    private List<SubtotalItem> SummaryList { set; get; } = new List<SubtotalItem>();
    public class SubtotalItem
    {
        public class SubtotalCount
        {
            public int Total { set; get; }
            public int Error502 { set; get; }

            public int OverTotal { set; get; }
            public int OverError502 { set; get; }

            public SubtotalCount()
            {
                Total = 0;
                Error502 = 0;
                OverTotal = 0;
                OverError502 = 0;
            }
        }

        public string ReportServerName { get; set; }
        public Dictionary<string, SubtotalCount> SummaryData { get; set; }
        public SubtotalItem(string reportServerName)
        {
            ReportServerName = reportServerName;
            SummaryData = new Dictionary<string, SubtotalCount>();
            SummaryData.Add("robots", new SubtotalCount());
            SummaryData.Add("xdotool", new SubtotalCount());
            SummaryData.Add("Others", new SubtotalCount());
        }

        public void Statistics(string apiName, string reaponse)
        {
            SummaryData[apiName].Total++;
            if (reaponse == "502") { SummaryData[apiName].Error502++; }
        }

        public void OverStatistics(string apiName, string reaponse)
        {
            SummaryData[apiName].OverTotal++;
            if (reaponse == "502") { SummaryData[apiName].OverError502++; }

            Statistics(apiName, reaponse);
        }
    }

    public class MoniterSetting
    {
        public string ServerName { set; get; }
        public string ServerType { set; get; }

        public string ReportServerName
        {
            get
            {
                return $"{ServerName}_{ServerType}";
            }
        }
        public IEnumerable<string> LogPath()
        {
            string RootPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Web_Log\\{ServerName}");
            List<string> path = new List<string>();

            DateTime temp = ParseLogDate.LogStartDate;
            while (temp <= ParseLogDate.LogEndDate)
            {
                path.Add( System.IO.Path.Combine(RootPath, $"{ServerType}_apache_logs_{temp.ToString("yyyyMMdd")}"));
                temp = temp.AddDays(1);
            }
            return path;
        }
    }

    public class ParseLogDate
    {
        public static DateTime LogStartDate { set; get; }
        public static DateTime LogEndDate { set; get; }

        static ParseLogDate()
        {
            LogStartDate = new DateTime(2019, 10, 14);
            LogEndDate = new DateTime(2019, 10, 15);
        }
    }

    public void ParseFile(MoniterSetting moniter)
    {
        //https://dotnet-snippets.de/snippet/apache-log-file-parsen-regex/5969
        //24.236.252.67 - - [17/May/2015:10:05:40 +0000] "GET /favicon.ico HTTP/1.1" 200 3638 "-" "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:26.0) Gecko/20100101 Firefox/26.0"
        string logEntryPattern = "^([\\d.]+) (\\S+) (\\S+) \\[([\\w:/]+\\s[+\\-]\\d{4})\\] \"(.+?)\" (\\d{3}) (\\d+|-) \"([^\"]+)\" \"([^\"]+)\"";
        SubtotalItem subtotal = new SubtotalItem(moniter.ReportServerName);

        foreach (string path in moniter.LogPath())
        {          
            using (StreamReader sr = File.OpenText(path))
            {
                string s = String.Empty;
          
                while ((s = sr.ReadLine()) != null)
                {
                    //we're just testing read speeds
                    Match regexMatch = Regex.Match(s, logEntryPattern);

                    DateTime time;
                    if (!DateTime.TryParseExact(regexMatch.Groups[4].Value, "dd/MMM/yyyy:HH:mm:ss zzz",
                                               System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                               System.Globalization.DateTimeStyles.None, out time))
                    {
                        continue;
                    }

                    string Request = regexMatch.Groups[5].Value;
                    string Response = regexMatch.Groups[6].Value;
                    int BytesSent = 0;                    
                    if(!int.TryParse(regexMatch.Groups[7].Value, out BytesSent)){
                        BytesSent = 0;
                    }

                    string apiName = "Others";
                    if (Request == "GET /robots.txt HTTP/1.1") { apiName = "robots"; }
                    if (Request == "GET /projects/xdotool/ HTTP/1.1") { apiName = "xdotool"; }

                    if (BytesSent >= 30000)
                    {
                        subtotal.OverStatistics(apiName, Response);
                    }
                    else
                    {
                        subtotal.Statistics(apiName, Response);
                    }

                }
            }
        }

        SummaryList.Add(subtotal);
    }

    public void SaveResult()
    {
        CreateHtmlFile(SummaryToTable());
        SummaryList.Clear();
    }

    private void CreateHtmlFile(StringBuilder body)
    {
        StringBuilder main = new StringBuilder();
        main.AppendLine("<!DOCTYPE html><html lang='zh-TW'><head>");
        main.AppendLine("<meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'><meta http-equiv='X-UA-Compatible' content='ie=edge'>");
        main.AppendLine("<title>Report</title>");
        main.AppendLine(body.ToString());
        main.AppendLine("</head><body></body></html>");

        File.WriteAllText($"report_{DateTime.Now.ToString("yyyyMMdd")}.html", main.ToString());
    }

    private StringBuilder SummaryToTable()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<table width='100%' border='1' cellpadding='0' cellspacing='0' style='border:2px #26FF26 solid;text-align:center;'>");
        
        SubtotalItem TotalItem = new SubtotalItem("總計");
        sb.Append("<tr><th rowspan='2'>Server</th>"); 
        foreach (var item in TotalItem.SummaryData.Keys)
        {
            sb.AppendFormat("<th colspan='4'>{0}</th>", item); 
        }
        sb.Append("<th colspan='2'>小計</th></tr>"); 

        sb.Append("<tr>"); 
        foreach (var item in TotalItem.SummaryData.Keys)
        {
            sb.Append("<th>超過8秒筆數</th>"); 
            sb.Append("<th>超過8秒502筆數</th>"); 
            sb.Append("<th>總筆數</th>"); 
            sb.Append("<th>502筆數</th>"); 
        }
        sb.Append("<th>超過8秒筆數</th>");      
        sb.Append("<th>總筆數</th>"); 
        sb.Append("</tr>"); 


        foreach (var i in SummaryList)
        {
            sb.AppendFormat("<tr><td><span style='font-weight:bolder;'>{0}</span></td>", i.ReportServerName); 
            foreach (var k in i.SummaryData.Keys)
            {
                sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData[k].OverTotal);
                sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData[k].OverError502);     
                sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData[k].Total);
                sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData[k].Error502);  
            }
            sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData.Sum(m => m.Value.OverTotal));
            sb.AppendFormat("<td valign='top'><span style='font-weight:bolder;'>{0}</span></td>", i.SummaryData.Sum(m => m.Value.Total));
            
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        return sb;
    }  

}