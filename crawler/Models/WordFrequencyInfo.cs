using System.Collections.Generic;
using System.Linq;

namespace crawler.Models
{
    public class WordFrequencyInfo
    {
        /// <summary>
        /// Слово
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// TF
        /// </summary>
        public Dictionary<int, double> TF { get; set; }

        /// <summary>
        /// IDF
        /// </summary>
        public double IDF { get; set; }

        /// <summary>
        /// TF-IDF
        /// </summary>
        public Dictionary<int, double> TFIDF => TF.ToDictionary(x => x.Key, x => TF[x.Key] * IDF);
    }
}