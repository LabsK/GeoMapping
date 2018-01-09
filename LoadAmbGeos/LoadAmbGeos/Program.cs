using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


//using System.Json;
using System.Net.Http;
using System.IO;

using System.Web;

using System.Data.SqlClient;

namespace LoadAmbGeos
{
    class Program
    {
        static void Main(string[] args)
        {

            //string zip = "92612";
            //var cont = GetLatitudeLongitude(zip);

            string lat = "";
            string lon = "";
            string country = "";

            for (int i = 1; i <= 2100000000; i++)
            {

                if ((i % 1000000) == 0)
                {
                    var idZip = GetZip();

                    //int id = idZip.Result.Item1;
                    int id = 0;
                    string zip = idZip.Result.Item2;

                    try
                    {
                        var cont = GetLatiLongitude(zip);

                        //System.Threading.Thread.Sleep(300);

                        lat = cont.Result.Item1.ToString();
                        lon = cont.Result.Item2.ToString();

                        if (lat == "0" && lat == "0")
                        {
                            Console.WriteLine("END of Loop");
                            //return;
                        }

                        country = cont.Result.Item3;

                        if (lat != "0" && lat != "0")
                        {

                            if (UpdateZipCompare(id, zip, lat, lon, country).Result)
                            {
                            }
                            else
                            {
                                Console.WriteLine("Did not update! Zip -> " + zip + " Latitude ->  " + cont.Result.Item1.ToString() + " Longitude ->  " + cont.Result.Item2.ToString());
                            }

                        }
                        else
                        {
                            if (country == "ZERO_RESULTS")          //invalid zipcode even from google
                            {
                                Console.WriteLine("ZERO_RESULTS - Not a valid Zipcode -> " + zip);
                                if (UpdateZipCompareInvalidZip(id, zip, country).Result)
                                {
                                }
                                else
                                {
                                    Console.WriteLine("Did not update! Zip -> " + zip + " Latitude ->  " + cont.Result.Item1.ToString() + " Longitude ->  " + cont.Result.Item2.ToString());
                                }
                            }
                            else if (country == "Not Valid US Zip")
                            {
                                Console.WriteLine("ZERO_RESULTS - Not a valid US Zipcode -> " + zip);
                                if (UpdateZipCompareInvalidZip(id, zip, country).Result)
                                {
                                }
                                else
                                {
                                    Console.WriteLine("Did not update! Zip -> " + zip + " Latitude ->  " + cont.Result.Item1.ToString() + " Longitude ->  " + cont.Result.Item2.ToString());
                                }
                            }
                            else
                            {
                                Console.WriteLine("You have exceeded your daily request quota for this API, Zip -> " + zip);
                            }
                        }

                    }

                    catch (Exception e)
                    {
                        Console.WriteLine("Did not update! Zip -> " + zip + " Latitude ->  " + lat + " Longitude ->  " + lon);
                    }

                }
            }
            //Console.WriteLine("Here is the content " + cont.ToString());

            Console.WriteLine("End of Execution!");
            Console.ReadLine();
        }

        static async Task<Tuple<int, string>> GetZip()
        {
            string zip = null;
            int id = 0;


            using (SqlConnection conn = new SqlConnection("Data Source= \\SQLEXPRESS; Initial Catalog=DCIM;persist security info=True; Integrated Security=SSPI;"))
            {
                //string sql = @"select top 1 * from [Dealer] where len(MailZip) = 4 and ModifiedBy is null;";
                string sql = @"select top 1 * from [ProdAmbZip] where GLatitude is null;";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    cmd.CommandTimeout = 100000;
                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (rdr.Read())
                        {
                            //id = Convert.ToInt32(rdr["CompanyId"]);
                            zip = Convert.ToString(rdr["ZipCode"]);
                        }
                    }
                }

            }

