using System;
using System.Collections;
using System.ComponentModel;
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
            if (inputString == "") return new byte[0];

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
            if (!(obj is Article article)) return false;

            if (article.Hash.Length != 0)
            {
                return StructuralComparisons.StructuralEqualityComparer.Equals(Hash, article.Hash);
            }

            return article.Title == Title && article.Date == Date;

            //return obj is Article articleObj && (articleObj.Hash.Length != 0 && StructuralComparisons.StructuralEqualityComparer.Equals(Hash, articleObj.Hash));
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }

    public enum ArticleType
    {
        [Description("Vodovod i kanalizacija")]
        Vik,
        [Description("Nastavni zavod za javno zdravstvo")]
        Nzjz,
        [Description("Elektrodalmacija")]
        Hep,
        [Description("Agencija za pravni promet i posredovanje nekretninama")]
        Apn
    }
}
