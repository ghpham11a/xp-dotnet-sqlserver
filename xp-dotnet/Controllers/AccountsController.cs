using Microsoft.AspNetCore.Mvc;
using XpDotnetSqlServer.Models;
using XpDotnetSqlServer.Services;

namespace XpDotnetSqlServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsService _accountsService;

        public AccountsController(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        {
            var accounts = await _accountsService.GetAllAsync();
            return Ok(accounts);
        }

        // GET: api/Accounts/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Account>> GetById(int id)
        {
            var account = await _accountsService.GetByIdAsync(id);
            if (account == null)
                return NotFound();
            return Ok(account);
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<Account>> Create([FromBody] Account newAccount)
        {
            // Insert new account
            var newId = await _accountsService.CreateAsync(newAccount);

            // Fetch the created record to return in response
            var createdAccount = await _accountsService.GetByIdAsync(newId);

            // Return 201 Created with URI to the newly created account
            return CreatedAtAction(nameof(GetById), new { id = newId }, createdAccount);
        }

        // PUT: api/Accounts/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Account updatedAccount)
        {
            if (id != updatedAccount.Id)
            {
                return BadRequest("URL id and entity id mismatch");
            }

            var success = await _accountsService.UpdateAsync(id, updatedAccount);
            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _accountsService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
