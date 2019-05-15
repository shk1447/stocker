using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Util
    {
        private static readonly DateTime localBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

        public static Int32 GetUnixTimeInt32(DateTime now)
        {
            return Convert.ToInt32((now - localBase).TotalSeconds);
        }

        public static Int64 GetUnixTimeInt64(DateTime now)
        {
            return Convert.ToInt64((now - localBase).TotalMilliseconds);
        }

        public static void ModuleInit<T>(string moduleDirectoryPath, Dictionary<string, T> addModules)
            where T : class
        {
            if (!Directory.Exists(moduleDirectoryPath))
            {
                return;
            }

            var files = Directory.GetFiles(moduleDirectoryPath);
            var dllFiles = files.Where(x => Path.GetExtension(x).Equals(".dll", StringComparison.OrdinalIgnoreCase));

            foreach (var fileName in dllFiles)
            {
                var moduleKey = Path.GetFileNameWithoutExtension(fileName);
                var module = GetFileInterface<T>(fileName);

                if (module != null && !addModules.ContainsKey(moduleKey))
                {
                    addModules.Add(moduleKey, module);
                }
            }
        }

        public static T GetFileInterface<T>(string fileName)
            where T : class
        {
            //var assembly = Assembly.LoadFrom(fileName);
            try
            {
                var assembly = Assembly.Load(File.ReadAllBytes(fileName));
                foreach (var type in assembly.GetTypes())
                {
                    //if (type is T)
                    //{
                    //}
                    //var interfaces = type.GetInterfaces();
                    //if (interfaces.Length == 1 && interfaces[0].GUID.Equals(typeof(T).GUID))
                    if (typeof(T).IsAssignableFrom(type))
                    {
                        var name = Path.GetFileNameWithoutExtension(fileName);
                        var module = Activator.CreateInstance(type) as T;
                        if (module != null)
                        {
                            return module;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Http Web Request Post or Get Service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string PostOrGetHttpWebRequest(string url, string postData = "")
        {
            //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate
            //{
            //    return true;
            //});

            bool isPost = string.IsNullOrWhiteSpace(postData) ? false : true;

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = isPost ? "POST" : "GET";
            webRequest.ContentType = isPost ? "application/json" : "text/json";
            webRequest.Proxy = null;
            webRequest.ServicePoint.MaxIdleTime = 120000;
            webRequest.ServicePoint.ConnectionLimit = 30;
            webRequest.ServicePoint.ConnectionLeaseTimeout = 120000;
            webRequest.Timeout = 120000;

            if (isPost)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                webRequest.ContentLength = byteArray.Length;
                using (var writeStream = webRequest.GetRequestStream())
                {
                    writeStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            string responseStr = string.Empty;

            using (var response = webRequest.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        responseStr = streamReader.ReadToEnd();
                    }
                }
            }

            return responseStr;
        }

        public static T ChangeClass<T, ST>(ST info)
        {
            var stProps = typeof(ST).GetProperties();
            var nProps = typeof(T).GetProperties();

            T instance = Activator.CreateInstance<T>();

            foreach (var stProp in stProps)
            {
                var nProp = nProps.FirstOrDefault(x => x.Name == stProp.Name);
                if (nProp != null)
                {
                    var stVal = GetPropertyValue(stProp, info);
                    SetPropertyValue(nProp, instance, stVal);
                }
            }

            return instance;
        }

        public static object GetPropertyValue(PropertyInfo propertyInfo, object obj)
        {
            object data = null;
            if (propertyInfo.CanRead)
            {
                data = propertyInfo.GetValue(obj);
            }
            return data;
        }

        public static void SetPropertyValue(PropertyInfo propertyInfo, object obj, object value)
        {
            if (propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(obj, value);
            }
        }

        public static object FromJson(byte[] bytes, Type type)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(ms);
            }
        }

        //public static T FromJson<T>(byte[] bytes)
        //{
        //    return JsonConvert.DeserializeObject<T>(info);

        //    using (var ms = new MemoryStream(bytes))
        //    {
        //        var serializer = new DataContractJsonSerializer(typeof(T));
        //        return (T)serializer.ReadObject(ms);
        //    }
        //}

        public static T FromJson<T>(string data)
            where T : class
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(data);
            //var bytes = Encoding.UTF8.GetBytes(data);
            //return FromJson<T>(bytes);
        }

        public static string ToJson<T>(T info)
        {
            return JsonConvert.SerializeObject(info);


            //using (MemoryStream stream = new MemoryStream())
            //{
            //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            //    ser.WriteObject(stream, info);
            //    stream.Position = 0;
            //    using (StreamReader sr = new StreamReader(stream))
            //    {
            //        return sr.ReadToEnd();
            //    }
            //}
        }
    }
}
