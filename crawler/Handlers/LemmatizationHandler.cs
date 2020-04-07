using System;
using System.IO;
using LemmaSharp;

namespace crawler.Handlers
{
    public static class LemmatizationHandler
    {
        private const string EnLemmatizerpath = @"Lemmatization\full7z-mlteast-en.lem";
        private const string RuLemmatizerpath = @"Lemmatization\full7z-mlteast-ru.lem";
        private static readonly Lemmatizer enLemmatizer;
        private static readonly Lemmatizer ruLemmatizer;

        private static readonly FileStream enStream;
        private static readonly FileStream ruStream;
        
        static LemmatizationHandler()
        {
            enStream = File.OpenRead(EnLemmatizerpath);
            ruStream = File.OpenRead(RuLemmatizerpath);
            enLemmatizer = new Lemmatizer(enStream);
            ruLemmatizer = new Lemmatizer(ruStream);
        }

        public static string Lemmatize(string word)
        {
            try
            {
                using (ruStream)
                {
                    return ruLemmatizer.Lemmatize(word);
                }
            }
            catch(Exception _)
            {
                using (enStream)
                {
                    return enLemmatizer.Lemmatize(word);
                }
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }

            return string.Empty;
        }
    }
}