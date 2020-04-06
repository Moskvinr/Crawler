using System.Collections.Generic;
using crawler.Models;

namespace crawler.Handlers
{
    public class IndexerHandler
    {
        public Dictionary<string, List<int>> IndexedWords { get; set; } = new Dictionary<string, List<int>>();
        public void IndexWords(List<HtmlInfo> htmlInfos)
        {
            htmlInfos.ForEach(info =>
            {
                info.Content.WordsInfo.ForEach(word =>
                {
                    if (!IndexedWords.ContainsKey(word.LemmatizedWord))
                    {
                        IndexedWords.Add(word.LemmatizedWord, new List<int> {word.DocNumber});
                    }
                    else
                    {
                        IndexedWords[word.LemmatizedWord].Add(word.DocNumber);
                    }
                });
            });
        }
    }
}