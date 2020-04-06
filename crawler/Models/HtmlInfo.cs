using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace crawler.Models
{
    /// <summary>
    /// Сущность информации о html-странице
    /// </summary>
    public class HtmlInfo
    {
        /// <summary>
        /// Ссылка
        /// </summary>
        public string Link { get; set; }
        
        /// <summary>
        /// Содержимое страницы
        /// </summary>
        public ContentInfo Content { get; set; }
        
        /// <summary>
        /// Уровень глубины
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        public string ParentLink { get; set; }
        
        /// <summary>
        /// Признак посещённости
        /// </summary>
        public bool IsVisited { get; set; }
        
        /// <summary>
        /// Коллекция со ссылками на дочерние
        /// </summary>
        public List<string> ChildLinks { get; set; }
    }
}