using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Modelling
{
    public class Model
    {
        private readonly List<ModelObject> _objects = [];
        private readonly IModelHierarchyPolicy _hierarchyPolicy;

        /// <summary>
        /// Initializes an empty model using the default CommonLibrary hierarchy policy.
        /// </summary>
        public Model()
            : this(new DefaultModelHierarchyPolicy())
        {
        }

        /// <summary>
        /// Initializes an empty model using a custom hierarchy policy.
        /// </summary>
        public Model(IModelHierarchyPolicy hierarchyPolicy)
        {
            _hierarchyPolicy = hierarchyPolicy
                ?? throw new ArgumentNullException(nameof(hierarchyPolicy));
        }

        /// <summary>
        /// Gets a read-only view of all model objects in insertion order.
        /// Mutations must be performed through <see cref="Model"/> methods.
        /// </summary>
        public IReadOnlyList<ModelObject> Objects => _objects;

        /// <summary>
        /// Returns a snapshot of all root objects in insertion order.
        /// </summary>
        public IReadOnlyList<ModelObject> GetRoots() =>
            _objects.Where(x => x.ParentId is null).ToArray();

        /// <summary>
        /// Returns the parent of an object, or <see langword="null"/> when the object,
        /// or its referenced parent, does not exist.
        /// </summary>
        public ModelObject? GetParent(Guid objectId)
        {
            ModelObject? modelObject = FindById(objectId);
            return modelObject?.ParentId is Guid parentId
                ? FindById(parentId)
                : null;
        }

        /// <summary>
        /// Returns a snapshot of the direct children in insertion order.
        /// An unknown parent identifier returns an empty snapshot.
        /// </summary>
        public IReadOnlyList<ModelObject> GetChildren(Guid parentId)
        {
            if (FindById(parentId) is null)
                return Array.Empty<ModelObject>();

            return _objects.Where(x => x.ParentId == parentId).ToArray();
        }

        /// <summary>
        /// Returns a depth-first pre-order snapshot of all descendants. Siblings follow
        /// insertion order. An unknown identifier returns an empty snapshot.
        /// </summary>
        public IReadOnlyList<ModelObject> GetDescendants(Guid parentId)
        {
            if (FindById(parentId) is null)
                return Array.Empty<ModelObject>();

            var descendants = new List<ModelObject>();
            var visited = new HashSet<Guid> { parentId };

            AddDescendants(parentId, descendants, visited);
            return descendants;
        }

        /// <summary>
        /// Returns a snapshot starting at the direct parent and ending at the root.
        /// An unknown identifier returns an empty snapshot.
        /// </summary>
        public IReadOnlyList<ModelObject> GetAncestors(Guid objectId)
        {
            ModelObject? current = FindById(objectId);
            if (current is null)
                return Array.Empty<ModelObject>();

            var ancestors = new List<ModelObject>();
            var visited = new HashSet<Guid> { objectId };

            while (current.ParentId is Guid parentId && visited.Add(parentId))
            {
                current = FindById(parentId);
                if (current is null)
                    break;

                ancestors.Add(current);
            }

            return ancestors;
        }

        /// <summary>
        /// Determines whether an object is below the specified possible ancestor.
        /// Unknown identifiers return <see langword="false"/>.
        /// </summary>
        public bool IsDescendantOf(Guid objectId, Guid possibleAncestorId) =>
            objectId != possibleAncestorId &&
            GetAncestors(objectId).Any(x => x.Id == possibleAncestorId);

        /// <summary>
        /// Returns all parts when <paramref name="assemblyId"/> is <see langword="null"/>.
        /// For an assembly, returns direct parts or all nested parts when recursive.
        /// Unknown and non-assembly identifiers return an empty snapshot.
        /// </summary>
        public IReadOnlyList<Part> GetParts(Guid? assemblyId = null, bool recursive = false)
        {
            if (assemblyId is null)
                return _objects.OfType<Part>().ToArray();

            if (FindById(assemblyId.Value) is not Assembly)
                return Array.Empty<Part>();

            IEnumerable<ModelObject> candidates = recursive
                ? GetDescendants(assemblyId.Value)
                : GetChildren(assemblyId.Value);

            return candidates.OfType<Part>().ToArray();
        }

        /// <summary>
        /// Returns the main part of an assembly, or <see langword="null"/> when the
        /// assembly or its configured direct child part does not exist.
        /// </summary>
        public Part? GetMainPart(Guid assemblyId)
        {
            if (FindById(assemblyId) is not Assembly { MainPartId: Guid mainPartId } assembly)
                return null;

            return FindById(mainPartId) is Part { ParentId: var parentId } part
                && parentId == assembly.Id
                    ? part
                    : null;
        }

        /// <summary>Returns all direct child parts except the configured main part.</summary>
        public IReadOnlyList<Part> GetSecondaryParts(Guid assemblyId)
        {
            if (FindById(assemblyId) is not Assembly assembly)
                return Array.Empty<Part>();

            return GetChildren(assemblyId)
                .OfType<Part>()
                .Where(part => part.Id != assembly.MainPartId)
                .ToArray();
        }

        /// <summary>
        /// Sets or clears the main part. A configured main part must be a direct child
        /// of the assembly.
        /// </summary>
        public void SetMainPart(Guid assemblyId, Guid? partId)
        {
            if (FindById(assemblyId) is not Assembly assembly)
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.InvalidParentType,
                    assemblyId,
                    null,
                    $"Object '{assemblyId}' is not an assembly.");
            }

            if (partId is null)
            {
                assembly.MainPartId = null;
                return;
            }

            if (FindById(partId.Value) is not Part part)
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.MainPartNotFound,
                    assemblyId,
                    partId,
                    $"Main part '{partId}' was not found or is not a part.");
            }

            if (part.ParentId != assembly.Id)
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.MainPartNotDirectChild,
                    assemblyId,
                    partId,
                    $"Main part '{partId}' must be a direct child of assembly '{assemblyId}'.");
            }

            assembly.MainPartId = part.Id;
        }

        /// <summary>
        /// Validates the complete hierarchy without mutating the model. All safely
        /// detectable errors are returned in an immutable snapshot.
        /// </summary>
        public ModelHierarchyValidationResult ValidateHierarchy()
        {
            var errors = new List<ModelHierarchyValidationError>();
            var groupsById = _objects.GroupBy(x => x.Id).ToArray();
            var objectsById = groupsById.ToDictionary(x => x.Key, x => x.First());

            foreach (IGrouping<Guid, ModelObject> group in groupsById.Where(x => x.Count() > 1))
            {
                foreach (ModelObject duplicate in group.Skip(1))
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.DuplicateObjectId,
                        duplicate.Id,
                        duplicate.ParentId,
                        $"More than one object has Id '{duplicate.Id}'."));
                }
            }

            foreach (Assembly assembly in _objects.OfType<Assembly>().Where(x => x.MainPartId.HasValue))
            {
                Guid mainPartId = assembly.MainPartId!.Value;
                if (!objectsById.TryGetValue(mainPartId, out ModelObject? mainPart) || mainPart is not Part)
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.MainPartNotFound,
                        assembly.Id,
                        mainPartId,
                        $"Main part '{mainPartId}' was not found or is not a part."));
                }
                else if (mainPart.ParentId != assembly.Id)
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.MainPartNotDirectChild,
                        assembly.Id,
                        mainPartId,
                        $"Main part '{mainPartId}' is not a direct child of assembly '{assembly.Id}'."));
                }
            }

            foreach (ModelObject modelObject in _objects.Where(x => x is IPartOwnedModelObject))
            {
                var partOwnedObject = (IPartOwnedModelObject)modelObject;
                Guid? expectedPartId = modelObject.ParentId is Guid parentId
                    && objectsById.TryGetValue(parentId, out ModelObject? parent)
                    && parent is Part
                        ? parentId
                        : null;

                if (partOwnedObject.PartId != expectedPartId)
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.PartReferenceMismatch,
                        modelObject.Id,
                        modelObject.ParentId,
                        $"PartId '{partOwnedObject.PartId}' does not match part parent '{expectedPartId}'."));
                }
            }

            foreach (ModelObject modelObject in _objects)
            {
                if (modelObject.ParentId is not Guid parentId)
                {
                    if (!_hierarchyPolicy.CanBeRoot(modelObject))
                    {
                        errors.Add(new ModelHierarchyValidationError(
                            ModelHierarchyErrorCodes.InvalidRootType,
                            modelObject.Id,
                            null,
                            $"Object type '{modelObject.GetType().Name}' cannot be a root."));
                    }

                    continue;
                }

                if (modelObject.Id == parentId)
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.SelfParent,
                        modelObject.Id,
                        parentId,
                        "An object cannot be its own parent."));
                    continue;
                }

                if (!objectsById.TryGetValue(parentId, out ModelObject? parent))
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.ParentNotFound,
                        modelObject.Id,
                        parentId,
                        $"Parent '{parentId}' was not found."));
                    continue;
                }

                if (!_hierarchyPolicy.CanParent(parent, modelObject))
                {
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.InvalidParentType,
                        modelObject.Id,
                        parentId,
                        $"Object type '{parent.GetType().Name}' cannot parent '{modelObject.GetType().Name}'."));
                }
            }

            var states = objectsById.Keys.ToDictionary(x => x, _ => VisitState.Unvisited);
            var reportedCycles = new HashSet<Guid>();

            foreach (Guid objectId in objectsById.Keys)
                VisitForCycles(objectId, objectsById, states, reportedCycles, errors);

            return new ModelHierarchyValidationResult(errors.ToArray());
        }

        /// <summary>
        /// Throws <see cref="ModelHierarchyValidationException"/> when hierarchy
        /// validation produces one or more errors.
        /// </summary>
        public void EnsureValidHierarchy()
        {
            ModelHierarchyValidationResult result = ValidateHierarchy();
            if (!result.IsValid)
                throw new ModelHierarchyValidationException(result);
        }

        /// <summary>
        /// Atomically adds an object as a root or as a child of an existing object.
        /// Duplicate identifiers and invalid parent relations are rejected.
        /// </summary>
        public void Add(ModelObject modelObject, Guid? parentId = null)
        {
            ArgumentNullException.ThrowIfNull(modelObject);

            if (_objects.Any(x => x.Id == modelObject.Id))
                throw new DuplicateModelObjectIdException(modelObject.Id);

            ValidateParentChange(modelObject, parentId);
            modelObject.ParentId = parentId;
            SynchronizePartReference(modelObject, parentId);
            _objects.Add(modelObject);
        }

        /// <summary>
        /// Atomically assigns a parent. A <see langword="null"/> parent detaches the
        /// object as a root when allowed by the hierarchy policy.
        /// </summary>
        public void SetParent(Guid objectId, Guid? parentId)
        {
            ModelObject modelObject = FindById(objectId)
                ?? throw new ModelObjectNotFoundException(objectId);

            ValidateParentChange(modelObject, parentId);
            modelObject.ParentId = parentId;
            SynchronizePartReference(modelObject, parentId);
        }

        /// <summary>
        /// Moves an object and its existing descendants below another parent.
        /// </summary>
        public void MoveSubtree(Guid objectId, Guid? newParentId) =>
            SetParent(objectId, newParentId);

        /// <summary>
        /// Makes an object a root when allowed by the hierarchy policy.
        /// </summary>
        public void Detach(Guid objectId) => SetParent(objectId, null);

        /// <summary>
        /// Removes an object and its complete subtree. Returns <see langword="false"/>
        /// when the supplied instance is not part of this model.
        /// </summary>
        public bool Remove(ModelObject modelObject)
        {
            ArgumentNullException.ThrowIfNull(modelObject);

            if (!_objects.Contains(modelObject))
                return false;

            RemoveSubtree(modelObject.Id);
            return true;
        }

        /// <summary>
        /// Removes a complete subtree and returns a snapshot in post-order:
        /// descendants precede the subtree root. An unknown identifier is rejected.
        /// </summary>
        public IReadOnlyList<ModelObject> RemoveSubtree(Guid objectId)
        {
            ModelObject root = FindById(objectId)
                ?? throw new ModelObjectNotFoundException(objectId);

            var removed = GetDescendants(objectId).Reverse().ToList();
            removed.Add(root);

            var removedIds = removed.Select(x => x.Id).ToHashSet();
            foreach (Assembly assembly in _objects.OfType<Assembly>()
                         .Where(x => x.MainPartId is Guid mainPartId && removedIds.Contains(mainPartId)))
            {
                assembly.MainPartId = null;
            }
            _objects.RemoveAll(x => removedIds.Contains(x.Id));
            return removed;
        }

        public T? Get<T>(Guid id) where T : ModelObject
            => _objects.OfType<T>().FirstOrDefault(x => x.Id == id);

        public IEnumerable<T> GetObjects<T>() where T : ModelObject
            => _objects.OfType<T>();

        private ModelObject? FindById(Guid id) =>
            _objects.FirstOrDefault(x => x.Id == id);

        private void ValidateParentChange(ModelObject modelObject, Guid? parentId)
        {
            Assembly? mainPartAssembly = _objects.OfType<Assembly>()
                .FirstOrDefault(x => x.MainPartId == modelObject.Id);
            if (mainPartAssembly is not null && parentId != mainPartAssembly.Id)
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.MainPartNotDirectChild,
                    modelObject.Id,
                    parentId,
                    $"Main part '{modelObject.Id}' must remain a direct child of assembly '{mainPartAssembly.Id}'.");
            }

            if (parentId is null)
            {
                if (!_hierarchyPolicy.CanBeRoot(modelObject))
                {
                    throw new InvalidModelParentException(
                        ModelHierarchyErrorCodes.InvalidRootType,
                        modelObject.Id,
                        null,
                        $"Object type '{modelObject.GetType().Name}' cannot be a root.");
                }

                return;
            }

            if (modelObject.Id == parentId.Value)
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.SelfParent,
                    modelObject.Id,
                    parentId,
                    "An object cannot be its own parent.");
            }

            ModelObject parent = FindById(parentId.Value)
                ?? throw new ModelObjectNotFoundException(parentId.Value);

            if (!_hierarchyPolicy.CanParent(parent, modelObject))
            {
                throw new InvalidModelParentException(
                    ModelHierarchyErrorCodes.InvalidParentType,
                    modelObject.Id,
                    parentId,
                    $"Object type '{parent.GetType().Name}' cannot parent '{modelObject.GetType().Name}'.");
            }

            if (IsDescendantOf(parent.Id, modelObject.Id))
                throw new ModelHierarchyCycleException(modelObject.Id, parentId);
        }

        private static void SynchronizePartReference(ModelObject modelObject, Guid? parentId)
        {
            if (modelObject is IPartOwnedModelObject partOwnedObject)
                partOwnedObject.PartId = parentId;
        }

        private void AddDescendants(
            Guid parentId,
            List<ModelObject> descendants,
            HashSet<Guid> visited)
        {
            foreach (ModelObject child in _objects.Where(x => x.ParentId == parentId))
            {
                if (!visited.Add(child.Id))
                    continue;

                descendants.Add(child);
                AddDescendants(child.Id, descendants, visited);
            }
        }

        private static void VisitForCycles(
            Guid objectId,
            IReadOnlyDictionary<Guid, ModelObject> objectsById,
            IDictionary<Guid, VisitState> states,
            ISet<Guid> reportedCycles,
            ICollection<ModelHierarchyValidationError> errors)
        {
            if (states[objectId] == VisitState.Visited)
                return;

            if (states[objectId] == VisitState.Visiting)
            {
                if (reportedCycles.Add(objectId))
                {
                    ModelObject modelObject = objectsById[objectId];
                    errors.Add(new ModelHierarchyValidationError(
                        ModelHierarchyErrorCodes.CycleDetected,
                        objectId,
                        modelObject.ParentId,
                        $"A hierarchy cycle includes object '{objectId}'."));
                }

                return;
            }

            states[objectId] = VisitState.Visiting;
            ModelObject current = objectsById[objectId];

            if (current.ParentId is Guid parentId &&
                parentId != objectId &&
                objectsById.ContainsKey(parentId))
            {
                VisitForCycles(parentId, objectsById, states, reportedCycles, errors);
            }

            states[objectId] = VisitState.Visited;
        }

        private enum VisitState
        {
            Unvisited,
            Visiting,
            Visited
        }
    }



}
