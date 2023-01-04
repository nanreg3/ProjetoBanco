using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Data;
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
            if (ContaExists(conta.CPF) == false)
            {
                _context.Conta.Add(conta);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetConta", new { id = conta.ID }, conta);
            }

            return Conflict("Já existe uma conta com esse CPF.");
        }

        //Com essa conta é possível realizar transferências para outras contas...;
        [HttpPut("Transferencia")]
        public async Task<IActionResult> Transferencia(string contaOrigem, double valor, string contaDestino)
        {
            if (!ContaExists(contaOrigem))
            {
                return BadRequest("Verifique o CPF informado da conta de ORIGEM.");
            }
            if (!ContaExists(contaDestino))
            {
                return BadRequest("Verifique o CPF informado da conta de DESTINO.");
            }
            if(valor < 0)
            {
                return BadRequest("Permitido apenas valores positivos.");
            }

            Conta _contaOrigem = await GetConta(contaOrigem);
            if (_contaOrigem.Saldo < valor)
            {
                return BadRequest("Saldo insuficiente.");
            }
            if (valor > 2000)
            {
                return BadRequest("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000.");
            }
            _contaOrigem.Saldo -= valor;
            var _contaDestino = await GetConta(contaDestino);
            _contaDestino.Saldo += valor;

            _context.Entry(_contaOrigem).State = EntityState.Modified;
            _context.Entry(_contaDestino).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(contaOrigem))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //... e depositar;
        //Não aceitamos valores negativos nas contas;
        //Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000;
        //As transferências entre contas são gratuitas e ilimitadas;
        [HttpPut("Deposito")]
        public async Task<IActionResult> Deposito(string conta, double valor)
        {
            if (!ContaExists(conta))
            {
                return BadRequest("Verifique o CPF informado da conta.");
            }
            if(valor < 0)
            {
                return BadRequest("Permitido apenas valores positivos.");
            }
            if (valor > 2000)
            {
                return BadRequest("Por questão de segurança cada transação de depósito não pode ser maior do que R$2.000.");
            }

            Conta _conta = await GetConta(conta);
            _conta.Saldo += valor;

            _context.Entry(_conta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaExists(conta))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
