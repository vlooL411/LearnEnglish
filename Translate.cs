using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LearnEnglish
{
    static class TranslateYandex
    {
        static readonly string Api_Key = "trnsl.1.1.20190611T171443Z.ceb2f51b81458b80.f5ed850462b386bdea1b139090bfc7ef77ef8a10";
        static public string lang = "en-ru";
        static string httpQuery = "https://translate.yandex.net/api/v1.5/tr.json/translate?lang=" + lang + "&key=" + Api_Key + "&text=";
        static readonly HttpClient client = new HttpClient();

        static public string Translate(string word) { return RequestTrYandex(word); }
        static public string RequestTrYandexSimple(string word)
        {
            try { return ConvertStringTranslateWord(client.GetAsync(httpQuery + word).Result.Content.ReadAsStringAsync().Result); }
            catch { return ""; }
        }
        static string glasnie = "aeioyu";
        static public string RequestTrYandex(string word)
        {
            var str = "";
            try
            {
                string trText = RequestTrYandexSimple(word);
                string trWord2 = RequestTrYandexSimple("to " + word).Replace("to ", "");
                string trWord3 = RequestTrYandexSimple("the " + word).Replace("the ", "");
                string a = "a ";
                foreach (var i in glasnie)
                    if (word[0] == i) { a = "an "; break; }
                string trWord4 = RequestTrYandexSimple(a + word).Replace(a, "");
                str += trText;
                if (trText != trWord2) str += ", " + trWord2;
                if (trText != trWord3 && trWord2 != trWord3) str += ", " + trWord3;
                if (trText != trWord4 && trWord2 != trWord4 && trWord3 != trWord4) str += ", " + trWord4;
            }
            catch { }
            return str;
        }
        static public string RequestTrYandexBig(string word, char shift)
        {
            var line = "";
            var uri = new Uri(httpQuery);
            int count = word.Length / 100;
            if ((count == 0 && word.Length > 0) || count * 100 - word.Length < 0) count++;
            int pos = 0;
            var temp = "";
            for (int i = 0; i < count; i++)
            {
                var step = pos + 100 < word.Length ? pos + 100 : word.Length;
                while (pos < step)
                {
                    bool t;
                    for (int k = pos; ; k++)
                    {
                        if (k > step - 1) { t = true; break; }
                        if (word[k] == shift) { t = false; break; }
                    }
                    if (t) break;
                    if (!t)
                        while (word[pos] != shift)
                            temp += word[pos++];
                    temp += word[pos++];
                }
                try { line += ConvertStringTranslateWord(client.PostAsync(uri, new StringContent("text=" + temp, Encoding.UTF8, "application/x-www-form-urlencoded")).Result.Content.ReadAsStringAsync().Result); }
                catch { }
                temp = "";
            }
            return line;
        }

        static string ConvertStringTranslateWord(string str) => str.Split('[')[1].Split(']')[0].Split('"')[1];
    }
}
