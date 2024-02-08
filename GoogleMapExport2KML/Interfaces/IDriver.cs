namespace GoogleMapExport2KML.Interfaces;

public interface IDriver : IDisposable
{
    IBrowser Browser { get; }
}
