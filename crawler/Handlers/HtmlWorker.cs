using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using crawler.Models;
using HtmlAgilityPack;

namespace crawler.Handlers
{
    public class HtmlWorker
    {
        private HttpClient _client;
        private string _inputUrl;
        private List<HtmlInfo> _htmlInfos;

        public List<HtmlInfo> HtmlInfos => _htmlInfos;

        public HtmlWorker(string inputUrl)
        {
            _client = new HttpClient();
            _inputUrl = inputUrl;
            _htmlInfos = new List<HtmlInfo>();
        }

        public async IAsyncEnumerable<HtmlInfo> GetPages()
        {
            var needTakeChildLinks = true;
            yield return await GetPageInfo(_inputUrl, 0, needTakeChildLinks);
            
            var indexOfElementWithChildLinks = 0;
            for (var i = 0; i < 50; i++)
            {
                needTakeChildLinks = false;
                var linksCount = _htmlInfos[indexOfElementWithChildLinks].ChildLinks.Count;
                if (linksCount < 100)
                {
                    indexOfElementWithChildLinks++;
                    needTakeChildLinks = true;
                }
                yield return await GetPageInfo(_htmlInfos[indexOfElementWithChildLinks].ChildLinks[i], i + 1, needTakeChildLinks);
            }
        }

        private async Task<HtmlInfo> GetPageInfo(string url, int docNumber, bool needTakeChild)
        {
            var page = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            var doc = new HtmlDocument();
            var contentInner = await page.Content.ReadAsStringAsync();
            doc.LoadHtml(contentInner);
            
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());
            
            var wordsInfo = doc.DocumentNode.SelectNodes("//text()")
                .Select(node => node.InnerText.Trim())
                .SelectMany(word => word.Split(' '))
                .Select(word => new string(word.Where(char.IsLetter).ToArray()))
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x=> new WordInfo
                {
                    Word = x,
                    DocNumber = docNumber
                })
                .ToList();

            var childLinks = needTakeChild ? doc.DocumentNode.SelectNodes("//a[@href]")
                .Where(node => node != null).Select(node => node?.Attributes["href"]?.Value)
                .Where(x => x != null && Uri.IsWellFormedUriString(x, UriKind.Absolute) && x.StartsWith("http"))
                .Distinct()
                .ToList() : new List<string>();
            var result = new HtmlInfo
            {
                Content = new ContentInfo
                {
                    Content = string.Join(" ",doc.DocumentNode.SelectNodes("//text()")
                        .Select(node => node.InnerText.Trim())),
                    WordsInfo = wordsInfo
                },
                ChildLinks = childLinks,
                IsVisited = true,
                Link = url,
                Level = docNumber,
                ParentLink = null
            };
            _htmlInfos.Add(result);
            return result;
        }
    }
}