namespace CommonLibrary.Models
{
    public class NestedContextInfo
    {
        public BaseEurocodeContext Child { get; set; }
        public List<(BaseEurocodeContext Parent, string PropertyName)> Parents { get; }

        public NestedContextInfo(BaseEurocodeContext child)
        {
            Child = child;
            Parents = [];
        }
    }


}
