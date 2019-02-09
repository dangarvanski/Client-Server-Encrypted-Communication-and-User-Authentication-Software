
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Numerics;

namespace Server
{
    public partial class Form1 : Form
    {
        Socket sock;
        Socket accept;
        string lastEntry;
        Random rnd = new Random();
        BigInteger primeNumber;
        BigInteger baseNumber;
        BigInteger secretNumber;
        BigInteger publicNumber;
        BigInteger sharedKey;
        string userName;
        string password;

        bool socketOpen = false;
        bool keyExchange = false;
        bool primeSent = false;
        bool userAccepted = false;

        public Form1()
        {
            InitializeComponent();
        }

        Socket socket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (socketOpen == false)
            {
                socketOpen = true;
                sock = socket();
                sock.Bind(new IPEndPoint(0, 3));
                sock.Listen(0);
                richTextBox2.Text = ("Socket status: Open");

                //Creates an array with all the user names!
                //                                              !!!ADDRESS TO THE USERS LOCATION GOES BELLOW!!!!
                DirectoryInfo users = new DirectoryInfo(@"C:\Users\danai\Desktop\GUI\Server\Server\Users");
                FileInfo[] Files = users.GetFiles("*.txt");

                //------------------GENERATING NUMBERS AND CALCULATING PUBLIC KEY FOR DEFFIE-HELLMAN------------------//
                //Generating numbers for the Deffie-Hellman key exchange
                primeNumber = new BigInteger(rnd.Next(2, 99));
                baseNumber = new BigInteger(rnd.Next(2, 99));
                secretNumber = new BigInteger(rnd.Next(2, 99));

                //Calculating the server's public key and assining it to a string ready for dispatch.
                publicNumber = BigInteger.ModPow(baseNumber, secretNumber, primeNumber);
                string publicKey = publicNumber.ToString();
                string systemMessages = "";
                //-------------------------------------GENERATING NUMBERS END-----------------------------------------//


                new Thread(delegate ()
                {
                    accept = sock.Accept();
                    MessageBox.Show("Connection Established!");
                    sock.Close();

                    while (true)
                    {
                        try
                        {
                            byte[] buffer = new byte[255];

                            //---------------------------SENDS THE PRIMARY KEY--------------------------------//
                            if (keyExchange == false && primeSent == false)
                            {
                                string prime = primeNumber.ToString();
                                //Sending the prime and base numbers and the server public Key to Client for the Diffie-Hellman key exchange
                                byte[] data = Encoding.Default.GetBytes(prime);
                                accept.Send(data, 0, data.Length, 0);
                                primeSent = true;
                            }
                            //----------------------------------END-------------------------------------------//

                            //Converts the raceived data to int and checks the lengts to make sure it's not an empty array.
                            int rec = accept.Receive(buffer, 0, buffer.Length, 0);
                            if (rec <= 0)
                            {
                                throw new SocketException();
                            }
                            //Removes empty space after the last character in array if there are any
                            Array.Resize(ref buffer, rec);


                            //------------------SENDING PRIMERY, BASE AND PUBLIC KEY AND AWATING CLIENT PUBLIC KEY------------------//
                            systemMessages = Encoding.Default.GetString(buffer);
                            if (systemMessages == "PrimeTrue")
                            {
                                string baseNum = baseNumber.ToString();
                                byte[] data = Encoding.Default.GetBytes(baseNum);
                                accept.Send(data, 0, data.Length, 0);

                                Array.Clear(buffer, 0, buffer.Length);
                            }

                            else if (systemMessages == "BaseTrue")
                            {
                                byte[] data = Encoding.Default.GetBytes(publicKey);
                                accept.Send(data, 0, data.Length, 0);

                                Array.Clear(buffer, 0, buffer.Length);
                            }

                            else if (keyExchange == false)
                            {
                                string clientPublicKey = Encoding.Default.GetString(buffer);
                                BigInteger clientPublicNumber = Int32.Parse(clientPublicKey);
                                sharedKey = BigInteger.ModPow(clientPublicNumber, secretNumber, primeNumber);
                                keyExchange = true;
                                Array.Clear(buffer, 0, buffer.Length);
                            }
                            //----------------------------------------KEY EXCHANGE END----------------------------------------------//


                            //-----------------------CHECKS IF THE INCOME DATA IS A UN(USER NAME) OR PW(PASSWORD)---------------------//
                            else if (keyExchange == true && userAccepted == false)
                            {
                                if (systemMessages.Substring(0, 2) == "UN")
                                {
                                    userName = systemMessages.Substring(2) + ".txt";

                                    for (int i = 0; i < Files.Length; i++)
                                    {
                                        string userFiles = Files[i].Name;
                                        if (userName != userFiles)
                                        {
                                            byte[] data = Encoding.Default.GetBytes("Wrong user");
                                            accept.Send(data, 0, data.Length, 0);
                                        }
                                        else
                                        {
                                            byte[] data = Encoding.Default.GetBytes("RequestPassword");
                                            accept.Send(data, 0, data.Length, 0);
                                            break;
                                        }
                                    }
                                }

                                else if (systemMessages.Substring(0, 2) == "PW")
                                {
                                    password = systemMessages.Substring(2);
                                    bool passwordNotMatch = false;
                                    //                                              !!!ADDRESS TO THE USERS LOCATION GOES BELLOW!!!!
                                    var fileStream = new FileStream(@"C:\Users\danai\Desktop\GUI\Server\Server\Users\" + userName, FileMode.Open, FileAccess.Read);
                                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            if (line == password)
                                            {
                                                byte[] data1 = Encoding.Default.GetBytes("Access Granted!");
                                                accept.Send(data1, 0, data1.Length, 0);
                                                passwordNotMatch = false;
                                                userAccepted = true;
                                                break;
                                            }
                                            else
                                            {
                                                passwordNotMatch = true;
                                            }
                                        }

                                        if (passwordNotMatch == true)
                                        {
                                            byte[] data2 = Encoding.Default.GetBytes("Password does not match!");
                                            accept.Send(data2, 0, data2.Length, 0);
                                        }
                                    }
                                }
                            }

                            Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.Text = ("Client: " + Encoding.Default.GetString(buffer));
                                lastEntry = Encoding.Default.GetString(buffer);
                            });
                        }
                        catch
                        {
                            MessageBox.Show("Connection has been terminated!");
                            break;
                        }
                    }
                    Application.Exit();
                }
                    ).Start();
            }
            else
            {
                MessageBox.Show("Socket Already open!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string message = Encryption.Encipher(textBox1.Text, sharedKey);
                richTextBox1.Text = ("Server: " + textBox1.Text);
                byte[] data = Encoding.Default.GetBytes(message);
                accept.Send(data, 0, data.Length, 0);
                textBox1.Text = String.Empty;
            }
            catch
            {
                MessageBox.Show("Message could not be delivered!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //listBox2.Items.Add(lastEntry);
            /*bool passwordNotMatch = false;
            string password = Encryption.Decipher(lastEntry, 10);

            var fileStream = new FileStream(@"G:\Networks and Security\My Work\GUI\Server\Server\Password.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line == password)
                    {
                        byte[] data1 = Encoding.Default.GetBytes("Access Granted!");
                        accept.Send(data1, 0, data1.Length, 0);
                        passwordNotMatch = false;
                        break;
                    }
                    else
                    {
                        passwordNotMatch = true;
                    }
                }

                if (passwordNotMatch == true)
                {
                    byte[] data2 = Encoding.Default.GetBytes("Password does not match!");
                    accept.Send(data2, 0, data2.Length, 0);
                }
            }*/

            string login = Encryption.Encipher("RequestLogIn", sharedKey);
            byte[] loginR = Encoding.Default.GetBytes(login);
            accept.Send(loginR, 0, loginR.Length, 0);

        }

        //----------------------DECIPHER MESSAGE----------------------//
        private void button5_Click(object sender, EventArgs e)
        {
            string decrypted = Encryption.Decipher(lastEntry, sharedKey);
            listBox2.Items.Add("Client: " + decrypted);
        }
        //---------------------------END------------------------------//
    }
}
