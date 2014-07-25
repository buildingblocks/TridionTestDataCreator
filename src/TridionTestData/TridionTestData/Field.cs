using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace TridionTestData
{
    public class Field
    {
        private Fields fields;
        public ItemFieldDefinitionData definition;

        public Field(Fields _fields, ItemFieldDefinitionData _definition)
        {
            fields = _fields;
            definition = _definition;
        }

        public string Name
        {
            get { return definition.Name; }
        }
        public Type Type
        {
            get { return definition.GetType(); }
        }
        public bool Mandatory
        {
            get { return definition.MinOccurs != 0; }
        }
        
        public string Value
        {
            get
            {
                return Values.Count > 0 ? Values[0] : null;
            }
            set
            {
                if (Values.Count == 0) fields.AddFieldElement(definition);
                Values[0] = value;
            }
        }
        public ValueCollection Values
        {
            get
            {
                return new ValueCollection(fields, definition);
            }
        }

        public void AddValue(string value = null)
        {
            XmlElement newElement = fields.AddFieldElement(definition);
            if (value != null) newElement.InnerText = value;
        }

        public void RemoveValue(string value)
        {
            var elements = fields.GetFieldElements(definition);
            foreach (var element in elements)
            {
                if (element.InnerText == value)
                {
                    element.ParentNode.RemoveChild(element);
                }
            }
        }

        public void RemoveValue(int i)
        {
            var elements = fields.GetFieldElements(definition).ToArray();
            elements[i].ParentNode.RemoveChild(elements[i]);
        }

        public IEnumerable<Fields> SubFields
        {
            get
            {
                var embeddedFieldDefinition = definition as EmbeddedSchemaFieldDefinitionData;
                if (embeddedFieldDefinition != null)
                {
                    var elements = fields.GetFieldElements(definition);
                    foreach (var element in elements)
                    {
                        yield return new Fields(fields, embeddedFieldDefinition.EmbeddedFields, (XmlElement)element);
                    }
                }
            }
        }

        public Fields GetSubFields(int i = 0)
        {
            var embeddedFieldDefinition = definition as EmbeddedSchemaFieldDefinitionData;
            if (embeddedFieldDefinition != null)
            {
                var elements = fields.GetFieldElements(definition);
                if (i == 0 && !elements.Any())
                {
                    // you can always set the first value of any field without calling AddValue, so same applies to embedded fields
                    AddValue();
                    elements = fields.GetFieldElements(definition);
                }
                return new Fields(fields, embeddedFieldDefinition.EmbeddedFields, elements.ToArray()[i]);
            }
            else
            {
                throw new InvalidOperationException("You can only GetSubField on an EmbeddedSchemaField");
            }
        }
        // The subfield with the given name of this field
        public Field this[string name]
        {
            get { return GetSubFields()[name]; }
        }
        // The subfields of the given value of this field
        public Fields this[int i]
        {
            get { return GetSubFields(i); }
        }

        public LinkToCategoryData Category 
        { 
            get
            {
                var keywordFieldDefinition = this.definition as KeywordFieldDefinitionData;
                if ((keywordFieldDefinition) != null)
                {
                    return keywordFieldDefinition.Category;
                }
                return null;
            }
        }

        public LinkToSchemaData[] AllowedTargetSchemas
        {
            get
            {
                var componentLinkFieldDefinition = this.definition as ComponentLinkFieldDefinitionData;
                if (componentLinkFieldDefinition != null)
                {
                    return componentLinkFieldDefinition.AllowedTargetSchemas;
                }
                var multimediaLinkFieldDefinition = this.definition as MultimediaLinkFieldDefinitionData;
                if (multimediaLinkFieldDefinition != null)
                {
                    return multimediaLinkFieldDefinition.AllowedTargetSchemas;
                }
                return null;
            }
        }

    }
}