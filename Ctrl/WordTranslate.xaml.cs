using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LearnEnglish.Ctrl
{
    public partial class WordTranslate : UserControl
    {
        Word word;
        public WordTranslate()
        {
            InitializeComponent();
            Loaded += (o, e) => word = DataContext as Word;
        }
        void Translate()
        {
            if (word != null)
                if (word?.Translate == null)
                {
                    word.Translate = TranslateYandex.Translate(word.Text);
                    Work.SaveSafe();
                    Dispatcher.Invoke(() =>
                    {
                        DataContext = null;
                        DataContext = word;
                    });
                }
        }
        async void UserControl_MouseDoubleClick(object o, MouseButtonEventArgs e) => await Task.Run(() => Translate());

        void UserControl_MouseEnter(object o, MouseEventArgs e) => bts.Visibility = Visibility.Visible;
        void UserControl_MouseLeave(object o, MouseEventArgs e) => bts.Visibility = Visibility.Hidden;

        void ChangeType(byte i)
        {
            if (word.TypeID != i)
            {
                Visibility = Visibility.Collapsed;
                Work.TypeWordAsync(word, i); ;
            }
        }
        void K_Click(object o, RoutedEventArgs e) => ChangeType(2);
        void W_Click(object o, RoutedEventArgs e) => ChangeType(3);
        void B_Click(object o, RoutedEventArgs e) => ChangeType(4);

        void R_Click(object o, RoutedEventArgs e) => ChangeType(1);
    }
}