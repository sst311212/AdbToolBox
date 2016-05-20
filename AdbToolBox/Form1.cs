using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace AdbToolBox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(StartADB);
            worker.RunWorkerAsync();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://ahzol.com");
        }
        
        public void StartADB(object sender, DoWorkEventArgs e)
        {
            Process adb = new MyProcess("adb.exe", "start-server");
            adb.Start();
            ADBTick.Start();
        }

        private void DetectDevices_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> devices = new List<string>();
            Process adb = new MyProcess("adb.exe", "devices");
            adb.Start();
            adb.StandardOutput.ReadLine();
            while (true)
            {
                string i = adb.StandardOutput.ReadLine();
                if (i != "")
                    devices.Add(i);
                else
                    break;
            }
            Process fst = new MyProcess("fastboot.exe", "devices");
            fst.Start();
            while (true)
            {
                string i = fst.StandardError.ReadLine();
                if (i == "" || i == null)
                    continue;
                if (i.Contains("htc_"))
                    break;
                devices.Add(i);
            }
            e.Result = devices;
        }

        private void DetectDevices_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> devices = e.Result as List<string>;
            foreach (string i in devices)
                if (listBox1.Items.Contains(i) == false)
                    listBox1.Items.Add(i);
            for (int i = 0; i < listBox1.Items.Count; i++)
                if (devices.Contains(listBox1.Items[i]) == false)
                    listBox1.Items.Remove(listBox1.Items[i]);
        }

        private void ADBTick_Tick(object sender, EventArgs e)
        {
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += new DoWorkEventHandler(DetectDevices_DoWork);
            thread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DetectDevices_RunWorkerCompleted);
            thread.RunWorkerAsync(thread);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string device = listBox1.SelectedItem as string;
            if (device == null || device == "")
            {
                label2.Text = "";
                label3.Text = "";
                label4.Text = "";
                label5.Text = "";
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
            }
            else if (device.Contains("fastboot"))
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button3.Text = "rebootRUU";
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = false;
                device = device.Split('\t')[0];
                Process fst = new MyProcess("fastboot.exe");
                fst.StartInfo.Arguments = "-s " + device + " getvar all";
                fst.Start();
                string i = fst.StandardError.ReadToEnd();
                if (i.Contains("product"))
                    label2.Text = "Product：" + i.Substring(i.IndexOf("product")).Split(' ')[1].Split('\n')[0];
                if (i.Contains("version-main"))
                    label3.Text = "Version：" + i.Substring(i.IndexOf("version-main")).Split(' ')[1].Split('\n')[0];
                if (i.Contains("mid"))
                    label4.Text = "MID：" + i.Substring(i.IndexOf("mid")).Split(' ')[1].Split('\n')[0];
                if (i.Contains("cid"))
                    label5.Text = "CID：" + i.Substring(i.IndexOf("cid")).Split(' ')[1].Split('\n')[0];
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button3.Text = "Recovery";
                button4.Enabled = true;
                if (device.Contains("sideload"))
                    button8.Enabled = true;
                device = device.Split('\t')[0];
                Process adb = new MyProcess("adb.exe");
                adb.StartInfo.Arguments = "-s " + device + " shell getprop";
                adb.Start();
                string i = adb.StandardOutput.ReadToEnd();
                if (i.Contains("ro.product.manufacturer"))
                    label2.Text = "Manufacturer：" + i.Substring(i.IndexOf("ro.product.manufacturer")).Split('[')[1].Split(']')[0];
                if (i.Contains("ro.product.model"))
                    label3.Text = "Model：" + i.Substring(i.IndexOf("ro.product.model")).Split('[')[1].Split(']')[0];
                if (i.Contains("ro.product.version"))
                    label4.Text = "Version：" + i.Substring(i.IndexOf("ro.product.version")).Split('[')[1].Split(']')[0];
                if (i.Contains("ro.mid"))
                    label5.Text = "MID：" + i.Substring(i.IndexOf("ro.mid")).Split('[')[1].Split(']')[0];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string device = listBox1.SelectedItem as string;
            if (device.Contains("fastboot"))
            {
                device = device.Split('\t')[0];
                Process fst = new MyProcess("fastboot.exe");
                fst.StartInfo.Arguments = "-s " + device + " reboot-bootloader";
                fst.Start();
            }
            else
            {
                device = device.Split('\t')[0];
                Process adb = new MyProcess("adb.exe");
                adb.StartInfo.Arguments = "-s " + device + " reboot bootloader";
                adb.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string device = listBox1.SelectedItem as string;
            if (device.Contains("fastboot"))
            {
                device = device.Split('\t')[0];
                Process fst = new MyProcess("fastboot.exe");
                fst.StartInfo.Arguments = "-s " + device + " oem reboot-download";
                fst.Start();
            }
            else
            {
                device = device.Split('\t')[0];
                Process adb = new MyProcess("adb.exe");
                adb.StartInfo.Arguments = "-s " + device + " reboot download";
                adb.Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string device = listBox1.SelectedItem as string;
            if (device.Contains("fastboot"))
            {
                device = device.Split('\t')[0];
                Process fst = new MyProcess("fastboot.exe");
                fst.StartInfo.Arguments = "-s " + device + " oem rebootRUU";
                fst.Start();
            }
            else
            {
                device = device.Split('\t')[0];
                Process adb = new MyProcess("adb.exe");
                adb.StartInfo.Arguments = "-s " + device + " reboot recovery";
                adb.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string device = listBox1.SelectedItem as string;
            if (device.Contains("fastboot"))
            {
                device = device.Split('\t')[0];
                Process fst = new MyProcess("fastboot.exe");
                fst.StartInfo.Arguments = "-s " + device + " reboot";
                fst.Start();
            }
            else
            {
                device = device.Split('\t')[0];
                Process adb = new MyProcess("adb.exe");
                adb.StartInfo.Arguments = "-s " + device + " reboot";
                adb.Start();
            }
        }

        public void FlashAsyncWorker(object sender, DoWorkEventArgs e)
        {
            List<string> para = e.Argument as List<string>;
            Process p = new Process();
            p.StartInfo.FileName = para[0];
            p.StartInfo.Arguments = para[1];
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }

        public void FlashSomeThing(string partition)
        {
            FileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                List<string> Para = new List<string>();
                if (partition == "sideload")
                {
                    Para.Add("adb.exe");
                    Para.Add("sideload " + dialog.FileName);
                }
                else
                {
                    Para.Add("fastboot.exe");
                    Para.Add("flash " + partition + " " + dialog.FileName);
                }
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(FlashAsyncWorker);
                worker.RunWorkerAsync(Para);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FlashSomeThing("boot");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FlashSomeThing("recovery");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FlashSomeThing("zip");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FlashSomeThing("sideload");
        }
    }

    public partial class MyProcess : Process
    {
        public MyProcess(string FileName = null, string Arguments = null)
        {
            this.StartInfo.CreateNoWindow = true;
            this.StartInfo.RedirectStandardError = true;
            this.StartInfo.RedirectStandardOutput = true;
            this.StartInfo.UseShellExecute = false;
            this.StartInfo.FileName = FileName;
            this.StartInfo.Arguments = Arguments;
        }
    }
}
