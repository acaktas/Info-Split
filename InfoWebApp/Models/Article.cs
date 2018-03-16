using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace InfoWebApp.Models
{
    public class Article
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string ShortText { get; set; }
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Hash = GetHash(_text);
            }
        }

        public static byte[] GetHash(string inputString)
        {
            using (var sha = new SHA256Managed())
            {
                var textData = Encoding.UTF8.GetBytes(inputString);
                return sha.ComputeHash(textData);
            }
        }

        public string Link { get; set; }
        public DateTime? Date { get; set; }
        public byte[] Hash { get; private set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public bool IsAlert { get; set; }
        public bool IsSent { get; set; }
        public ArticleType ArticleType { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Article articleObj && StructuralComparisons.StructuralEqualityComparer.Equals(Hash, articleObj.Hash);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }

    public enum ArticleType
    {
        Vik,
        Nzjz
    }
}
