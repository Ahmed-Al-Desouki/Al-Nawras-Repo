using Al_Nawras.Application.Common.Interfaces;
using Al_Nawras.Application.Common.Interfaces.Repositories;
using Al_Nawras.Application.Common.Models;
using Al_Nawras.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Clients.Commands.CreateClient
{
    public class CreateClientHandler
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientHandler(IClientRepository clientRepository, IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateClientCommand command,
            CancellationToken cancellationToken = default)
        {
            var emailExists = await _clientRepository.ExistsByEmailAsync(command.Email, cancellationToken);
            if (emailExists)
                return Result<Guid>.Failure($"A client with email {command.Email} already exists.");

            var client = new Client(
                command.Name,
                command.Email,
                command.Phone,
                command.Country,
                command.CompanyName,
                command.AssignedSalesUserId
            );

            await _clientRepository.AddAsync(client, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(client.Id);
        }
    }
}
