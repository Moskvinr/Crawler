using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using crawler.Handlers;
using crawler.Models;

namespace crawler
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите ссылку: ");
            var input = Console.ReadLine()?.Trim();
            
            var filesProvider = new FilesProvider();
            
            if (input != string.Empty)
            {
                var indexHandler = new IndexerHandler();
                var worker = new HtmlWorker(input);
                
                await filesProvider.WriteAllData(worker.GetPages());
            
                indexHandler.IndexWords(worker.HtmlInfos);
            
                await filesProvider.WriteInvertedList(indexHandler.IndexedWords);
                
                var tfIdfInfo = indexHandler.IndexedWords
                    .Select(x => new {x.Key, Value = x.Value.Distinct()})
                    .Select(x =>
                        new WordFrequencyInfo
                        {
                            Word = x.Key,
                            IDF = Math.Log10(worker.HtmlInfos.Count / (double) x.Value.Count()),
                            TF = x.Value.ToDictionary(q => q, q =>
                            {
                                var docContentInfo = worker.HtmlInfos.FirstOrDefault(z => z.Level == q)?.Content;
            
                                var wordsCount =
                                    (double) docContentInfo.WordsInfo.Count(z => z.LemmatizedWord == x.Key);
            
                                return wordsCount / docContentInfo.WordsCount;
                            })
                        });
            
                await filesProvider.WriteTfIdf(tfIdfInfo);
            }
            else
            {
                var indexedWords = await filesProvider.ReadInvertedList();
            
                Console.WriteLine("Введите запрос: ");
                var searchQuery = Console.ReadLine();
            
                //searchQuery = "Android OR fb OR Биробиджан";
            
                var query = QueryWorker.ParseQuery(searchQuery);
                var searchResult = indexedWords.Where(query.Compile()).ToList();
            
                var words = searchQuery.Split(' ');
            
                var result = QueryWorker.GetDocNumbers(words, searchResult);
            
                Console.WriteLine("Результаты:");
                foreach (var item in result)
                {
                    Console.Write(item + " ");
                }
            }

            var sourceTfIdfList = await filesProvider.ReadTfIdfList();
            var searchQueryTfIdf = filesProvider.GetSearchQueryTfIdf(sourceTfIdfList, new[] {"ГАБДУЛИНА"}).ToList();
            filesProvider.GetRelevantDocs(await filesProvider.ReadTfIdfList(), searchQueryTfIdf);
            Console.WriteLine();
        }
    }
}