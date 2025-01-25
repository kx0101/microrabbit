using MediatR;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroRabbit.Transfer.Api.Controllers;

[Route("api/Transfer")]
[ApiController]
public class BankingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITransferService _transferService;

    public BankingController(IMediator mediator, ITransferService transferService)
    {
        _mediator = mediator;
        _transferService = transferService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TransferLog>> Get()
    {
        return Ok(_transferService.GetTransferLogs());
    }
}

