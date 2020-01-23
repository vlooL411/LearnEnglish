using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LearnEnglish
{
    static class Work
    {
        static public EnglishEntities bd = new EnglishEntities();
        static public Frame frame = new Frame() { NavigationUIVisibility = NavigationUIVisibility.Hidden };
        static public Frame frameSelectStory = new Frame() { NavigationUIVisibility = NavigationUIVisibility.Hidden };
        static public MainWindow main;
        static public StoryShow storyShow = new StoryShow();
        static public SelectStory selectStory = new SelectStory();
        static public TestStoryWord Test = new TestStoryWord();

        static public object saveSafe = new object();
        static public object readfile = new object();

        static public void WaitingClose() { lock (readfile) lock (saveSafe) { } }
        static public void Save()
        {
            try { bd.SaveChanges(); }
            catch
            {
                new Thread(() =>
                {
                    main.AddProcessBackground("Save error changes");
                    Thread.Sleep(5000);
                    main.DelProcessBackground("Save error changes");
                }).Start();
            }
        }
        static public void SaveSafe()
        {
            main.AddProcessBackground("Save changes");
            lock (saveSafe) Save();
            main.DelProcessBackground("Save changes");
        }

        static public object LcokChangeTypeWork = new object();
        static public async void TypeWordAsync(Word w, byte i) => await Task.Run(() => TypeWord(w, i));
        static public void TypeWord(Word w, byte i)
        {
            if (w.TypeID == i || (w.TypeID == null || w.TypeID == 1) && i == 1) return;
            w.TypeID = i;
            SaveSafe();
            storyShow.RemoveElement(w);
            selectStory.ComputeKnowAndUnCount();
        }

        static public void AllTranslate(Story s)
        {
            main.AddProcessBackground($"Translate story name: {s.Name}");
            int end = 0;
            do
            {
                lock (saveSafe)
                {
                    try
                    {
                        var swords = s.Story_Word.Where(sw => (sw.Word.Translate == null || sw.Word.Translate == "") && (sw.Word.TypeID != 3 || sw.Word.TypeID == 4));
                        end = swords.Count();
                        if (end > 250) swords.Take(100).AsParallel().ForAll(sw => sw.Word.Translate = TranslateYandex.Translate(sw.Word.Word1));
                        else swords.AsParallel().ForAll(sw => sw.Word.Translate = TranslateYandex.Translate(sw.Word.Word1));
                    }
                    catch { }
                    Save();
                }
            }
            while (end > 0);
            main.DelProcessBackground($"Translate story name: {s.Name}");
        }
        static public void AllTranslate()
        {
            main.AddProcessBackground("Translate all stories");
            int end = 0;
            int translateWordTime = 10;
            do
            {
                lock (saveSafe)
                {
                    try
                    {
                        var words = bd.Words.Where(w => (w.Translate == null || w.Translate == "") && (w.TypeID != 3 || w.TypeID == 4));
                        end = words.Count() - translateWordTime;
                        words.Take(translateWordTime).AsParallel().ForAll(w => w.Translate = TranslateYandex.Translate(w.Word1));
                    }
                    catch { end += translateWordTime; }
                    Save();
                }
            }
            while (end > 0);
            main.DelProcessBackground("Translate all stories");
        }
        static public async void AllTranslateAsync(Story s) => await Task.Run(() => AllTranslate(s));
        static public async void AllTranslateAsync() => await Task.Run(() => AllTranslate());

        static public string[] Decompize(string line)
        {
            line = line.ToLower();
            line = line.Replace("'", " ");
            line = line.Replace("n't", " ");

            foreach (var l in line)
                if ((l < 97 || l > 122) && l != 32)
                    line = line.Replace(l, ' ');

            return line.Split(' ').Where(l => l != "" && l != "'").Distinct().ToArray();
        }

        static public void Story_WordAdd(string w, Story s) => Story_WordAdd(new Word() { Word1 = w }, s);
        static public void Story_WordAdd(Word w, Story s)
        {
            var words = bd.Words.Where(wr => wr.Word1 == w.Word1);
            try { s.Story_Word.Add(new Story_Word() { Word = words.Count() == 0 ? w : words.First() }); }
            catch { Thread.Sleep(10); Story_WordAdd(w, s); }
        }

        static public object lockStoryAdd = new object();
        static public Story StoryAdd(string[] line, string name)
        {
            var s = new Story() { Name = name };
            lock (saveSafe)
            {
                bd.Stories.Add(s);
                foreach (var l in line)
                    Story_WordAdd(l, s);
                Save();
            }
            return s;
        }
        static public void StoryDelete(Story s)
        {
            if (s == null) return;
            lock (readfile)
            {
                bd.Story_Word.RemoveRange(s.Story_Word);
                try { bd.Stories.Remove(s); } catch { }
            }
            SaveSafe();
        }

        static bool CheckRepeatNameStory(string name)
        {
            try { return bd.Stories.Where(st => st.Name == name).Count() == 0; }
            catch { Thread.Sleep(10); return CheckRepeatNameStory(name); }
        }
        static public async Task<Story> ReadFile(string path)
        {
            var extend = path.Split('.').Last();
            if (File.Exists(path) && extend == "txt" || extend == "csv")
            {
                string[] line = null;
                var read = new StreamReader(File.OpenRead(path));
                var token = new CancellationTokenSource();

                var taskReadFile = Task.Run(() =>
                {
                    string str = null;
                    try
                    {
                        str = read.ReadToEnd();
                        read.Close();
                    }
                    catch
                    {
                        read.Close();
                        MessageBox.Show($"Error open file,\npath: {path}", "Warning");
                        return false;
                    }
                    line = Decompize(str);
                    return true;
                }, token.Token);
                await taskReadFile;

                var name = path.Split('\\').Last().Split('.')[0];
                if (!CheckRepeatNameStory(name))
                    if (MessageBox.Show($"File name: {name}\nDo you want adding file,\nso it's story in the same yet exist",
                                    "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        token.Cancel();
                        read.Close();
                        return null;
                    }
                    else
                    {
                        string randstr;
                        do { randstr = new Random().Next().ToString(); }
                        while (!CheckRepeatNameStory(name + randstr));
                        name += randstr;
                    }

                taskReadFile.Wait();
                if (taskReadFile.Result)
                    return StoryAdd(line, name);
            }
            else MessageBox.Show($"File isn't existing or finding or extend file wrong,\npath: {path}", "Warning");
            return null;
        }
    }
}