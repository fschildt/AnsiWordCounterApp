using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;

namespace AnsiWordCounterApp
{
    public class CountedWord
    {
        public String word { get; set; }
        public int count { get; set; }

        public CountedWord(string word, int count)
        {
            this.word = word;
            this.count = count;
        }
    }

    public class AnsiWordCounter
    {
        private int mProgressPercentage; // -1 indicates error
        private string mErrorMessage;
        private List<CountedWord> mCountedWords;

        private Thread mWorker = null;


        public void Start(string path)
        {
            Reset();
            mWorker = new Thread(DoWork);
            mWorker.Start(path);
        }

        public void Reset()
        {
            if (mWorker != null)
            {
                mWorker.Abort();
                mWorker.Join();
                mWorker = null;
            }

            mProgressPercentage = 0;
            mErrorMessage = null;
            mCountedWords = null;
        }

        public List<CountedWord> GetCountedWords()
        {
            return mCountedWords;
        }

        public int GetProgressPercentage()
        {
            return mProgressPercentage;
        }

        public string GetErrorMessage()
        {
            return mErrorMessage;
        }


        private void DoWork(object path)
        {
            string text = ReadAnsiFile((string)path, out string errorMessage);
            if (text == null)
            {
                mErrorMessage = errorMessage;
                mProgressPercentage = -1;
                return;
            }

            List<CountedWord> countedWords = Parse(text);
            mProgressPercentage = 99;

            mCountedWords = countedWords.OrderByDescending(w => w.count).ToList();
            mProgressPercentage = 100;
        }

        private List<CountedWord> Parse(string text)
        {
            Dictionary<string, CountedWord> dict = new Dictionary<string, CountedWord>();

            int charsProcessed = 0;
            int charsTotal = text.Length;

            WordLocation location = new WordLocation();
            GetNextWordLocation(text, location);
            while (location.c0 != -1)
            {
                string sub = text.Substring(location.c0, location.len);
                if (dict.TryGetValue(sub, out CountedWord existing))
                {
                    existing.count += 1;
                }
                else
                {
                    CountedWord toAdd = new CountedWord(sub, 1);
                    dict.Add(toAdd.word, toAdd);
                }

                charsProcessed = location.c0 + location.len;
                mProgressPercentage = (int)(99.0 * charsProcessed / charsTotal); // [0,99] because parsing is roughly 100x slower than sorting

                GetNextWordLocation(text, location);
            }

            List<CountedWord> countedWords = new List<CountedWord>(dict.Values);
            return countedWords;
        }


        private static string ReadAnsiFile(string path, out string errorMessage)
        {
            try
            {
                string text = File.ReadAllText(path, Encoding.GetEncoding("Windows-1252"));
                errorMessage = null;
                return text;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return null;
            }
        }

        private class WordLocation
        {
            public int c0 = 0;
            public int len = 0;
        }

        private static void GetNextWordLocation(string text, WordLocation location)
        {
            char[] whitespace = { ' ', '\t', '\n', '\r', '\f', '\v' };

            int c0 = location.c0 + location.len;
            int len;

            // find c0
            int next_whitespace = text.IndexOfAny(whitespace, c0);
            while (c0 == next_whitespace)
            {
                c0 += 1;
                next_whitespace = text.IndexOfAny(whitespace, c0);
            }
            if (c0 >= text.Length)
            {
                c0 = -1;
            }

            // find len
            if (next_whitespace == -1)
            {
                len = text.Length - c0;
            }
            else
            {
                len = next_whitespace - c0;
            }

            location.c0 = c0; 
            location.len = len;
        }
    }
}
