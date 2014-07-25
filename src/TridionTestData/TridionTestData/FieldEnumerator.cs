using System;
using System.Collections;
using System.Collections.Generic;
using Tridion.ContentManager.CoreService.Client;

namespace TridionTestData
{
    public class FieldEnumerator : IEnumerator<Field>
    {
        private Fields fields;
        private ItemFieldDefinitionData[] definitions;

        // Enumerators are positioned before the first element until the first MoveNext() call
        int position = -1;

        public FieldEnumerator(Fields _fields, ItemFieldDefinitionData[] _definitions)
        {
            fields = _fields;
            definitions = _definitions;
        }

        public bool MoveNext()
        {
            position++;
            return (position < definitions.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Field Current
        {
            get
            {
                try
                {
                    return new Field(fields, definitions[position]);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}