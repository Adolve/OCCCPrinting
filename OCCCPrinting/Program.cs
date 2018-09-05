using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using OCCCPrinting.Persistence;

namespace OCCCPrinting
{
    class Program
    {
        static void Main(string[] args)
        {
            // add a job "Adobe PDF"

            PrintServer myPrintServer = new PrintServer();
            var myPrintQueue = myPrintServer.GetPrintQueue("Adobe PDF");

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/job.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            var obj = (string)formatter.Deserialize(stream);
            stream.Close();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var job = javaScriptSerializer.Deserialize<PrintSystemJobInfo>(obj);

            

            // Access to the database
            var db = new OCCCPrintingDbContext();
            var printTracks = db.PrintTracks;
            Console.WriteLine(printTracks.FirstOrDefault().StudentId);
            Console.WriteLine("Hello World!");
            /////////////////////////
            
            // mewPrintJobs event subscription
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
                new EventQuery("SELECT * FROM    __InstanceCreationEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PrintJob'"));
            startWatch.EventArrived += new EventArrivedEventHandler(
                mewPrintJobs_EventArrived);
            startWatch.Start();
            /////////////////////////

            Console.ReadKey();

        }

        static  void mewPrintJobs_EventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject printJob = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;

            string printerName = printJob.Properties["Name"].Value.ToString().Split(',')[0];
            int JobId = Convert.ToInt32(printJob.Properties["JobId"].Value);

            PrintServer myPrintServer = new PrintServer();
            var myPrintQueue = myPrintServer.GetPrintQueue(printerName);
            var job = myPrintQueue.GetJob(JobId);
            job.Pause();
            
            // Serialize
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            var jobSerialize = javaScriptSerializer.Serialize(job);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/job.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, jobSerialize);
            stream.Close();
            ///////////////////////////
            Console.WriteLine("#pages before: "+job.NumberOfPages);
            int i = 0;
            while (job.IsSpooling)
            {
                job = myPrintQueue.GetJob(JobId);
                Console.WriteLine(i);
                i++;
            }
            
            Console.WriteLine("#pages after: " + job.NumberOfPages);
            
        

            /*

            foreach (PropertyData prop in e.NewEvent.Properties)
            {
                string val = prop.Value == null ? "null" : prop.Value.ToString();
            }
            
            string v = "";
            foreach (PropertyData propp in printJob.Properties)
            {
                string name = propp.Name;
                string val = propp.Value == null ? "null" : propp.Value.ToString();
                val += "\n";
                v += name + ":" + val;
            }
            
            //System.Threading.Thread.Sleep(500);
            
            while (!GetPrintJobsCollection(printJob.Properties["Name"].Value.ToString(),
                Convert.ToInt32(printJob.Properties["JobId"].Value))) ;
             
            // Console.WriteLine(v);
             

            // Showing message box for the password 
            Task.Run(() =>
            {
                var dialogResult = MessageBox.Show(v, "Title", MessageBoxButtons.OKCancel);
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    MessageBox.Show("OK Clicked");
                else
                    MessageBox.Show("Cancel Clicked");
            });
            //////////////////////////////////////// 
            */
            Console.WriteLine("catched!");
        }
    }
}
