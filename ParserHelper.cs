using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class ParserHelper
{
    public class LogItem
    {
        public string IPAddress { set; get; }
        public DateTime? DateTime { set; get; }
        public string Request { set; get; }
        public string Response { set; get; }
        public string BytesSent { set; get; }
        public string Referer { set; get; }
        public string Browser { set; get; } 
        public string OriginLine { set; get; }
        public int PrivilegeLevel {set; get; }
    }

    private int SetRecPrivilege(string request)
    {
        string [] hightLevel = new string[] { "GET /robots.txt HTTP/1.1" ,  "GET /projects/xdotool/ HTTP/1.1" };

        for(int i=0; i<hightLevel.Length; i++){
            if(hightLevel[i] == request) {
                return i + 1;
            }
        }

        return 9999;
    }

    public void ParseFile(string fileName)    
    {
        //https://dotnet-snippets.de/snippet/apache-log-file-parsen-regex/5969
        string logEntryPattern = "^([\\d.]+) (\\S+) (\\S+) \\[([\\w:/]+\\s[+\\-]\\d{4})\\] \"(.+?)\" (\\d{3}) (\\d+|-) \"([^\"]+)\" \"([^\"]+)\"";

        DateTime CheckStartTime = new DateTime(2015, 5, 21, 5, 05, 20);
        DateTime CheckEndTime = new DateTime(2015, 5, 21, 5, 06, 20);      

        List<LogItem> list = new List<LogItem>();
        using (StreamReader sr = File.OpenText(fileName))
        {
            string s = String.Empty;
        
            while ((s = sr.ReadLine()) != null)
            {
                //we're just testing read speeds
                Match regexMatch = Regex.Match(s, logEntryPattern);
               
                DateTime time;
                
                if(!DateTime.TryParseExact(regexMatch.Groups[4].Value, "dd/MMM/yyyy:HH:mm:ss zzz", 
			                               System.Globalization.DateTimeFormatInfo.InvariantInfo,
			                               System.Globalization.DateTimeStyles.None, out time))
                {
                  continue;
                }
                              
                if(CheckStartTime > time) { continue; }
                if(CheckEndTime < time) { continue; }

                string referer = "";
                if (!regexMatch.Groups[8].Value.Equals("-")){
                    referer =  regexMatch.Groups[8].Value;
                }
                list.Add(
                    new LogItem() {
                        IPAddress = regexMatch.Groups[1].Value,
                        DateTime = time,
                        Request = regexMatch.Groups[5].Value,
                        Response = regexMatch.Groups[6].Value,
                        BytesSent = regexMatch.Groups[7].Value,
                        Referer = referer,
                        Browser = regexMatch.Groups[9].Value,
                        OriginLine = s,
                        PrivilegeLevel = SetRecPrivilege(regexMatch.Groups[5].Value)
                    }
                );
            }            
        }

        CreateHtmlFile(list);
        SendMail();
    }

    public void CreateHtmlFile(IEnumerable<LogItem> list) {

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<table width='100%' border='0' cellpadding='0' cellspacing='0' bgcolor='#F2F2F2'>");

        sb.AppendLine("<tr><td valign='top'>");
        sb.AppendLine("<span style='font-weight:bolder; margin-left:10px;'>Web01</span>");
        sb.AppendLine("</td></tr>");

        var resultList = list.OrderBy(m => m.PrivilegeLevel).ThenBy(m => m.Request)
                             .GroupBy(m => m.Request)
                             .ToDictionary(o => o.Key, o => o.ToList());
                             
        foreach(var key in resultList.Keys){
            sb.AppendFormat("<tr><td valign='top'><span style='padding-top:8px; font-size:12px; font-weight:bolder; margin-left:10px;'>{0} ({1}筆)</span></td></tr>", key, resultList[key].Count);

            sb.AppendLine("<tr><td valign='top'><ol style='padding-top:8px; font-size:12px'>"); 
            foreach (var Item in resultList[key].OrderBy(m => m.DateTime))
            {                
                sb.AppendLine($"<li>{Item.OriginLine}</li>");
            }
            sb.AppendLine("</ol></td></tr>");
        }    

        sb.AppendLine("</table>");

        using (StreamWriter sw = new StreamWriter("result.html"))
        {
            sw.Write(sb.ToString());
        }
    }

    public void SendMail(){
        System.Net.Mail.MailMessage MyMail = new System.Net.Mail.MailMessage();
        MyMail.From = new System.Net.Mail.MailAddress("xxx@domain.com.tw");
        MyMail.To.Add("test@test.com"); //設定收件者Email            
        MyMail.Subject = "Email Test";
        MyMail.Body = File.ReadAllText("result.html") ; //設定信件內容
        MyMail.IsBodyHtml = true; //是否使用html格式

        System.Net.Mail.SmtpClient MySMTP = new System.Net.Mail.SmtpClient("smtp.hinet.net");
        MySMTP.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory;
        MySMTP.PickupDirectoryLocation = AppDomain.CurrentDomain.BaseDirectory;        
        try
        {
            MySMTP.Send(MyMail);
            MyMail.Dispose(); //釋放資源
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

}