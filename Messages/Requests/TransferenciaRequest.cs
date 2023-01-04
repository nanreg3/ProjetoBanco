using System.Globalization;

namespace ProjetoBanco.Messages
{
    public class TransferenciaRequest
    {
        public string CPFContaOrigem { get; set; }
        public double Valor { get; set; }
        public string CPFContaDestino { get; set; }

        public override string ToString()
        {
            return "CPF de origem: "
                + CPFContaDestino
                + ", Valor de transação R$: "
                + Valor.ToString("F2")
                + ", CPF do favorecido: "
                + CPFContaDestino
                + ".";
        }

    }
}
