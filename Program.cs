using System.Threading;
using System.Diagnostics;
using System.IO;
using TagLib;

namespace StreamStatusUpdater
{
    internal class Program
    {
        static string OutputSongName = "";
        static string OutputCountDown = "";
        static string OutputMessages = "";

        static string CurrentSong = "";
        static int TargetHrs = 0;
        static int TargetMins = 0;
        static int TargetSecs = 0;

        static bool ReachedZero = false;

        static ThreadStart PG = new ThreadStart(ProcessGather);
        static ThreadStart TU = new ThreadStart(TimerUpdate);

        static Thread T1 = new Thread(PG);
        static Thread T2 = new Thread(TU);

        static void Main(string[] args)
        {
            Console.WriteLine("Tere tulemast otseülekande juhtimissüsteemi!");
            Console.WriteLine("\nPalun määrake väljundfailide asukohad!");
            Console.WriteLine("\nMuusika väljund: ");
            OutputSongName = Console.ReadLine().ToString().Replace("\"", "");
            Console.WriteLine("Taimeri väljund: ");
            OutputCountDown = Console.ReadLine().ToString().Replace("\"", "");
            Console.WriteLine("Telegraafi väljund: ");
            OutputMessages = Console.ReadLine().ToString().Replace("\"", "");
            Console.WriteLine("\nMuusika nimi aktiivne!");
            T1.Start();
            Console.WriteLine("\nTaimeri seadistamine:");
            bool error = true;
            while (error)
            {
                try
                {
                    Console.WriteLine("\nTund (0-23): ");
                    TargetHrs = int.Parse(Console.ReadLine());
                    if ((TargetHrs > 23) || (TargetHrs < 0))
                    {
                        continue;
                    }
                    error = false;
                }
                catch { }
            }
            error = true;
            while (error)
            {
                try
                {
                    Console.WriteLine("Minut (0-60): ");
                    TargetMins = int.Parse(Console.ReadLine());
                    if ((TargetMins > 60) || (TargetMins < 0))
                    {
                        continue;
                    }
                    error = false;
                }
                catch { }
            }
            error = true;
            while (error)
            {
                try
                {
                    Console.WriteLine("Sekundid (0-60): ");
                    TargetSecs = int.Parse(Console.ReadLine());
                    if ((TargetSecs > 60) || (TargetSecs < 0))
                    {
                        continue;
                    }
                    error = false;
                }
                catch { }
            }
            Console.WriteLine("\nTaimer aktiivne!");
            T2.Start();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nProgrammi sulgemiseks vajutage Ctrl+C.\nKui soovite telegraafi määrata/muuta, kirjutage\nsoovitud teade ning seejärel vajutage sisestusklahvi.\n\nTelegraaf:");
                string output = Console.ReadLine();
                if (!string.IsNullOrEmpty(output))
                {
                    bool locked = true;
                    while (locked)
                    {
                        try
                        {
                            using (FileStream fs = System.IO.File.Create(OutputMessages))
                            {
                                StreamWriter sw = new StreamWriter(fs);
                                sw.Write(output);
                                sw.Flush();
                                sw.Dispose();
                            }
                            locked = false;
                        }
                        catch
                        {
                            /* File is in use by another process, try again */
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        static void TimerUpdate()
        {
            while (true)
            {
                int TimeHrs = DateTime.Now.Hour;
                int TimeMins = DateTime.Now.Minute;
                int TimeSecs = DateTime.Now.Second;

                int DeltaHrs = TargetHrs - TimeHrs - 1;
                if (DeltaHrs < 0) { DeltaHrs += 24; }
                int DeltaMins = TargetMins - TimeMins - 1;
                if (DeltaMins < 0) { DeltaMins += 60; }
                int DeltaSecs = TargetSecs - TimeSecs - 1;
                if (DeltaSecs < 0) { DeltaSecs += 60; }

                string StrHrs = DeltaHrs.ToString() + " tund";
                string StrMins = DeltaMins.ToString() + " minut";
                string StrSecs = DeltaSecs.ToString() + " sekund";


                if (DeltaHrs != 1) { StrHrs += "i"; }
                if (DeltaMins != 1) { StrMins += "it"; }
                if (DeltaSecs != 1) { StrSecs += "it"; }

                StrHrs += ", "; StrMins += ", ";;

                if (DeltaHrs == 0) { StrHrs = ""; }
                if (DeltaMins == 0) { StrMins = ""; }
                if (DeltaSecs == 0) { StrSecs = ""; StrMins = StrMins.Replace(", ", ""); }

                if ((DeltaMins == 0) && (DeltaSecs == 0)) { StrHrs = StrHrs.Replace(", ", ""); }

                if (StrHrs + StrMins + StrSecs == "")
                {
                    ReachedZero = true;
                }

                if (ReachedZero)
                {
                    StrHrs = "Otseülekanne algab peagi";
                }

                bool locked = true;
                while (locked)
                {
                    try
                    {
                        using (FileStream fs = System.IO.File.Create(OutputCountDown))
                        {
                            StreamWriter sw = new StreamWriter(fs);

                            if (ReachedZero)
                            {
                                sw.Write("Otseülekanne algab peagi...");
                            }
                            else
                            {
                                sw.Write("Otseülekande alguseni on jäänud " + StrHrs + StrMins + StrSecs);
                            }
                            sw.Flush();
                            sw.Dispose();
                        }
                        locked = false;
                    }
                    catch
                    {
                        /* File is in use by another process, try again */
                        Thread.Sleep(100);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        static void ProcessGather()
        {
            while (true)
            {
                Process[] processes = Process.GetProcesses();
                bool success = false;
                foreach (Process p in processes)
                {
                    if (!String.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        if (p.MainWindowTitle.Contains("VLC"))
                        {
                            if (p.MainWindowTitle.Contains("-"))
                            {
                                CurrentSong = p.MainWindowTitle.Replace(p.MainWindowTitle.Split('-')[^1], "").Replace(".mp3", "");
                                CurrentSong = CurrentSong.Substring(0, CurrentSong.Length - 2);
                            } else
                            {
                                CurrentSong = "Hetkel ei mängi ühtegi muusikapala";
                            }
                            success = true;
                            break;
                        }
                        else if (p.ProcessName.Contains("mpc"))
                        {
                            TagLib.File f;
                            try
                            {
                                f = TagLib.File.Create(p.MainWindowTitle, ReadStyle.PictureLazy);
                            } catch
                            {
                                try
                                {
                                    f = TagLib.File.Create(p.MainWindowTitle);
                                } catch
                                {
                                    CurrentSong = p.MainWindowTitle.Split('\\')[^1];
                                    CurrentSong = CurrentSong.Replace("." + CurrentSong.Split('.')[^1], "");

                                    if (CurrentSong == "Drag")
                                    {
                                        CurrentSong = "Muusika vahetamine...";
                                    }
                                    if (CurrentSong == "Media Player Classic Home Cinema")
                                    {
                                        CurrentSong = "Hetkel ei mängi ühtegi muusikapala";
                                    }
                                    success = true;
                                    continue;
                                }
                            }
                            Tag tags = f.GetTag(TagTypes.Id3v2);
                            if (tags == null)
                            {
                                tags = f.GetTag(TagTypes.Id3v1);
                            }
                            if (tags == null)
                            {
                                tags = f.GetTag(TagTypes.AllTags);
                            }
                            if (tags == null)
                            {
                                CurrentSong = p.MainWindowTitle.Split('\\')[^1];
                                CurrentSong = CurrentSong.Replace("." + CurrentSong.Split('.')[^1], "");
                                success = true;
                                break;
                            }
                            if ((tags.Artists.Length == 0) && (tags.Album == null) && (tags.Title != null))
                            {
                                CurrentSong = tags.Title;
                            }
                            else if ((tags.Album == null) && (tags.Title != null))
                            {
                                CurrentSong = string.Join(", ", tags.Artists) + " - " + tags.Title;
                            }
                            else if (tags.Title == null)
                            {
                                CurrentSong = p.MainWindowTitle.Split('\\')[^1];
                                CurrentSong = CurrentSong.Replace("." + CurrentSong.Split('.')[^1], "");
                            }
                            else
                            {
                                if (tags.Artists.Length > 0)
                                {
                                    CurrentSong = string.Join(", ", tags.Artists) + " - " + tags.Title + " <" + tags.Album + ">";
                                } else
                                {
                                    CurrentSong = tags.Title + " <" + tags.Album + ">";
                                }
                            }
                            success = true;
                            break;
                        }
                    }
                }
                if (!success)
                {
                    CurrentSong = "Meediumiesitajat pole avatud";
                }
                bool locked = true;
                while (locked)
                {
                    try
                    {
                        using (FileStream fs = System.IO.File.Create(OutputSongName))
                        {
                            StreamWriter sw = new StreamWriter(fs);
                            sw.Write(CurrentSong);
                            sw.Flush();
                            sw.Dispose();
                        }
                        locked = false;
                    }
                    catch
                    {
                        /* File is in use by another process, try again */
                        Thread.Sleep(100);
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}