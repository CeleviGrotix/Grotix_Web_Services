using FluentAssertions;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using Moq;
using Xunit;

namespace Profiles.Api.Tests.Unit;

public class ProfilesDomainAndServiceTests
{
    // ==========================================
    // TDD: User Aggregate (3 Tests)
    // ==========================================
    [Fact]
    public void UserConstructor_ValidData_CreatesInstance()
    {
        var email = UserEmail.Create("test@grotix.pe");
        var user = new User(100, email, 4, "Juan Perez");

        user.IdentityId.Should().Be(100);
        user.Name.Should().Be("Juan Perez");
        user.Email.Value.Should().Be("test@grotix.pe"); // corregido
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AssignRole_InvalidRoleId_ThrowsArgumentException()
    {
        var user = new User(100, UserEmail.Create("test@test.com"), 4);
        
        Action act = () => user.AssignRole(0);
        act.Should().Throw<ArgumentException>().WithMessage("RoleId inválido.");
    }

    [Fact]
    public void UpdateProfile_ValidData_UpdatesProperties()
    {
        var user = new User(100, UserEmail.Create("test@test.com"), 4, "Viejo Nombre");
        user.UpdateProfile("Nuevo Nombre", "123456789", null, null);

        user.Name.Should().Be("Nuevo Nombre");
        user.TaxId.Should().Be("123456789");
    }

    // ==========================================
    // TDD: Contract Aggregate (3 Tests)
    // ==========================================
    [Fact]
    public void ContractConstructor_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1); // Fecha de fin anterior a la de inicio

        Action act = () => new Contract(1, start, end, ContractStatus.Active, 10, 5, 100, ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
        act.Should().Throw<ArgumentException>().WithMessage("EndDate no puede ser anterior a StartDate.");
    }

    [Fact]
    public void ContractConstructor_NegativeLimits_ThrowsArgumentException()
    {
        Action act = () => new Contract(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(10), ContractStatus.Active, -1, 5, 100, ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
        act.Should().Throw<ArgumentException>().WithMessage("MaxZones no puede ser negativo.");
    }

    [Fact]
    public void UpdateContract_ValidData_UpdatesLimits()
    {
        var contract = new Contract(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(10), ContractStatus.Active, 10, 5, 100, ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);
        
        contract.Update(null, null, 20, 10, true, null, null, null);

        contract.MaxZones.Should().Be(20);
        contract.MaxMicrocontrollers.Should().Be(10);
        contract.IsSuspended.Should().BeTrue();
    }

    // ==========================================
    // TDD: UserCommandService (4 Tests)
    // ==========================================
    [Fact]
    public async Task CreateUser_RoleDoesNotExist_ThrowsArgumentException()
    {
        var mockRoleRepo = new Mock<IRoleRepository>();
        mockRoleRepo.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false); // Rol no existe

        var service = new UserCommandService(new Mock<IUserRepository>().Object, mockRoleRepo.Object, new Mock<IAssociationRepository>().Object, new Mock<IProfilesUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(new CreateUserCommand(100, UserEmail.Create("test@test.com"), 99, "Nombre", null, null, null, true));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El rol 99 no existe.");
    }

    [Fact]
    public async Task CreateUser_IdentityAlreadyLinked_ThrowsArgumentException()
    {
        var mockRoleRepo = new Mock<IRoleRepository>();
        mockRoleRepo.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdentityIdAsync(100)).ReturnsAsync(new User(100, UserEmail.Create("a@a.com")));

        var service = new UserCommandService(mockUserRepo.Object, mockRoleRepo.Object, new Mock<IAssociationRepository>().Object, new Mock<IProfilesUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(new CreateUserCommand(100, UserEmail.Create("test@test.com"), 4, "Nombre", null, null, null, true));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Ya existe un perfil vinculado a esa identidad.");
    }

    [Fact]
    public async Task UpdateUserProfile_UserNotFound_ThrowsKeyNotFoundException()
    {
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null); // Usuario no existe

        var service = new UserCommandService(mockUserRepo.Object, new Mock<IRoleRepository>().Object, new Mock<IAssociationRepository>().Object, new Mock<IProfilesUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(new UpdateUserProfileCommand(1, "Nombre", null, null, null));
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Usuario 1 no encontrado.");
    }

    [Fact]
    public async Task AssignUserRole_ValidRequest_AssignsRole()
    {
        var mockUserRepo = new Mock<IUserRepository>();
        var user = new User(100, UserEmail.Create("a@a.com"), 4);
        mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var mockRoleRepo = new Mock<IRoleRepository>();
        mockRoleRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);

        var service = new UserCommandService(mockUserRepo.Object, mockRoleRepo.Object, new Mock<IAssociationRepository>().Object, new Mock<IProfilesUnitOfWork>().Object);

        var updated = await service.Handle(new AssignUserRoleCommand(1, 2));
        updated.RoleId.Should().Be(2);
    }
}
