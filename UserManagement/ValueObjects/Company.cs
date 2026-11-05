using System;
using System.Collections.Generic;
using System.Text;

namespace UserManagement.Domain.ValueObjects
{
   public  class Company
    {
        public string? Name { get; set; }

        /// <summary>
        /// Şirketin sloganı (catch phrase: akılda kalıcı tanıtım cümlesi).
        /// Örnek: "Multi-layered client-server neural-net"
        /// </summary>
        public string? CatchPhrase { get; set; }

        /// <summary>
        /// Şirketin faaliyet alanını anlatan iş jargonu ifadesi.
        /// Örnek: "harness real-time e-markets"
        /// Alan adı kaynak veriden (jsonplaceholder) olduğu gibi alındı.
        /// </summary>
        public string? Bs { get; set; }
    }
}
