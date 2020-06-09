using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LearnEnglish
{
    public partial class StoryShow : Page
    {
        public delegate void Function();
        public event Function OnLoadWords, UnLoadWords;
        public StoryShow()
        {
            InitializeComponent();
            OnLoadWords += () => image.Visibility = Visibility.Visible;
            UnLoadWords += () => image.Visibility = Visibility.Collapsed;
        }
        Task taskRunFill;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        public async void Fill(Story s)
        {
            OnLoadWords.Invoke();
            if (taskRunFill?.Status == TaskStatus.Running) tokenSource.Cancel();
            await (taskRunFill = Task.Run(() =>
            {
                if (s == null) return;
                Dispatcher.Invoke(async () =>
                {
                    DataContext = s;
                    WordTranslate.Items.Clear();

                    var swords = s.Story_Word.Where(sw => sw.Word.TypeID == null || sw.Word?.TypeID == 1);
                    Count.Text = swords.Count().ToString();
                    await Task.Run(() =>
                    {
                        swords.OrderBy(sw => sw.Word.Text).AsParallel().AsOrdered().
                            ForAll(sw => Dispatcher.Invoke(() => WordTranslate.Items.Add(sw?.Word)));
                        Dispatcher.Invoke(() => UnLoadWords.Invoke());
                    }, tokenSource.Token);
                });
            }, tokenSource.Token));
        }
        public async void Fill(string header, byte i)
        {
            OnLoadWords.Invoke();
            if (taskRunFill?.Status == TaskStatus.Running) tokenSource.Cancel();
            await (taskRunFill = Task.Run(() =>
                Dispatcher.Invoke(async () =>
                {
                    DataContext = new Story() { Name = header };
                    WordTranslate.Items.Clear();
                    var words = Work.bd.Words.Where(w => w.TypeID == i);
                    Count.Text = words.Count().ToString();
                    await Task.Run(() =>
                    {
                        words.OrderBy(a => a.Text).AsParallel().AsOrdered().
                          ForAll(w => Dispatcher.Invoke(() => WordTranslate.Items.Add(w)));
                        Dispatcher.Invoke(() => UnLoadWords.Invoke());
                    }, tokenSource.Token);
                }), tokenSource.Token));
        }

        public void RemoveElement(Word w) => Dispatcher.Invoke(() =>
             {
                 WordTranslate.Items.Remove(w);
                 if (DataContext != null)
                     Count.Text = (DataContext as Story).Story_Word.Where(sw => sw.Word.TypeID == null || sw.Word?.TypeID == 1).Count().ToString();
             });

        void TestRun_Click(object o, System.Windows.RoutedEventArgs e)
        {
            if (WordTranslate.Items.Count < 5) return;
            var words = new List<Word>();
            foreach (var w in WordTranslate.Items)
                words.Add(w as Word);
            Work.Test.Fill(words, Header.Text);
            Work.frame.Navigate(Work.Test);
        }
    }
}