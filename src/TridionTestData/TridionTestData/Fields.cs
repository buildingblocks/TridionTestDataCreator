using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace TridionTestData
{
    public class Fields : IEnumerable<Field>
    {
        private ItemFieldDefinitionData[] definitions;
        private XmlNamespaceManager namespaceManager;

        private XmlElement root; // the root element under which these fields live

        // at any point EITHER data OR parent has a value
        private SchemaFieldsData data; // the schema fields data as retrieved from the core service
        private Fields parent; // the parent fields (so we're an embedded schema), where we can find the data

        public Fields(SchemaFieldsData _data, ItemFieldDefinitionData[] _definitions, string _content = null, string _rootElementName = null)
        {
            data = _data;
            definitions = _definitions;
            var content = new XmlDocument();
            if (!string.IsNullOrEmpty(_content))
            {
                content.LoadXml(_content);
            }
            else
            {
                content.AppendChild(content.CreateElement(string.IsNullOrEmpty(_rootElementName) ? _data.RootElementName : _rootElementName, _data.NamespaceUri));
            }
            root = content.DocumentElement;
            namespaceManager = new XmlNamespaceManager(content.NameTable);
            namespaceManager.AddNamespace("custom", _data.NamespaceUri);
        }
        public Fields(Fields _parent, ItemFieldDefinitionData[] _definitions, XmlElement _root)
        {
            definitions = _definitions;
            parent = _parent;
            root = _root;
        }

        public static Fields ForContentOf(SchemaFieldsData _data)
        {
            return new Fields(_data, _data.Fields);
        }
        public static Fields ForContentOf(SchemaFieldsData _data, ComponentData _component)
        {
            return new Fields(_data, _data.Fields, _component.Content);
        }
        public static Fields ForMetadataOf(SchemaFieldsData _data, RepositoryLocalObjectData _item)
        {
            return new Fields(_data, _data.MetadataFields, _item.Metadata, "Metadata");
        }

        public string NamespaceUri
        {
            get { return data != null ? data.NamespaceUri : parent.NamespaceUri; }
        }
        public XmlNamespaceManager NamespaceManager
        {
            get { return parent != null ? parent.namespaceManager : namespaceManager; }
        }

        internal IEnumerable<XmlElement> GetFieldElements(ItemFieldDefinitionData definition)
        {
            return root.SelectNodes("custom:" + definition.Name, NamespaceManager).OfType<XmlElement>();
        }
        internal XmlElement AddFieldElement(ItemFieldDefinitionData definition)
        {
            var newElement = root.OwnerDocument.CreateElement(definition.Name, NamespaceUri);

            XmlNodeList nodes = root.SelectNodes("custom:" + definition.Name, NamespaceManager);
            XmlElement referenceElement = null;
            if (nodes.Count > 0)
            {
                referenceElement = (XmlElement)nodes[nodes.Count - 1];
            }
            else
            {
                // this is the first value for this field, find its position in the XML based on the field order in the schema
                bool foundUs = false;
                for (int i = definitions.Length - 1; i >= 0; i--)
                {
                    if (!foundUs)
                    {
                        if (definitions[i].Name == definition.Name)
                        {
                            foundUs = true;
                        }
                    }
                    else
                    {
                        var values = GetFieldElements(definitions[i]);
                        if (values.Count() > 0)
                        {
                            referenceElement = values.Last();
                            break; // from for loop
                        }
                    }
                } // for every definition in reverse order
            } // no existing values found
            root.InsertAfter(newElement, referenceElement); // if referenceElement is null, will insert as first child
            return newElement;
        }

        public IEnumerator<Field> GetEnumerator()
        {
            return (IEnumerator<Field>)new FieldEnumerator(this, definitions);
        }
        public bool Exists(string _name)
        {
            return definitions.Any(def => def.Name == _name);
        }
        public Field this[string _name]
        {
            get
            {
                var definition = definitions.First<ItemFieldDefinitionData>(ifdd => ifdd.Name == _name);
                if (definition == null) throw new ArgumentOutOfRangeException("Unknown field '" + _name + "'");
                return new Field(this, definition);
            }
        }

        public override string ToString()
        {
            return root.OuterXml;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        IEnumerator<Field> IEnumerable<Field>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}