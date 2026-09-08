# CommonLibrary model hierarchy

## Types and namespace

All hierarchy types are in `CommonLibrary.Modelling`:

- `Model`, `ModelObject`, `Assembly`, `Part`, `Beam`, `Slab`
- `IModelHierarchyPolicy`, `DefaultModelHierarchyPolicy`
- `ModelHierarchyValidationResult`, `ModelHierarchyValidationError`
- hierarchy exception types and `ModelHierarchyErrorCodes`

## Queries

`Model` exposes `GetRoots`, `GetParent`, `GetChildren`, `GetDescendants`, `GetAncestors`, `IsDescendantOf`, and `GetParts`.

Returned collections are snapshots. Direct children and roots follow `Model.Objects` insertion order. Descendants use depth-first pre-order with siblings in insertion order. Ancestors start at the direct parent and end at the root. Queries for unknown identifiers return an empty snapshot, `null`, or `false`, as appropriate.

## Mutations

`Add`, `SetParent`, `MoveSubtree`, and `Detach` validate the complete requested relation before changing `ParentId`. `RemoveSubtree` removes descendants before the subtree root and returns that post-order snapshot. The compatibility method `Remove(ModelObject)` now removes the complete subtree.

Mutation failures use:

- `ModelObjectNotFoundException`
- `DuplicateModelObjectIdException`
- `InvalidModelParentException`
- `ModelHierarchyCycleException`
- `ModelHierarchyValidationException`

Stable validation codes are `DuplicateObjectId`, `ParentNotFound`, `SelfParent`, `CycleDetected`, `InvalidParentType`, and `InvalidRootType`.

## Parent policy

The default policy permits every `ModelObject` as a root for compatibility with existing loads and future functional objects. Only an `Assembly` can be a parent by default, and it can contain another `Assembly` or a `Part`.

Package consumers can implement `IModelHierarchyPolicy` and pass it to `Model(IModelHierarchyPolicy)` to support additional root and parent-child rules without changing CommonLibrary.

## Serialization

`ModelObject.ParentId` is a nullable `Guid`. Existing serialized objects without `ParentId` deserialize with `ParentId = null` and therefore remain roots. `Model.Objects` remains externally read-only; model mutations are managed through `Model` methods.

## Package

The implementation remains in the current `Kaskon.CommonLibrary` package version `6.0.1`. Publishing a new package version is a separate release action.
