using System;
using System.Linq;
using System.Windows;


using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Web;
using System.IO;

namespace CodeAbbeySaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        

        private void Save_button_Click(object sender, RoutedEventArgs e)
        {
            Save_button.IsEnabled = false;

            var client = new CookieAwareWebClient();
            client.BaseAddress = @"http://www.codeabbey.com/";
            var loginData = new NameValueCollection();
            loginData.Add("username", Login_textBox.Text);
            loginData.Add("password", Passowrd_passwordBox.Password);
            loginData.Add("email", "");
            client.UploadValues("index/login", "POST", loginData);
   
            string htmlSource = client.DownloadString("index/user_profile");


            var sourceLinksRegex = new Regex("/index/task_solution\\?task=(?<ProblemName>.+?)&.+?lang=(?<Language>.+?)\"");
            var sourceRegex = new Regex("<code.+?>(?<Code>.+?)</code>", RegexOptions.Singleline);

            var matches = sourceLinksRegex.Matches(htmlSource);

            foreach(Match m in matches)
            {
                var url = "http://www.codeabbey.com" + m.ToString().Replace("\"", "");
                var relativeUrl = m.ToString().Replace("\"", "");

                var lang = HttpUtility.UrlDecode(m.Groups["Language"].Value);
                var problemName = HttpUtility.UrlDecode(m.Groups["ProblemName"].Value);

                var sourcePageHtml = client.DownloadString(relativeUrl);


                var source = HttpUtility.HtmlDecode(sourceRegex.Match(sourcePageHtml).Groups["Code"].Value);

                var validLangFolder = CleanFileName(lang);
                var validFileNameWithProblemCode = CleanFileName(problemName) + ".txt";

                if (!Directory.Exists(validLangFolder))
                    Directory.CreateDirectory(validLangFolder);

                File.WriteAllText(validLangFolder + @"\" + validFileNameWithProblemCode, source);
            }

            Save_button.IsEnabled = true;

            MessageBox.Show("Done!");
        }



        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }



        public class CookieAwareWebClient : WebClient
        {
            private CookieContainer cookie = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = cookie;
                }
                return request;
            }
        }
    }
}
