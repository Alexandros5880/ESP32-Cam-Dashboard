using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ComboBox = System.Windows.Controls.ComboBox;

namespace IPCamera
{
    public partial class Settings : Window
    {

        List<Users> users;

        public Settings()
        {
            InitializeComponent();

            this.Update_settings_page();

            this.FillUsers();

            // Fill the TextBoxes With the Data
            sms_account_ssid.Text = MainWindow.twilioAccountSID;
            sms_account_token.Text = MainWindow.twilioAccountToken;
            sms_account_phone.Text = MainWindow.twilioNumber;
        }


        protected override void OnClosed(EventArgs e)
        {
            MainWindow.settings_oppened = false;
            Console.WriteLine("settings_oppened: " + Convert.ToString(MainWindow.settings_oppened));
            this.Close();
        }



        private void Button_pictures_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.ToString().Equals("OK"))
                {
                    if(dialog.SelectedPath != "")
                    {
                        Camera.pictures_dir = dialog.SelectedPath;
                        txtEditor_pictures.Text = Camera.pictures_dir;
                    }
                }
            }
        }



        private void Button_videos_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.ToString().Equals("OK"))
                {
                    if (dialog.SelectedPath != "")
                    {
                        Camera.videos_dir = dialog.SelectedPath;
                        txtEditor_videos.Text = Camera.videos_dir;
                    }
                }
            }
        }




        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Save Paths
            if (txtEditor_pictures.Text != "" && txtEditor_videos.Text != "")
            {
                // Clear DataBase
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = "DELETE FROM dbo.FilesDirs";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Save to DataBase Pictures
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    query = $"INSERT INTO dbo.FilesDirs (id, Name, Path) VALUES (@id,@name,@path)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", 1);
                        command.Parameters.AddWithValue("@name", "Pictures");
                        command.Parameters.AddWithValue("@path", txtEditor_pictures.Text);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                }
                // Save to DataBase Videos
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    query = $"INSERT INTO dbo.FilesDirs (id, Name, Path) VALUES (@id,@name,@path)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", 2);
                        command.Parameters.AddWithValue("@name", "Videos");
                        command.Parameters.AddWithValue("@path", txtEditor_videos.Text);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                }
            }
            // Save URLS
            List<Cameras> cams = new List<Cameras>(8);
            try
            {
                if (url_1.Text.Length > 0 && name_1.Text.Length > 0 &&
                    name_1.Text.Length > 0 && password_1.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_1.Text, name_1.Text, username_1.Text, password_1.Text, camera1_esp32.IsChecked.Value));
                }
                if (url_2.Text.Length > 0 && name_2.Text.Length > 0 &&
                    name_2.Text.Length > 0 && password_2.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_2.Text, name_2.Text, username_2.Text, password_2.Text, camera2_esp32.IsChecked.Value));
                }
                if (url_3.Text.Length > 0 && name_3.Text.Length > 0 &&
                    name_3.Text.Length > 0 && password_3.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_3.Text, name_3.Text, username_3.Text, password_3.Text, camera3_esp32.IsChecked.Value));
                }
                if (url_4.Text.Length > 0 && name_4.Text.Length > 0 &&
                    name_4.Text.Length > 0 && password_4.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_4.Text, name_4.Text, username_4.Text, password_4.Text, camera4_esp32.IsChecked.Value));
                }
                if (url_5.Text.Length > 0 && name_5.Text.Length > 0 &&
                    name_5.Text.Length > 0 && password_5.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_5.Text, name_5.Text, username_5.Text, password_5.Text, camera5_esp32.IsChecked.Value));
                }
                if (url_6.Text.Length > 0 && name_6.Text.Length > 0 &&
                    name_6.Text.Length > 0 && password_6.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_6.Text, name_6.Text, username_6.Text, password_6.Text, camera6_esp32.IsChecked.Value));
                }
                if (url_7.Text.Length > 0 && name_7.Text.Length > 0 &&
                    name_7.Text.Length > 0 && password_7.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_7.Text, name_7.Text, username_7.Text, password_7.Text, camera7_esp32.IsChecked.Value));
                }
                if (url_8.Text.Length > 0 && name_8.Text.Length > 0 &&
                    name_8.Text.Length > 0 && password_8.Text.Length > 0)
                {
                    cams.Add(new Cameras(url_8.Text, name_8.Text, username_8.Text, password_8.Text, camera8_esp32.IsChecked.Value));
                }
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nParamnAME:{ex.ParamName}\n{ex.Message}");
            }
            int urls_num = cams.Count;
            // If urls.Count > 0
            if (urls_num > 0)
            {
                // Clear Database
                SqlConnection con = new SqlConnection(Camera.DB_connection_string);
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "DELETE FROM dbo.MyCameras ",
                    Connection = con
                };
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                foreach (Cameras d in cams)
                {
                    Guid guid = Guid.NewGuid();
                    String my_id = guid.ToString();
                    // Save Data To Database
                    using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                    {
                        String query = $"INSERT INTO dbo.MyCameras (id,urls,name,username,password,isEsp32 ) VALUES (@id,@urls,@name,@username,@password,@isESP)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@id", my_id);
                            command.Parameters.AddWithValue("@urls", d.url);
                            command.Parameters.AddWithValue("@name", d.name);
                            command.Parameters.AddWithValue("@username", d.username);
                            command.Parameters.AddWithValue("@password", d.password);
                            command.Parameters.AddWithValue("@isESP", d.isEsp32);
                            connection.Open();
                            int result = command.ExecuteNonQuery();
                            // Check Error
                            if (result < 0)
                                System.Windows.MessageBox.Show("Error inserting data into Database!");
                        }
                    }
                }
            }
            else
            {
                // Clear Database
                SqlConnection con = new SqlConnection(Camera.DB_connection_string);
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = "DELETE FROM dbo.MyCameras ",
                    Connection = con
                };
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            // Save Email Sender And Password
            if ( (!email_send_textbox.Text.Equals(MainWindow.email_send)) ||
                    (!pass_send_textbox.Password.Equals(MainWindow.pass_send)))
            {
                Console.WriteLine(pass_send_textbox.Password);
                // If email is an valid email
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email_send_textbox.Text);
                    if (addr.Address == email_send_textbox.Text)
                    {
                        // Delete From Table The Last
                        SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                        String query = $"DELETE FROM dbo.EmailSender";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cn.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                        cn.Close();
                        // Save Data To Database
                        using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                        {
                            query = $"INSERT INTO dbo.EmailSender (Email,Pass) VALUES (@email,@pass)";
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@email", email_send_textbox.Text);
                                command.Parameters.AddWithValue("@pass", pass_send_textbox.Password);
                                connection.Open();
                                result = command.ExecuteNonQuery();
                                // Check Error
                                if (result < 0)
                                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                            }
                        }
                    } else
                    {
                        if (!email_send_textbox.Text.Equals(""))
                        {
                            System.Windows.MessageBox.Show("Not Valid Email!");
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    if (!email_send_textbox.Text.Equals(""))
                    {
                        System.Windows.MessageBox.Show("Not Valid Email!");
                    }
                    else
                    {
                        Console.WriteLine($"Source:{ex.Source}\n\n{ex.Message}");
                    }
                } 
            }
            // Save SMS sid, token, phone
            if (!sms_account_ssid.Text.Equals(MainWindow.twilioAccountSID) ||
                !sms_account_token.Text.Equals(MainWindow.twilioAccountToken) ||
                !sms_account_phone.Text.Equals(MainWindow.twilioNumber))
            {
                // Delete From Table The Last
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.SMS";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Save Data To Database
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    query = $"INSERT INTO dbo.SMS (AccountSID,AccountTOKEN,Phone) VALUES (@sid,@token,@phone)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sid", sms_account_ssid.Text);
                        command.Parameters.AddWithValue("@token", sms_account_token.Text);
                        command.Parameters.AddWithValue("@phone", sms_account_phone.Text);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                }
            }
            // Ask to Restart The Application
            MessageBoxResult res = System.Windows.MessageBox.Show("Restart ?", "Question", (MessageBoxButton)MessageBoxButtons.OKCancel);
            if (res.ToString() == "OK")
            {
                // Close Settings Window
                this.Close();
                // Restart App Application
                MainWindow.RestartApp();
            }
        }


        private void Update_settings_page()
        {
            // Feel files paths
            txtEditor_pictures.Text = Camera.pictures_dir;
            txtEditor_videos.Text = Camera.videos_dir;
            // Saved Files Formats
            avi_checkbox.IsChecked = Camera.avi_format;
            mp4_checkbox.IsChecked = Camera.mp4_format;
            webm_checkbox.IsChecked = Camera.webm_format;
            // Feel the urls
            if (Camera.count > 0)
            {
                if (MainWindow.cameras[0].url != "" && MainWindow.cameras[0].name != "")
                {
                    url_1.Text = MainWindow.cameras[0].url;
                    name_1.Text = MainWindow.cameras[0].name;
                    username_1.Text = MainWindow.cameras[0].Username;
                    password_1.Text = MainWindow.cameras[0].Password;
                    camera1_esp32.IsChecked = MainWindow.cameras[0].isEsp32;
                }
            }
            if (Camera.count > 1)
            {
                if (MainWindow.cameras[1].url != "" && MainWindow.cameras[1].name != "")
                {
                    url_2.Text = MainWindow.cameras[1].url;
                    name_2.Text = MainWindow.cameras[1].name;
                    username_2.Text = MainWindow.cameras[1].Username;
                    password_2.Text = MainWindow.cameras[1].Password;
                    camera2_esp32.IsChecked = MainWindow.cameras[1].isEsp32;
                }
            }
            if (Camera.count > 2)
            {
                if (MainWindow.cameras[2].url != "" && MainWindow.cameras[2].name != "")
                {
                    url_3.Text = MainWindow.cameras[2].url;
                    name_3.Text = MainWindow.cameras[2].name;
                    username_3.Text = MainWindow.cameras[2].Username;
                    password_3.Text = MainWindow.cameras[2].Password;
                    camera3_esp32.IsChecked = MainWindow.cameras[2].isEsp32;
                }
            }
            if (Camera.count > 3)
            {
                if (MainWindow.cameras[3].url != "" && MainWindow.cameras[3].name != "")
                {
                    url_4.Text = MainWindow.cameras[3].url;
                    name_4.Text = MainWindow.cameras[3].name;
                    username_4.Text = MainWindow.cameras[3].Username;
                    password_4.Text = MainWindow.cameras[3].Password;
                    camera4_esp32.IsChecked = MainWindow.cameras[3].isEsp32;
                }
            }
            if (Camera.count > 4)
            {
                if (MainWindow.cameras[4].url != "" && MainWindow.cameras[4].name != "")
                {
                    url_5.Text = MainWindow.cameras[4].url;
                    name_5.Text = MainWindow.cameras[4].name;
                    username_5.Text = MainWindow.cameras[4].Username;
                    password_5.Text = MainWindow.cameras[4].Password;
                    camera5_esp32.IsChecked = MainWindow.cameras[4].isEsp32;
                }
            }
            if (Camera.count > 5)
            {
                if (MainWindow.cameras[5].url != "" && MainWindow.cameras[5].name != "")
                {
                    url_6.Text = MainWindow.cameras[5].url;
                    name_6.Text = MainWindow.cameras[5].name;
                    username_6.Text = MainWindow.cameras[5].Username;
                    password_6.Text = MainWindow.cameras[5].Password;
                    camera6_esp32.IsChecked = MainWindow.cameras[5].isEsp32;
                }
            }
            if (Camera.count > 6)
            {
                if (MainWindow.cameras[6].url != "" && MainWindow.cameras[6].name != "")
                {
                    url_7.Text = MainWindow.cameras[6].url;
                    name_7.Text = MainWindow.cameras[6].name;
                    username_7.Text = MainWindow.cameras[6].Username;
                    password_7.Text = MainWindow.cameras[6].Password;
                    camera7_esp32.IsChecked = MainWindow.cameras[6].isEsp32;
                }
            }
            if (Camera.count > 7)
            {
                if (MainWindow.cameras[7].url != "" && MainWindow.cameras[7].name != "")
                {
                    url_8.Text = MainWindow.cameras[7].url;
                    name_8.Text = MainWindow.cameras[7].name;
                    username_8.Text = MainWindow.cameras[7].Username;
                    password_8.Text = MainWindow.cameras[7].Password;
                    camera8_esp32.IsChecked = MainWindow.cameras[7].isEsp32;
                }
            }
            // Update Email Sender And Pasword
            email_send_textbox.Text = MainWindow.email_send;
            pass_send_textbox.Password = MainWindow.pass_send;
            // Update Robotic . CameraSelector cameras
            camera_selector.Items.Add("Select a camera");
            camera_selector.SelectedIndex = camera_selector.Items.IndexOf("Select a camera");
            foreach (Camera cam in MainWindow.cameras)
            {
                camera_selector.Items.Add(cam.name);
            }
        }


        // Fill Users Table With Users
        public void FillUsers()
        {
            // Save list with users before
            users = new List<Users>(MainWindow.myUsers);
            // Add the List To DataGrid
            users_grid.ItemsSource = MainWindow.myUsers;
            // Make Id Column No Editable
            users_grid.AutoGeneratingColumn += (object sender, DataGridAutoGeneratingColumnEventArgs e) =>
            {
                if (e.Column.Header.ToString() == "Id")
                {
                    e.Column.IsReadOnly = true; // Makes the column as read only
                    e.Column.Width = 33;
                }
                if (e.Column.Header.ToString() == "FirstName")
                {
                    e.Column.Width = 100;
                }
                if (e.Column.Header.ToString() == "LastName")
                {
                    e.Column.Width = 100;
                }
                if (e.Column.Header.ToString() == "Email")
                {
                    e.Column.Width = 300;
                }
                if (e.Column.Header.ToString() == "Password")
                {
                    e.Column.Width = 150;
                }
                if (e.Column.Header.ToString() == "Licences")
                {
                    e.Column.IsReadOnly = true; // Makes the column as read only
                }
            };
            users_grid.CanUserDeleteRows = true;
        }

        // Users Apply Button
        private void U_Apply_Click(object sender, RoutedEventArgs e)
        {
            // Commit Changes to the List with users
            users_grid.CommitEdit();
            // Chech If Delete a users
            if (users.Count > MainWindow.myUsers.Count)
            {
                Console.WriteLine("DELETE OK");
                foreach (Users u in users)
                {
                    if (!MainWindow.myUsers.Contains(u))
                    {
                        // Delete This User From DB
                        SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                        String query = $"DELETE FROM dbo.Users WHERE Id='{u.Id}'";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cn.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                        cn.Close();
                    }
                }
            }
            else // Update Users On DB
            {
                int counter = 0;
                foreach (Users u in MainWindow.myUsers)
                {
                    Users old_user = users[counter];
                    // If A record changeds updated
                    if ((old_user.Firstname.Equals(u.Firstname)) || 
                            (old_user.Lastname.Equals(u.Lastname)) || 
                            (old_user.Email.Equals(u.Email)) || 
                            (old_user.Phone.Equals(u.Phone)))
                    {
                        Console.WriteLine("UPDATE OK");
                        //Console.WriteLine($"ID: {u.Id}  FName: {u.Firstname}  LName: {u.Lastname}  Email: {u.Email}  Phone: {u.Phone}");
                        // Update DataBase with this user
                        SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                        String query = $"UPDATE dbo.Users SET FirstName='{u.Firstname}', " +
                                                        $"LastName='{u.Lastname}', Email='{u.Email}', " +
                                                        $"Phone='{u.Phone}' WHERE Id='{u.Id}'";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cn.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                        cn.Close();
                        counter++;
                    }
                }
            }
            // Refresch Users Table
            this.Close();
            new Settings().Show();
        }

        // Users Add Button
        private void U_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String fname = FirstName.Text;
                String lname = LastName.Text;
                String email = Email.Text;
                String phone = Phone.Text;
                String selection = new_user_licenses.SelectedValue.ToString();
                if (selection.Contains("Admin"))
                {
                    selection = "Admin";
                }
                else if (selection.Contains("Employee"))
                {
                    selection = "Employee";
                }
                String password = Password.Text;
                String repeat_pass = Repeat_Pass.Text;
                if (password.Equals(repeat_pass))
                {
                    // Insert to DB First to create an Id and then update MainWindow.myUsers
                    String query = $"INSERT INTO dbo.Users (FirstName, LastName, Email, Phone, Licences, Password)" +
                                                            $" VALUES (@fname, @lname, @email, @phone, @licences, @pass)";
                    using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@fname", fname);
                            command.Parameters.AddWithValue("@lname", lname);
                            command.Parameters.AddWithValue("@email", email);
                            command.Parameters.AddWithValue("@phone", phone);
                            command.Parameters.AddWithValue("@licences", selection);
                            command.Parameters.AddWithValue("@pass", password);
                            connection.Open();
                            int result = command.ExecuteNonQuery();
                            // Check Error
                            if (result < 0)
                                System.Windows.MessageBox.Show("Error inserting data into Database!");
                        }
                        //connection.Close();
                        // Get The New User User From DB And Add Him To MainWindow.myUsers
                        query = $"SELECT Id, FirstName, LastName, Email, Phone, Licences, Password FROM dbo.Users " +
                                                    $"WHERE FirstName=@fname AND LastName=@lname AND Email=@email AND Phone=@phone AND Password=@pass";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@fname", fname);
                            command.Parameters.AddWithValue("@lname", lname);
                            command.Parameters.AddWithValue("@email", email);
                            command.Parameters.AddWithValue("@phone", phone);
                            command.Parameters.AddWithValue("@pass", password);
                            connection.Open();
                            SqlDataReader dataReader = command.ExecuteReader();
                            while (dataReader.Read())
                            {
                                int id = (int)dataReader["Id"];
                                String fname2 = dataReader["FirstName"].ToString().Trim();
                                String lname2 = dataReader["LastName"].ToString().Trim();
                                String email2 = dataReader["Email"].ToString().Trim();
                                String phone2 = dataReader["Phone"].ToString().Trim();
                                String licences = dataReader["Licences"].ToString().Trim();
                                String pass = dataReader["Password"].ToString().Trim();
                                // Create The Usres Objects
                                Users user = new Users(id, fname2, lname2, email2, phone2, licences, pass);
                                MainWindow.myUsers.Add(user);
                            }
                        }
                        connection.Close();
                    }
                    Console.WriteLine("ADD OK");
                    // Refresch Users Table
                    this.Close();
                    new Settings().Show();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Rong Password.");
                }
            }
            catch(System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    System.Windows.MessageBox.Show("This User Exists");
                }
            }
        }

        // Files format checkboxes
        private void AVI_chencked(object sender, EventArgs e)
        {
            Camera.avi_format = true;
            Camera.mp4_format = false;
            Camera.webm_format = false;
            mp4_checkbox.IsChecked = false;
            webm_checkbox.IsChecked = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 1);
                        command.Parameters.AddWithValue("@mp4", 0);
                        command.Parameters.AddWithValue("@webm", 0);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }
        private void AVI_unchencked(object sender, EventArgs e)
        {
            Camera.avi_format = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 0);
                        command.Parameters.AddWithValue("@mp4", 0);
                        command.Parameters.AddWithValue("@webm", 0);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }

        private void MP4_chencked(object sender, EventArgs e)
        {
            Camera.mp4_format = true;
            Camera.avi_format = false;
            Camera.webm_format = false;
            avi_checkbox.IsChecked = false;
            webm_checkbox.IsChecked = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 0);
                        command.Parameters.AddWithValue("@mp4", 1);
                        command.Parameters.AddWithValue("@webm", 0);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }
        private void MP4_unchencked(object sender, EventArgs e)
        {
            Camera.mp4_format = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 0);
                        command.Parameters.AddWithValue("@mp4", 0);
                        command.Parameters.AddWithValue("@webm", 0);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }

        private void WEBM_chencked(object sender, EventArgs e)
        {
            Camera.webm_format = true;
            Camera.mp4_format = false;
            Camera.avi_format = false;
            mp4_checkbox.IsChecked = false;
            avi_checkbox.IsChecked = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 0);
                        command.Parameters.AddWithValue("@mp4", 0);
                        command.Parameters.AddWithValue("@webm", 1);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }
        private void WEBM_unchencked(object sender, EventArgs e)
        {
            Camera.webm_format = false;
            try
            {
                // Delete Data From DB
                SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                String query = $"DELETE FROM dbo.FilesFormats";
                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                int result = cmd.ExecuteNonQuery();
                if (result < 0)
                    System.Windows.MessageBox.Show("Error inserting data into Database!");
                cn.Close();
                // Insert Data To DB
                query = $"INSERT INTO dbo.FilesFormats (avi, mp4, webm) VALUES (@avi, @mp4, @webm)";
                using (SqlConnection connection = new SqlConnection(Camera.DB_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@avi", 0);
                        command.Parameters.AddWithValue("@mp4", 0);
                        command.Parameters.AddWithValue("@webm", 0);
                        connection.Open();
                        result = command.ExecuteNonQuery();
                        // Check Error
                        if (result < 0)
                            System.Windows.MessageBox.Show("Error inserting data into Database!");
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"Source:{ex.Source}\nLine:{ex.LineNumber}\n{ex.Message}");
            }
        }

        // When select a tab
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // SMS Hyper Link Func
        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        // Whene Robotic Select Camera Combo Box change
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String cam_name = camera_selector.SelectedItem.ToString();
            if (!cam_name.Equals("Select a camera"))
            {
                Console.WriteLine($"Selected Camera: {cam_name}");
                foreach (Camera cam in MainWindow.cameras)
                {
                    if (cam.name.Equals(cam_name))
                    {
                        up_text.Text = cam.up_req;
                        down_text.Text = cam.down_req;
                        right_text.Text = cam.right_req;
                        left_text.Text = cam.left_req;
                    }
                }
            }
            else
            {
                Console.WriteLine("No Camera Selected");
                up_text.Text = "";
                down_text.Text = "";
                right_text.Text = "";
                left_text.Text = "";
            }
        }

        // Save the cameras remote controll settings
        private void Apply_get_req_Click(object sender, RoutedEventArgs e)
        {
            // Save UP, DOWN, RIGHT, LEFT Buttons
            if (CheckURL(up_text.Text) && CheckURL(down_text.Text) &&
                    CheckURL(right_text.Text) && CheckURL(left_text.Text))
            {
                String cam_name = camera_selector.SelectedItem.ToString();
                if (!cam_name.Equals("Select a camera"))
                {
                    Console.WriteLine("Update DATABASE");
                    // Update Data To Database
                    SqlConnection cn = new SqlConnection(Camera.DB_connection_string);
                    String query = "UPDATE dbo.myCameras SET Up_req=@up, Down_req=@down, Left_req=@left, Right_req=@right WHERE name=@cam_name";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@up", up_text.Text);
                    cmd.Parameters.AddWithValue("@down", down_text.Text);
                    cmd.Parameters.AddWithValue("@left", left_text.Text);
                    cmd.Parameters.AddWithValue("@right", right_text.Text);
                    cmd.Parameters.AddWithValue("@cam_name", cam_name);
                    cn.Open();
                    int result = cmd.ExecuteNonQuery();
                    cn.Close();
                    if (result < 0)
                        System.Windows.MessageBox.Show("Error inserting data into Database!");
                    else
                    {
                        // Ask to Restart The Application
                        MessageBoxResult res = System.Windows.MessageBox.Show("Restart ?", "Question", (MessageBoxButton)MessageBoxButtons.OKCancel);
                        if (res.ToString() == "OK")
                        {
                            // Close Settings Window
                            this.Close();
                            // Restart App Application
                            MainWindow.RestartApp();
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Select a camera.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Setup cameras http/https urls");
            }
        }


        // Check if the texts is a valis urls
        private static bool CheckURL(String url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
        


    }



    public class Cameras
    {
        public String url;
        public String name;
        public String username;
        public String password;
        public bool isEsp32 = false;
        public Cameras(String u, String n, String un, String p, bool isEsp)
        {
            this.url = u;
            this.name = n;
            this.username = un;
            this.password = p;
            this.isEsp32 = isEsp;
        }
        ~Cameras()
        {

        }
    }
}
