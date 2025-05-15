
using ContentPlatform.Api.Entities;

namespace ContentPlatform.Api.Busi.Logic.EdgeDriver;

public interface IEdgeDriver : IDisposable
{
    string DriverCode { get;}
    void Run(DriverEntity driverConfig);
    void Stop();
    void DoRead(List<TagEntity> tags);
    void DoWrite(List<TagEntity> tags);
}
