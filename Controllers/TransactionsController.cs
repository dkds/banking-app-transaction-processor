using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TransactionProcessor.Data;
using TransactionProcessor.Exceptions;
using TransactionProcessor.Models;

namespace TransactionProcessor.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionProcessorContext _context;
        private readonly IDistributedCache _cache;
        private readonly HttpClient _client;

        public TransactionsController(TransactionProcessorContext context, IHttpClientFactory httpClientFactory, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
            _client = httpClientFactory.CreateClient("ApiManager");
        }

        // GET: /Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransaction([FromQuery] string? accountNo)
        {
            if (_context.Transaction == null)
            {
                return NotFound();
            }
            if (accountNo == null)
            {
                return await _context.Transaction.ToListAsync();
            }
            return await _context.Transaction.Where(t => t.AccountNumberFrom == accountNo).ToListAsync();
        }

        // GET: /Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            if (_context.Transaction == null)
            {
                return NotFound();
            }
            var transaction = await _context.Transaction.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // PUT: /Transactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
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

        // POST: /Transactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            if (_context.Transaction == null)
            {
                return Problem("Entity set 'TransactionProcessorContext.Transaction'  is null.");
            }

            if (transaction.AccountNumberFrom == null && transaction.AccountNumberTo == null)
            {
                return BadRequest("AccountDto number is required");
            }
            else if (transaction.AccountNumberFrom == null)
            {
                try
                {
                    await CheckAccountValidity(transaction.AccountNumberTo, -1);
                }
                catch (InvalidAccountStatusException e)
                {
                    return BadRequest("Account status not supported for transactions: " + e.Message);
                }

                transaction.AccountNumberFrom = transaction.AccountNumberTo;
                transaction.AccountNumberTo = null;
                transaction.Type = TransactionType.Credit;
                transaction.Notes = "Deposited;" + transaction.Notes;
                await saveTransaction(transaction);
            }
            else if (transaction.AccountNumberTo == null)
            {
                try
                {
                    await CheckAccountValidity(transaction.AccountNumberFrom, transaction.Amount);
                }
                catch (InsufficientFundsException e)
                {
                    return BadRequest("Insufficient funds in the account: " + e.Message);
                }
                catch (InvalidAccountStatusException e)
                {
                    return BadRequest("Account status not supported for transactions: " + e.Message);
                }

                transaction.Type = TransactionType.Debit;
                transaction.Notes = "Cashed;" + transaction.Notes;
                await saveTransaction(transaction);
            }
            else
            {
                try
                {
                    await CheckAccountValidity(transaction.AccountNumberFrom, transaction.Amount);
                }
                catch (InsufficientFundsException e)
                {
                    return BadRequest("Insufficient funds in the account: " + e.Message);
                }
                catch (InvalidAccountStatusException e)
                {
                    return BadRequest("Account status not supported for transactions: " + e.Message);
                }

                transaction.Type = TransactionType.Debit;
                transaction.Notes = "Transfered;" + transaction.Notes;
                await saveTransaction(transaction);

                var creditTransaction = new Transaction
                {
                    AccountNumberFrom = transaction.AccountNumberTo,
                    AccountNumberTo = transaction.AccountNumberFrom,
                    Amount = transaction.Amount,
                    Time = transaction.Time,
                    Type = TransactionType.Credit,
                    Notes = "GeneratedCredit;" + transaction.Notes
                };
                await saveTransaction(creditTransaction);

            }
            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);

            async Task saveTransaction(Transaction transaction)
            {
                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();

                transaction.ReferenceNumber = transaction.Id.ToString("D12");
                await _context.SaveChangesAsync();

                await UpdateAccount(transaction);
            }
        }


        // DELETE: /Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            if (_context.Transaction == null)
            {
                return NotFound();
            }
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transaction.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> UpdateAccount(Transaction transaction)
        {
            HttpResponseMessage response = await _client.PutAsJsonAsync<TransactionAmountDto>("customer-service/Accounts/" + transaction.AccountNumberFrom + "/Balance", new TransactionAmountDto
            {
                Amount = transaction.Amount,
                Type = transaction.Type
            });
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidAccountStatusException("Account update failed");
            }
            return true;
        }

        private async Task<bool> CheckAccountValidity(string accountNumber, decimal amount)
        {
            if (accountNumber != null)
            {
                var account = await GetCachedAccount(accountNumber);
                if (account == null)
                {
                    List<AccountDto>? accounts = await _client.GetFromJsonAsync<List<AccountDto>>("customer-service/Accounts?accountNo=" + accountNumber);
                    if (accounts == null || accounts.Count == 0)
                    {
                        throw new InvalidAccountStatusException("No Account");
                    }
                    account = accounts.First();
                }
                if (account.Status != AccountStatus.Active)
                {
                    throw new InvalidAccountStatusException(account.Status.ToString());
                }
                if (account.Balance < amount)
                {
                    throw new InsufficientFundsException(account.Balance.ToString());
                }
            }
            return true;
        }

        private async Task<AccountDto> GetCachedAccount(string accountNumber)
        {
            if (accountNumber != null)
            {
                string? accountString = await _cache.GetStringAsync("accounts:" + accountNumber);

                if (accountString != null)
                {
                    return JsonSerializer.Deserialize<AccountDto>(accountString);

                }
            }
            return null;
        }

        private bool TransactionExists(int id)
        {
            return (_context.Transaction?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
