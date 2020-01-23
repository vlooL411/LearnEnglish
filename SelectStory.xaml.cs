using LearnEnglish.Ctrl;
using Microsoft.Win32;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LearnEnglish
{
    public partial class SelectStory : Page
    {
        public SelectStory()
        {
            InitializeComponent();
            frame.Children.Add(Work.frameSelectStory);
            Work.frameSelectStory.Navigate(Work.storyShow);
            Loaded += (o, e) => Work.AllTranslateAsync(); ;
            Update();
        }

        async void Update() => await Task.Run(() =>
                 {
                     ComputeKnowAndUnCount();
                     Dispatcher.Invoke(() =>
                     {
                         Stories.Items.Clear();
                         foreach (var s in Work.bd.Stories.OrderBy(d => d.Name))
                             Stories.Items.Add(new StoryCtrl() { DataContext = s });
                     });
                 });
        CancellationTokenSource tokenComputeCount = new CancellationTokenSource();
        public async void ComputeKnowAndUnCount()
        {
            if (tokenComputeCount.IsCancellationRequested) tokenComputeCount.Cancel();
            await Task.Run(() => Dispatcher.Invoke(() =>
            {
                try
                {
                    StoryCount.Text = Work.bd.Stories.Count().ToString();
                    KnowCount.Text = Work.bd.Story_Word.Select(a => a.Word).Distinct().Where(w => w.TypeID == 2).Count().ToString();
                    UnknowCount.Text = Work.bd.Story_Word.Select(a => a.Word).Distinct().Where(w => w.TypeID == 1 || w.TypeID == null).Count().ToString();
                    foreach (var item in Stories.Items) (item as StoryCtrl).ComputeKnowAll();
                }
                catch { }
            }), tokenComputeCount.Token);
        }
        public void RemoveElement(Story s)
        {
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < Stories.Items.Count; i++)
                    if ((Stories.Items[i] as StoryCtrl).DataContext == s)
                    {
                        Stories.Items.RemoveAt(i);
                        break;
                    }
            });
            ComputeKnowAndUnCount();
        }
        void Stories_SelectionChanged(object o, SelectionChangedEventArgs e) => Dispatcher.Invoke(() =>
                      Work.storyShow.Fill((Stories.SelectedItem as StoryCtrl)?.DataContext as Story));

        void AddingStory_Click(object o, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "(*.txt)|*.txt|(*.csv)|*.csv";
            dialog.ShowDialog();
            ReadFilesAsync(dialog.FileNames);
        }
        void AddingStory_DragDrop(object o, DragEventArgs e) => ReadFilesAsync(e.Data.GetData(DataFormats.FileDrop) as string[]);

        async void ReadFilesAsync(string[] paths) => await Task.Run(() => ReadFiles(paths));
        void ReadFiles(string[] paths)
        {
            foreach (var p in paths)
                Work.main.AddProcessBackground(p);
            lock (Work.readfile)
                foreach (var p in paths)
                    new Thread(() =>
                    {
                        var story = Work.ReadFile(p).Result;
                        if (story != null)
                            Dispatcher.Invoke(() => Stories.Items.Add(new StoryCtrl() { DataContext = story }));
                        Work.main.DelProcessBackground(p);
                    })
                    { IsBackground = true }.Start();
            ComputeKnowAndUnCount();
            lock (Work.readfile)
                Work.AllTranslateAsync();
        }

        void Know_Click(object o, RoutedEventArgs e) { Work.storyShow.Fill("KnowLedge", 2); Stories.SelectedIndex = -1; }
        void White_Click(object o, RoutedEventArgs e) { Work.storyShow.Fill("White list", 3); Stories.SelectedIndex = -1; }
        void Black_Click(object o, RoutedEventArgs e) { Work.storyShow.Fill("Black list", 4); Stories.SelectedIndex = -1; }
    }
}