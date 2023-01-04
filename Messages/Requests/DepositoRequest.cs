using System.Globalization;

namespace ProjetoBanco.Messages
{
    public class DepositoRequest
    {
        public string CPFConta { get; set; }
        public double Valor { get; set; }

        public override string ToString()
        {
            return "CPF da conta: "
                + CPFConta
                + ", Valor de transação R$: "
                + Valor.ToString("F2")
                + ".";
        }

    }
}