            Tuple<int, string> idZip = new Tuple<int, string>(id, zip);
            return idZip;
        }

        static async Task<bool> UpdateZipCompare(int id, string zip, string latitude, string longitude, string country)
        {

            using (SqlConnection conn = new SqlConnection("Data Source= \\SQLEXPRESS; Initial Catalog=DCIM;persist security info=True; Integrated Security=SSPI;"))    //user id=SqlAdmin;password=;"))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    //cmd.CommandText = "update Dealer set ModifiedBy = @ModifiedBy, ModifiedDate = @ModifiedDate, ZipLatitude = @GLatitude, ZipLongitude = @GLongitude where CompanyId = @Id and Zip = @Zip";   
                    cmd.CommandText = "update [ProdAmbZip] set GLatitude = @GLatitude, GLongitude = @GLongitude, Country = @Country where ZipCode = @Zip";
                    //cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Zip", zip);
                    cmd.Parameters.AddWithValue("@GLatitude", latitude);
                    cmd.Parameters.AddWithValue("@GLongitude", longitude);
                    cmd.Parameters.AddWithValue("@Country", country);
                    //cmd.Parameters.AddWithValue("@ModifiedBy", "Ungarala@dciartform.com");
                    //cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    bool result = await cmd.ExecuteNonQueryAsync() > 0;

                    return result;
                }

            }
        }


        static async Task<bool> UpdateZipCompareInvalidZip(int id, string zip, string country)
        {

            using (SqlConnection conn = new SqlConnection("Data Source=\\SQLEXPRESS; Initial Catalog=DCIM;persist security info=True; Integrated Security=SSPI;"))   //user id=SqlAdmin;password=;"))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    //cmd.CommandText = "update Dealer set Country = @Country where CompanyId = @Id and ZipCode = @Zip";  //this breaks as not up to date
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Zip", zip);
                    cmd.Parameters.AddWithValue("@Country", country);

                    bool result = await cmd.ExecuteNonQueryAsync() > 0;

                    return result;
                }

            }
        }

        //static async Task<string> GetLatitudeLongitude(string zip)
        //{
        //    try
        //    {
        //        //http://maps.googleapis.com/maps/api/geocode/json?address=53209
        //        var client = new HttpClient();
        //        //client.DefaultRequestHeaders.Add("Accept", "application/json;");     
        //        //var response = client.GetStringAsync("http://maps.googleapis.com/maps/api/geocode/json?address='" + zip + "'");        
        //        var response = client.GetStringAsync("http://maps.googleapis.com/maps/api/geocode/json?address=92612");

        //        var strReponse = response.ToString();
        //        //var responseString = await response.Content.ReadAsStringAsync();

        //        return strReponse;

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        Console.ReadLine();
        //    }

        //    return null;
        //}


        //static class SqlReaderExtension
        //{
        //    public static async Task<T> ReadAsync<T>(this SqlDataReader reader, string fieldName)
        //    {
        //        if (reader == null) throw new ArgumentNullException(nameof(reader));
        //        if (string.IsNullOrEmpty(fieldName))
        //            throw new ArgumentException("Value cannot be null or empty.", nameof(fieldName));

        //        int idx = reader.GetOrdinal(fieldName);
        //        return await reader.GetFieldValueAsync<T>(idx);
        //    }
        //}



        static async Task<Tuple<decimal, decimal, string>> GetLatiLongitude(string zip)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create("http://maps.googleapis.com/maps/api/geocode/json?address=" + zip);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            //webrequest.Headers.Add("Username", "xyz");
            //webrequest.Headers.Add("Password", "abc");
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            //StreamReader responseStream = new StreamReader(@"C:\Users\Ungarala\Documents\Visual Studio 2013\new 1.txt");
            string result = string.Empty;
            result = responseStream.ReadToEnd();


            webresponse.Close();

            if (result.Contains("You have exceeded your daily request quota for this API"))
            {

            }
            else if (result.Contains("ZERO_RESULTS"))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                dynamic obj = jss.Deserialize<dynamic>(result);

                string status = obj["status"];
                if (status == "ZERO_RESULTS")
                {
                    return new Tuple<decimal, decimal, string>(0, 0, "ZERO_RESULTS");
                }
                else
                {
                    return new Tuple<decimal, decimal, string>(0, 0, "Not Valid US Zip");
                }
            }
            else
            {
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                dynamic obj = jss.Deserialize<dynamic>(result);

                decimal latitude = obj["results"][0]["geometry"]["location"]["lat"];
                decimal longitude = obj["results"][0]["geometry"]["location"]["lng"];

                string country = obj["results"][0]["formatted_address"];

                string[] strArr = country.Split(' ');
                country = strArr[strArr.Length - 1];

                Tuple<decimal, decimal, string> x = new Tuple<decimal, decimal, string>(Math.Round(latitude, 5), Math.Round(longitude, 5), country);


                return x;
            }

            return new Tuple<decimal, decimal, string>(0, 0, "");
        }

        //static void PrintValues(JsonNodes nodes, string parent)
        //{
        //    Console.WriteLine(string.Format("Name: {0}", nodes.Node));
        //    Console.WriteLine(string.Format("Value: {0}", nodes.NodeValue));
        //    Console.WriteLine("IsNode: true");
        //    Console.WriteLine(string.Format("Parent: {0}", parent));
        //    Console.WriteLine();

        //    if (parent == string.Empty)
        //    {
        //        parent += nodes.Node;
        //    }
        //    else
        //    {
        //        parent += string.Format("/{0}", nodes.Node);
        //    }

        //    foreach (JsonNodesAttribute attribute in nodes.Attributes)
        //    {
        //        Console.WriteLine(string.Format("Name: {0}", attribute.Key));
        //        Console.WriteLine(string.Format("Value: {0}", attribute.Value));
        //        Console.WriteLine("IsNode: false");
        //        Console.WriteLine(string.Format("Parent: {0}", parent));
        //    }

        //    Console.WriteLine();

        //    foreach (JsonNodes childNode in nodes.Nodes)
        //    {
        //        PrintValues(childNode, parent);
        //    }
        //}
    }
}
