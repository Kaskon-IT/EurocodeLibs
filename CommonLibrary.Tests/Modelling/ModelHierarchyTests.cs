using System.Text.Json;

namespace CommonLibrary.Tests.Modelling;

public class ModelHierarchyTests
{
    [Fact]
    public void EmptyModelIsValid()
    {
        var model = new Model();

        Assert.True(model.ValidateHierarchy().IsValid);
        Assert.Empty(model.GetRoots());
    }

    [Fact]
    public void AssemblyBeamAndSlabMayBeIndependentRoots()
    {
        var model = new Model();
        var assembly = new Assembly();
        var beam = new Beam();
        var slab = new Slab();

        model.Add(assembly);
        model.Add(beam);
        model.Add(slab);

        Assert.Equal(new ModelObject[] { assembly, beam, slab }, model.GetRoots());
        Assert.True(model.ValidateHierarchy().IsValid);
    }

    [Fact]
    public void AssemblyMayContainAssembliesAndParts()
    {
        var model = new Model();
        var root = new Assembly();
        var childAssembly = new Assembly();
        var beam = new Beam();
        var slab = new Slab();

        model.Add(root);
        model.Add(childAssembly, root.Id);
        model.Add(beam, childAssembly.Id);
        model.Add(slab, root.Id);

        Assert.True(model.ValidateHierarchy().IsValid);
        Assert.Equal(new ModelObject[] { childAssembly, slab }, model.GetChildren(root.Id));
    }

    [Fact]
    public void AssemblySupportsMainPartSecondaryPartsAndSubAssemblies()
    {
        var model = new Model();
        var assembly = new Assembly();
        var subAssembly = new Assembly();
        var mainPart = new Beam();
        var secondaryPart = new Slab();

        model.Add(assembly);
        model.Add(subAssembly, assembly.Id);
        model.Add(mainPart, assembly.Id);
        model.Add(secondaryPart, assembly.Id);
        model.SetMainPart(assembly.Id, mainPart.Id);

        Assert.Same(mainPart, model.GetMainPart(assembly.Id));
        Assert.Equal(new Part[] { secondaryPart }, model.GetSecondaryParts(assembly.Id));
        Assert.Equal(mainPart.Id, assembly.MainPartId);
        Assert.Contains(subAssembly, model.GetChildren(assembly.Id));
        Assert.True(model.ValidateHierarchy().IsValid);
    }

