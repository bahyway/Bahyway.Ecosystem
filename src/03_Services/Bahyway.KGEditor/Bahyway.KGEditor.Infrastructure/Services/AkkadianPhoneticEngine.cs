using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Bahyway.KGEditor.Domain.Interfaces;

namespace Bahyway.KGEditor.Infrastructure.Services
{
    public class AkkadianPhoneticEngine : IPhoneticEngine
    {
        public string ExtractSkeleton(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            // Normalize (Remove Tashkeel/Diacritics)
            var text = input.Normalize(NormalizationForm.FormKD);
            var cleanText = Regex.Replace(text, @"\p{M}", "");

            var sb = new StringBuilder();
            foreach (var c in cleanText.ToUpperInvariant())
            {
                switch (c)
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'O':
                    case 'U':
                    case 'Y':
                        continue; // Skip Vowels
                    case 'K':
                    case 'Q':
                        sb.Append('K'); break;
                    case 'Z':
                    case 'S':
                        sb.Append('S'); break;
                    default:
                        if (char.IsLetter(c)) sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public string GenerateVectorColor(string skeleton)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(skeleton));
                return $"#{hash[0]:X2}{hash[1]:X2}{hash[2]:X2}";
            }
        }

        public string DetectLanguageContext(string input)
        {
            if (input.Any(c => c == 'چ' || c == 'پ' || c == 'گ' || c == 'ژ')) return "Persian/Urdu";
            if (input.Any(c => c >= 0x0600 && c <= 0x06FF)) return "Arabic";
            return "Latin/English";
        }
    }

}
