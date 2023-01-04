using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Data;
using ProjetoBanco.Messages;
using ProjetoBanco.Models;

namespace ProjetoBanco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContasController : ControllerBase
    {
        private readonly ProjetoBancoContext _context;

        public ContasController(ProjetoBancoContext context)
        {
            _context = context;
        }

        //Para abrir uma conta é necessário apenas o nome completo e CPF da pessoa, mas só é permitido uma conta por pessoa;
        [HttpPost("Abertura")]
        public async Task<ActionResult<Conta>> Abertura(Conta conta)
        {
            if (!ContaExists(conta.CPF))
            {
                conta.Nome = conta.Nome.ToUpper();
                _context.Conta.Add(conta);
                await _context.SaveChangesAsync();

                return Ok("Abertura de conta realizada com sucesso! Bem vido ao nosso Banco Sr(a) " + conta.Nome);
            }

            return Conflict("Já existe uma conta com esse CPF.");
        }

        //Com essa conta é possível realizar transferências para outras contas...;
        [HttpPut("Transferencia")]
        public async Task<ActionResult<string>> Transferencia(TransferenciaRequest transferencia)
        {
            if (!ContaExists(transferencia.CPFContaOrigem))
            {
                return BadRequest("Verifique o CPF informado da conta de ORIGEM.");
            }
            if (!ContaExists(transferencia.CPFContaDestino))
            {
                return BadRequest("Verifique o CPF informado da conta de DESTINO.");
            }
            if(transferencia.Valor < 0)
            {
                return BadRequest("Permitido apenas valores positivos.");
            }
            if(transferencia.CPFContaOrigem == transferencia.CPFContaDestino)
            {
                return BadRequest("Não é possivel realizar transferencias para mesma conta, verifique os dados do favorecido.");
            }

            Conta _contaOrigem = await GetConta(transferencia.CPFContaOrigem);
            if (_contaOrigem.Saldo < transferencia.Valor)
            {
                return BadRequest("Saldo insuficiente.");
            }
            if (transferencia.Valor > 2000)
            {
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return "Transferencia realizada com sucesso! Seu saldo atualizado é: R$ " + _contaOrigem.Saldo.ToString("F2");
        }

        //... e depositar;
        //Não aceitamos valores negativos nas contas;
        //Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000;
        //As transferências entre contas são gratuitas e ilimitadas;
        [HttpPut("Deposito")]
        public async Task<ActionResult<string>> Deposito(DepositoRequest depositoRequest)
        {
            if (!ContaExists(depositoRequest.CPFConta))
            {
                return BadRequest("Verifique o CPF informado da conta.");
            }
            if(depositoRequest.Valor < 0)
            {
                return BadRequest("Permitido apenas valores positivos.");
            }
            if (depositoRequest.Valor > 2000)
            {
                return BadRequest("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000.");
            }

            Conta _conta = await GetConta(depositoRequest.CPFConta);
            _conta.Saldo += depositoRequest.Valor;

            _context.Entry(_conta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(depositoRequest.CPFConta))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return "Depósito realizado com sucesso! Seu saldo atualizado é: R$ " + _conta.Saldo.ToString("F2");
        }

        [HttpGet("BuscarTodas")]
        public async Task<ActionResult<IEnumerable<Conta>>> GetConta()
        {
            return await _context.Conta.ToListAsync();
        }

        [HttpGet("BuscarPorCPF/{cpf}")]
        public async Task<Conta> GetConta(string cpf)
        {
            var conta = await _context.Conta.Where(c => c.CPF == cpf).FirstOrDefaultAsync();

            if (conta == null)
            {
                return null;
            }

            return conta;
        }

        private bool ContaExists(string cpf)
        {
            return _context.Conta.Any(e => e.CPF == cpf);
        }
    }
}
