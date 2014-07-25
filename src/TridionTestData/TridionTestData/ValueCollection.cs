using System.Xml;
using Tridion.ContentManager.CoreService.Client;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// A wrapper around the content or metadata fields of a Tridion item.
/// </summary>
/// 
namespace TridionTestData
{
    public class ValueCollection
    {
        private Fields fields;
        private ItemFieldDefinitionData definition;

        public ValueCollection(Fields _fields, ItemFieldDefinitionData _definition)
        {
            fields = _fields;
            definition = _definition;
        }

        public int Count
        {
            get { return fields.GetFieldElements(definition).Count(); }
        }

        public bool IsLinkField
        {
            get { return definition is ComponentLinkFieldDefinitionData || definition is ExternalLinkFieldDefinitionData || definition is MultimediaLinkFieldDefinitionData; }
        }
        public bool IsRichTextField
        {
            get { return definition is XhtmlFieldDefinitionData; }
        }

        public string this[int i]
        {
            get
            {
                XmlElement[] elements = fields.GetFieldElements(definition).ToArray();
                if (i >= elements.Length) throw new IndexOutOfRangeException();
                if (IsLinkField)
                {
                    return elements[i].Attributes["xlink:href"].Value;
                }
                else
                {
                    return elements[i].InnerXml.ToString(); // used to be InnerText
                }
            }
            set
            {
                XmlElement[] elements = fields.GetFieldElements(definition).ToArray<XmlElement>();
                if (i >= elements.Length) throw new IndexOutOfRangeException();
                if (IsLinkField)
                {
                    elements[i].SetAttribute("href", "http://www.w3.org/1999/xlink", value);
                    elements[i].SetAttribute("type", "http://www.w3.org/1999/xlink", "simple");
                    // TODO: should we clear the title for MMCLink and CLink fields? They will automatically be updated when we save the xlink:href.
                }
                else
                {
                    if (IsRichTextField)
                    {
                        elements[i].InnerXml = value;
                    }
                    else
                    {
                        elements[i].InnerText = value;
                    }
                }
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return fields.GetFieldElements(definition).Select<XmlElement, string>(elm => IsLinkField ? elm.Attributes["xlink:href"].Value : elm.InnerXml.ToString()
            ).GetEnumerator();
        }
    }

}