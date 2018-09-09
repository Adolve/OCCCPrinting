using System;
using System.Collections.Concurrent;
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
using System.Runtime.CompilerServices;

namespace OCCCPrinting
{

    class Program
    {
        static private OCCCPrintingDbContext db;

        static async Task Main(string[] args)
        {

            Console.WriteLine(Password(123));


            // Initialization of the db context 
            db = new OCCCPrintingDbContext();

            // Making sure that the spooler service is running
            AsyncPump.Run(async delegate
            {
                await ExecuteCmd.ExecuteCommandAsync("net start spooler");
            });


            Console.WriteLine("The app is running!!");

            // Start watching the printing event
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
            new EventQuery("SELECT * FROM    __InstanceCreationEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PrintJob'"));
            startWatch.EventArrived += new EventArrivedEventHandler(mewPrintJobs_EventArrived);
            startWatch.Start();

            // 
            Thread.Sleep(999999999);
            //Console.ReadKey();

        }

        public static async void mewPrintJobs_EventArrived(object sender, EventArrivedEventArgs e)
        {
            
                // Loading the setting file
                var settings = Properties.Settings.Default;
                settings.Reload();

                //Check if the program is enable
                Console.WriteLine("Checking if the program is enable");
                if (!settings.IsPageLimitEnable)
                {
                    Console.WriteLine("Page Limit Disabled");
                    return;
                }

                // Getting the print even infos
                Console.WriteLine("Getting the print even infos");
                ManagementBaseObject printJob = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
                string printerName = printJob.Properties["Name"].Value.ToString().Split(',')[0];
                int JobId = Convert.ToInt32(printJob.Properties["JobId"].Value);

                // Exclude the printers from the excluded printers list 
                Console.WriteLine("Exclude the printers from the excluded printers list ");
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

                // Waiting for the Spooling to end
                Console.WriteLine("Waiting for the Spooling to end...");
                while (job.IsSpooling)
                {
                    job.Refresh();
                }
                Console.WriteLine("Spooling ended");

                Console.WriteLine("Stoping the Spooler Service");
                var StopSpoolerTask = ExecuteCmd.ExecuteCommandAsync("net stop spooler");

                int numberOfPages = job.NumberOfPages;


                Console.WriteLine("number of pages : " + numberOfPages);
                if (settings.PageLimit < numberOfPages)
                {
                    MessageBox.Show("You can only print " + settings.PageLimit + " pages per day", "Prompt",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                    ExecuteCmd.ExecuteCommandSync("net start spooler");
                    job.Cancel();
                    return;
                }


                PasswordPrompt form = new PasswordPrompt();
                form.ShowDialog();

                Console.WriteLine("Waiting for the spooler to stop to start it again...");
                AsyncPump.Run(async delegate
                {
                    await StopSpoolerTask;
                });
                var StartSpoolerTask = ExecuteCmd.ExecuteCommandAsync("net start spooler");

                Task<PrintTrack> queryRequest = GetPrintTrack(form.StudentId);

                long sid;
                long.TryParse(form.StudentId, out sid);

                var password = Password(sid);
                if (form.Password == password)
                {
                    Console.WriteLine("printing ...");
                    PrintTrack printTrack=null ;
                    AsyncPump.Run(async delegate
                    {
                        printTrack = await queryRequest;
                        await StartSpoolerTask;
                    });
                    if (printTrack != null)
                    {
                        if (settings.PageLimit >= printTrack.PagesPrinted + numberOfPages)
                        {
                            job.Resume();
                            printTrack.PagesPrinted += numberOfPages;
                            db.SaveChanges();
                        }
                        else
                        {
                            job.Cancel();
                            MessageBox.Show("You can only print " + (settings.PageLimit - printTrack.PagesPrinted) + " pages more for today");
                        }
                    }
                    else
                    {
                        if (settings.PageLimit >= numberOfPages)
                        {
                            job.Resume();
                            MessageBox.Show("number of pages : " + numberOfPages);
                            PrintTrack newPrintTrack = new PrintTrack
                            {
                                StudentId = form.StudentId,
                                Password = form.Password,
                                PagesPrinted = numberOfPages,
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
                    AsyncPump.Run(async delegate
                    {
                        await StartSpoolerTask;
                    });

                    job.Cancel();
                    MessageBox.Show("Invalid password!");
                }

                form.Dispose();

                Console.WriteLine("Done!");
            
            
        }

        public async static Task<PrintTrack> GetPrintTrack(string studentId)
        {
            var printTracks = db.PrintTracks;
            PrintTrack printTrack = await printTracks.FirstOrDefaultAsync(p => p.StudentId == studentId);
            return printTrack;
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
