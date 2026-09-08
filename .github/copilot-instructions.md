# Copilot Instructions

## Project Guidelines
- Before concluding that a steel profile type is absent, search for shared/base-derived types such as RectangularHollowSection : StaalProfiel across the full solution and referenced profile repositories.

## Model Structure
- In CommonLibrary, Model is a forest: Assembly and concrete Part types may be roots; Assembly may contain Assemblies and Parts; Parts may not parent hierarchy objects; ParentId is only the model-tree relation; hierarchy policy must remain extensible for custom ModelObject types.