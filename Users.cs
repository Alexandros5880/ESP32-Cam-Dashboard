using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera
{
    public class Users
    {
        private int id;
        private String firstname = "";
        private String lastname = "";
        private String email = "";
        private String phone = "";
        public static int Count = 0;
        public String licences;
        public String password;

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public String Firstname
        {
            get { return this.firstname; }
            set { this.firstname = value; }
        }

        public String Lastname
        {
            get { return this.lastname; }
            set { this.lastname = value; }
        }

        public String Email
        {
            get { return this.email; }
            set { this.email = value; }
        }

        public String Phone
        {
            get { return this.phone; }
            set { this.phone = value; }
        }

        public String Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public String Licences
        {
            get { return this.licences; }
            set { this.licences = value; }
        }

        // Constructor
        public Users(int id, String fname, String lname, String email, String phone, String licences, String password)
        {
            this.Id = id;
            this.Firstname = fname;
            this.Lastname = lname;
            this.Email = email;
            this.Phone = phone;
            this.Licences = licences;
            this.Password = password;
            Count++;
        }

        // Destructor
        ~Users()
        {
            Count--;
        }
    }
}
