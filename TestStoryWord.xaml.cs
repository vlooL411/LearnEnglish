using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LearnEnglish
{
    public class TestPage
    {
        public TestPage() { Words = new List<Word>(); }
        public string Header { get; set; }
        public int CurrentWord { get; set; }
        public List<Word> Words { get; set; }
    }
    public partial class TestStoryWord : Page
    {
        List<Word> words;
        TestPage test;
        public TestStoryWord() => InitializeComponent();
        public void Fill(List<Word> ws, string header)
        {
            words = ws.Where(w => w.Translate != null && w != null && w.Text != null).ToList();
            if (words.Count == 0) Work.frame.GoBack();
            test = new TestPage() { Header = header };
            Next();
        }
        void Back_Click(object o, RoutedEventArgs e) => Work.frame.GoBack();

        Word GetWord()
        {
            Word word = words.Count == 0 ? null : words[new Random().Next(0, words.Count)];
            words.Remove(word);
            return word;
        }
        void Next()
        {
            if (words == null || words?.Count < 5) Work.frame.GoBack();

            test.Words.Clear();
            var count = words.Count % 4;
            for (int i = 0; i < (count != 0 ? 4 : count); i++)
                test.Words.Add(GetWord());

            if (!test.Words.Any())
                test.CurrentWord = new Random().Next(0, test.Words.Count);

            CurrentWord.Text = test.Words[test.CurrentWord].Text;
            DataContext = null;
            DataContext = test;
        }
        void Next_Click(object o, RoutedEventArgs e)
        {
            ListBoxWords.SelectedIndex = -1;
            foreach (var st in ListBoxWords.Items)
                (st as StackPanel).Background = Brushes.Transparent;

            Continue.Visibility = Visibility.Hidden;
            Next();
        }

        void ListBox_SelectionChanged(object o, SelectionChangedEventArgs e)
        {
            foreach (var st in ListBoxWords.Items)
                (st as StackPanel).Background = Brushes.Red;
            (ListBoxWords.Items[test.CurrentWord] as StackPanel).Background = Brushes.Green;

            Continue.Visibility = Visibility.Visible;
        }
    }
}