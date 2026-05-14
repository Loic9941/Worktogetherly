using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Materials.Common;
using WorkTogetherly.Application.Materials.GetAllMaterials;
using WorkTogetherly.Presentation.Controllers.Amenity;

namespace TestWorkTogetherly.Presentation.Material;

public class MaterialsControllerTests
{
    private static readonly List<MaterialResult> SampleMaterials =
    [
        new(1, "Projecteur"),
        new(2, "Tableau blanc"),
    ];

    private static MaterialsController CreateController(IMediator mediator, Guid? userId = null)
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

    [Fact]
    public async Task GetAll_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleMaterials);
        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleMaterials);
    }

    [Fact]
    public async Task GetAll_SendsGetAllMaterialsQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>())
                .Returns(SampleMaterials);
        var controller = CreateController(mediator, Guid.NewGuid());

        await controller.GetAll();

        await mediator.Received(1).Send(Arg.Any<GetAllMaterialsQuery>(), Arg.Any<CancellationToken>());
    }
}
