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

        public IReadOnlyList<ModelObject> Objects => _objects;

        public void Add(ModelObject modelObject)
        {
            ArgumentNullException.ThrowIfNull(modelObject);

            if (_objects.Any(x => x.Id == modelObject.Id))
                throw new InvalidOperationException(
                    $"ModelObject met Id '{modelObject.Id}' bestaat al.");

            _objects.Add(modelObject);
        }

        public bool Remove(ModelObject modelObject)
            => _objects.Remove(modelObject);

        public T? Get<T>(Guid id) where T : ModelObject
            => _objects.OfType<T>().FirstOrDefault(x => x.Id == id);

        public IEnumerable<T> GetObjects<T>() where T : ModelObject
            => _objects.OfType<T>();
    }



}
