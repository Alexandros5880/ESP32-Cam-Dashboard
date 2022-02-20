using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IPCamera
{


    public class HttpServer
    {
        private string[] prefixes;
        private HttpListener listener;
        public bool run;
        public String ip = "";
        public String port = "";
        public String prefix = "";
        public Camera cam;

        public HttpServer()
        {
            // Create a listener.
            this.listener = new HttpListener();
        }
        
        public void setup()
        {
            if (!this.ip.Equals("") && !this.port.Equals(""))
            {
                Console.WriteLine("Listener Setup OK.");
                String url = $"http://{ip}:{port}/{prefix}/";
                Console.WriteLine(url);
                this.prefixes = new string[] { url };
                if (!HttpListener.IsSupported)
                {
                    Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                    return;
                }
                // URI prefixes are required,
                // for example "http://contoso.com:8080/index/".
                if (prefixes == null || prefixes.Length == 0)
                    throw new ArgumentException("prefixes");
                // Add the prefixes.
                foreach (string s in prefixes)
                {
                    try
                    {
                        this.listener.Prefixes.Add(s);
                    } catch (System.Net.HttpListenerException ex)
                    {
                        Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                    }
                }
                this.listener.Start();
                Console.WriteLine("Listening...");
            } else
            {
                Console.WriteLine("Listener Setup NOT OK.");
            }
        }

        public async Task ListenAsync()
        {
            
            while (this.run)
            {
                try
                {
                    var context = await this.listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;


                    // Get Frame Buffer
                    System.Windows.Media.Imaging.BitmapSource bitmapsource = this.cam.video.Frame_GetCurrent();
                    MemoryStream outStream = new MemoryStream();
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                    enc.Save(outStream);
                    byte[] frame = outStream.GetBuffer();


                    // Send Frames
                    response.StatusCode = 200;
                    response.ContentLength64 = frame.Length;
                    response.ContentType = "image/jpeg";
                    response.KeepAlive = true;
                    response.Headers.Add("Refresh", "0");
                    response.OutputStream.Write(frame, 0, frame.Length);
                    response.OutputStream.Close();



                    /*
                    // Create Buffer With HTML
                    string responseString = "<HTML><HEAD><meta http-equiv='refresh' content='0'" +
                        "></HEAD><BODY>" +
                        "Hello World" +
                        "</BODY></HTML>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    // Send Buffer
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                    */

                    //enc.Frames.Clear();
                    //response.Headers.Clear();
                } catch (Exception ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
            }
            
        }

        public void close()
        {
            this.run = false;
            //this.listener.Prefixes.Clear();
            Console.WriteLine("Stop Listener.");
            this.listener.Stop();
            this.listener.Close();
            this.listener = new HttpListener();
        }

    }

}
