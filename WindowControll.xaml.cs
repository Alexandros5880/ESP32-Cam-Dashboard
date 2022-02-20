using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Input;
using VisioForge.Types;
using System.Windows.Media;
using System.Windows.Controls;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace IPCamera
{
    /// <summary>
    /// Interaction logic for WindowControll.xaml
    /// </summary>
    public partial class WindowControll : Window
    {
        public Camera camera;
        public String url = "";
        public bool remote_start_setup = false;
        public bool remote_detection = false;
        public bool remote_recognition = false;
        String cam_resolution_order = "";

        public WindowControll(Camera cam)
        {
            try
            {
                InitializeComponent();
            } catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }

            this.DataContext = this;
            // Setup this_camera
            this.camera = cam;
            this.url = this.camera.url;
            // Add Title
            cameras_title.Content = this.camera.name;
            
            // Chech if Face_Recognition, Face Detection  is checked
            Face_det.IsChecked = (this.camera.Detection);
            Face_rec.IsChecked = (this.camera.Recognition);
            // Setup Brightness and Contrast Labels and Sliders
            brightness_label.Content = $"Brightness: {this.camera.Brightness}";
            contrast_label.Content   = $"Contrast:   {this.camera.Contrast}";
            darkness_label.Content   = $"Darkness:   {this.camera.Darkness}";
            brightness_slider.Value  = this.camera.Brightness;
            contrast_slider.Value    = this.camera.Contrast;
            darkness_slider.Value    = this.camera.Darkness;
            sensitivity_value_label.Content = $"{this.camera.On_move_sensitivity}";
            sensitivity_slider.Value = this.camera.On_move_sensitivity;
            // Setup recording Button
            if (this.camera.Recording)
            {
                rec_label.Content = "Recording";
                rec_label.Foreground = Brushes.Red;
            }
            else
            {
                rec_label.Content = "Stop Recording";
                rec_label.Foreground = Brushes.Gray;
            }
            // Setup On Movement checkboxes
            sms_checkbox.IsChecked = (this.camera.On_move_sms);
            email_checkbox.IsChecked = (this.camera.On_move_email);
            pic_checkbox.IsChecked = (this.camera.On_move_pic);
            rec_checkbox.IsChecked = (this.camera.On_move_rec);
            // Setuo Network streaming Settings
            network_streaming_checkbox.IsChecked = this.camera.net_stream;
            network_streaming_port.Text = Convert.ToString(this.camera.net_stream_port);
            network_streaming_prefix.Text = this.camera.net_stream_prefix;
            // Setup Remotes Cameras Settisng
            if(this.camera.isEsp32)
            {
                update_remote_cameras_status();
            } else
            {
                remote_resolution.IsEnabled = false;
                cameras_quality_slider.IsEnabled = false;
                cameras_brightness_slider.IsEnabled = false;
                cameras_contrast_slider.IsEnabled = false;
                cameras_saturation_slider.IsEnabled = false;
                remote_specialeffect.IsEnabled = false;
                cameras_awb_checkbox.IsEnabled = false;
                cameras_awb_gain_checkbox.IsEnabled = false;
                remote_wb_mode.IsEnabled = false;
                cameras_aec_sensor_checkbox.IsEnabled = false;
                cameras_aec_dsp_checkbox.IsEnabled = false;
                cameras_ae_level_slider.IsEnabled = false;
                cameras_agc_checkbox.IsEnabled = false;
                cameras_gain_ceiling_slider.IsEnabled = false;
                cameras_bpc_checkbox.IsEnabled = false;
                cameras_wpc_checkbox.IsEnabled = false;
                cameras_raw_gma_checkbox.IsEnabled = false;
                cameras_lens_correction_checkbox.IsEnabled = false;
                cameras_h_mirror_checkbox.IsEnabled = false;
                cameras_v_flip_checkbox.IsEnabled = false;
                cameras_dcw_downsize_en_checkbox.IsEnabled = false;
                cameras_color_bar_checkbox.IsEnabled = false;
                cameras_face_detection_checkbox.IsEnabled = false;
                cameras_face_recognition_checkbox.IsEnabled = false;
                cameras_get_still_button.IsEnabled = false;
                cameras_start_stream_button.IsEnabled = false;
                cameras_enroll_face_button.IsEnabled = false;
                cameras_reboot_button.IsEnabled = false;
                cameras_hostpot_button.IsEnabled = false;
            }
        }


        // X Button Clicked
        private void X_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Setup Remotes Cameras Settings
        private void update_remote_cameras_status()
        {
            try
            {
                // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                //Console.WriteLine("Old Url: " + this.url);
                int found = this.url.IndexOf(":81");
                String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                ur_l += "/status?username=" + this.camera.username + "&password=" + this.camera.password;
                Console.WriteLine("New Url: " + ur_l);
                HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode.ToString().Equals("OK"))
                    {
                        Stream ReceiveStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(ReceiveStream);
                        string responseFromServer = reader.ReadToEnd();
                        dynamic data = JObject.Parse(responseFromServer);

                        // Set Remote Rezolution
                        String val = Convert.ToString(data.framesize).Trim();
                        cam_resolution_order = val;
                        switch (val)
                        {
                            case "0": this.remote_resolution.SelectedIndex = 8; break;
                            case "3": this.remote_resolution.SelectedIndex = 7; break;
                            case "4": this.remote_resolution.SelectedIndex = 6; break;
                            case "5": this.remote_resolution.SelectedIndex = 5; break;
                            case "6": this.remote_resolution.SelectedIndex = 4; break;
                            case "7": this.remote_resolution.SelectedIndex = 3; break;
                            case "8": this.remote_resolution.SelectedIndex = 2; break;
                            case "9": this.remote_resolution.SelectedIndex = 1; break;
                            case "10": this.remote_resolution.SelectedIndex = 0; break;
                        };
                        // Set remote Quality 10 / 63
                        val = Convert.ToString(data.quality).Trim();
                        cameras_quality_slider.Value = Convert.ToDouble(val);
                        // Set remote Britness  -2 / 2
                        val = Convert.ToString(data.brightness).Trim();
                        cameras_brightness_slider.Value = Convert.ToDouble(val);
                        // Set remote Contrast  -2 / 2
                        val = Convert.ToString(data.contrast).Trim();
                        cameras_contrast_slider.Value = Convert.ToDouble(val);
                        // Set remote Saturation  -2 / 2
                        val = Convert.ToString(data.saturation).Trim();
                        cameras_saturation_slider.Value = Convert.ToDouble(val);
                        // Set Remote Special Effect
                        val = Convert.ToString(data.special_effect).Trim();
                        switch (val)
                        {
                            case "0": this.remote_specialeffect.SelectedIndex = 0; break;
                            case "1": this.remote_specialeffect.SelectedIndex = 1; break;
                            case "2": this.remote_specialeffect.SelectedIndex = 2; break;
                            case "3": this.remote_specialeffect.SelectedIndex = 3; break;
                            case "4": this.remote_specialeffect.SelectedIndex = 4; break;
                            case "5": this.remote_specialeffect.SelectedIndex = 5; break;
                            case "6": this.remote_specialeffect.SelectedIndex = 6; break;
                        };
                        // Set remote AWB  0/1
                        val = Convert.ToString(data.awb).Trim();
                        cameras_awb_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote AWB_Gain  0/1
                        val = Convert.ToString(data.awb_gain).Trim();
                        cameras_awb_gain_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set Remote Rezolution
                        val = Convert.ToString(data.wb_mode).Trim();
                        switch (val)
                        {
                            case "0": this.remote_wb_mode.SelectedIndex = 0; break;
                            case "1": this.remote_wb_mode.SelectedIndex = 1; break;
                            case "2": this.remote_wb_mode.SelectedIndex = 2; break;
                            case "3": this.remote_wb_mode.SelectedIndex = 3; break;
                            case "4": this.remote_wb_mode.SelectedIndex = 4; break;
                        };
                        // Set remote AEC  0/1
                        val = Convert.ToString(data.aec).Trim();
                        cameras_aec_sensor_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote AEC dsp  0/1
                        val = Convert.ToString(data.aec2).Trim();
                        cameras_aec_dsp_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote AE Level  -2 / 2
                        val = Convert.ToString(data.ae_level).Trim();
                        cameras_ae_level_slider.Value = Convert.ToDouble(val);
                        // Set remote AGC 0/1
                        val = Convert.ToString(data.agc).Trim();
                        cameras_agc_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote Gain Ceiling -2 / 2
                        val = Convert.ToString(data.gainceiling).Trim();
                        cameras_gain_ceiling_slider.Value = Convert.ToDouble(val);
                        // Set remote BPC 0/1
                        val = Convert.ToString(data.bpc).Trim();
                        cameras_bpc_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote WPC 0/1
                        val = Convert.ToString(data.wpc).Trim();
                        cameras_wpc_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote RAW GMA 0/1
                        val = Convert.ToString(data.raw_gma).Trim();
                        cameras_raw_gma_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote LENS Corection 0/1
                        val = Convert.ToString(data.lenc).Trim();
                        cameras_lens_correction_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote H Mirror 0/1
                        val = Convert.ToString(data.hmirror).Trim();
                        cameras_h_mirror_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote H Mirror 0/1
                        val = Convert.ToString(data.vflip).Trim();
                        cameras_v_flip_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set remote DCW 0/1
                        val = Convert.ToString(data.dcw).Trim();
                        cameras_dcw_downsize_en_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set Color Bar 0/1
                        val = Convert.ToString(data.colorbar).Trim();
                        cameras_color_bar_checkbox.IsChecked = (val == "1" ? true : false);
                        // Set Face Detection 0/1
                        val = Convert.ToString(data.face_detect).Trim();
                        cameras_face_detection_checkbox.IsChecked = (val == "1" ? true : false);
                        this.remote_detection = (val == "1" ? true : false);
                        // Set Face Recognition 0/1
                        val = Convert.ToString(data.face_recognize).Trim();
                        cameras_face_recognition_checkbox.IsChecked = (val == "1" ? true : false);
                        this.remote_recognition = (val == "1" ? true : false);
                        remote_start_setup = true;
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            this.camera.camera_oppened = false;
            this.Close();
        }


        // Face Detection Checked
        private void Face_Detection_Chencked(object sender, EventArgs e)
        {
            if (!this.camera.Detection)
            {
                this.camera.Detection = true;
                try
                {
                    // Update DataBase this Camera Object field Face Detection 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.MyCameras SET Face_Detection='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
                // Restart Camera
                this.camera.Stop();
                this.camera.Start();
            }
        }

        // Face Detection Unchecked
        private void Face_Detection_UNChencked(object sender, EventArgs e)
        {
            if (this.camera.Detection)
            {
                this.camera.Detection = false;
                if (this.camera.Recognition)
                {
                    this.camera.Recognition = false;
                    Face_rec.IsChecked = (this.camera.recognition);
                }
                try
                {
                    // Update DataBase this Camera Object field Face Detection 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.MyCameras SET Face_Detection='{0}', Face_Recognition='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
                // Restart Camera
                this.camera.Stop();
                this.camera.Start();
            }
        }

        // Face Recognition Chekced
        private void Face_Recognition_Chencked(object sender, EventArgs e)
        {
            if (!this.camera.Recognition)
            {
                this.camera.Recognition = true;
                if (!this.camera.Detection)
                {
                    this.camera.Detection = true;
                    Face_det.IsChecked = (this.camera.detection);
                }
                try
                {
                    // Update DataBase this Camera Object field Face Detection 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.MyCameras SET Face_Recognition='{1}', Face_Detection='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
                // Restart Camera
                this.camera.Stop();
                this.camera.Start();
            }
        }

        // Face Recognition Unchecked
        private void Face_Recognition_UNChencked(object sender, EventArgs e)
        {
            if (this.camera.Recognition)
            {
                this.camera.Recognition = false;
                try
                {
                    // Update DataBase this Camera Object field Face Detection 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.MyCameras SET Face_Recognition='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
                // Restart Camera
                this.camera.Stop();
                this.camera.Start();
            }
        }


        // Britness slider function
        private void Brightness_func(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            brightness_label.Content = $"Brightness: {val}";
            this.camera.Brightness = val;
            // Save data to Database
            try
            {
                // Update DataBase this Camera Object field Face Detection 1
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"UPDATE dbo.MyCameras SET Brightness='{val}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }


        // Contrast slider function
        private void Contrast_func(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            contrast_label.Content = $"Contrast: {val}";
            this.camera.Contrast = val;
            // Save data to Database
            try
            {
                // Update DataBase this Camera Object field Face Detection 1
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"UPDATE dbo.MyCameras SET Contrast='{val}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }

        // Darkness slider function
        private void Darkness_func(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = Convert.ToInt32(e.NewValue);
            darkness_label.Content = $"Darkness: {val}";
            this.camera.Darkness = val;
            // Save data to Database
            try
            {
                // Update DataBase this Camera Object field Face Detection 1
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"UPDATE dbo.MyCameras SET Darkness='{val}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }

        // UP, DOWN, LEFT,RIGHT use Http request api to controll he camera
        private void GET_request(String url)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    String result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
                client.Dispose();
            }
            catch(System.InvalidOperationException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }

        private void UP_button_click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse UP!");
            GET_request("URL_get rest UP");
        }

        private void DOWN_button_click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse DOWN!");
            GET_request("URL_get rest DOWN");
        }

        private void LEFT_button_click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse LEFT!");
            GET_request("URL_get rest LEFT");
        }

        private void RIGHT_button_click(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Mouse RIGHT!");
            GET_request("URL_get rest RIGHT");
        }

        private void TAKE_PIC_button_click(object sender, MouseButtonEventArgs e)
        {
            this.camera.Take_pic();
        }

        // Start Recording is checked
        private void Start_REC_button_click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!this.camera.Recording)
                {
                    this.camera.Recording = true;
                    rec_label.Content = "Recording";
                    rec_label.Foreground = Brushes.Red;
                    // Update DataBase this Camera Object field Recording 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET Recording='{true}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Stop Recording is Checked
        private void Stop_REC_button_click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (this.camera.Recording)
                {
                    this.camera.Recording = false;
                    rec_label.Content = "Stop Recording";
                    rec_label.Foreground = Brushes.Gray;
                    // Update DataBase this Camera Object field Recording 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET Recording='{false}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }



        // SMS CheckBoxes
        private void Sms_chencked(object sender, EventArgs e)
        {
            try
            {
                if (!this.camera.On_move_sms)
                {
                    this.camera.On_move_sms = true;
                    // Update DataBase this Camera Object field On_Move_SMS 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_SMS='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }
        private void Sms_unchencked(object sender, EventArgs e)
        {
            try
            {
                if (this.camera.On_move_sms)
                {
                    this.camera.On_move_sms = false;
                    // Update DataBase this Camera Object field On_Move_SMS 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_SMS='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Email CheckBoxes
        private void Email_chencked(object sender, EventArgs e)
        {
            try
            {
                if (!this.camera.On_move_email)
                {
                    this.camera.On_move_email = true;
                    // Update DataBase this Camera Object field On_Move_EMAIL 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_EMAIL='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }
        private void Email_unchencked(object sender, EventArgs e)
        {
            try
            {
                if (this.camera.On_move_email)
                {
                    this.camera.On_move_email = false;
                    // Update DataBase this Camera Object field On_Move_EMAIL 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_EMAIL='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Picture Checkbox
        private void Pic_chencked(object sender, EventArgs e)
        {
            try
            {
                if (!this.camera.On_move_pic)
                {
                    this.camera.On_move_pic = true;
                    // Update DataBase this Camera Object field On_Move_Pic 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_Pic='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }
        private void Pic_unchencked(object sender, EventArgs e)
        {
            try
            {
                if (this.camera.On_move_pic)
                {
                    this.camera.On_move_pic = false;
                    // Update DataBase this Camera Object field On_Move_Pic 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_Pic='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Recording Checkbox
        private void Rec_chencked(object sender, EventArgs e)
        {
            try
            {
                if (!this.camera.On_move_rec)
                {
                    this.camera.On_move_rec = true;
                    // Update DataBase this Camera Object field On_Move_Rec 1
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_Rec='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }
        private void Rec_unchencked(object sender, EventArgs e)
        {
            try
            {
                if (this.camera.On_move_rec)
                {
                    this.camera.On_move_rec = false;
                    // Update DataBase this Camera Object field On_Move_Rec 0
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET On_Move_Rec='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                    // Restart Camera
                    this.camera.Stop();
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Set The Sensitivity
        private void Sensitivity_func(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                // Update Cameras Move_Sensitivity
                int val = Convert.ToInt32(e.NewValue);
                sensitivity_value_label.Content = $"{val}";
                if (this.camera != null)
                {
                    this.camera.On_move_sensitivity = val;
                    Console.WriteLine(this.camera.On_move_sensitivity.ToString());
                    // Update DataBases Move_Sensitivity
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET Move_Sensitivity='{this.camera.On_move_sensitivity}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
            catch (System.NullReferenceException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
            }
        }

        // Remote Camera Resolution
        private void Resolution_combobox_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    ComboBox cmb = sender as ComboBox;
                    String selection = cmb.SelectedValue.ToString();
                    if (selection.Contains("QQVGA(160X120)"))
                    {
                        cam_resolution_order = "0";
                    } else if (selection.Contains("HQVGA(240X176)"))
                    {
                        cam_resolution_order = "3";
                    }
                    else if (selection.Contains("QVGA(320X240)"))
                    {
                        cam_resolution_order = "4";
                    }
                    else if (selection.Contains("CIF(400X296)"))
                    {
                        cam_resolution_order = "5";
                    }
                    else if (selection.Contains("VGA(640X480)"))
                    {
                        cam_resolution_order = "6";
                    }
                    else if (selection.Contains("SVGA(800X600)"))
                    {
                        cam_resolution_order = "7";
                    }
                    else if (selection.Contains("XGA(1024X768)"))
                    {
                        cam_resolution_order = "8";
                    }
                    else if (selection.Contains("SXGA(1280X1024)"))
                    {
                        cam_resolution_order = "9";
                    }
                    else if (selection.Contains("UXGA(1600X1200)"))
                    {
                        cam_resolution_order = "10";
                    }
                    if (!selection.Contains("Change"))
                    {
                        try
                        {
                            // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                            // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                            //Console.WriteLine("Old Url: " + this.url);
                            int found = this.url.IndexOf(":81");
                            String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                            ur_l += "/control?var=framesize&val=" + cam_resolution_order;
                            //Console.WriteLine("New Url: " + ur_l);
                            HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                            request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode.ToString().Equals("OK"))
                                {
                                    Thread.Sleep(5000);
                                    this.camera.Start();
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        
        // When Cameras Quality Changed
        private void Quality_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt16(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=quality&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remote Cameras Brightness
        private void Brightness_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt16(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=brightness&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remote Cameras Contrast
        private void Contrast_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt16(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=contrast&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remote Cameras Saturasion
        private void Saturation_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt32(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=saturation&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remote Camera Specialeffect
        private void Specialeffect_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    ComboBox cmb = sender as ComboBox;
                    String selection = cmb.SelectedValue.ToString();
                    String order = "";
                    if (selection.Contains("No Effect"))
                    {
                        order = "0";
                    }
                    else if (selection.Contains("Negative"))
                    {
                        order = "1";
                    }
                    else if (selection.Contains("Grayscale"))
                    {
                        order = "2";
                    }
                    else if (selection.Contains("Red Tint"))
                    {
                        order = "3";
                    }
                    else if (selection.Contains("Green Tint"))
                    {
                        order = "4";
                    }
                    else if (selection.Contains("Blue Tint"))
                    {
                        order = "5";
                    }
                    else if (selection.Contains("Sepia"))
                    {
                        order = "6";
                    }
                    try
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=special_effect&val=" + order;
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remotes Camera AWB
        private void AWB_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=awb&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=awb&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remotes Camera AWB GAIN
        private void AWB_GAIN_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=awb_gain&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=awb_gain&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remotes Camera WB Mode
        private void WB_MODE_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    ComboBox cmb = sender as ComboBox;
                    String selection = cmb.SelectedValue.ToString();
                    String order = "";
                    if (selection.Contains("Auto"))
                    {
                        order = "0";
                    }
                    else if (selection.Contains("Sunny"))
                    {
                        order = "1";
                    }
                    else if (selection.Contains("Cloudy"))
                    {
                        order = "2";
                    }
                    else if (selection.Contains("Office"))
                    {
                        order = "3";
                    }
                    else if (selection.Contains("Home"))
                    {
                        order = "4";
                    }
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                    //Console.WriteLine("Old Url: " + this.url);
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=wb_mode&val=" + order;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remotes Camera AEC Sensor
        private void AEC_SENSOR_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=aec&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=aec&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Remotes Camera AEC DSP SensorSensor
        private void AEC_DSP_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=aec2&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=aec2&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // AE LEVEL Changed
        private void AE_LEVEL_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt32(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=ae_level&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // AGC Checked
        private void AGC_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=agc&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=agc&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // GAIN CEILING Changed
        private void GAIN_CEILINGL_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int val = Convert.ToInt32(e.NewValue);
                if (remote_start_setup)
                {
                    this.camera.Stop();
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // http://192.168.1.50/control?var=quality&val=10
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=agc_gain&val=" + val;
                    //Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // BPC Changed
        private void BPC_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=bpc&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=bpc&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // WPC Changed
        private void WPC_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=wpc&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=wpc&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // RAW GMA Changed
        private void RAW_GMA_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=raw_gma&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=raw_gma&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        // LENS CORRECTION Changed
        private void LENS_CORRECTION_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=lenc&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=lenc&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        // H MIRROR Changed
        private void H_MIRROR_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=hmirror&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=hmirror&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        // V FLIP Changed
        private void V_FLIP_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=vflip&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=vflip&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        // DCW DOWNSIZE Changed
        private void DCW_DOWNSIZE_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=dcw&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=dcw&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        //  COLOR BAR Changed
        private void COLOR_BAR_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (c.IsChecked.Value)
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=colorbar&val=1";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                    else
                    {
                        this.camera.Stop();
                        // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                        // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                        //Console.WriteLine("Old Url: " + this.url);
                        int found = this.url.IndexOf(":81");
                        String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                        ur_l += "/control?var=colorbar&val=0";
                        //Console.WriteLine("New Url: " + ur_l);
                        HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                        request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                                this.camera.Start();
                                //MainWindow.RestartApp();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        //  FACE DETECTION Changed
        private void FACE_DETECTION_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    if (Convert.ToInt16(cam_resolution_order) <= 5)
                    {
                        CheckBox c = sender as CheckBox;
                        if (c.IsChecked.Value)
                        {
                            this.camera.Stop();
                            // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                            // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                            //Console.WriteLine("Old Url: " + this.url);
                            int found = this.url.IndexOf(":81");
                            String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                            ur_l += "/control?var=face_detect&val=1";
                            //Console.WriteLine("New Url: " + ur_l);
                            HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                            request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode.ToString().Equals("OK"))
                                {
                                    this.remote_detection = true;
                                    this.camera.Start();
                                    //MainWindow.RestartApp();
                                }
                            }
                        }
                        else
                        {
                            this.camera.Stop();
                            // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                            // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                            //Console.WriteLine("Old Url: " + this.url);
                            int found = this.url.IndexOf(":81");
                            String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                            ur_l += "/control?var=face_detect&val=0";
                            //Console.WriteLine("New Url: " + ur_l);
                            HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                            request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode.ToString().Equals("OK"))
                                {
                                    this.remote_detection = false;
                                    this.remote_recognition = false;
                                    this.camera.Start();
                                    //MainWindow.RestartApp();
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cameras Resolution lowest from 680x480");
                        cameras_face_detection_checkbox.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        //  FACE RECOGNITION Changed
        private void FACE_RECOGNITION_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (remote_start_setup)
                {
                    CheckBox c = sender as CheckBox;
                    if (this.remote_detection)
                    {
                        if (c.IsChecked.Value)
                        {
                            this.camera.Stop();
                            // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                            // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                            //Console.WriteLine("Old Url: " + this.url);
                            int found = this.url.IndexOf(":81");
                            String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                            ur_l += "/control?var=face_recognize&val=1";
                            //Console.WriteLine("New Url: " + ur_l);
                            HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                            request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode.ToString().Equals("OK"))
                                {
                                    this.remote_recognition = true;
                                    this.camera.Start();
                                    //MainWindow.RestartApp();
                                }
                            }
                        }
                        else
                        {
                            this.camera.Stop();
                            // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                            // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                            //Console.WriteLine("Old Url: " + this.url);
                            int found = this.url.IndexOf(":81");
                            String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                            ur_l += "/control?var=face_recognize&val=0";
                            //Console.WriteLine("New Url: " + ur_l);
                            HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                            request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode.ToString().Equals("OK"))
                                {
                                    this.remote_recognition = false;
                                    this.camera.Start();
                                    //MainWindow.RestartApp();
                                }
                            }
                        }
                    }
                    else
                    {
                        c.IsChecked = false;
                        MessageBox.Show("Face Detection is disabled");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Button Get Still Clicked
        private void GET_STILL_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create Random a number with 13 digits
                Random ran = new Random();
                int num_1 = ran.Next(0000000, 10000000);
                int num_2 = ran.Next(000000, 10000000);
                String val = num_1.ToString() + num_2.ToString();
                // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                //Console.WriteLine("Old Url: " + this.url);
                int found = this.url.IndexOf(":81");
                String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                ur_l += "/capture?_cb=0";
                //Console.WriteLine("New Url: " + ur_l);
                HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode.ToString().Equals("OK"))
                    {
                        this.camera.Start();
                        //MainWindow.RestartApp();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Button RESTART Clicked
        private void RESTART_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.camera.Stop();
                // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                //Console.WriteLine("Old Url: " + this.url);
                int found = this.url.IndexOf(":81");
                String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                ur_l += "/restart?username=" + this.camera.username + "&password=" + this.camera.password;
                Console.WriteLine("New Url: " + ur_l);
                HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            Thread.Sleep(5000);
                            ((CheckBox)cameras_face_detection_checkbox).IsChecked = false;
                            this.remote_detection = false;
                            ((CheckBox)cameras_face_recognition_checkbox).IsChecked = false;
                            this.remote_recognition = false;
                            this.camera.Start();
                            //MainWindow.RestartApp();
                        }
                    }
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                    this.camera.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Button Enroll Face Clicked (Save a face on DB)
        private void ENROLL_FACE_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.remote_recognition)
                {
                    // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                    // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                    //Console.WriteLine("Old Url: " + this.url);
                    int found = this.url.IndexOf(":81");
                    String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                    ur_l += "/control?var=face_enroll&val=1";
                    Console.WriteLine("New Url: " + ur_l);
                    HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                    request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                    try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response.StatusCode.ToString().Equals("OK"))
                            {
                            }
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                    }
                } else
                {
                    MessageBox.Show("Face Recognition is disabled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Button REBOOT Clicked
        private void Reboot_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.camera.Stop();
                // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                //Console.WriteLine("Old Url: " + this.url);
                int found = this.url.IndexOf(":81");
                String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                ur_l += "/reboot?username=" + this.camera.username + "&password=" + this.camera.password;
                Console.WriteLine("New Url: " + ur_l);
                HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                        
                        }
                    }
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Button HostPot Clicked
        private void Hostpot_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.camera.Stop();
                // Url now = http://192.168.1.50:81/stream?username=alexandrosplatanios&password=Platanios719791
                // Expected Url = http://192.168.1.50/control?var=framesize&val=0
                //Console.WriteLine("Old Url: " + this.url);
                int found = this.url.IndexOf(":81");
                String ur_l = this.url.Substring(0, found); // = http://192.168.1.50/
                ur_l += "/hostpot?username=" + this.camera.username + "&password=" + this.camera.password;
                Console.WriteLine("New Url: " + ur_l);
                HttpWebRequest request = WebRequest.CreateHttp(ur_l);
                request.Method = "GET"; // or "POST", "PUT", "PATCH", "DELETE", etc.
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                        
                        }
                    }
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }


        // Helper Function For Network streaming
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Network Streaming CheckBox
        private void Network_stream_check(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox c = sender as CheckBox;
                if (c.IsChecked.Value)
                {
                    if (network_streaming_port.Text.Length > 0)
                    {
                        this.camera.Net_stream = true;
                        // Update DataBase this Camera
                        SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                        String query = $"UPDATE dbo.myCameras SET net_stream='{1}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cn.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                        cn.Close();
                    } else
                    {
                        MessageBox.Show("Fill ip & port!");
                        c.IsChecked = false;
                    }
                }
                else
                {
                    this.camera.Net_stream = false;
                    // Update DataBase this Camera
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = $"UPDATE dbo.myCameras SET net_stream='{0}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Network Streaming Port
        private void network_streaming_port_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox n = sender as TextBox;
                this.camera.Net_stream_port = (String)n.Text;
                // Update DataBase this Camera
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"UPDATE dbo.myCameras SET net_stream_port='{(String)n.Text}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Network Streaming prefix
        private void network_streaming_prefix_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox n = sender as TextBox;
                this.camera.net_stream_prefix = (String)n.Text;
                // Update DataBase this Camera
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"UPDATE dbo.myCameras SET net_stream_prefix='{(String)n.Text}' WHERE urls='{this.camera.url}' AND Name='{this.camera.name}'";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
            }
        }

        // Start Button Clicked (Starting camera)
        private void Start_Clicked(object sender, RoutedEventArgs e)
        {
            this.camera.Start();
        }

        // Stop Button Clicked (Stoping camera)
        private void Stop_Clicked(object sender, RoutedEventArgs e)
        {
            this.camera.Stop();
        }

        
    }

        
    }
