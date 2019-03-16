using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace DoorbellPush
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral t;

        int gpioPin = 26; //4;
        GpioController controll;
        GpioPin pin;

        DateTime lastSentNotification , filterTime;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
           t=  taskInstance.GetDeferral();
            controll = GpioController.GetDefault();
            pin = controll.OpenPin(gpioPin);
            pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            pin.ValueChanged += Pin_ValueChanged;
            pin.DebounceTimeout = TimeSpan.FromMilliseconds(10);
            ResetDateAndFilterTime();

            SendNotification();
        }

        private  void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            
            if(args.Edge==GpioPinEdge.RisingEdge && CheckIsNotSameNotification() )
            {
                ResetDateAndFilterTime();
                SendNotification();
            }
           // throw new NotImplementedException();
        }

        private async void SendNotification()
        {
            HttpClient client = new HttpClient();
            dynamic strObj = new ExpandoObject ();
            strObj.message = "IOTSecret";
            var str = JsonConvert.SerializeObject(strObj);
            var content = new StringContent( str,Encoding.UTF8, "application/json");
            //client.DefaultRequestHeaders.Add("CONTENT_TYPE", "application/json");
            client.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {

                var resp = await client.PostAsync("https://mywebservice.com/Push.php", content);
                var cont = await resp.Content.ReadAsStringAsync();
            }
            catch (Exception e) 
            {

              
            }
        }

        private bool CheckIsNotSameNotification()
        {
            if (DateTime.Now>filterTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ResetDateAndFilterTime()
        {
            lastSentNotification = DateTime.Now;
            filterTime = DateTime.Now.Add(TimeSpan.FromSeconds(5));
        }
    }
}
