using Colorful;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int DA = 244;
            int V = 212;
            int ID = 255;
            Console.WriteAscii("Test: Web Server...", Color.FromArgb(DA, V, ID));
                       
            var server = new WebServer("http://*:51111/", @"C:\D\Workspace\webroot");
            try
            {
                server.Start();

                Console.WriteLine("Server running... press Enter to stop", Color.YellowGreen);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.OrangeRed);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("Server shutting down...", Color.OrangeRed);
            Thread.Sleep(2000);
        }

    }

    class WebServer
    {
        #region Fields
        private HttpListener _listener;
        private string _basefolder;

        private ColorAlternator mainAlternator;
        private ColorAlternatorFactory alternatorFactory;
        #endregion

        public WebServer(string uriPrefix, string baseFolder)
        {
            this._listener = new HttpListener();
            this._listener.Prefixes.Add(uriPrefix);
            this._basefolder = baseFolder;

            this.alternatorFactory = new ColorAlternatorFactory();
            this.mainAlternator = alternatorFactory.GetAlternator(2, Color.Plum, Color.PaleVioletRed);
        }

        public async void Start()
        {
            try
            {
                this._listener.Start();
                Console.WriteLine();
                Console.WriteLine("Server started", Color.GreenYellow);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, Color.OrangeRed);
                return;
            }
            

            while (true)
            {
                try
                {
                    var context = await this._listener.GetContextAsync();
                    Console.WriteLine("");
                    Console.WriteLine("New Request to " + context.Request.Url, Color.CadetBlue);
                    Console.WriteLine("From " + context.Request.UserHostAddress + " (" /*+ Dns.GetHostEntry(context.Request.UserHostAddress).HostName*/ + ")", Color.CadetBlue);
                    _ = Task.Run(() => this.ProcessRequestAsync(context));
                }
                catch (HttpListenerException ex)
                {
                    this.mainAlternator = alternatorFactory.GetAlternator(2, Color.Red, Color.IndianRed);
                    Console.WriteLineAlternating(ex.Message, this.mainAlternator);
                    this.RestoreDefaultConsoleAlternating();
                    break;
                }
                catch (InvalidOperationException ex)
                {
                    this.mainAlternator = alternatorFactory.GetAlternator(2, Color.Red, Color.IndianRed);
                    Console.WriteLineAlternating(ex.Message, this.mainAlternator);
                    this.RestoreDefaultConsoleAlternating();
                    break;
                }
            }
        }

        public void Stop()
        {
            this._listener.Stop();
            Console.WriteLine();
            Console.WriteLine("Server stoped", Color.OrangeRed);
            Console.WriteLine();
        }

        
        #region private methods
        private void RestoreDefaultConsoleAlternating()
        {
            this.mainAlternator = alternatorFactory.GetAlternator(2, Color.Plum, Color.PaleVioletRed);
        }

        private async void ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                string filename = Path.GetFileName(context.Request.RawUrl);
                string path = Path.Combine(this._basefolder, filename);
                byte[] msg;

                if (!File.Exists(path))
                {
                    this.mainAlternator = alternatorFactory.GetAlternator(2, Color.Orange, Color.OrangeRed);
                    Console.WriteLineAlternating("Resource not found: " + path, this.mainAlternator);
                    this.RestoreDefaultConsoleAlternating();

                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    msg = Encoding.UTF8.GetBytes("Sorry, that page does not exists");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    msg = File.ReadAllBytes(path);

                    this.mainAlternator = alternatorFactory.GetAlternator(2, Color.GreenYellow, Color.Green);
                    Console.WriteLineAlternating("Resource sended: " + path, this.mainAlternator);
                    this.RestoreDefaultConsoleAlternating();
                }

                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                {
                    await s.WriteAsync(msg, 0, msg.Length);
                }
            }
            catch (Exception ex)
            {
                this.mainAlternator = alternatorFactory.GetAlternator(2, Color.MediumVioletRed, Color.OrangeRed);
                Console.WriteLineAlternating("Request error: " + ex.Message, this.mainAlternator);
                this.RestoreDefaultConsoleAlternating();
            }
        }
        #endregion
    }
}
