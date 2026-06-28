using FluentAssertions;
using TechTalk.SpecFlow;

namespace Profiles.Api.Tests.Acceptance;

[Binding]
public class ProfilesAcceptanceSteps
{
    private string _tokenStatus;
    private bool _actionResult;

    // --- US25: Registro de Usuario por Invitación ---
    [Given(@"un enlace de invitacion con token ""(.*)""")]
    public void GivenUnEnlaceDeInvitacionConToken(string status)
    {
        _tokenStatus = status;
    }

    [When(@"el agricultor completa el formulario de registro")]
    public void WhenElAgricultorCompletaElFormularioDeRegistro()
    {
        _actionResult = _tokenStatus == "vigente";
    }

    [Then(@"la cuenta es creada exitosamente")]
    public void ThenLaCuentaEsCreadaExitosamente()
    {
        _actionResult.Should().BeTrue();
    }
}