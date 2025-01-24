using MediatR;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroRabbit.Banking.Api.Controllers;

[Route("api/Banking")]
[ApiController]
public class BankingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAccountService _accountService;

    public BankingController(IMediator mediator, IAccountService accountService)
    {
        _mediator = mediator;
        _accountService = accountService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Account>> Get()
    {
        return Ok(_accountService.GetAccounts());
    }
}
