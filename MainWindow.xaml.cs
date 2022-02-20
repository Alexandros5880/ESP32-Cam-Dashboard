using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisioForge.Types.OutputFormat;
using VisioForge.Types.VideoEffects;
using VisioForge.Controls.UI.WPF;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace IPCamera
{

    public partial class MainWindow : Window
    {

        public static MainWindow main_window;
        public static List<Camera> cameras = new List<Camera>();
        public static List<Users> myUsers = new List<Users>();
        public static Users user;
        public static Grid cams_grid;
        public static String email_send;
        public static String pass_send;
        public Login login;
        public static bool logged = false;
        public static bool Logged
        {
            set
            {
                MainWindow.logged = value;
                // Save the logged User
                if (MainWindow.logged)
                {
                    try
                    {
                        // Save to DataBase Pictures
                        using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                        {
                            String query = $"INSERT INTO dbo.Logged (Id) VALUES (@user)";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@user", MainWindow.user.Email);
                                connection.Open();
                                int result = command.ExecuteNonQuery();
                                // Check Error
                                if (result < 0)
                                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                            }
                        }
                    }
                    catch( Exception ex)
                    {
                        MessageBox.Show($"MainWindows.Logged = True Error: \n{ex.Message}");
                    }
                    
                }
                else
                {
                    // Clear DataBase
                    try
                    {
                        SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                        String query = "DELETE FROM dbo.Logged";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cn.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                        cn.Close();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
                    }
                }
            }
            get { return MainWindow.logged; }
        }
        private Settings settings;
        public static bool settings_oppened = false;
        public static bool login_oppened = false;
        public static String twilioNumber;
        public static String twilioAccountSID;
        public static String twilioAccountToken;
        

        public MainWindow()
        {
            try
            {
                Console.WriteLine("First Step");
                // Runs only one time and install some requarements
                if (Install_Requarements.First_time_runs)
                {
                    Console.WriteLine("Second Step");
                    try
                    {
                        // Create Database Connection String
                        string db_file_path = $"{Install_Requarements.GetRootDir()}\\Database1.mdf";
                        Console.WriteLine($"DB Dir: {db_file_path}");
                        Camera.DB_connection_string = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={db_file_path};Integrated Security=True";
                        Console.WriteLine($"Database Connation String: {Camera.DB_connection_string}");
                        //MessageBox.Show($"DB Dir: {db_file_path}\n\nDatabase Connation String: {Camera.DB_connection_string}");
                        // Install Requarements
                        Install_Requarements.Install_Req();
                        try
                        {
                            // Create an Admin User
                            String query = $"INSERT INTO dbo.Users (FirstName, LastName, Email, Phone, Licences, Password)" +
                                                                    $" VALUES (@fname, @lname, @email, @phone, @licences, @pass)";
                            using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                            {
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@fname", "admin");
                                    command.Parameters.AddWithValue("@lname", "admin");
                                    command.Parameters.AddWithValue("@email", "admin@admin.com");
                                    command.Parameters.AddWithValue("@phone", "");
                                    command.Parameters.AddWithValue("@licences", "Admin");
                                    command.Parameters.AddWithValue("@pass", "1234");
                                    connection.Open();
                                    int result = command.ExecuteNonQuery();
                                    // Check Error
                                    if (result < 0)
                                    {
                                        Console.WriteLine("Error inserting Admin into Database!");
                                    }
                                }
                                connection.Close();
                            }
                        }
                        catch (System.Data.SqlClient.SqlException ex)
                        {
                            Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
                        }
                        // Application Varaible to false this code won't runs again
                        Install_Requarements.First_time_runs = false;
                    }
                    catch (Exception ex)
                    {
                        // Application Varaible to false this code won't runs again
                        Install_Requarements.First_time_runs = true;
                        Console.WriteLine($"\n\nSource:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}\n\n");
                        Thread.Sleep(5000);
                    }
                }
                Console.WriteLine("Thrierd Step");
                //MessageBox.Show($"Continue ???");

                // Initialize Main Window
                try
                {
                    InitializeComponent();
                }
                catch (System.Windows.Markup.XamlParseException ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                    Thread.Sleep(5000);
                }


                // Set a Hundeler for this main window
                main_window = this;
                // Setup login logout button for start
                login_logout_b.Click += (object sender, RoutedEventArgs e) =>
                {
                    this.Loggin_clicked();
                };
                // Handler to cameras grid
                cams_grid = cameras_grid;
                // Update Urls From Database
                UpdatesFromDB();
                // Open he Cameras Windows
                CreateVideosPage();

                //  DispatcherTimer setup (Thread Excecutes date update every 1 second)
                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
            }
            catch (System.IO.FileLoadException ex)
            {
                Console.WriteLine($"\n\n[ERROR] MainWindow\n\n");
                Thread.Sleep(5000);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nStackTrace:{ex.StackTrace}\n{ex.Message}");
                Thread.Sleep(5000);
            }
        }

        // Set DateTime
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current time 
            date.Content = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        // Restart Application
        public static void RestartApp()
        {
            MainWindow old_win = main_window;
            System.Windows.Forms.Application.Restart();
            old_win.Close();
        }


        // Get The saved Cameras From Database
        public void UpdatesFromDB()
        {
            cameras.Clear();
            // Get Data From DB
            using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
            {
                // Get Files Paths Data
                String query = "SELECT Name, Path FROM dbo.FilesDirs";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        String name = dataReader["Name"].ToString().Trim();
                        String path = dataReader["Path"].ToString().Trim();
                        if (name == "Pictures")
                        {
                            Camera.pictures_dir = path;
                        }
                        if (name == "Videos")
                        {
                            Camera.videos_dir = path;
                        }
                    }
                }
                connection.Close();

                // Get  Files Format Data
                query = "SELECT avi, mp4, webm FROM dbo.FilesFormats";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        // String avi = dataReader["avi"].ToString().Trim();
                        //String mp4 = dataReader["mp4"].ToString().Trim();
                        //String webm = dataReader["webm"].ToString().Trim();
                        Camera.avi_format = (dataReader["avi"].ToString().Trim() == "True");
                        Camera.mp4_format = (dataReader["mp4"].ToString().Trim() == "True");
                        Camera.webm_format = (dataReader["webm"].ToString().Trim() == "True");
                    }
                }
                connection.Close();

                // Get Cameras Data
                query = "SELECT * FROM dbo.myCameras";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        String id = dataReader["id"].ToString().Trim();
                        String url = dataReader["urls"].ToString().Trim();
                        String name = dataReader["name"].ToString().Trim();
                        String username = dataReader["username"].ToString().Trim();
                        String password = dataReader["password"].ToString().Trim();
                        String detection = dataReader["Face_Detection"].ToString().Trim();
                        String recognition = dataReader["Face_Recognition"].ToString().Trim();
                        String recording = dataReader["Recording"].ToString().Trim();
                        String on_move_sms = dataReader["On_Move_SMS"].ToString().Trim();
                        String on_move_email = dataReader["On_Move_EMAIL"].ToString().Trim();
                        String on_move_pic = dataReader["On_Move_Pic"].ToString().Trim();
                        String on_move_rec = dataReader["On_Move_Rec"].ToString().Trim();
                        
                        String up = dataReader["Up_req"].ToString().Trim();
                        String down = dataReader["Down_req"].ToString().Trim();
                        String right = dataReader["Right_req"].ToString().Trim();
                        String left = dataReader["Left_req"].ToString().Trim();
                        if (up.Equals("NULL"))
                        {
                            up = "";
                        }
                        if (down.Equals("NULL"))
                        {
                            down = "";
                        }
                        if (right.Equals("NULL"))
                        {
                            right = "";
                        }
                        if (left.Equals("NULL"))
                        {
                            left = "";
                        }

                        int brightness = (int)dataReader["Brightness"];
                        int contrast = (int)dataReader["Contrast"];
                        int darkness = (int)dataReader["Darkness"];
                        int move_sensitivity = (int)dataReader["Move_Sensitivity"];

                        String net_stream_port_l = (String)dataReader["net_stream_port"].ToString().Trim();
                        String net_stream_prefix_l = (String)dataReader["net_stream_prefix"].ToString().Trim();
                        String net_stream_l = (String)dataReader["net_stream"].ToString().Trim();

                        bool isEsp = (dataReader["isEsp32"].ToString().Trim() == "True");
                        try
                        {
                            bool rec = (recording == "True");
                            Camera cam = new Camera(url, name, id, rec)
                            {
                                Username = username,
                                Password = password,
                                Brightness = brightness,
                                Contrast = contrast,
                                Darkness = darkness,
                                Detection = (detection == "True"),
                                Recognition = (recognition == "True"),
                                On_move_sms = (on_move_sms == "True"),
                                On_move_email = (on_move_email == "True"),
                                On_move_pic = (on_move_pic == "True"),
                                On_move_rec = (on_move_rec == "True"),
                                On_move_sensitivity = move_sensitivity,
                                up_req = up,
                                down_req = down,
                                right_req = right,
                                left_req = left,
                                net_stream_port = net_stream_port_l,
                                net_stream_prefix = net_stream_prefix_l,
                                Net_stream = (net_stream_l == "True"),
                                isEsp32 = isEsp
                            };
                            MainWindow.cameras.Add(cam);
                        }
                        catch (System.ArgumentException ex)
                        {
                            Console.WriteLine($"Source:{ex.Source}\nParamName:{ex.ParamName}\n{ex.Message}");
                        }
                    }
                }
                connection.Close();

                // Get Users Data
                myUsers.Clear();
                query = "SELECT Id, FirstName, LastName, Email, Phone, Licences, Password FROM dbo.Users";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        int id = (int)dataReader["Id"];
                        String fname = dataReader["FirstName"].ToString().Trim();
                        String lname = dataReader["LastName"].ToString().Trim();
                        String email = dataReader["Email"].ToString().Trim();
                        String phone = dataReader["Phone"].ToString().Trim();
                        String licences = dataReader["Licences"].ToString().Trim();
                        String pass = dataReader["Password"].ToString().Trim();
                        // Create The Usres Objects
                        Users user = new Users(id, fname, lname, email, phone, licences, pass);
                        MainWindow.myUsers.Add(user);
                    }
                }
                connection.Close();

                // Get Email_send Pass_send
                query = "SELECT Email, Pass FROM dbo.EmailSender";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        email_send = dataReader["Email"].ToString().Trim();
                        pass_send = dataReader["Pass"].ToString().Trim();
                    }
                }
                connection.Close();

                // Get SMS SID, SMS TOKEN, SMS PHONE
                query = "SELECT AccountSID,AccountTOKEN,Phone FROM dbo.SMS";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        twilioAccountSID = dataReader["AccountSID"].ToString().Trim();
                        twilioAccountToken = dataReader["AccountTOKEN"].ToString().Trim();
                        twilioNumber = dataReader["Phone"].ToString().Trim();
                    }
                }
                connection.Close();

                // Get Logged User If Existes
                query = "SELECT Id FROM dbo.Logged";
                String user_email = "";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        user_email = dataReader["Id"].ToString().Trim();
                    }
                }
                connection.Close();
                try
                {
                    var u = from user in MainWindow.myUsers where user.Email.Equals(user_email) select user;
                    MainWindow.user = u.Single();
                    MainWindow.logged = true;
                    MainWindow.main_window.login_logout_b.Content = "Logout";
                    MainWindow.main_window.login_logout_b.Click += (object send, RoutedEventArgs ev) =>
                    {
                        MainWindow.main_window.Loggout_clicked();
                    };
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Source:{ex.Source}\n{ex.Message}");
                }
                
            }
        }

        // Loggin Button Click
        public void Loggin_clicked()
        {
            if (login_oppened == false)
            {
                login_oppened = true;
                this.login = new Login();
                this.login.Show();
            }
            else
            {
                this.login.Activate();
            }
        }

        // Loggout Button Click
        public void Loggout_clicked()
        {
            MainWindow.user = null;
            MainWindow.Logged = false;
            MainWindow.main_window.login_logout_b.Content = "Login";
            login_logout_b.Click += (object sender, RoutedEventArgs e) =>
            {
                this.Loggin_clicked();
            };
        }

        // When Click Start Button
        private void Start_clicked(object sender, RoutedEventArgs e)
        {
            foreach (Camera cam in cameras)
            {
                Console.WriteLine("Starting: " + cam.url);
                cam.Start();
            }
        }

        // When Clecked Stop Button
        private void Stop_clicked(object sender, RoutedEventArgs e)
        {
            foreach (Camera cam in cameras)
            {
                Console.WriteLine("Stoping: " + cam.url);
                cam.Stop();
            }
        }

        // On Close Button
        protected override void OnClosed(EventArgs e)
        {
            foreach (Camera cam in cameras)
            {
                cam.Stop();
            }
            this.Close();
        }

        // When Click Settings Button
        private void Settings_clicked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Logged && MainWindow.myUsers.Contains(MainWindow.user)
                && (MainWindow.user.Licences.Equals("Admin")) )
            {
                if (settings_oppened == false)
                {
                    settings_oppened = true;
                    Console.WriteLine("settings_oppened: " + Convert.ToString(settings_oppened));
                    this.settings = new Settings();
                    settings.Show();
                }
                else
                {
                    this.settings.Activate();
                }
            }
        }

        // X Button Click
        private void X_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Find How Many Cameras is connected and open the write UI
        public void CreateVideosPage()
        {
            // Dynamic add columns and rows
            int count_rows = 0;
            int counter = 0;
            foreach (Camera cam in cameras)
            {
                // New Row
                if (counter == 3)
                {
                    cameras_grid.RowDefinitions.Add(new RowDefinition());
                    count_rows++;
                    Grid.SetColumn(cam.video, counter);
                    cam.coll = counter;
                    Grid.SetRow(cam.video, count_rows);
                    cam.row = count_rows;
                    cameras_grid.Children.Add(cam.video);
                    counter = 0;
                }
                else
                {
                    Grid.SetColumn(cam.video, counter);
                    cam.coll = counter;
                    Grid.SetRow(cam.video, count_rows);
                    cam.row = count_rows;
                    cameras_grid.Children.Add(cam.video);
                    cameras_grid.ColumnDefinitions.Add(new ColumnDefinition());
                    counter++;
                }
            }  
        }
    } // Stop Class

}
