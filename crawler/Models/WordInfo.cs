namespace crawler.Models
{
    public class WordInfo
    {
        /// <summary>
        /// Слово
        /// </summary>
        public string Word { get; set; }
        
        /// <summary>
        /// Лемматизированное слово
        /// </summary>
        public string LemmatizedWord { get; set; }
        
        /// <summary>
        /// Номер документа
        /// </summary>
        public int DocNumber { get; set; }
    }
}