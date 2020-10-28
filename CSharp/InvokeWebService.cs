using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Services.Description;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

//https://www.itread01.com/content/1546603934.html
namespace EasyNet.Core
{
    /// <summary>
    /// 動態呼叫WebService
    /// </summary>
    public class WebServicesHelper
    {
        #region InvokeWebService

        /// <summary>   
        /// 呼叫WebService（不帶SoapHeader）
        /// </summary>   
        /// <param name="wsUrl">WebService地址</param>   
        /// <param name="methodName">方法名稱</param>   
        /// <param name="args">引數列表</param>   
        /// <returns>返回呼叫結果</returns>   
        public static object InvokeWebService(string wsUrl, string methodName, object[] args)
        {
            return InvokeWebService(wsUrl, null, methodName, null, args);
        }

        /// <summary>   
        /// 呼叫WebService（帶SoapHeader）
        /// </summary>   
        /// <param name="wsUrl">WebService地址</param>   
        /// <param name="methodName">方法名稱</param>   
        /// <param name="soapHeader">SOAP頭</param>   
        /// <param name="args">引數列表</param>   
        /// <returns>返回呼叫結果</returns>
        public static object InvokeWebService(string wsUrl, string methodName, SoapHeader soapHeader, object[] args)
        {
            return InvokeWebService(wsUrl, null, methodName, soapHeader, args);
        }

        /// <summary>   
        /// 呼叫WebService（帶classname）
        /// </summary>   
        /// <param name="wsUrl">WebService地址</param>   
        /// <param name="classname">類名</param>    
        /// <param name="methodName">方法名稱</param>    
        /// <param name="args">引數列表</param>   
        /// <returns>返回呼叫結果</returns>
        public static object InvokeWebService(string wsUrl, string classname, string methodName, object[] args)
        {
            return InvokeWebService(wsUrl, classname, methodName, args);
        }

        /// <summary>   
        /// 呼叫WebService
        /// </summary>   
        /// <param name="wsUrl">WebService地址</param>   
        /// <param name="className">類名</param>   
        /// <param name="methodName">方法名稱</param>   
        /// <param name="soapHeader">SOAP頭</param>
        /// <param name="args">引數列表</param>   
        /// <returns>返回呼叫結果</returns>
        public static object InvokeWebService(string url, string classname, string methodname, SoapHeader soapHeader, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if ((classname == null) || (classname == ""))
            {
                classname = GetWsClassName(url);
            }

            try
            {
                //獲取WSDL
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客戶端代理類程式碼
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider csc = new CSharpCodeProvider();
                ICodeCompiler icc = csc.CreateCompiler();

                //設定編譯引數
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                //編譯代理類
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }

                //生成代理例項，並呼叫方法
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);


                #region soapheader資訊
                FieldInfo[] arry = t.GetFields();

                FieldInfo fieldHeader = null;
                //soapheader 物件值
                object objHeader = null;
                if (soapHeader != null)
                {
                    fieldHeader = t.GetField(soapHeader.ClassName + "Value");

                    Type tHeader = assembly.GetType(@namespace + "." + soapHeader.ClassName);
                    objHeader = Activator.CreateInstance(tHeader);

                    foreach (KeyValuePair<string, object> property in soapHeader.Properties)
                    {
                        FieldInfo[] arry1 = tHeader.GetFields();
                        int ts = arry1.Count();
                        FieldInfo f = tHeader.GetField(property.Key);
                        if (f != null)
                        {
                            f.SetValue(objHeader, property.Value);
                        }
                    }
                }

                if (soapHeader != null)
                {
                    //設定Soap頭
                    fieldHeader.SetValue(obj, objHeader);
                }

                #endregion


                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                return mi.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }





        /// <summary>
        ///  獲取WebService的類名
        /// </summary>
        /// <param name="wsUrl"></param>
        /// <returns></returns>
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');

            return pps[0];
        }
        #endregion
    }


    /// <summary>
    /// SOAP頭
    /// </summary>
    public class SoapHeader
    {
        /// <summary>
        /// 構造一個SOAP頭
        /// </summary>
        public SoapHeader()
        {
            this.Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// 構造一個SOAP頭
        /// </summary>
        /// <param name="className">SOAP頭的類名</param>
        public SoapHeader(string className)
        {
            this.ClassName = className;
            this.Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// 構造一個SOAP頭
        /// </summary>
        /// <param name="className">SOAP頭的類名</param>
        /// <param name="properties">SOAP頭的類屬性名及屬性值</param>
        public SoapHeader(string className, Dictionary<string, object> properties)
        {
            this.ClassName = className;
            this.Properties = properties;
        }

        /// <summary>
        /// SOAP頭的類名
        /// </summary>
        public string ClassName
        {
            get;
            set;
        }

        /// <summary>
        /// SOAP頭的類屬性名及屬性值
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get;
            set;
        }

        /// <summary>
        /// 為SOAP頭增加一個屬性及值
        /// </summary>
        /// <param name="name">SOAP頭的類屬性名</param>
        /// <param name="value">SOAP頭的類屬性值</param>
        public void AddProperty(string name, object value)
        {
            if (this.Properties == null)
            {
                this.Properties = new Dictionary<string, object>();
            }
            Properties.Add(name, value);
        }
    }



    /// <summary>
    /// 獲取城市資訊
    /// </summary>
    /// <param name="citycode"></param>
    /// <returns></returns>
    public DataSet GetCityData(string CityCode)
    {
        try
        {
            //WebServices 地址 例如：http://www.****.com/WebService1.asmx
            string url = System.Configuration.ConfigurationManager.AppSettings["Url"].Trim().ToString();

            //WebServices 方法名
            string methodname = "GetCityData";

            //WebServices 引數 如果有多個引數按順序以逗號隔開 { param1 , param2 , param3 }
            object[] param = { CityCode };
            //呼叫WebServices
            object obj = WebServicesHelper.InvokeWebService(url, methodname, param);

            //返回值
            DataSet ds = (DataSet)obj; //注意這裡的返回值不一定是DataSet 根據你所呼叫的WebServices來決定 

            return ds;
        }
        catch (Exception ex)
        {
            LogHelper.WriteLog("", ex);
            return null;
        }
    }

}