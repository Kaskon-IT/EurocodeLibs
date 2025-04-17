namespace CommonLibrary.Interfaces
{

    public interface IInitializeWithProjectInfo
    {
        void InitProjectInfo(object projectInfo);
    }

    public interface IInitializeWithAssemblage
    {
        void Initialize(object assemblage);

    }
}
