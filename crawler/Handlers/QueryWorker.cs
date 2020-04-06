using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace crawler.Handlers
{
    public static class QueryWorker
    {
        public enum WordType
        {
            Word,
            And,
            Or,
            Not
        }
        public static Expression<Func<KeyValuePair<string, List<int>>, bool>> ParseQuery(string query)
        {
            var parameter = Expression.Parameter(typeof(KeyValuePair<string, List<int>>), "x");
            var member = Expression.Property(parameter, "Key");

            var splittedQuery = query.Split(' ').ToList();

            var expressions = new List<(string word, WordType wordType)>();

            foreach (var item in splittedQuery)
            {
                var itemIndex = splittedQuery.FindIndex(x => x.Equals(item));
                var expressionsLength = expressions.Count;
                switch (item)
                {
                    case "AND":
                        expressions.Add((item, WordType.And));
                        break;
                    case "OR":
                        expressions.Add((item, WordType.Or));
                        break;
                    case "NOT":
                        expressions.Add((item, WordType.Not));
                        break;
                    default:
                        expressions.Add((item, WordType.Word));
                        break;
                }
            }

            var wordTypes = expressions.Where(x => x.wordType == WordType.Word).ToList();

            var predicate = PredicateBuilder.False<KeyValuePair<string, List<int>>>();

            foreach (var keyword in wordTypes.Select(x => x.word).ToList())
            {
                var temp = keyword;
                predicate = predicate.Or(p => p.Key == temp);
            }

            
            return predicate;
        }

        public static List<int> GetDocNumbers(string[] words, List<KeyValuePair<string, List<int>>> searchResult)
        {
            var indexList = new List<int>();
            var lastOperation = WordType.Word;
            foreach (var word in words)
            {
                var item = searchResult.FirstOrDefault(x => x.Key == word);
                
                switch (word)
                {
                    case "AND":
                        lastOperation = WordType.And;
                        break;
                    case "OR":
                        lastOperation = WordType.Or;
                        break;
                    case "NOT":
                        lastOperation = WordType.Not;
                        break;
                }

                if (item.Key != null && indexList.Count > 0)
                {
                    if (lastOperation == WordType.And)
                        indexList = indexList.Intersect(item.Value).ToList();
                    
                    if (lastOperation == WordType.Or)
                        indexList = indexList.Union(item.Value).ToList();
                    
                    if (lastOperation == WordType.Not)
                        indexList = indexList.Except(item.Value).ToList();
                }
                else if (item.Value != null)
                {
                    indexList = item.Value;
                }
            }

            return indexList.Distinct().OrderBy(x => x).ToList();
        }
    }
    
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T> ()  { return f => true;  }
        public static Expression<Func<T, bool>> False<T> () { return f => false; }
 
        public static Expression<Func<T, bool>> Or<T> (this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke (expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse (expr1.Body, invokedExpr), expr1.Parameters);
        }
 
        public static Expression<Func<T, bool>> And<T> (this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke (expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso (expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}