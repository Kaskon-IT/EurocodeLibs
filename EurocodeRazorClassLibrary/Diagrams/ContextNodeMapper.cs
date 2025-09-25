using Blazor.Diagrams.Core.Models;
using CommonLibrary;
using CommonLibrary.Extensions;

namespace EurocodeRazorClassLibrary.Diagrams
{
    public class ContextNode : NodeModel
    {
        public BaseEurocodeContext Context { get; set; }

        public ContextNode(BaseEurocodeContext ctx) : base(ctx.Id.ToString())
        {
            Context = ctx;
            this.SetPosition(0, 0); // later layout aanpassen
            this.Title = ctx.GetType().Name;
        }
    }


    public class ContextGraphBuilder
    {
        public IEnumerable<NodeModel> Nodes { get; private set; } = new List<NodeModel>();
        public IEnumerable<LinkModel> Links { get; private set; } = new List<LinkModel>();





        public void BuildGraph(BaseEurocodeContext root)
        {
            var nodes = new Dictionary<Guid, ContextNode>();
            var links = new List<LinkModel>();

            void AddNodeRecursive(BaseEurocodeContext ctx)
            {
                if (!nodes.ContainsKey(ctx.Id))
                {
                    var node = new ContextNode(ctx);
                    nodes[ctx.Id] = node;
                }

                foreach (var child in ctx.GetNestedContexts().SelectMany(n => new[] { n.Child }))
                {
                    if (!nodes.ContainsKey(child.Id))
                        nodes[child.Id] = new ContextNode(child);

                    links.Add(new LinkModel(nodes[ctx.Id], nodes[child.Id]));

                    AddNodeRecursive(child);
                }
            }




            AddNodeRecursive(root);

            Nodes = nodes.Values;
            Links = links;
        }
    }

}
