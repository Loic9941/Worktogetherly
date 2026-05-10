using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Materials.Common;
using WorkTogetherly.Application.Materials.GetAllMaterials;
using WorkTogetherly.Application.Rules.Common;
using WorkTogetherly.Application.Rules.GetAllRules;
using WorkTogetherly.Presentation.Controllers.Amenity;

namespace TestWorkTogetherly.Presentation.Amenity;

public class AmenitiesControllerTests
{
    private static readonly List<MaterialResult> SampleMaterials =
    [
        new(1, "Projecteur"),
        new(2, "Tableau blanc"),
    ];

    private static readonly List<RuleResult> SampleRules =
    [
        new(1, "Pas de bruit"),
        new(2, "Animaux interdits"),
    ];

    private static MaterialsController CreateMaterialsController(IMediator mediator, Guid? userId = null)
    {
        var controller = new MaterialsController(mediator);
        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
        return controller;
    }

    private static RulesController CreateRulesController(IMediator mediator, Guid? userId = null)
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

    // ── MaterialsController ───────────────────────────────────────────────────

    [Fact]
    public async Task Materials_GetAll_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleMaterials);
        var controller = CreateMaterialsController(mediator, Guid.NewGuid());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleMaterials);
    }

    [Fact]
    public async Task Materials_GetAll_SendsGetAllMaterialsQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleMaterials);
        var controller = CreateMaterialsController(mediator, Guid.NewGuid());

        await controller.GetAll();

        await mediator.Received(1).Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>());
    }

    // ── RulesController ───────────────────────────────────────────────────────

    [Fact]
    public async Task Rules_GetAll_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleRules);
        var controller = CreateRulesController(mediator, Guid.NewGuid());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleRules);
    }

    [Fact]
    public async Task Rules_GetAll_SendsGetAllRulesQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleRules);
        var controller = CreateRulesController(mediator, Guid.NewGuid());

        await controller.GetAll();

        await mediator.Received(1).Send(Arg.Any<GetAllRulesQuery>(), Arg.Any<CancellationToken>());
    }
}
