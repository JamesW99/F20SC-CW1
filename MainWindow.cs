using UI = Gtk.Builder.ObjectAttribute;
using Gtk;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;  
using System.Runtime.Serialization.Formatters.Binary;  

namespace MyBrowser
{
    class MainWindow : Window
    {
        // statement UI
        [UI] SearchEntry URLBar = null;
        [UI] SearchEntry BulkBar = null; // string "Bulk.txt"
        [UI] Button BulkButton = null;
        [UI] Button FavoriteMenuButton = null;
        [UI] TextView TextView = null;
        [UI] Button HomeButton = null;
        [UI] Button PrevButton = null;
        [UI] Button Rename = null;
        [UI] Button NextButton = null;
        [UI] Box GotoMenuBox = null;
        [UI] Box HistoryMenuBox = null;
        [UI] Button BulkDownloadButton = null;
        [UI] Button FavoriteButton  = null;
        [UI] Box FavoriteMenuBox = null;
        [UI] Popover EditPopover = null;
        [UI] Popover RenamePopover = null;
        [UI] Entry RenameEntry = null;
        List <KeyValuePair <string, string>> fav = new List <KeyValuePair<string, string>>();

        public static HttpClient client = null;

        Fetcher Fetcher;
        String webtitle = "";

        // List<string> BackList;
        List<string> BackList = new List<string>();
        int Current;
        
        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        public void SerializeNow() {  
            object c = fav;
            using (FileStream fs = File.OpenWrite("favorite.dat")) {
              BinaryFormatter b = new BinaryFormatter();
              b.Serialize(fs, c);
              log("SerializeNow success!");
            }
        }  
        // iniFavorite
        public void DeSerializeNow() {  
            try{
                using (FileStream fs = File.OpenRead("favorite.dat")) {
                BinaryFormatter b = new BinaryFormatter();  
                fav = (List <KeyValuePair <string, string>>) b.Deserialize(fs);
                }
            }catch (Exception e) {
              log("failed to read favorite: " + e);
            }
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
        
        void updateFavoriteMenu()
        {
            foreach (Widget Child in FavoriteMenuBox.Children)
                FavoriteMenuBox.Remove(Child);

            foreach (KeyValuePair<string, string> KV in fav)
            {
                ModelButton Tmp = new ModelButton();
                Tmp.Text = KV.Key;
                Tmp.Show();
                Tmp.Clicked += async delegate {
                    Console.WriteLine("??? activated!");
                    await navigateTo(KV.Value.ToString(), true);
                };
                Tmp.ButtonPressEvent += async delegate (object x, ButtonPressEventArgs y) {
                    EditPopover.RelativeTo = Tmp;
                    EditPopover.Show();
                };
                FavoriteMenuBox.Add(Tmp);
            }
        }

        // void FavoriteAddButton(){
        //     KeyValuePair<String, String> tmp = new KeyValuePair<string, string>(webtitle ,URLBar.Text);
        //     fav.Add(tmp);
        // }

        public  List<String> history =  new List<string>();
        
        void iniHistory(){
            int counter = 0;
            foreach (string line in System.IO.File.ReadLines(@"History.txt")) 
            {  
                history.Add(line);
                counter++;
            }
            foreach (String s in history){
                ModelButton Tmp = new ModelButton();
                Tmp.Text = s;
                Tmp.Show();
                Tmp.Clicked += async delegate {
                    await navigateTo(s);
                };
                // Tmp.ButtonPressEvent += async delegate (object x, ButtonPressEventArgs y) {
                //     log("event is " + y);
                // };
                HistoryMenuBox.Add(Tmp);
            }
        }
       
        void updateHistoryMenu(String URL)
        {
            ModelButton Tmp = new ModelButton();
            Tmp.Text = URL;
            Tmp.Show();
            Tmp.Clicked += async delegate {
                await navigateTo(URL);
            };
            HistoryMenuBox.Add(Tmp);
            
        }

        // homepage
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            iniHistory();
            DeSerializeNow();
            updateFavoriteMenu();

            DeleteEvent += Window_DeleteEvent;
            URLBar.Activated += OnUrlEnter;
            BulkBar.Activated += OnpathEnter;

            BulkDownloadButton.Clicked += async delegate
            {
              FileChooserDialog fcd = new Gtk.FileChooserDialog ("Open File", null, Gtk.FileChooserAction.Open);
              fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
              fcd.AddButton(Gtk.Stock.Open, Gtk.ResponseType.Ok);
              fcd.DefaultResponse = Gtk.ResponseType.Ok;
              fcd.SelectMultiple = false;

              Gtk.ResponseType response = (Gtk.ResponseType) fcd.Run ();
              if (response == Gtk.ResponseType.Ok)
              {
                foreach (string line in System.IO.File.ReadLines(fcd.Filename)) {
                    if (!line.Contains("://")){
                            String link = "http://" + line;
                            await Fetcher.fetch(link);
                        }
                        else{
                            await Fetcher.fetch(line);
                        }
                        int BodyLength = Fetcher.Body.Length;
                        log("<" + Fetcher.Code + "> <" + BodyLength + "> <" + line + ">");
                }
              }
              try{fcd.Destroy();} catch{}
            };

            //home page
            HomeButton.Clicked += async delegate
            {
                await navigateTo("https://www2.macs.hw.ac.uk/~yw2007/", true);
                // await navigateTo("http://168.138.47.113/file/", true);
                
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
            
            FavoriteButton.Clicked += async delegate{
                AddFavorite();
                // FavoriteAddButton();
            };

            // add Favorite
            FavoriteMenuButton.Clicked += async delegate
            {
                // AddFavorite();
            };
            
            // BulkButton right Clicked
            BulkDownloadButton.ButtonPressEvent += async delegate (object x, ButtonPressEventArgs y) 
            {
                // foreach (string line in System.IO.File.ReadLines(@"bulk.txt")) 
                foreach (string line in System.IO.File.ReadLines(@"bulk.txt")) {
                        if (!line.Contains("://")){
                            String link = "http://" + line;
                            await Fetcher.fetch(link);
                        }
                        else{
                            await Fetcher.fetch(line);
                        }
                        int BodyLength = Fetcher.Body.Length;
                        log("<" + Fetcher.Code + "> <" + BodyLength + "> <" + line + ">");
                }
            };

            RenamePopover.RelativeTo = Rename;
            RenamePopover.Position = PositionType.Right;
            Rename.Clicked += async delegate {
                RenamePopover.Show();
            };

            Fetcher = new Fetcher();
            navigateTo("http://www2.macs.hw.ac.uk/~yw2007/", true);
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


        async Task AddFavorite()
        {
            KeyValuePair<String, String> tmp = new KeyValuePair<string, string>(webtitle ,URLBar.Text);
            fav.Add(tmp);
            log(tmp.Key + " "+tmp.Value);
            SerializeNow();
            
        }

        
        

        async Task navigateTo(string URL, bool Save = false)
        {
            updateHistoryMenu(URL);

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
                Title =  "Status Code: " + Fetcher.Code + ", Web Title: " + Parser.getTitle();
                log("Text content:\n" + Parser.getTextSummary());
                updateGotoMenu(Parser);
                webtitle = Parser.getTitle();
            }
            catch (Exception e){
                log("Failed to parse: " + e);
            }
            
        }

        private async void OnUrlEnter(object sender, EventArgs args){
            await navigateTo(URLBar.Text, true);
        }

        private async void OnpathEnter(object sender, EventArgs args){
            BulkButton.Clicked += async delegate
            {
                // try{
                    // int counter = 0;
                    foreach (string line in System.IO.File.ReadLines(BulkBar.Text)) {
                        if (!line.Contains("://")){
                            String link = "http://" + line;
                            await Fetcher.fetch(link);
                        }
                        else{
                            await Fetcher.fetch(line);
                        }
                        int BodyLength = Fetcher.Body.Length;
                        log("<" + Fetcher.Code + "> <" + BodyLength + "> <" + line + ">");
                    }
            };
            
        }
    }
}
