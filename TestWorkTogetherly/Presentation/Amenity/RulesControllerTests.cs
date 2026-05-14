using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Rules.Common;
using WorkTogetherly.Application.Rules.GetAllRules;
using WorkTogetherly.Presentation.Controllers.Amenity;

namespace TestWorkTogetherly.Presentation.Rule;

public class RulesControllerTests
{
    private static readonly List<RuleResult> SampleRules =
    [
        new(1, "Pas de bruit"),
        new(2, "Animaux interdits"),
    ];

    private static RulesController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new RulesController(mediator);
        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleRules);
        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleRules);
    }

    [Fact]
    public async Task GetAll_SendsGetAllRulesQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleRules);
        var controller = CreateController(mediator, Guid.NewGuid());

        await controller.GetAll();

        await mediator.Received(1).Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>());
    }
}
