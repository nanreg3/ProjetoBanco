namespace ProjetoBanco.Messages
{
    public class TransferenciaRequest
    {
        public string CPFContaOrigem { get; set; }
        public double Valor { get; set; }
        public string CPFContaDestino { get; set; }

    }
}
