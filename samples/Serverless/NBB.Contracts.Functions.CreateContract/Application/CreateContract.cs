using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Functions.CreateContract.Application
{
    public class CreateContract
    {
        public class Command : IRequest
        {
            public Command(Guid contractId)
            {
                ContractId = contractId;
            }

            public Guid ContractId { get; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IMediator mediator;

            public Handler (IMediator mediator)
            {
                this.mediator = mediator;
            }
            public Task Handle(Command request, CancellationToken cancellationToken)
            {
                mediator.Publish(new ContractCreated());
                return Task.CompletedTask;
            }
        }
    }
}
