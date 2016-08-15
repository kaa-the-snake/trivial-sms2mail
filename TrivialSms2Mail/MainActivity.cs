using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Security.Cryptography.X509Certificates;
using Android.Telephony;

namespace TrivialSms2Mail
{
    [Activity(Label = "TrivialSms2Mail", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                    { return true; };

            var mailButton = FindViewById<Button>(Resource.Id.emailButton);
            mailButton.Click += delegate
            {
                 
                
                
            };
        }
    }

    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver")]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class SMSBroadcastReceiver : BroadcastReceiver
    {

        private const string Tag = "SMSBroadcastReceiver";
        private const string IntentAction = "android.provider.Telephony.SMS_RECEIVED";

        private const string SubscriptionExtra = "subscription";

        private string DestinationNumberFromIntent(Context context, Intent intent)
        {
            int sbsid = intent.GetIntExtra(SubscriptionExtra, -1);
            if (sbsid == -1)
                return "<number not available>";

            SubscriptionInfo info = SubscriptionManager.From(context).GetActiveSubscriptionInfo(sbsid);
            return $"{info.Number} on {info.CarrierName}";
            //return string.Format("{0} {1} at {2}", info.Number, info.DisplayName, info.CarrierName);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Android.Util.Log.Info(Tag, "Intent received: " + intent.Action);

            

            if (intent.Action != IntentAction) return;

            Android.Telephony.SmsMessage[] messages = Android.Provider.Telephony.Sms.Intents.GetMessagesFromIntent(intent);
            

            for (var i = 0; i < messages.Length; i++)
            {
                var cl = new System.Net.Mail.SmtpClient();
                cl.Host = "smtp.gmail.com";
                cl.Port = 587;
                cl.EnableSsl = true;
                cl.Credentials = new System.Net.NetworkCredential("akiritchun@gmail.com", "nightmail");
                var msg = new System.Net.Mail.MailMessage("akiritchun@gmail.com", "akiritchun@gmail.com");
                msg.Body = String.Format("Сообщение от {0} для {1}", messages[i].OriginatingAddress, DestinationNumberFromIntent(context, intent));
                msg.Subject = messages[i].MessageBody;
                try
                {
                    cl.Send(msg);
                }
                catch (System.Net.Mail.SmtpException e)
                {
                    string errmsg = "Error: " + e.Message;
                    //e.StackTrace
                    var shower = Toast.MakeText(Application.Context, errmsg, ToastLength.Long);
                    shower.Show();
                }
            }            
        }
    }
}

