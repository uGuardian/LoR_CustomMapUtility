//------------------------------------------------------------------------------
// Derived from System.Collections.Specialized.ListDictionary                                                            
//------------------------------------------------------------------------------
 
namespace uGuardian.Collections.Specialized {
    using System;
    using System.Collections;
    using System.Collections.Generic;
 
    /// <devdoc>
    ///  <para> 
    ///    This is a simple generic implementation of IDictionary using a singly linked list. This
    ///    will be smaller and faster than a Hashtable if the number of elements is 10 or less.
    ///    This should not be used if performance is important for large numbers of elements.
    ///  </para>
    /// </devdoc>
    [Serializable]
    public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        DictionaryNode head;
        int version;
        int count;
		readonly IComparer comparer;
        [NonSerialized]
        private Object _syncRoot;
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListDictionary() {
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListDictionary(IComparer comparer) {
            this.comparer = comparer;
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public TValue this[TKey key] {
            get {
                if (key == null) {
                    throw new ArgumentNullException("key", "Key cannot be null.");
                }
                DictionaryNode node = head;
                if (comparer == null) {
                    while (node != null) {
                        var oldKey = node.key;
                        if ( oldKey!= null && oldKey.Equals(key)) {
                            return node.value;
                        }
                        node = node.next;
                    }
                }
                else {
                    while (node != null) {
                        var oldKey = node.key;                        
                        if (oldKey != null && comparer.Compare(oldKey, key) == 0) {
                            return node.value;
                        }
                        node = node.next;
                    }
                }
                return default;
            }
            set {
                if (key == null) {
                    throw new ArgumentNullException("key", "Key cannot be null.");
                }
                version++;
                DictionaryNode last = null;
                DictionaryNode node;
                for (node = head; node != null; node = node.next) {
                    var oldKey = node.key;
                    if ((comparer == null) ? oldKey.Equals(key) : comparer.Compare(oldKey, key) == 0) {
                        break;
                    } 
                    last = node;
                }
                if (node != null) {
                    // Found it
                    node.value = value;
                    return;
                }
				// Not found, so add a new one
				DictionaryNode newNode = new DictionaryNode {
					key = key,
					value = value
				};
				if (last != null) {
                    last.next = newNode;
                }
                else {
                    head = newNode;
                }
                count++;
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Count {
            get {
                return count;
            }
        }   
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection<TKey> Keys {
            get {
                return new NodeKeyCollection(this);
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsFixedSize {
            get {
                return false;
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object SyncRoot {
            get {
                if( _syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new Object(), null);    
                }
                return _syncRoot; 
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection<TValue> Values {
            get {
                return new NodeValueCollection(this);
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(TKey key, TValue value) {
            if (key == null) {
                throw new ArgumentNullException("key", "Key cannot be null.");
            }
            version++;
            DictionaryNode last = null;
            DictionaryNode node;
            for (node = head; node != null; node = node.next) {
                var oldKey = node.key;
                if ((comparer == null) ? oldKey.Equals(key) : comparer.Compare(oldKey, key) == 0) {
                    throw new ArgumentException("An item with the same key has already been added. Key: {0}");
                } 
                last = node;
            }
			// Not found, so add a new one
			DictionaryNode newNode = new DictionaryNode {
				key = key,
				value = value
			};
			if (last != null) {
                last.next = newNode;
            }
            else {
                head = newNode;
            }
            count++;
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Clear() {
            count = 0;
            head = null;
            version++;
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool ContainsKey(TKey key) {
            if (key == null) {
                throw new ArgumentNullException("key", "Key cannot be null.");
            }
            for (DictionaryNode node = head; node != null; node = node.next) {
                var oldKey = node.key;
                if ((comparer == null) ? oldKey.Equals(key) : comparer.Compare(oldKey, key) == 0) {
                    return true;
                }
            }
            return false;
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)  {
            if (array==null)
                throw new ArgumentNullException("array");
            if (index < 0) 
                throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
            
            if (array.Length - index < count)
                throw new ArgumentException("Insufficient space in the target location to copy the information.");
 
            for (DictionaryNode node = head; node != null; node = node.next) {
                array.SetValue(new KeyValuePair<TKey, TValue>(node.key, node.value), index);
                index++;
            }
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new NodeEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new NodeEnumerator(this);
		}
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Remove(TKey key) {
            if (key == null) {
                throw new ArgumentNullException("key", "Key cannot be null.");
            }
            version++;
            DictionaryNode last = null;
            DictionaryNode node;
            for (node = head; node != null; node = node.next) {
                var oldKey = node.key;
                if ((comparer == null) ? oldKey.Equals(key) : comparer.Compare(oldKey, key) == 0) {
                    break;
                } 
                last = node;
            }
            if (node == null) {
                return false;
            }          
            if (node == head) {
                head = node.next;
            } else {
                last.next = node.next;
            }
            count--;
            return true;
        }
 
        private class NodeEnumerator : IDictionaryEnumerator {
			readonly ListDictionary<TKey, TValue> list;
            DictionaryNode current;
			readonly int version;
            bool start;
 
 
            public NodeEnumerator(ListDictionary<TKey, TValue> list) {
                this.list = list;
                version = list.version;
                start = true;
                current = null;
            }
 
            public object Current {
                get {
                    return Entry;
                }
            }
 
            public DictionaryEntry Entry {
                get {
                    if (current == null) {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return new DictionaryEntry(current.key, current.value);
                }
            }
 
            public object Key {
                get {
                    if (current == null) {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return current.key;
                }
            }
 
            public object Value {
                get {
                    if (current == null) {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return current.value;
                }
            }
 
            public bool MoveNext() {
                if (version != list.version) {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                if (start) {
                    current = list.head;
                    start = false;
                }
                else if (current != null) {
                    current = current.next;
                }
                return (current != null);
            }
 
            public void Reset() {
                if (version != list.version) {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                start = true;
                current = null;
            }
            
        }
 
 
        private class NodeKeyCollection : ICollection<TKey> {
			readonly ListDictionary<TKey, TValue> list;
 
            public NodeKeyCollection(ListDictionary<TKey, TValue> list) {
                this.list = list;
            }
 
            void ICollection.CopyTo(Array array, int index)  {
                if (array==null)
                    throw new ArgumentNullException("array");
                if (index < 0) 
                    throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
                for (DictionaryNode node = list.head; node != null; node = node.next) {
                    array.SetValue(node.key, index);
                    index++;
                }
            }
 
            int ICollection.Count {
                get {
                    int count = 0;
                    for (DictionaryNode node = list.head; node != null; node = node.next) {
                        count++;
                    }
                    return count;
                }
            }   
 
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }
 
            object ICollection.SyncRoot {
                get {
                    return list.SyncRoot;
                }
            }
 
            IEnumerator IEnumerable.GetEnumerator() {
                return new NodeKeyValueEnumerator(list, isKeys);
            }
 
 
            private class NodeKeyValueEnumerator: IEnumerator {
				readonly ListDictionary<TKey, TValue> list;
                DictionaryNode current;
				readonly int version;
				readonly bool isKeys;
                bool start;
 
                public NodeKeyValueEnumerator(ListDictionary<TKey, TValue> list, bool isKeys) {
                    this.list = list;
                    this.isKeys = isKeys;
                    this.version = list.version;
                    this.start = true;
                    this.current = null;
                }
 
                public object Current {
                    get {
                        if (current == null) {
                            throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                        }
                        if (!this.isKeys) {
							return this.current.value;
						}
						return this.current.key;
                    }
                }
 
                public bool MoveNext() {
                    if (version != list.version) {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    if (start) {
                        current = list.head;
                        start = false;
                    }
                    else if (current != null) {
                        current = current.next;
                    }
                    return (current != null);
                }
 
                public void Reset() {
                    if (version != list.version) {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    start = true;
                    current = null;
                }
            }        
        }

        private class NodeValueCollection : ICollection<TValue> {
			readonly ListDictionary<TKey, TValue> list;
 
            public NodeValueCollection(ListDictionary<TKey, TValue> list) {
                this.list = list;
            }
 
            void ICollection.CopyTo(Array array, int index)  {
                if (array==null)
                    throw new ArgumentNullException("array");
                if (index < 0) 
                    throw new ArgumentOutOfRangeException("index", "Non-negative number required.");
                for (DictionaryNode node = list.head; node != null; node = node.next) {
                    array.SetValue(node.value, index);
                    index++;
                }
            }
 
            int ICollection.Count {
                get {
                    int count = 0;
                    for (DictionaryNode node = list.head; node != null; node = node.next) {
                        count++;
                    }
                    return count;
                }
            }   
 
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }
 
            object ICollection.SyncRoot {
                get {
                    return list.SyncRoot;
                }
            }
 
            IEnumerator IEnumerable.GetEnumerator() {
                return new NodeKeyValueEnumerator(list, isKeys);
            }
 
 
            private class NodeKeyValueEnumerator: IEnumerator {
				readonly ListDictionary<TKey, TValue> list;
                DictionaryNode current;
				readonly int version;
				readonly bool isKeys;
                bool start;
 
                public NodeKeyValueEnumerator(ListDictionary<TKey, TValue> list, bool isKeys) {
                    this.list = list;
                    this.isKeys = isKeys;
                    this.version = list.version;
                    this.start = true;
                    this.current = null;
                }
 
                public object Current {
                    get {
                        if (current == null) {
                            throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                        }
                        if (!this.isKeys) {
							return this.current.value;
						}
						return this.current.key;
                    }
                }
 
                public bool MoveNext() {
                    if (version != list.version) {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    if (start) {
                        current = list.head;
                        start = false;
                    }
                    else if (current != null) {
                        current = current.next;
                    }
                    return (current != null);
                }
 
                public void Reset() {
                    if (version != list.version) {
                        throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }
                    start = true;
                    current = null;
                }
            }        
        }
 
        [Serializable]
        private class DictionaryNode {
            public TKey key;
            public TValue value;
            public DictionaryNode next;
        }
    }
}