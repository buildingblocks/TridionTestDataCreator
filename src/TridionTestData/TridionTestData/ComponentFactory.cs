using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using log4net;

namespace TridionTestData
{
    public class ComponentFactory
    {
        private ILog _log;
        private CoreServiceClient _client;

        private readonly string _tridionDomain;
        private readonly string _tridionPort;

        private readonly string _username;
        private readonly string _password;
        private readonly string _version;

        private readonly ReadOptions _readOptions = new ReadOptions();

        private string TridionEndpointAddress
        {
            get { return String.Format("net.tcp://{0}:{1}/CoreService/2011/netTcp", _tridionDomain, _tridionPort); }
        }

        private CoreServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    var binding = new NetTcpBinding
                    {
                        MaxReceivedMessageSize = 2147483647,
                        ReaderQuotas = new XmlDictionaryReaderQuotas
                        {
                            MaxStringContentLength = 2147483647,
                            MaxArrayLength = 2147483647
                        }
                    };
                    var endpoint = new EndpointAddress(TridionEndpointAddress);

                    _client = new CoreServiceClient(binding, endpoint);
                    if (_client.ChannelFactory.Credentials != null)
                        _client.ChannelFactory.Credentials.Windows.ClientCredential = new NetworkCredential(_username,
                                                                                                            _password);

                    _log.Debug("Connected to Tridion");
                }

                return _client;
            }
        }

        public ComponentFactory(string tridionDomain, string tridionPort, string username,string password,string version,ILog log)
        {
            _tridionDomain = tridionDomain;
            _tridionPort = tridionPort;
            _username = username;
            _password = password;
            _version = version;
            _log = log;
        }

        public void CreateComponents(string schemaPublicationTcmId, TcmId testContentFolderTcmId)
        {
            var versionFolder = CreateVersionFolder(testContentFolderTcmId);
            if (versionFolder != null)
            {
                try
                {
                    var schemaXml = GetSchemas(schemaPublicationTcmId);

                    foreach (XElement element in schemaXml.Elements())
                    {
                        var schemaTcmId = new TcmId(element.Attribute("ID").Value);

                        //Create Mandatory components
                        CreateComponentFromSchema(schemaTcmId, versionFolder, true);

                        //Create Non Mandatory components
                        CreateComponentFromSchema(schemaTcmId, versionFolder, false);
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message);
                }
            }
            else
            {
                _log.Error("No Version Folder Created");
            }
        }

        private TcmId CreateVersionFolder(TcmId testContentFolderTcmId)
        {
            try
            {
                var folder = Client.GetDefaultData(ItemType.Folder, testContentFolderTcmId.ToString()) as FolderData;
                if (folder != null)
                {
                    folder.Title = String.Format("{0} - {1}", _version, DateTime.Now);
                    IdentifiableObjectData savedFolder = Client.Save(folder, _readOptions);
                    return new TcmId(savedFolder.Id);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
            _log.Error("Failed to create version folder");
            return null;
        }

        private XElement GetSchemas(string schemaPublicationTcmId)
        {
            var filter = new RepositoryItemsFilterData
            {
                ItemTypes = new[] { ItemType.Schema, },
                Recursive = true
            };
            XElement schemaXml = Client.GetListXml(schemaPublicationTcmId, filter);
            return schemaXml;
        }

        private string CreateComponentFromSchema(TcmId schemaTcmId, TcmId versionFolder, bool onlyMandatory)
        {
            SchemaData schema = Client.Read(
                    schemaTcmId.ForPublication(versionFolder.PublicationId).ToString(),
                    _readOptions
                ) as SchemaData;

            SchemaFieldsData schemaFields = Client.ReadSchemaFields(
                    schemaTcmId.ForPublication(versionFolder.PublicationId).ToString(),
                    true, _readOptions
                );

            if (schema != null && schema.Purpose == SchemaPurpose.Component)
            {

                //Create a new component in the folder specified
                var component = Client.GetDefaultData(
                    ItemType.Component,
                    versionFolder.ToString()
                                    ) as ComponentData;

                if (component != null)
                {
                    component.Schema = new LinkToSchemaData
                        {
                            IdRef = schemaTcmId.ForPublication(versionFolder.PublicationId).ToString()
                        };

                    var fields = Fields.ForContentOf(schemaFields);
                    var metadataFields = Fields.ForMetadataOf(schemaFields, component);

                    PopulateFieldData(onlyMandatory, fields, versionFolder);
                    PopulateFieldData(onlyMandatory, metadataFields, versionFolder);

                    component.Content = fields.ToString();
                    Console.WriteLine(component.Content);

                    if (metadataFields.Any())
                    {
                        component.Metadata = metadataFields.ToString();
                        Console.WriteLine(component.Metadata);
                    }

                    component.Title = onlyMandatory
                                          ? "Mandatory : " + schema.Title + " Content"
                                          : "Non Mandatory : " + schema.Title + " Content";

                    try
                    {
                        //Save the component
                        IdentifiableObjectData savedComponent = Client.Save(component, _readOptions);

                        //Check in using the Id of the new object
                        Client.CheckIn(savedComponent.Id, null);

                        return savedComponent.Id;
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message);
                    }
                }
                else
                {
                    _log.Error("Error creating default component");
                }
            }

            return string.Empty;
        }

        private void PopulateFieldData(bool onlyMandatory, Fields fields, TcmId versionFolder)
        {
            // let's first quickly list all values of all fields
            foreach (Field field in fields)
            {
                Console.WriteLine("{0} (Type={1})", field.Name, field.Type);

                //Populate the feild if it is Mandatory or the onlyMandatory bool feild is false
                if (!onlyMandatory | field.Mandatory)
                {
                    if (field.Type == typeof(ComponentLinkFieldDefinitionData))
                    {
                        
                    }
                    else if (field.Type == typeof(DateFieldDefinitionData))
                    {
                        field.AddValue(TestContent.Date);
                    }
                    else if (field.Type == typeof(EmbeddedSchemaFieldDefinitionData))
                    {
                    }
                    else if (field.Type == typeof(ExternalLinkFieldDefinitionData))
                    {
                    }
                    else if (field.Type == typeof(KeywordFieldDefinitionData))
                    {
                    }
                    else if (field.Type == typeof(MultiLineTextFieldDefinitionData))
                    {
                        field.Value = TestContent.RichText;
                    }
                    else if (field.Type == typeof(MultimediaLinkFieldDefinitionData))
                    {
                        field.Value = TestContent.Image;
                    }
                    else if (field.Type == typeof(NumberFieldDefinitionData))
                    {
                        field.Value = "1";
                    }
                    else if (field.Type == typeof(SingleLineTextFieldDefinitionData))
                    {
                        var def = field.definition as SingleLineTextFieldDefinitionData;
                        if (def != null && def.List != null)
                        {
                            field.Value = def.List.Entries.FirstOrDefault();
                        }
                        else
                        {
                            field.Value = TestContent.Text;
                        }
                    }
                    else if (field.Type == typeof(XhtmlFieldDefinitionData))
                    {
                        field.Value = TestContent.RichText;
                    }
                }
            }
        }

    }
}
