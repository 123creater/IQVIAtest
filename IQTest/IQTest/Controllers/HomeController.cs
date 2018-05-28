using IQTest.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IQTest.Controllers
{
    public class HomeController : Controller
    {
        List<Tweet> TotalTweet = new List<Tweet>();
        //Hosted web API REST Service base url  
        string Baseurl = "https://badapi.iqvia.io/";
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> getData()
        {
            DateTime StartDate = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            DateTime EndDate = new DateTime(2017, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);

            foreach (DateTime day in EachCalendarDay(StartDate, EndDate))
            {
                bool? recordAdded = await TweetsbyDate(new DateTime(day.Year, day.Month, day.Day, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(day.Year, day.Month, day.Day, 23, 59, 59, 999, DateTimeKind.Utc));

                //if response received is 100 tweets then loop by hour
                if (recordAdded == false)
                {
                    for (int hour = 0; hour < 24; hour++)
                    {
                        bool? hourAdded = await TweetsbyDate(new DateTime(day.Year, day.Month, day.Day, hour, day.Minute, day.Second, day.Millisecond, DateTimeKind.Utc), new DateTime(day.Year, day.Month, day.Day, hour, 59, 59, 999, DateTimeKind.Utc));

                        //if response received in hour is 100 tweets then loop by minute
                        if (hourAdded == false)
                        {
                            for (int min = 0; min < 60; min++)
                            {
                                bool? minAdded = await TweetsbyDate(new DateTime(day.Year, day.Month, day.Day, day.Hour, min, 0, 0, DateTimeKind.Utc), new DateTime(day.Year, day.Month, day.Day, day.Hour, min, 59, 999, DateTimeKind.Utc));

                                //if response received in minute is 100 tweets then loop by second 
                                if (minAdded == false)
                                {
                                    for (int sec = 0; sec < 60; sec++)
                                    {
                                        await TweetsbyDate(new DateTime(day.Year, day.Month, day.Day, day.Hour, day.Minute, sec, 0, DateTimeKind.Utc), new DateTime(day.Year, day.Month, day.Day, day.Hour, day.Minute, sec, 999, DateTimeKind.Utc));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //returning the Tweet list to view  
            return Json(JsonConvert.SerializeObject(TotalTweet), JsonRequestBehavior.AllowGet);
        }

        public static IEnumerable<DateTime> EachCalendarDay(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date.Date <= endDate.Date; date = date.AddDays(1)) yield
            return date;
        }


        public async Task<bool?> TweetsbyDate(DateTime sDate, DateTime eDate)
        {
            List<Tweet> Tweets = new List<Tweet>();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource using HttpClient  
                HttpResponseMessage Res = await client.GetAsync("api/v1/Tweets?startDate=" + sDate.ToString("o") + "&endDate=" + eDate.ToString("o"));

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = Res.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Tweet list  
                    Tweets = JsonConvert.DeserializeObject<List<Tweet>>(Response);

                    if (Tweets.Count() == 100)
                    {
                        return false;
                    }
                    else
                    {
                        TotalTweet.AddRange(Tweets);
                        return true;
                    }
                }
                else
                {
                    return null;
                }
            }

        }
    }
}
