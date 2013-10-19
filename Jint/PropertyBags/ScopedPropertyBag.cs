using System;
using System.Collections.Generic;
using System.Text;
using Jint.Native;

namespace Jint.PropertyBags
{
    public class ScopedPropertyBag : IPropertyBag
    {
        public void EnterScope()
        {
            _currentScope = new List<Stack<Descriptor>>();
            _scopes.Push(_currentScope);
        }

        public void ExitScope()
        {
            foreach (Stack<Descriptor> desc in _currentScope)
            {
                desc.Pop();
            }
            _scopes.Pop();
            _currentScope = _scopes.Peek();
        }

        private readonly Dictionary<string, Stack<Descriptor>> _bag = new Dictionary<string, Stack<Descriptor>>();
        private readonly Stack<List<Stack<Descriptor>>> _scopes = new Stack<List<Stack<Descriptor>>>();
        private List<Stack<Descriptor>> _currentScope;

        #region IPropertyBag Members

        public Jint.Native.Descriptor Put(string name, Jint.Native.Descriptor descriptor)
        {
            Stack<Descriptor> stack;
            if (!_bag.TryGetValue(name, out stack))
            {
                stack = new Stack<Descriptor>();
                _bag.Add(name, stack);
            }
            stack.Push(descriptor);
            _currentScope.Add(stack);
            return descriptor;
        }

        public void Delete(string name)
        {
            Stack<Descriptor> stack;
            if (_bag.TryGetValue(name, out stack) && _currentScope.Contains(stack))
            {
                stack.Pop();
                _currentScope.Remove(stack);
            }

        }

        public Jint.Native.Descriptor Get(string name)
        {
            Stack<Descriptor> stack;
            if (_bag.TryGetValue(name, out stack))
                return stack.Count > 0 ? stack.Peek() : null;
            return null;
        }

        public bool TryGet(string name, out Jint.Native.Descriptor descriptor)
        {
            descriptor = Get(name);
            return descriptor != null;
        }

        public int Count
        {
            get { return _bag.Count; }
        }

        public IEnumerable<Jint.Native.Descriptor> Values
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Descriptor>> Members

        public IEnumerator<KeyValuePair<string, Jint.Native.Descriptor>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
