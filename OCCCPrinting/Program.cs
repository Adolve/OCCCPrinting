using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using OCCCPrinting.Persistence;
using System.Diagnostics;

namespace OCCCPrinting
{
    
    class Program
    {

        static void Main(string[] args)
        {
            
            // Access to the database
            //var db = new OCCCPrintingDbContext();
            //var printTracks = db.PrintTracks;
            //Console.WriteLine(printTracks.FirstOrDefault().StudentId);
            Console.WriteLine("Hello World!");
            /////////////////////////

            PasswordPrompt form = new PasswordPrompt();
            form.ShowDialog();
            if(form.StudentId == "123")
            {
                MessageBox.Show("Nice");
            }
            else
            {
                MessageBox.Show("What!!");
            }

            // mewPrintJobs event subscription
            //ManagementEventWatcher startWatch = new ManagementEventWatcher(
            //new EventQuery("SELECT * FROM    __InstanceCreationEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PrintJob'"));
            //startWatch.EventArrived += new EventArrivedEventHandler(
            //    mewPrintJobs_EventArrived);
            //startWatch.Start();
            /////////////////////////

            Console.ReadKey();

        }

        static void mewPrintJobs_EventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject printJob = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;

            string printerName = printJob.Properties["Name"].Value.ToString().Split(',')[0];
            int JobId = Convert.ToInt32(printJob.Properties["JobId"].Value);

            PrintServer myPrintServer = new PrintServer();
            var myPrintQueue = myPrintServer.GetPrintQueue(printerName);
            var job = myPrintQueue.GetJob(JobId);
            job.Pause();   
            
            
            int i = 0;
            while (job.IsSpooling)
            {
                job.Refresh();
                //job = myPrintQueue.GetJob(JobId);
                Console.WriteLine("Wait!!");
                i++;
            }

            Console.WriteLine("Stop the Spooler");
            RunCommand("net stop spooler");            
            Console.WriteLine("number of pages : " + job.NumberOfPages);



            int promptValue = Prompt.ShowDialog("Test", "123");
            Console.WriteLine("Stating the Spooler");
            
            RunCommand("net start spooler");
            RunCommand("net start spooler");
            if (promptValue ==5)
            {
                Console.WriteLine("printing ...");
                job.Resume();
            }
            else
            {
                job.Cancel();
            }

            //Console.ReadLine();

            
            
            


            
            

            

            

            // Showing message box for the password 
            //Task.Run(() =>
            //{
            //    var dialogResult = MessageBox.Show(v, "Title", MessageBoxButtons.OKCancel);
            //    if (dialogResult == System.Windows.Forms.DialogResult.OK)
            //        MessageBox.Show("OK Clicked");
            //    else
            //        MessageBox.Show("Cancel Clicked");
            //});
            //////////////////////////////////////// 
            
            Console.WriteLine("Done!");
        }
        public static void RunCommand(string command)
        {
            String temp = @"/c "+command;
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = temp;
            cmdsi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            Process cmd = Process.Start(cmdsi);
            cmd.WaitForExit();
        }

    }
    public static class Prompt
    {
        public static int ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 200;
            
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.ShowDialog();
            return (int)inputBox.Value;
        }
    }

}
