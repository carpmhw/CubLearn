using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

public class HttpClientHelper
{
    public async Task<string> DownloadPageAsync(string url)
    {
        string content = await HttpClientServiceB.Instance.GetStringAsync(url);

        return content;
    }

    public async Task<string> PostAsync(string url)
    {
        // 指定 authorization header
        HttpClientServiceB.Instance.DefaultRequestHeaders.Add("authorization", "token {api token}");

        string content;
        try
        {
            // 準備寫入的 data
            var postData = new { userId = 123422, title = "yowko 中文", body = "yowko test body 中文" };
            // 將 data 轉為 json
            string json = JsonConvert.SerializeObject(postData);
            // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
            // 發出 post 並取得結果
            HttpResponseMessage response = await HttpClientServiceB.Instance.PostAsync(url, contentPost).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            // 將回應結果內容取出並轉為 string 再透過 linqpad 輸出
            content = await response.Content.ReadAsStringAsync().ConfigureAwait(false); //.GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            content = ex.Message;
        }

        return content;
    }
}

public sealed class HttpClientServiceB
{
    private static readonly Lazy<HttpClient> lazy = new Lazy<HttpClient>(
        () =>
        {
            var result = new HttpClient();
            result.Timeout = TimeSpan.FromSeconds(15);

            var baseUri = new Uri("http://blog.yowko.com");
            result.BaseAddress = baseUri;
            //設定 1 分鐘沒有活動即關閉連線，預設 -1 (永不關閉)
            ServicePointManager.FindServicePoint(baseUri).ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            //設定 1 分鐘更新 DNS，預設 120000 (2 分鐘)
            ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            return result;
        });
    public static HttpClient Instance { get { return lazy.Value; } }

    private HttpClientServiceB() { }
}


public class WebRequestService
{
    public void Get()
    {
        //建立 WebRequest 並指定目標的 uri
        WebRequest request = WebRequest.Create("http://huan-lin.blogspot.com");
        // 使用 HttpWebRequest.Create 實際上也是呼叫 WebRequest.Create
        //WebRequest request = HttpWebRequest.Create("http://jsonplaceholder.typicode.com/posts");
        //指定 request 使用的 http verb
        request.Method = "GET";
        //使用 GetResponse 方法將 request 送出，如果不是用 using 包覆，請記得手動 close WebResponse 物件，避免連線持續被佔用而無法送出新的 request
        using (var httpResponse = (HttpWebResponse)request.GetResponse())
        //使用 GetResponseStream 方法從 server 回應中取得資料，stream 必需被關閉
        //使用 stream.close 就可以直接關閉 WebResponse 及 stream，但同時使用 using 或是關閉兩者並不會造成錯誤，養成習慣遇到其他情境時就比較不會出錯
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();

        }
    }

    public void PostJson()
    {
        //建立 WebRequest 並指定目標的 uri
        WebRequest request = WebRequest.Create("http://huan-lin.blogspot.com");
        //指定 request 使用的 http verb
        request.Method = "POST";
        //準備 post 用資料
        var postData = new { userId = 1, title = "yowko", body = "yowko test body 中文" };
        //指定 request 的 content type
        request.ContentType = "application/json; charset=utf-8";
        //指定 request header
        request.Headers.Add("authorization", "token apikey");
        //將需 post 的資料內容轉為 stream 
        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            streamWriter.Write(json);
            streamWriter.Flush();
        }
        //使用 GetResponse 方法將 request 送出，如果不是用 using 包覆，請記得手動 close WebResponse 物件，避免連線持續被佔用而無法送出新的 request
        using (var httpResponse = (HttpWebResponse)request.GetResponse())
        //使用 GetResponseStream 方法從 server 回應中取得資料，stream 必需被關閉
        //使用 stream.close 就可以直接關閉 WebResponse 及 stream，但同時使用 using 或是關閉兩者並不會造成錯誤，養成習慣遇到其他情境時就比較不會出錯
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();

            Console.WriteLine("==================================================="); 
            Console.WriteLine(result.Substring(0, 100));
        }
    }

    public void PostUrlEncode()
    {
        WebRequest request = HttpWebRequest.Create("http://huan-lin.blogspot.com");
        request.Method = "POST";
        //使用 application/x-www-form-urlencoded
        request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
        request.Headers.Add("authorization", "token apikey");
        //要傳送的資料內容(依字串表示)
        string postData = "id=9&name=yowko&body=yowko中文";
        //將傳送的字串轉為 byte array
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        //告訴 server content 的長度
        request.ContentLength = byteArray.Length;
        //將 byte array 寫到 request stream 中 
        using (Stream reqStream = request.GetRequestStream())
        {
            reqStream.Write(byteArray, 0, byteArray.Length);
        }
        using (var httpResponse = (HttpWebResponse)request.GetResponse())
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
             Console.WriteLine(result);
        }
    }
}