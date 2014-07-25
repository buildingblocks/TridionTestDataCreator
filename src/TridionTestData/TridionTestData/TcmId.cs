using System;

public class TcmId
{
    private readonly int _publicationId;
    private readonly int _itemId;
    private readonly int _type;

    private const int Publication = 1;

    public int PublicationId
    {
        get
        {
            return Type == Publication ? _itemId : _publicationId;
        }
    }

    public int ItemId
    {
        get
        {
            return _itemId;
        }
    }

    public int Type
    {
        get
        {
            return _type;
        }
    }

    public TcmId(int publication, int itemId, int type)
    {
        _publicationId = publication;
        _itemId = itemId;
        _type = type;
    }

    public TcmId(string publication, string itemId, string type)
    {
        if (!String.IsNullOrEmpty(publication))
        {
            Int32.TryParse(publication, out _publicationId);
        }

        if (!String.IsNullOrEmpty(itemId))
        {
            Int32.TryParse(itemId, out _itemId);
        }

        if (!String.IsNullOrEmpty(type))
        {
            Int32.TryParse(type, out _type);
        }
    }

    public TcmId(string tcmId)
    {
        string[] items = tcmId.Replace("tcm:", "").Split('-');

        // set publicaiton id
        if (items.Length > 0)
        {
            Int32.TryParse(items[0], out _publicationId);
        }
        else
        {
            _publicationId = 0;
        }

        // set item id
        if (items.Length > 1)
        {
            Int32.TryParse(items[1], out _itemId);
        }
        else
        {
            _itemId = 0;
        }

        // set item id
        if (items.Length > 2)
        {
            Int32.TryParse(items[2], out _type);
        }
        else
        {
            _type = 0;
        }
    }

    public override string ToString()
    {
        return String.Format(
            "tcm:{0}-{1}{2}",
            Type == Publication ? 0 : _publicationId,
            _itemId,
            (_type != 0) ? "-" + _type : "");
    }

    public TcmId ForPublication(int publication)
    {
        return new TcmId(publication, ItemId, Type);
    }
}