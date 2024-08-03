using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDictionaries.Dtos
{
    public class TranslationResponse
    {
        public IEnumerable<Translation> Translations { get; set; }
    }
}
