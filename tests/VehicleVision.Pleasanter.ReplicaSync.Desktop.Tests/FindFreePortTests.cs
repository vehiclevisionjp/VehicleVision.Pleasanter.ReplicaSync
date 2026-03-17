using System.Net;
using System.Net.Sockets;

namespace VehicleVision.Pleasanter.ReplicaSync.Desktop.Tests;

public class FindFreePortTests
{
    [Fact]
    public void FindFreePortShouldReturnValidPort()
    {
        var port = FindFreePort();

        Assert.InRange(port, 1024, 65535);
    }

    [Fact]
    public void FindFreePortShouldReturnPortsInValidRange()
    {
        var port1 = FindFreePort();
        var port2 = FindFreePort();

        // 連続呼び出しで異なるポートが返される可能性が高い（同一ポートでも正常だが通常は異なる）
        Assert.InRange(port1, 1024, 65535);
        Assert.InRange(port2, 1024, 65535);
    }

    [Fact]
    public void FindFreePortShouldReturnAvailablePort()
    {
        var port = FindFreePort();

        // 取得したポートで実際にリッスンできることを確認
        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
    }

    /// <summary>Desktop Program.cs と同じロジック。</summary>
    private static int FindFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
