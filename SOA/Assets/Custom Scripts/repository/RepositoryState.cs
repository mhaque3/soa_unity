using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class RepositoryState
    {

        private readonly HashSet<RepositoryObject> objects;

        public RepositoryState()
        {
            objects = new HashSet<RepositoryObject>();
        }

        public void Add(CacheKey key, Hash hash)
        {
            objects.Add(new RepositoryObject(key, hash));
        }

        public ICollection<CacheKey> Diff(RepositoryState other)
        {
            HashSet<CacheKey> diffSet = new HashSet<CacheKey>();

            HashSet<RepositoryObject> copy = new HashSet<RepositoryObject>(objects);
            copy.ExceptWith(other.objects);

            foreach(RepositoryObject obj in copy)
            {
                diffSet.Add(obj.key);
            }

            return diffSet;
        }

        private class RepositoryObject
        {
            public CacheKey key;
            public Hash hash;

            public RepositoryObject(CacheKey key, Hash hash)
            {
                this.key = key;
                this.hash = hash;
            }

            public override int GetHashCode()
            {
               return key.GetHashCode() ^ hash.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                RepositoryObject other = obj as RepositoryObject;
                if (other == null)
                    return false;

                return key.Equals(other.key)
                        && hash.Equals(other.hash);
            }
        }
        
    }
}
