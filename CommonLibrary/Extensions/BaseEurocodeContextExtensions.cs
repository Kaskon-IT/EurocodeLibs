using CommonLibrary.Models;
using System.Reflection;

namespace CommonLibrary.Extensions
{
    public static class EurocodeContextExtensions
    {
        public static IEnumerable<NestedContextInfo> GetNestedContexts(this BaseEurocodeContext ctx)
        {
            var map = new Dictionary<Guid, NestedContextInfo>();
            CollectNested(ctx, map);
            return map.Values;
        }

        private static void CollectNested(BaseEurocodeContext ctx, Dictionary<Guid, NestedContextInfo> map)
        {
            var props = ctx.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
            {
                var val = prop.GetValue(ctx);
                if (val == null) continue;

                if (val is BaseEurocodeContext child)
                {
                    AddRelation(ctx, prop.Name, child, map);
                }
                else if (val is IEnumerable<BaseEurocodeContext> list)
                {
                    // recursief (even uitzetten)
                    //foreach (var item in list)
                    //{
                    //    if (item != null)
                    //        AddRelation(ctx, prop.Name, item, map);
                    //}
                }
            }
        }

        private static void AddRelation(BaseEurocodeContext parent, string propName, BaseEurocodeContext child, Dictionary<Guid, NestedContextInfo> map)
        {
            if (!map.TryGetValue(child.Id, out var info))
            {
                info = new NestedContextInfo(child);
                map[child.Id] = info;
            }

            info.Parents.Add((parent, propName));
        }
    }


}
