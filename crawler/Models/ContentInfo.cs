using System.Collections.Generic;

namespace crawler.Models
{
    /// <summary>
    /// Информация о содержимом сайта
    /// </summary>
    public class ContentInfo
    {
        /// <summary>
        /// Содержимое
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// ЧИсло слов
        /// </summary>
        public int WordsCount => WordsInfo.Count;
        
        /// <summary>
        /// 
        /// </summary>
        public List<WordInfo> WordsInfo { get; set; }
    }
}