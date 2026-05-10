using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Application.Workspaces.DeleteWorkspacePhoto;
using WorkTogetherly.Application.Workspaces.GetMyWorkspace;
using WorkTogetherly.Application.Workspaces.GetWorkspaceDetails;
using WorkTogetherly.Application.Workspaces.Search;
using WorkTogetherly.Application.Workspaces.UpdateWorkspace;
using WorkTogetherly.Application.Workspaces.UploadWorkspacePhoto;
using WorkTogetherly.Domain.Interfaces;
using AppWorkspaceErrors = WorkTogetherly.Application.Errors.WorkspaceErrors;
using DomainWorkspaceErrors = WorkTogetherly.Domain.Errors.WorkspaceErrors;

namespace TestWorkTogetherly.Application.Workspace;

public class WorkspaceHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherId = Guid.NewGuid();

    private readonly IWorkspaceRepository _workspaceRepo = Substitute.For<IWorkspaceRepository>();
    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly IReviewRepository _reviewRepo = Substitute.For<IReviewRepository>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public WorkspaceHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    private static CreateWorkspaceCommand MakeCreateCommand(Guid? userId = null) =>
        new(userId ?? OwnerId, "WS", "Desc", "Addr", 48.8, 2.3, 10, true, [], []);

    private static UpdateWorkspaceCommand MakeUpdateCommand(int wsId = 1, Guid? userId = null) =>
        new(wsId, userId ?? OwnerId, "WS Updated", "Desc", "Addr", 48.8, 2.3, 10, true, [], []);

    // ── CreateWorkspaceHandler ────────────────────────────────────────────────

    [Fact]
    public async Task CreateWorkspace_WhenValid_AddsAndReturnsResult()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(workspace.Id, default).Returns(workspace);
        var handler = new CreateWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(MakeCreateCommand(), default);

        result.IsError.Should().BeFalse();
        await _workspaceRepo.Received(1).AddAsync(Arg.Any<WorkTogetherly.Domain.Entities.Workspace>(), default);
        await _workspaceRepo.Received(1).SaveChangesAsync(default);
    }

    // ── UpdateWorkspaceHandler ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateWorkspace_WhenNotFound_ReturnsNotFound()
    {
        _workspaceRepo.GetByIdAsync(1, default).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);
        var handler = new UpdateWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(MakeUpdateCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateWorkspace_WhenNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new UpdateWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(MakeUpdateCommand(userId: OtherId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task UpdateWorkspace_WhenValid_SavesAndReturnsResult()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new UpdateWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(MakeUpdateCommand(), default);

        result.IsError.Should().BeFalse();
        await _workspaceRepo.Received(1).SaveChangesAsync(default);
    }

    // ── GetMyWorkspaceHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMyWorkspace_WhenNoWorkspace_ReturnsNull()
    {
        _workspaceRepo.GetByUserIdWithDetailsAsync(OwnerId, default).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);
        var handler = new GetMyWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(new GetMyWorkspaceQuery(OwnerId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetMyWorkspace_WhenWorkspaceFound_ReturnsResult()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByUserIdWithDetailsAsync(OwnerId, default).Returns(workspace);
        var handler = new GetMyWorkspaceHandler(_workspaceRepo);

        var result = await handler.Handle(new GetMyWorkspaceQuery(OwnerId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(OwnerId);
    }

    // ── GetWorkspaceDetailsHandler ────────────────────────────────────────────

    [Fact]
    public async Task GetWorkspaceDetails_WhenNotFound_ReturnsNotFound()
    {
        _workspaceRepo.GetByIdAsync(1, default).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);
        var handler = new GetWorkspaceDetailsHandler(_workspaceRepo, _slotRepo, _reviewRepo, _clock);

        var result = await handler.Handle(new GetWorkspaceDetailsQuery(1, OwnerId, DateOnly.FromDateTime(DateTime.Today)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetWorkspaceDetails_WhenFound_ReturnsDetailsResult()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        _reviewRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        var handler = new GetWorkspaceDetailsHandler(_workspaceRepo, _slotRepo, _reviewRepo, _clock);

        var result = await handler.Handle(new GetWorkspaceDetailsQuery(1, OwnerId, DateOnly.FromDateTime(DateTime.Today)), default);

        result.IsError.Should().BeFalse();
        result.Value.IsOwner.Should().BeTrue();
    }

    // ── SearchWorkspacesHandler ───────────────────────────────────────────────

    [Fact]
    public async Task SearchWorkspaces_WhenCalled_ReturnsResults()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.SearchAsync(OwnerId, 48.8, 2.3, 20, Arg.Any<DateOnly>(), default)
            .Returns([workspace]);
        var handler = new SearchWorkspacesHandler(_workspaceRepo);

        var result = await handler.Handle(new SearchWorkspacesQuery(48.8, 2.3, 20, DateOnly.FromDateTime(DateTime.Today), OwnerId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchWorkspaces_WhenNoResults_ReturnsEmptyList()
    {
        _workspaceRepo.SearchAsync(OwnerId, 48.8, 2.3, 20, Arg.Any<DateOnly>(), default)
            .Returns([]);
        var handler = new SearchWorkspacesHandler(_workspaceRepo);

        var result = await handler.Handle(new SearchWorkspacesQuery(48.8, 2.3, 20, DateOnly.FromDateTime(DateTime.Today), OwnerId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // ── UploadWorkspacePhotoHandler ───────────────────────────────────────────

    [Fact]
    public async Task UploadWorkspacePhoto_WhenNotFound_ReturnsNotFound()
    {
        _workspaceRepo.GetByIdAsync(1, default).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);
        var handler = new UploadWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new UploadWorkspacePhotoCommand(1, OwnerId, Stream.Null, "photo.jpg", 100), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task UploadWorkspacePhoto_WhenNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new UploadWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new UploadWorkspacePhotoCommand(1, OtherId, Stream.Null, "photo.jpg", 100), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task UploadWorkspacePhoto_WhenValid_SavesFileAndReturnsResult()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _fileService.SaveWorkspacePhotoAsync(1, Arg.Any<Stream>(), "photo.jpg", default).Returns("uploads/workspaces/1/photo.jpg");
        var handler = new UploadWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new UploadWorkspacePhotoCommand(1, OwnerId, Stream.Null, "photo.jpg", 100), default);

        result.IsError.Should().BeFalse();
        await _workspaceRepo.Received(1).SaveChangesAsync(default);
    }

    // ── DeleteWorkspacePhotoHandler ───────────────────────────────────────────

    [Fact]
    public async Task DeleteWorkspacePhoto_WhenNotFound_ReturnsNotFound()
    {
        _workspaceRepo.GetByIdAsync(1, default).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);
        var handler = new DeleteWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new DeleteWorkspacePhotoCommand(1, OwnerId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task DeleteWorkspacePhoto_WhenNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new DeleteWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new DeleteWorkspacePhotoCommand(1, OtherId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task DeleteWorkspacePhoto_WhenPhotoExists_DeletesFileAndSaves()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        workspace.ReplacePhoto("uploads/workspaces/1/photo.jpg");
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new DeleteWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new DeleteWorkspacePhotoCommand(1, OwnerId), default);

        result.IsError.Should().BeFalse();
        await _fileService.Received(1).DeleteFileAsync("uploads/workspaces/1/photo.jpg", default);
        await _workspaceRepo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task DeleteWorkspacePhoto_WhenNoPhoto_DoesNotDeleteFile()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        var handler = new DeleteWorkspacePhotoHandler(_workspaceRepo, _fileService);

        var result = await handler.Handle(new DeleteWorkspacePhotoCommand(1, OwnerId), default);

        result.IsError.Should().BeFalse();
        await _fileService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), default);
    }
}
