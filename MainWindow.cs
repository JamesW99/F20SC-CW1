using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace MyBrowser
{
    class MainWindow : Window
    {
        // statement UI
        [UI] SearchEntry URLBar = null;
        [UI] TextView TextView = null;
        [UI] Button HomeButton = null;
        [UI] Button PrevButton = null;
        [UI] Button NextButton = null;
        [UI] Box GotoMenuBox = null;
        [UI] Box HistoryMenuBox = null;
        [UI] Button BulkButton = null;

        public static HttpClient client = null;

        Fetcher Fetcher;

        // List<string> BackList;
        List<string> BackList = new List<string>();
        int Current;
        

        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
            Fetcher = new Fetcher();
        }

        void updateGotoMenu(Parser Parser)
        {
            foreach (Widget Child in GotoMenuBox.Children)
                GotoMenuBox.Remove(Child);

            foreach (KeyValuePair<String, Uri> KV in Parser.findLinks())
            {
                ModelButton Tmp = new ModelButton();
                Tmp.Text = KV.Key;
                Tmp.Show();
                Tmp.Clicked += async delegate {
                    Console.WriteLine("??? activated!");
                    await navigateTo(KV.Value.ToString(), true);
                };
                GotoMenuBox.Add(Tmp);
            }
        }


        
        // public static List<String> history =  new List<string>();
        // void iniHistory(){
        //     int counter = 0;
        //     foreach (string line in System.IO.File.ReadLines(@"History.txt")) 
        //     {  
        //         history.Add(line);
        //         counter++;  
        //     }
        // }

        public  List<String> history =  new List<string>();
        void iniHistory(){
            int counter = 0;
            foreach (string line in System.IO.File.ReadLines(@"History.txt")) 
            {  
                history.Add(line);
                counter++;
            }
        }

        
        void updateHistoryMenu(String URL)
        {
            iniHistory();
            // foreach (Widget Child in HistoryMenuBox.Children)
            //     HistoryMenuBox.Remove(Child);
            
            ModelButton Tmp = new ModelButton();
            foreach (String s in history){
                Tmp.Text = s;
                HistoryMenuBox.Add(Tmp);
            }
        }

        // homepage
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            URLBar.Activated += OnUrlEnter;
            //home page
            HomeButton.Clicked += async delegate
            {
                // await navigateTo("https://www2.macs.hw.ac.uk/~yw2007/", true);
                await navigateTo("http://168.138.47.113/file/", true);
                
            };
            //back 
            PrevButton.Clicked += async delegate
            {
                if (Current <= BackList.Count && Current > 1)
                {   
                    Current --;
                    await navigateTo(BackList[Current-1], false);
                }
            };
            NextButton.Clicked += async delegate
            {
                if (Current < BackList.Count)
                {
                    Current ++;
                    await navigateTo(BackList[Current-1], false);
                }
            };
            
        }

        // quit
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Console.WriteLine("Exiting");
            Application.Quit();
        }

        void log(string Content)
        {
            Console.WriteLine(Content);
            TextView.Buffer.Text += Content + "\n";
        }

        List<String> WaitForDownload =  new List<string>();
        void BulkDownload()
        {
            BulkButton.Clicked += async delegate
            {
                try{
                    int counter = 0;
                    foreach (string line in System.IO.File.ReadLines(@"Bulk.txt")) {
                        log(line);
                        // request line
                        // get statuscode len(html) and url
                        counter++;
                    }
                    }
                    catch{log("open Bulk.txt faild");}

                    // try{
                    //     foreach()
                    // }
                    // catch{log("open Bulk.txt faild");}
            };
            

        }

        async Task AddFavorite(string link)
        {
            //add a side to Favorite.txt
            try
            {
                using StreamWriter file = new("Favorite.txt", append: true);
                // await file.WriteLineAsync(link);
            }
            catch (Exception e)
            {
                log("Failed to add: " + e);
            }
        }

        async Task DeleteFavorite(String link)
        {
            try
            {
                // read favorite.txt to a string[]
                string path = @"Favorite.txt";
                StreamReader sr = new StreamReader(path);
                sr.ReadLine().Replace(link,"");
                string[] lines =
                {
                    "First line", "Second line", "Third line" 
                };

                await File.WriteAllLinesAsync("favorite.txt", lines);
            }

            catch (Exception e)
            {
                log("Failed to add delete the Favorite: " + e);
            }
        }
        

        async Task navigateTo(string URL, bool Save = false)
        {
            if (!URL.Contains("://"))
                URL = "http://" + URL;
            log("Requesting " + URL);

            TextView.Buffer.Text = "";
            URLBar.Text = URL;

            //add the visiting site to history.txt
            try
            {
                using StreamWriter file = new("History.txt", append: true);
                await file.WriteLineAsync(URL);
            }
            catch (Exception e)
            {
                log("Failed to add: " + e);
            }

            if (Save == true)
            {
                Current ++;
                BackList.Add(URL);
            }
            else{
                Current = BackList.Count;
            }

            updateHistoryMenu(URL);
            // try{
            //     updateHistoryMenu(URL);
            // }
            // catch(Exception e){
            //     log("Failed to updateHistoryMenu: " + e);
            // }


            // request
            try{
                await Fetcher.fetch(URL);
                int BodyLength = Fetcher.Body.Length;
                log("Fetched content with code " + Fetcher.Code + " in " + BodyLength + " bytes");

                int LengthToShow = BodyLength < 100 ? BodyLength : 100;
                log("Content (first 100 bytes):\n" + Fetcher.Body.Substring(0, LengthToShow));
            }
            catch (Exception e)
            {
                log("Failed to fetch: " + e);
            }

            // Show the html code
            try{
                Parser Parser = new Parser(Fetcher.LastUri, Fetcher.Body);
                Title = Parser.getTitle();
                log("Text content:\n" + Parser.getTextSummary());
                updateGotoMenu(Parser);
            }
            catch (Exception e){
                log("Failed to parse: " + e);
            }
        }

        private async void OnUrlEnter(object sender, EventArgs args){
            await navigateTo(URLBar.Text, false);
        }
    }
}