    [Fact]
    public void PartMayParentReinforcementAndLoads()
    {
        var model = new Model();
        var beam = new Beam();
        var reinforcement = new WapeningGroep();
        var load = new PointLoad();

        model.Add(beam);
        model.Add(reinforcement, beam.Id);
        model.Add(load, beam.Id);

        Assert.Equal(beam.Id, reinforcement.ParentId);
        Assert.Equal(beam.Id, reinforcement.PartId);
        Assert.Equal(beam.Id, load.ParentId);
        Assert.Equal(beam.Id, load.PartId);
        Assert.Equal(new ModelObject[] { reinforcement, load }, model.GetChildren(beam.Id));
        Assert.True(model.ValidateHierarchy().IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidParentRelations))]
    public void PartCannotParentAssembliesOrParts(ModelObject parent, ModelObject child)
    {
        var model = new Model();
        model.Add(parent);

        InvalidModelParentException exception = Assert.Throws<InvalidModelParentException>(
            () => model.Add(child, parent.Id));

        Assert.Equal(ModelHierarchyErrorCodes.InvalidParentType, exception.ErrorCode);
        Assert.DoesNotContain(child, model.Objects);
    }

    public static TheoryData<ModelObject, ModelObject> InvalidParentRelations => new()
    {
        { new Beam(), new Assembly() },
        { new Slab(), new Assembly() },
        { new Beam(), new Beam() },
        { new Beam(), new Slab() },
        { new Slab(), new Slab() }
    };

    [Fact]
    public void MissingParentIsRejectedWithoutAddingObject()
    {
        var model = new Model();
        var beam = new Beam();

        ModelObjectNotFoundException exception = Assert.Throws<ModelObjectNotFoundException>(
            () => model.Add(beam, Guid.NewGuid()));

        Assert.Equal(ModelHierarchyErrorCodes.ParentNotFound, exception.ErrorCode);
        Assert.Empty(model.Objects);
    }

    [Fact]
    public void DuplicateIdentifierIsRejected()
    {
        Guid id = Guid.NewGuid();
        var model = new Model();
        model.Add(new Beam { Id = id });

        DuplicateModelObjectIdException exception = Assert.Throws<DuplicateModelObjectIdException>(
            () => model.Add(new Slab { Id = id }));

        Assert.Equal(ModelHierarchyErrorCodes.DuplicateObjectId, exception.ErrorCode);
    }

    [Fact]
    public void QueriesUseDocumentedOrderAndUnknownIdsAreTolerated()
    {
        var model = new Model();
        var root = new Assembly();
        var firstAssembly = new Assembly();
        var nestedBeam = new Beam();
        var rootSlab = new Slab();
        var secondAssembly = new Assembly();

        model.Add(root);
        model.Add(firstAssembly, root.Id);
        model.Add(nestedBeam, firstAssembly.Id);
        model.Add(rootSlab, root.Id);
        model.Add(secondAssembly, root.Id);

        Assert.Equal(
            new ModelObject[] { firstAssembly, nestedBeam, rootSlab, secondAssembly },
            model.GetDescendants(root.Id));
        Assert.Equal(new ModelObject[] { firstAssembly, root }, model.GetAncestors(nestedBeam.Id));
        Assert.Same(firstAssembly, model.GetParent(nestedBeam.Id));
        Assert.True(model.IsDescendantOf(nestedBeam.Id, root.Id));
        Assert.False(model.IsDescendantOf(rootSlab.Id, firstAssembly.Id));
        Assert.Empty(model.GetChildren(Guid.NewGuid()));
        Assert.Empty(model.GetDescendants(Guid.NewGuid()));
        Assert.Empty(model.GetAncestors(Guid.NewGuid()));
        Assert.Null(model.GetParent(Guid.NewGuid()));
    }

    [Fact]
    public void GetPartsSupportsAllDirectAndRecursiveQueries()
    {
        var model = new Model();
        var root = new Assembly();
        var childAssembly = new Assembly();
        var directBeam = new Beam();
        var nestedSlab = new Slab();
        var independentBeam = new Beam();

        model.Add(root);
        model.Add(childAssembly, root.Id);
        model.Add(directBeam, root.Id);
        model.Add(nestedSlab, childAssembly.Id);
        model.Add(independentBeam);

        Assert.Equal(new Part[] { directBeam }, model.GetParts(root.Id));
        Assert.Equal(new Part[] { nestedSlab, directBeam }, model.GetParts(root.Id, recursive: true));
        Assert.Equal(new Part[] { directBeam, nestedSlab, independentBeam }, model.GetParts());
        Assert.Empty(model.GetParts(Guid.NewGuid(), recursive: true));
    }

    [Fact]
    public void SetParentDetachAndMoveSubtreePreserveChildren()
    {
        var model = new Model();
        var firstRoot = new Assembly();
        var secondRoot = new Assembly();
        var childAssembly = new Assembly();
        var beam = new Beam();

        model.Add(firstRoot);
        model.Add(secondRoot);
        model.Add(childAssembly, firstRoot.Id);
        model.Add(beam, childAssembly.Id);

        model.MoveSubtree(childAssembly.Id, secondRoot.Id);
        Assert.Equal(secondRoot.Id, childAssembly.ParentId);
        Assert.Equal(childAssembly.Id, beam.ParentId);

        model.Detach(childAssembly.Id);
        Assert.Null(childAssembly.ParentId);
        Assert.Contains(childAssembly, model.GetRoots());
    }

    [Fact]
    public void ReparentingPartOwnedObjectSynchronizesPartId()
    {
        var model = new Model();
        var firstBeam = new Beam();
        var secondBeam = new Beam();
        var reinforcement = new WapeningGroep();
        model.Add(firstBeam);
        model.Add(secondBeam);
        model.Add(reinforcement, firstBeam.Id);

        model.SetParent(reinforcement.Id, secondBeam.Id);

        Assert.Equal(secondBeam.Id, reinforcement.ParentId);
        Assert.Equal(secondBeam.Id, reinforcement.PartId);

        model.Detach(reinforcement.Id);
        Assert.Null(reinforcement.ParentId);
        Assert.Null(reinforcement.PartId);
    }

    [Fact]
    public void MainPartMustBeDirectChildAndCannotBeMovedWhileConfigured()
    {
        var model = new Model();
        var firstAssembly = new Assembly();
        var secondAssembly = new Assembly();
        var mainPart = new Beam();
        var unrelatedPart = new Slab();
        model.Add(firstAssembly);
        model.Add(secondAssembly);
        model.Add(mainPart, firstAssembly.Id);
        model.Add(unrelatedPart, secondAssembly.Id);

        InvalidModelParentException invalidMainPart = Assert.Throws<InvalidModelParentException>(
            () => model.SetMainPart(firstAssembly.Id, unrelatedPart.Id));
        Assert.Equal(ModelHierarchyErrorCodes.MainPartNotDirectChild, invalidMainPart.ErrorCode);

        model.SetMainPart(firstAssembly.Id, mainPart.Id);
        InvalidModelParentException invalidMove = Assert.Throws<InvalidModelParentException>(
            () => model.SetParent(mainPart.Id, secondAssembly.Id));
        Assert.Equal(ModelHierarchyErrorCodes.MainPartNotDirectChild, invalidMove.ErrorCode);
        Assert.Equal(firstAssembly.Id, mainPart.ParentId);
    }

    [Fact]
    public void RemovingMainPartClearsAssemblyMainPartId()
    {
        var model = new Model();
        var assembly = new Assembly();
        var mainPart = new Beam();
        model.Add(assembly);
        model.Add(mainPart, assembly.Id);
        model.SetMainPart(assembly.Id, mainPart.Id);

        model.Remove(mainPart);

        Assert.Null(assembly.MainPartId);
        Assert.True(model.ValidateHierarchy().IsValid);
    }

    [Fact]
    public void SelfParentIsRejectedAtomically()
    {
        var model = new Model();
        var assembly = new Assembly();
        model.Add(assembly);

        InvalidModelParentException exception = Assert.Throws<InvalidModelParentException>(
            () => model.SetParent(assembly.Id, assembly.Id));

        Assert.Equal(ModelHierarchyErrorCodes.SelfParent, exception.ErrorCode);
        Assert.Null(assembly.ParentId);
    }

    [Fact]
    public void MovingObjectBelowDescendantIsRejectedAtomically()
    {
        var model = new Model();
        var root = new Assembly();
        var child = new Assembly();
        var grandchild = new Assembly();
        model.Add(root);
        model.Add(child, root.Id);
        model.Add(grandchild, child.Id);

        Assert.Throws<ModelHierarchyCycleException>(
            () => model.SetParent(root.Id, grandchild.Id));

        Assert.Null(root.ParentId);
        Assert.Equal(root.Id, child.ParentId);
    }

    [Fact]
    public void RemoveSubtreeUsesPostOrderAndLeavesOtherRootsUntouched()
    {
        var model = new Model();
        var root = new Assembly();
        var child = new Assembly();
        var beam = new Beam();
        var slab = new Slab();
        var otherRoot = new Beam();
        model.Add(root);
        model.Add(child, root.Id);
        model.Add(beam, child.Id);
        model.Add(slab, root.Id);
        model.Add(otherRoot);

        IReadOnlyList<ModelObject> removed = model.RemoveSubtree(root.Id);

        Assert.Equal(new ModelObject[] { slab, beam, child, root }, removed);
        Assert.Equal(new ModelObject[] { otherRoot }, model.Objects);
        Assert.True(model.ValidateHierarchy().IsValid);
    }

    [Fact]
    public void ExistingRemoveAlsoRemovesTheSubtree()
    {
        var model = new Model();
        var root = new Assembly();
        var beam = new Beam();
        model.Add(root);
        model.Add(beam, root.Id);

        Assert.True(model.Remove(root));
        Assert.Empty(model.Objects);
        Assert.False(model.Remove(root));
    }

    [Fact]
    public void ValidatorReportsMissingParentSelfParentAndInvalidParentType()
    {
        var model = new Model();
        var assembly = new Assembly();
        var beam = new Beam();
        var slab = new Slab();
        model.Add(assembly);
        model.Add(beam);
        model.Add(slab);

        assembly.ParentId = assembly.Id;
        beam.ParentId = Guid.NewGuid();
        slab.ParentId = beam.Id;

        ModelHierarchyValidationResult result = model.ValidateHierarchy();

        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.SelfParent);
        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.ParentNotFound);
        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.InvalidParentType);
    }

    [Fact]
    public void ValidatorReportsInvalidMainPartAndPartReference()
    {
        var model = new Model();
        var assembly = new Assembly();
        var beam = new Beam();
        var reinforcement = new WapeningGroep();
        model.Add(assembly);
        model.Add(beam, assembly.Id);
        model.Add(reinforcement, beam.Id);

        assembly.MainPartId = Guid.NewGuid();
        reinforcement.PartId = Guid.NewGuid();

        ModelHierarchyValidationResult result = model.ValidateHierarchy();

        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.MainPartNotFound);
        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.PartReferenceMismatch);
    }

    [Fact]
    public void ValidatorDetectsMultiObjectCycleWithoutLooping()
    {
        var model = new Model();
        var first = new Assembly();
        var second = new Assembly();
        var third = new Assembly();
        model.Add(first);
        model.Add(second, first.Id);
        model.Add(third, second.Id);
        first.ParentId = third.Id;

        ModelHierarchyValidationResult result = model.ValidateHierarchy();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.CycleDetected);
        Assert.Throws<ModelHierarchyValidationException>(() => model.EnsureValidHierarchy());
    }

    [Fact]
    public void ValidatorReportsDuplicateIdentifiersInCorruptCollection()
    {
        Guid id = Guid.NewGuid();
        var model = new Model();
        model.Add(new Beam { Id = id });
        GetMutableObjects(model).Add(new Slab { Id = id });

        ModelHierarchyValidationResult result = model.ValidateHierarchy();

        Assert.Contains(result.Errors, x => x.Code == ModelHierarchyErrorCodes.DuplicateObjectId);
    }

    [Fact]
    public void CustomPolicyCanAddNewParentAndRootRules()
    {
        var model = new Model(new CustomHierarchyPolicy());
        var container = new CustomContainer();
        var child = new CustomChild();
        model.Add(container);
        model.Add(child, container.Id);

        Assert.True(model.ValidateHierarchy().IsValid);
        Assert.Equal(new ModelObject[] { child }, model.GetChildren(container.Id));
    }

    [Fact]
    public void CustomPolicyCanRejectRootType()
    {
        var model = new Model(new CustomHierarchyPolicy());

        InvalidModelParentException exception = Assert.Throws<InvalidModelParentException>(
            () => model.Add(new CustomChild()));

        Assert.Equal(ModelHierarchyErrorCodes.InvalidRootType, exception.ErrorCode);
    }

    [Fact]
    public void ParentIdRoundTripsAndOldJsonDefaultsToRoot()
    {
        Guid parentId = Guid.NewGuid();
        var beam = new Beam { ParentId = parentId };

        string json = JsonSerializer.Serialize(beam);
        Beam? roundTripped = JsonSerializer.Deserialize<Beam>(json);
        Beam? oldModel = JsonSerializer.Deserialize<Beam>("{}");

        Assert.Equal(parentId, roundTripped?.ParentId);
        Assert.Null(oldModel?.ParentId);
    }

    [Fact]
    public void MainPartIdAndReinforcementPartIdRoundTrip()
    {
        Guid mainPartId = Guid.NewGuid();
        var assembly = new Assembly { MainPartId = mainPartId };
        var reinforcement = new WapeningGroep { PartId = mainPartId, ParentId = mainPartId };
        var options = new JsonSerializerOptions { IgnoreReadOnlyProperties = true };

        Assembly? roundTrippedAssembly = JsonSerializer.Deserialize<Assembly>(
            JsonSerializer.Serialize(assembly, options), options);
        WapeningGroep? roundTrippedReinforcement = JsonSerializer.Deserialize<WapeningGroep>(
            JsonSerializer.Serialize(reinforcement, options), options);

        Assert.Equal(mainPartId, roundTrippedAssembly?.MainPartId);
        Assert.Equal(mainPartId, roundTrippedReinforcement?.PartId);
        Assert.Equal(mainPartId, roundTrippedReinforcement?.ParentId);
    }

    private static List<ModelObject> GetMutableObjects(Model model)
    {
        System.Reflection.FieldInfo field = typeof(Model).GetField(
            "_objects",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Model backing collection was not found.");
        return (List<ModelObject>)field.GetValue(model)!;
    }

    private sealed class CustomContainer : ModelObject;

    private sealed class CustomChild : ModelObject;

    private sealed class CustomHierarchyPolicy : IModelHierarchyPolicy
    {
        public bool CanBeRoot(ModelObject child) => child is CustomContainer;

        public bool CanParent(ModelObject parent, ModelObject child) =>
            parent is CustomContainer && child is CustomChild;
    }
}
