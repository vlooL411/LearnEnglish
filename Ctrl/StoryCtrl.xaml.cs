using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LearnEnglish.Ctrl
{
    public partial class StoryCtrl : UserControl
    {
        Story story;
        public StoryCtrl()
        {
            InitializeComponent();
            DataContextChanged += (o, e) =>
            {
                story = DataContext as Story;
                ComputeKnowAll();
            };
        }
        public void ComputeKnowAll()
        {
            if (story == null) return;
            var know = story.Story_Word.Where(sw => sw.Word.TypeID == 2).Count();
            var all = story.Story_Word.Where(sw => sw.Word.TypeID == null || sw.Word.TypeID == 1 || sw.Word.TypeID == 2).Count();
            var per = know * 1.0f / all;
            Back.Background = new SolidColorBrush(Color.Multiply(Brushes.Green.Color, per));

            KnowCount.Text = per.ToString() + "%";
        }

        void Remove_Click(object o, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Work.main.AddProcessBackground($"Remove story name: {story.Name}");
                if (MessageBox.Show("Are you wanting to delete story?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Dispatcher.Invoke(() => Visibility = Visibility.Collapsed);
                    Work.StoryDelete(story);
                    Work.selectStory.RemoveElement(story);
                }
                Work.main.DelProcessBackground($"Remove story name: {story.Name}");
            }).Start();
        }

        void UserControl_MouseEnter(object o, RoutedEventArgs e) => remove.Visibility = Visibility.Visible;
        void UserControl_MouseLeave(object o, RoutedEventArgs e) => remove.Visibility = Visibility.Hidden;
    }
}