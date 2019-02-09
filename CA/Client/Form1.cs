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
using System.Numerics;
using System.IO;


namespace Client
{
    public partial class Form1 : Form
    {
        string lastEntry;
        public Socket sock;
        Random rnd = new Random();
        bool keyExchange = false;
        BigInteger secretNumber;
        BigInteger primeNumber;
        BigInteger baseNumber;
        BigInteger publicNumber;
        BigInteger serverPublicKey;
        BigInteger sharedKey;
        public static string userName = "";
        public static string password = "";

        bool primeAccepted = false;
        bool baseAccepted = false;
        bool ServerPublicKeyAccepted = false;
        public Form1()
        {
            InitializeComponent();
            sock = socket();
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
            secretNumber = new BigInteger(rnd.Next(2, 99));

            try
            {
                sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3));
                new Thread(() =>
                    {
                        read();
                    }).Start();
            }
            catch
            {
                MessageBox.Show("Connection Failed!");
            }
        }

        void read()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[255];

                    //Checks the length of the received data so if the message is empty it throws an exeption
                    int rec = sock.Receive(buffer, 0, buffer.Length, 0);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }

                    //Removes any empty spaces after the last character in the message
                    Array.Resize(ref buffer, rec);

                    //Executes only if the Deffie-Hellman key exchange hasn't been done
                    if (keyExchange == false)
                    {
                        try
                        {
                            string message = Encoding.Default.GetString(buffer);

                            //Accepts the prime number as sends a message back to notify the server that the prime number has been accepted.
                            if (primeAccepted == false)
                            {
                                primeNumber = Int32.Parse(message);
                                byte[] data = Encoding.Default.GetBytes("PrimeTrue");
                                sock.Send(data, 0, data.Length, 0);
                                primeAccepted = true;
                            }

                            else if (baseAccepted == false)
                            {
                                baseNumber = Int32.Parse(message);
                                byte[] data = Encoding.Default.GetBytes("BaseTrue");
                                sock.Send(data, 0, data.Length, 0);
                                baseAccepted = true;
                            }

                            else if (ServerPublicKeyAccepted == false)
                            {
                                serverPublicKey = Int32.Parse(message);

                                publicNumber = BigInteger.ModPow(baseNumber, secretNumber, primeNumber);
                                string publicKey = publicNumber.ToString();

                                byte[] data01 = Encoding.Default.GetBytes(publicKey);
                                sock.Send(data01, 0, data01.Length, 0);

                                ServerPublicKeyAccepted = true;
                            }

                            Array.Clear(buffer, 0, buffer.Length);
                            if (primeAccepted == true && baseAccepted == true && ServerPublicKeyAccepted == true)
                            {
                                sharedKey = BigInteger.ModPow(serverPublicKey, secretNumber, primeNumber);
                                keyExchange = true;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Key Exchange failed!");
                        }
                    }

                    if (keyExchange == true)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            if (Encryption.Decipher(Encoding.Default.GetString(buffer), sharedKey) == "RequestLogIn")
                            {
                                if (userName.Length == 0)
                                {
                                    MessageBox.Show("Server has requested Log In information.");

                                }
                                else
                                {
                                    byte[] data = Encoding.Default.GetBytes("UN" + userName);
                                    sock.Send(data, 0, data.Length, 0);
                                }
                            }
                            if (Encoding.Default.GetString(buffer) == "RequestPassword")
                            {
                                byte[] data = Encoding.Default.GetBytes("PW" + password);
                                sock.Send(data, 0, data.Length, 0);
                            }
                            else
                            {
                                richTextBox1.Text = ("Server: " + Encoding.Default.GetString(buffer));
                                lastEntry = Encoding.Default.GetString(buffer);
                                richTextBox1.Update();
                            }
                        });
                    }
                }
                catch
                {
                    MessageBox.Show("Disconnected!");
                    sock.Close();
                    break;
                }
            }
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string message = Encryption.Encipher(textBox2.Text, sharedKey);
            richTextBox1.Text = ("Client: " + textBox2.Text);
            byte[] data = Encoding.Default.GetBytes(message);
            sock.Send(data, 0, data.Length, 0);
            textBox2.Text = string.Empty;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string decrypted = Encryption.Decipher(lastEntry, sharedKey);
            listBox1.Items.Add("Server: " + decrypted);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }
}
