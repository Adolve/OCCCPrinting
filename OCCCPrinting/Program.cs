﻿using System;
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
using System.Threading;
using OCCCPrinting.Models;
using System.Data.Entity;

namespace OCCCPrinting
{

    class Program
    {
        static private OCCCPrintingDbContext db;
        static void Main(string[] args)
        {

            // Access to the database
            db = new OCCCPrintingDbContext();

            Task a = RunCommand("net start spooler");
            a.Wait();


            Console.WriteLine("Hello World!");
            /////////////////////////
            long id = 783914; 
            Console.WriteLine(Password(id));


            // mewPrintJobs event subscription
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
            new EventQuery("SELECT * FROM    __InstanceCreationEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PrintJob'"));
            startWatch.EventArrived += new EventArrivedEventHandler(
                mewPrintJobs_EventArrived);
            startWatch.Start();
            /////////////////////////

            Console.ReadKey();

        }

        static async void mewPrintJobs_EventArrived(object sender, EventArrivedEventArgs e)
        {
            
            var settings = Properties.Settings.Default;
            settings.Reload();

            //Check if the program is enable
            if (!settings.IsPageLimitEnable)
            {
                Console.WriteLine("Page Limit Disabled");
                return;
            }            

            ManagementBaseObject printJob = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string printerName = printJob.Properties["Name"].Value.ToString().Split(',')[0];
            int JobId = Convert.ToInt32(printJob.Properties["JobId"].Value);

            // Expluded printers 
            string[] PrinterList = settings.ExcludedPrinters.Split(';');
            foreach (string printer in PrinterList)
            {
                if (printer == printerName)
                {
                    Console.WriteLine("Printer Expluded");
                    return;
                }
            }


            PrintServer myPrintServer = new PrintServer();
            var myPrintQueue = myPrintServer.GetPrintQueue(printerName);
            var job = myPrintQueue.GetJob(JobId);
            job.Pause();

            int i = 0;
            while (job.IsSpooling)
            {
                job.Refresh();                
                Console.WriteLine("Wait!!");
                i++;
            }

            Console.WriteLine("Stop the Spooler");
            await RunCommand("net stop spooler");
            Console.WriteLine("number of pages : " + job.NumberOfPages);
            if (settings.PageLimit < job.NumberOfPages)
            {
                MessageBox.Show("You can only print " + settings.PageLimit + " pages per day", "Prompt", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning, 
                    MessageBoxDefaultButton.Button1, 
                    MessageBoxOptions.DefaultDesktopOnly);

           

                await RunCommand("net start spooler");
                job.Cancel();
                return;
            }
            PasswordPrompt form = new PasswordPrompt();
            form.ShowDialog();

            Task<PrintTrack> queryRequest = GetPrintTrack(form.StudentId);

            Console.WriteLine("Stating the Spooler");
            await RunCommand("net start spooler");

            long sid;
            long.TryParse(form.StudentId, out sid);
            var password = Password(sid);
            if (form.Password == password)
            {
                Console.WriteLine("printing ...");                
                var printTrack = await queryRequest;

                if (printTrack != null)
                {
                    if(settings.PageLimit>=printTrack.PagesPrinted+job.NumberOfPages)
                    {
                        job.Resume();
                        MessageBox.Show("number of pages : " + job.NumberOfPages);
                        printTrack.PagesPrinted += job.NumberOfPages;
                        db.SaveChanges();
                    }
                    else
                    {
                        job.Cancel();
                        MessageBox.Show("You can only print "+ (settings.PageLimit - printTrack.PagesPrinted) + " pages more for today");
                    }
                }
                else
                {
                    if (settings.PageLimit >= job.NumberOfPages)
                    {
                        job.Resume();
                        MessageBox.Show("number of pages : " + job.NumberOfPages);
                        PrintTrack newPrintTrack = new PrintTrack
                        {
                            StudentId = form.StudentId,
                            Password = form.Password,
                            PagesPrinted = job.NumberOfPages,
                            Date = DateTime.Today,
                            ComputerName = System.Environment.MachineName
                        };
                        db.PrintTracks.Add(newPrintTrack);
                        db.SaveChanges();
                        
                    }                    

                }
                
                
            }
            else
            {
                job.Cancel();
                MessageBox.Show("Invalid password!");
            }

            

            

            

            form.Dispose();


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

        public async static Task<PrintTrack> GetPrintTrack(string studentId)
        {
            var printTracks = db.PrintTracks;
            PrintTrack printTrack = await printTracks.FirstOrDefaultAsync(p => p.StudentId == studentId);
            return printTrack;
        }

        public async static Task RunCommand(string command)
        {
            String temp = @"/c " + command;
            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = temp;
            cmdsi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            Process cmd = Process.Start(cmdsi);
            
            cmd.WaitForExit();
        }

        public static string Password(long id)
        {
            int todayDay = DateTime.Today.Day;
            int todayMonth = DateTime.Today.Month;
            int todayYear = DateTime.Today.Year;

            return ((id + todayMonth) * todayDay + (id + todayYear) * (id % todayDay + 1)).ToString("X");
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
}
