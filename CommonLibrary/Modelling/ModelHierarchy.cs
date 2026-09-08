namespace CommonLibrary.Modelling;

/// <summary>
/// Defines the stable error codes produced by model hierarchy validation.
/// </summary>
public static class ModelHierarchyErrorCodes
{
    public const string DuplicateObjectId = nameof(DuplicateObjectId);
    public const string ParentNotFound = nameof(ParentNotFound);
    public const string SelfParent = nameof(SelfParent);
    public const string CycleDetected = nameof(CycleDetected);
    public const string InvalidParentType = nameof(InvalidParentType);
    public const string InvalidRootType = nameof(InvalidRootType);
    public const string MainPartNotFound = nameof(MainPartNotFound);
    public const string MainPartNotDirectChild = nameof(MainPartNotDirectChild);
    public const string PartReferenceMismatch = nameof(PartReferenceMismatch);
}

/// <summary>
/// Defines which model objects may be roots and which parent-child relations are valid.
/// </summary>
public interface IModelHierarchyPolicy
{
    /// <summary>Determines whether <paramref name="child"/> may be a root object.</summary>
    bool CanBeRoot(ModelObject child);

    /// <summary>Determines whether <paramref name="parent"/> may parent <paramref name="child"/>.</summary>
    bool CanParent(ModelObject parent, ModelObject child);
}

/// <summary>
/// Implements the CommonLibrary hierarchy rules for assemblies and parts.
/// </summary>
public sealed class DefaultModelHierarchyPolicy : IModelHierarchyPolicy
{
    public bool CanBeRoot(ModelObject child) => true;

    public bool CanParent(ModelObject parent, ModelObject child) => parent switch
    {
        Assembly => child is Assembly or Part,
        Part => child is IPartOwnedModelObject,
        _ => false
    };
}

/// <summary>
/// Describes one model hierarchy validation error.
/// </summary>
public sealed record ModelHierarchyValidationError(
    string Code,
    Guid ObjectId,
    Guid? ParentId,
    string Message);

/// <summary>
/// Contains a snapshot of all errors found while validating a model hierarchy.
/// </summary>
public sealed class ModelHierarchyValidationResult
{
    public ModelHierarchyValidationResult(IReadOnlyList<ModelHierarchyValidationError> errors)
    {
        Errors = errors;
    }

    public bool IsValid => Errors.Count == 0;

    public IReadOnlyList<ModelHierarchyValidationError> Errors { get; }
}

/// <summary>
/// Base exception for invalid model hierarchy mutations.
/// </summary>
public class ModelHierarchyException : InvalidOperationException
{
    public ModelHierarchyException(string errorCode, string message)
        : base($"{errorCode}: {message}")
    {
        ErrorCode = errorCode;
        Data[nameof(ErrorCode)] = errorCode;
    }

    public string ErrorCode { get; }
}

public sealed class ModelObjectNotFoundException : ModelHierarchyException
{
    public ModelObjectNotFoundException(Guid objectId)
        : base(ModelHierarchyErrorCodes.ParentNotFound, $"ModelObject with Id '{objectId}' was not found.")
    {
        ObjectId = objectId;
    }

    public Guid ObjectId { get; }
}

public sealed class DuplicateModelObjectIdException : ModelHierarchyException
{
    public DuplicateModelObjectIdException(Guid objectId)
        : base(ModelHierarchyErrorCodes.DuplicateObjectId, $"ModelObject with Id '{objectId}' already exists.")
    {
        ObjectId = objectId;
    }

    public Guid ObjectId { get; }
}

public sealed class InvalidModelParentException : ModelHierarchyException
{
    public InvalidModelParentException(string errorCode, Guid objectId, Guid? parentId, string message)
        : base(errorCode, message)
    {
        ObjectId = objectId;
        ParentId = parentId;
    }

    public Guid ObjectId { get; }

    public Guid? ParentId { get; }
}

public sealed class ModelHierarchyValidationException : ModelHierarchyException
{
    public ModelHierarchyValidationException(ModelHierarchyValidationResult result)
        : base(
            result.Errors.FirstOrDefault()?.Code ?? ModelHierarchyErrorCodes.CycleDetected,
            $"The model hierarchy contains {result.Errors.Count} validation error(s).")
    {
        Result = result;
    }

    public ModelHierarchyValidationResult Result { get; }
}

public sealed class ModelHierarchyCycleException : ModelHierarchyException
{
    public ModelHierarchyCycleException(Guid objectId, Guid? parentId)
        : base(
            ModelHierarchyErrorCodes.CycleDetected,
            $"Setting parent '{parentId}' for object '{objectId}' would create a cycle.")
    {
        ObjectId = objectId;
        ParentId = parentId;
    }

    public Guid ObjectId { get; }

    public Guid? ParentId { get; }
}
