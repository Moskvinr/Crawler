using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using crawler.Models;

namespace crawler.Handlers
{
    public class FilesProvider
    {
        private const string IndexTxt = "output\\index.txt";
        private const string OutputDir = "output";

        public FilesProvider()
        {
            Directory.CreateDirectory(OutputDir);
        }

        public async Task WriteAllData(IAsyncEnumerable<HtmlInfo> htmlInfoEnumerator)
        {
            File.Delete(IndexTxt);
            await using var writer = new StreamWriter(IndexTxt, true);
            await foreach (var htmlInfo in htmlInfoEnumerator)
            {
                writer.WriteLine($"{htmlInfo.Level} {htmlInfo.Link}");
                await using var fileInfoWriter = new StreamWriter($"{OutputDir}\\{htmlInfo.Level}.txt");
                {
                    fileInfoWriter.WriteLine($"content={htmlInfo.Content.Content}");
                    try
                    {
                        htmlInfo.Content.WordsInfo.ForEach(word =>
                            word.LemmatizedWord = LemmatizationHandler.Lemmatize(word.Word));
                        htmlInfo.Content.WordsInfo.ForEach(word =>
                            fileInfoWriter.WriteLine($"word={LemmatizationHandler.Lemmatize(word.Word)}"));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    fileInfoWriter.WriteLine($"wordsCount={htmlInfo.Content.WordsCount}");
                }
            }
        }

        public async Task WriteInvertedList(Dictionary<string, List<int>> indexedWordsEnumerator)
        {
            var orderedIndexWords = indexedWordsEnumerator.OrderBy(x => x.Key);

            await using var fileInfoWriter = new StreamWriter($"{OutputDir}\\InvertedList.txt");
            {
                try
                {
                    foreach (var item in orderedIndexWords)
                    {
                        await fileInfoWriter.WriteAsync(item.Key);
                        await fileInfoWriter.WriteAsync(" -> ");

                        await fileInfoWriter.WriteAsync(string.Join(',', item.Value.Distinct()));

                        await fileInfoWriter.WriteLineAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public async Task<Dictionary<string, List<int>>> ReadInvertedList()
        {
            var indexedWords = new Dictionary<string, List<int>>();
            using (var sr = new StreamReader(new FileStream($"output\\InvertedList.txt", FileMode.Open)))
            {
                try
                {
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        var splitted = line.Split("->");
                        var keyWord = splitted[0].Trim() ?? "ALARM";
                        var docNumbers = splitted[1].Split(',').Select(x => Convert.ToInt32(x)).ToList();

                        indexedWords.Add(keyWord, docNumbers);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return indexedWords;
        }

        public async Task WriteTfIdf(IEnumerable<WordFrequencyInfo> tfIdfInfo)
        {
            await using var fileInfoWriter = new StreamWriter($"{OutputDir}\\TFIDFList.txt");
            {
                try
                {
                    foreach (var item in tfIdfInfo)
                    {
                        await fileInfoWriter.WriteLineAsync($"Слово: \"{item.Word}\"");

                        await fileInfoWriter.WriteLineAsync($"IDF = {Math.Round(item.IDF, 5)}");

                        await fileInfoWriter.WriteAsync("TF: ");
                        foreach (var (docNumber, tf) in item.TF)
                        {
                            await fileInfoWriter.WriteAsync($"[{docNumber}, {Math.Round(tf, 5)}], ");
                        }

                        await fileInfoWriter.WriteLineAsync();

                        await fileInfoWriter.WriteAsync("TF-IDF: ");
                        foreach (var (docNumber, tfidf) in item.TFIDF)
                        {
                            await fileInfoWriter.WriteAsync($"[{docNumber}, {Math.Round(tfidf, 5)}], ");
                        }

                        await fileInfoWriter.WriteLineAsync();
                        await fileInfoWriter.WriteLineAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public async Task<List<List<TFDto>>> ReadTfIdfList()
        {
            var tfIdfList = new List<WordFrequencyInfo>();
            using (var sr = new StreamReader(new FileStream($"output\\TFIDFList.txt", FileMode.Open)))
            {
                try
                {
                    var dictInfo = new WordFrequencyInfo();
                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();
                        if (line.StartsWith("Слово:"))
                        {
                            tfIdfList.Add(dictInfo);
                            dictInfo = new WordFrequencyInfo();
                            dictInfo.Word = line.Replace("Слово: ", "").Replace("\"", "");
                        }

                        if (line.StartsWith("IDF ="))
                        {
                            dictInfo.IDF = Convert.ToDouble(line.Replace("IDF = ", ""));
                        }

                        if (line.StartsWith("TF:"))
                        {
                            var temp = line.Replace("TF: ", "").TrimEnd(',').Split("], ");
                            var dict = new Dictionary<int, double>();

                            foreach (var item in temp.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                            {
                                var itemTemp = item.Replace("[", "").Replace("]", "").Split(", ");
                                dict.Add(Convert.ToInt32(itemTemp[0]), Convert.ToDouble(itemTemp[1]));
                            }

                            dictInfo.TF = dict;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            var sourceVector = new List<List<TFDto>>();
            
            for (var i = 0; i < 50; i++)
            {
                var abc = tfIdfList.Skip(1)
                    .Select(x => new TFDto
                    {
                        DocNumber = i,
                        Word = x.Word,
                        TFIDF = x.TFIDF.ToList().Where(y => y.Key == i).Select(o => o.Value).FirstOrDefault()
                    }).ToList();
                
                sourceVector.Add(abc);
            }
            
            return sourceVector;
        }

        private IEnumerable<(int docNumber, int wordsCount)> GetWordsCount()
        {
            var wordsCountList = new List<(int docNumber, int wordsCount)>();
            for (var i = 0; i < 51; i++)
            {
                using (var sr = new StreamReader(new FileStream($"output\\{i}.txt", FileMode.Open)))
                {
                    try
                    {
                        var text = sr.ReadToEnd();
                        var wordsCount = Convert.ToInt32(text.Split("\n").FirstOrDefault(x => x.Contains("wordsCount="))
                            ?.Replace("wordsCount=", "").Replace("\r", ""));
                        wordsCountList.Add((i, wordsCount));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            return wordsCountList;
        }

        public List<List<TFDto>> GetSearchQueryTfIdf(List<List<TFDto>> sourceVector, string[] words)
        {
            var result = new List<List<TFDto>>(sourceVector);
            
            foreach (var temp in result.SelectMany(item => item.Where(temp => words.Contains(temp.Word) && temp.TFIDF > 0)))
            {
                temp.TFIDF = 1 / (double) result.Count;
            }
            
            foreach (var temp in result.SelectMany(item => item.Where(temp => !words.Contains(temp.Word))))
            {
                temp.TFIDF = 0;
            }
            
            return result;
        }

        public void GetRelevantDocs(List<List<TFDto>> sourceVector, List<List<TFDto>> queryVector)
        {
            var result = new List<TFDto>();

            for (var i = 0; i < sourceVector.Count; i++)
            {
                for (var j = 0; j < sourceVector[i].Count; j++)
                {
                    var temp = sourceVector[i][j].Word == queryVector[i][j].Word ? sourceVector[i][j].TFIDF * queryVector[i][j].TFIDF : 0;
                    result.Add(new TFDto
                    {
                        DocNumber = sourceVector[i][j].DocNumber,
                        Word = sourceVector[i][j].Word,
                        TFIDF = temp
                    });
                }
            }

            result = result.OrderByDescending(x => x.TFIDF).GroupBy(p => p.DocNumber)
                .Select(g => g.First())
                .Where(x => x.TFIDF > 0)
                .Take(10)
                .ToList();

            foreach (var item in result)
            {
                Console.Write(item.DocNumber + " ");
            }
        }
    }
    
    public class TFDto
    {
        public int DocNumber { get; set; }
        public string Word { get; set; }
        public double TFIDF { get; set; }
    }
}