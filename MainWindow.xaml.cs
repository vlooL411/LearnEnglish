using System.Windows;

namespace LearnEnglish
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Work.main = this;
            InitializeComponent();
            gr.Children.Add(Work.frame);
            Work.frame.Navigate(Work.selectStory);
            Closed += (o, e) => Work.WaitingClose(); //Ожидает завершения чтения добавленных файлов
        }

        void ChangeCurrentProccess() =>
            CurrentProcess.Text = ListsProcess.Items.Count == 0 ? "" : ListsProcess.Items[0].ToString();
        public void AddProcessBackground(string back) => Dispatcher.Invoke(() =>
                {
                    try
                    {
                        ListsProcess.Items.Add(back);
                        ChangeCurrentProccess();
                    }
                    catch { }
                });
        public void DelProcessBackground(string back) => Dispatcher.Invoke(() =>
                {
                    try
                    {
                        ListsProcess.Items.Remove(back);
                        ChangeCurrentProccess();
                    }
                    catch { }
                });
        void TurnOnAlert_Click(object o, RoutedEventArgs e) => AlertProcesses.Visibility = AlertProcesses.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }
}