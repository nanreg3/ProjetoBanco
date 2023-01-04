using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjetoBanco.Data;
using ProjetoBanco.Messages;
using ProjetoBanco.Models;

namespace ProjetoBanco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContasController : ControllerBase
    {
        private readonly ILogger<ContasController> _logger;
        private readonly ProjetoBancoContext _context;

        public ContasController(ProjetoBancoContext context, ILogger<ContasController> logger)
        {
            _logger = logger;
            _context = context;
        }

        //Para abrir uma conta é necessário apenas o nome completo e CPF da pessoa, mas só é permitido uma conta por pessoa;
        [HttpPost("Abertura")]
        public async Task<ActionResult<Conta>> Abertura(Conta conta)
        {
            try
            {
                if (!ContaExists(conta.CPF))
                {
                    conta.Nome = conta.Nome.ToUpper();
                    _context.Conta.Add(conta);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Abertura de conta realizada com sucesso. Dados da conta: " + conta);
                    return Ok("Abertura de conta realizada com sucesso! Bem vido ao nosso Banco Sr(a) " + conta.Nome);
                }

                _logger.LogError("Já existe uma conta com esse CPF." + conta);
                return Conflict("Já existe uma conta com esse CPF.");
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Erro ao salvar nova conta. Dados da conta: " + conta);
                throw;
            }

        }

        //Com essa conta é possível realizar transferências para outras contas...;
        [HttpPut("Transferencia")]
        public async Task<ActionResult<string>> Transferencia(TransferenciaRequest transferencia)
        {
            if (!ContaExists(transferencia.CPFContaOrigem))
            {
                _logger.LogWarning("Verifique o CPF informado da conta de ORIGEM. Dados da transferencia: " + transferencia);
                return BadRequest("Verifique o CPF informado da conta de ORIGEM.");
            }
            if (!ContaExists(transferencia.CPFContaDestino))
            {
                _logger.LogWarning("Verifique o CPF informado da conta de DESTINO. Dados da transferencia: " + transferencia);
                return BadRequest("Verifique o CPF informado da conta de DESTINO.");
            }
            if (transferencia.Valor < 0)
            {
                _logger.LogWarning("Permitido apenas valores positivos. Dados da transferencia: " + transferencia);
                return BadRequest("Permitido apenas valores positivos.");
            }
            if (transferencia.CPFContaOrigem == transferencia.CPFContaDestino)
            {
                _logger.LogWarning("Não é possivel realizar transferencias para mesma conta, verifique os dados do favorecido. Dados da transferencia: " + transferencia);
                return BadRequest("Não é possivel realizar transferencias para mesma conta, verifique os dados do favorecido.");
            }

            Conta _contaOrigem = await GetConta(transferencia.CPFContaOrigem);
            if (_contaOrigem.Saldo < transferencia.Valor)
            {
                _logger.LogWarning("Saldo insuficiente. Dados da transferencia: " + transferencia);
                return BadRequest("Saldo insuficiente.");
            }
            if (transferencia.Valor > 2000)
            {
                _logger.LogWarning("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000. Dados da transferencia: " + transferencia);
                return BadRequest("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000.");
            }
            _contaOrigem.Saldo -= transferencia.Valor;
            var _contaDestino = await GetConta(transferencia.CPFContaDestino);
            _contaDestino.Saldo += transferencia.Valor;

            _context.Entry(_contaOrigem).State = EntityState.Modified;
            _context.Entry(_contaDestino).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(transferencia.CPFContaOrigem))
                {
                    _logger.LogError("Conta de origem não encontrada. Dados da transferencia: " + transferencia);
                    return NotFound();
                }
                if (!ContaExists(transferencia.CPFContaDestino))
                {
                    _logger.LogError("Conta do favorecido não encontrada. Dados da transferencia: " + transferencia);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Erro ao salvar trasferencia. Dados da transferencia: " + transferencia);
                    throw;
                }
            }

            _logger.LogInformation("Transferencia realizada com sucesso. Dados da transferencia: " + transferencia);
            return "Transferencia realizada com sucesso! Seu saldo atualizado é: R$ " + _contaOrigem.Saldo.ToString("F2");
        }

        //... e depositar;
        //Não aceitamos valores negativos nas contas;
        //Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000;
        //As transferências entre contas são gratuitas e ilimitadas;
        [HttpPut("Deposito")]
        public async Task<ActionResult<string>> Deposito(DepositoRequest deposito)
        {
            if (!ContaExists(deposito.CPFConta))
            {
                _logger.LogWarning("Verifique o CPF informado da conta. Dados do Depósito: " + deposito);
                return BadRequest("Verifique o CPF informado da conta.");
            }
            if (deposito.Valor < 0)
            {
                _logger.LogWarning("Permitido apenas valores positivos. Dados do Depósito: " + deposito);
                return BadRequest("Permitido apenas valores positivos.");
            }
            if (deposito.Valor > 2000)
            {
                _logger.LogWarning("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000. Dados do Deposito: " + deposito);
                return BadRequest("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000.");
            }

            Conta _conta = await GetConta(deposito.CPFConta);
            _conta.Saldo += deposito.Valor;

            _context.Entry(_conta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(deposito.CPFConta))
                {
                    _logger.LogError("Conta não encontrada. Dados do Depósito: " + deposito);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Erro ao salvar depósito. Dados do Depósito: " + deposito);
                    throw;
                }
            }

            _logger.LogInformation("Depósito realizado com sucesso. Dados do Deposito: " + deposito);
            return "Depósito realizado com sucesso! Seu saldo atualizado é: R$ " + _conta.Saldo.ToString("F2");
        }

        [HttpGet("BuscarTodas")]
        public async Task<ActionResult<IEnumerable<Conta>>> GetConta()
        {
            try
            {
                _logger.LogInformation("Lista de contas exibidas com sucesso.");
                return await _context.Conta.ToListAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Erro ao listar todas as contas.");
                throw;
            }
        }

        [HttpGet("BuscarPorCPF/{cpf}")]
        public async Task<Conta> GetConta(string cpf)
        {
            var conta = await _context.Conta.Where(c => c.CPF == cpf).FirstOrDefaultAsync();

            if (conta == null)
            {
                _logger.LogWarning("Conta não localizada. Dados da conta: " + conta);
                return null;
            }

            try
            {
                _logger.LogInformation("Conta exibida com sucesso. Dados da conta: " + conta);
                return conta;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Erro ao listar conta. Dados da conta: " + conta);
                throw;
            }
        }

        private bool ContaExists(string cpf)
        {
            return _context.Conta.Any(e => e.CPF == cpf);
        }

    }
}
